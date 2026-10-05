package com.smartsolar.microgrid.ui.operator

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.databinding.FragmentOperatorSlotsBinding
import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class OperatorSlotsFragment : Fragment() {
    private var binding: FragmentOperatorSlotsBinding? = null
    private val adapter = OperatorSlotAdapter(::confirmAvailabilityChange)
    private val viewModel: OperatorSlotsViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { OperatorSlotsViewModel(app.operatorRepository) }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentOperatorSlotsBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.slotList?.adapter = adapter
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        binding?.refreshButton?.setOnClickListener { viewModel.load() }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) { viewModel.state.collect(::render) }
        }
    }

    private fun confirmAvailabilityChange(slot: EnergySlot) {
        val makeAvailable = !slot.isAvailable
        MaterialAlertDialogBuilder(requireContext())
            .setTitle(if (makeAvailable) R.string.mark_available_title else R.string.mark_unavailable_title)
            .setMessage(if (makeAvailable) R.string.mark_available_message else R.string.mark_unavailable_message)
            .setNegativeButton(R.string.cancel, null)
            .setPositiveButton(if (makeAvailable) R.string.mark_available else R.string.mark_unavailable) { _, _ ->
                viewModel.setAvailability(slot, makeAvailable)
            }
            .show()
    }

    private fun render(state: OperatorSlotsState) {
        val current = binding ?: return
        val hasContent = state.slots.isNotEmpty()
        current.progressIndicator.isVisible = state.isLoading || state.mutatingSlotId != null
        current.errorContainer.isVisible = state.error != null
        current.slotList.isVisible = !state.isLoading && state.error == null && hasContent
        current.emptyMessage.isVisible = !state.isLoading && state.error == null && !hasContent
        current.refreshButton.isEnabled = !state.isLoading && state.mutatingSlotId == null
        adapter.mutatingSlotId = state.mutatingSlotId
        adapter.submitList(state.slots)

        state.error?.let {
            current.errorMessage.text = if (it.code == "OPERATOR_STATION_NOT_ASSIGNED") {
                getString(R.string.operator_station_not_assigned)
            } else it.message
            if (it.httpStatus == 401) findNavController().navigate(R.id.action_global_loginSelectionFragment)
        }
        state.mutationError?.let {
            val message = if (it.code == "SLOT_HAS_ACTIVE_RESERVATION") {
                getString(R.string.slot_active_reservation_conflict)
            } else it.message
            MaterialAlertDialogBuilder(requireContext())
                .setTitle(R.string.slot_update_failed)
                .setMessage(message)
                .setPositiveButton(android.R.string.ok) { _, _ -> viewModel.clearMutationError() }
                .setOnDismissListener { viewModel.clearMutationError() }
                .show()
        }
    }

    override fun onDestroyView() {
        binding?.slotList?.adapter = null
        binding = null
        super.onDestroyView()
    }
}
