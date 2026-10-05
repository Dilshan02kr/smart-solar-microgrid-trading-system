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
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.databinding.FragmentOperatorReservationsBinding
import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class OperatorReservationsFragment : Fragment() {
    private var binding: FragmentOperatorReservationsBinding? = null
    private val adapter = OperatorReservationAdapter()
    private val viewModel: OperatorReservationsViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { OperatorReservationsViewModel(app.operatorRepository) }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentOperatorReservationsBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.reservationList?.adapter = adapter
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        binding?.statusGroup?.addOnButtonCheckedListener { _, checkedId, isChecked ->
            if (isChecked) viewModel.selectStatus(
                if (checkedId == R.id.approved_operator_filter) ReservationStatus.APPROVED else ReservationStatus.PENDING,
            )
        }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) { viewModel.state.collect(::render) }
        }
    }

    private fun render(state: OperatorReservationsState) {
        val current = binding ?: return
        current.progressIndicator.isVisible = state.isLoading
        current.errorContainer.isVisible = state.error != null
        current.reservationList.isVisible = !state.isLoading && state.error == null && state.visibleReservations.isNotEmpty()
        current.emptyMessage.isVisible = !state.isLoading && state.error == null && state.visibleReservations.isEmpty()
        state.error?.let {
            current.errorMessage.text = it.message
            if (it.httpStatus == 401) findNavController().navigate(R.id.action_global_loginFragment)
        }
        val selected = if (state.selectedStatus == ReservationStatus.APPROVED) R.id.approved_operator_filter else R.id.pending_operator_filter
        if (current.statusGroup.checkedButtonId != selected) current.statusGroup.check(selected)
        adapter.submitList(state.visibleReservations)
    }

    override fun onDestroyView() {
        binding?.reservationList?.adapter = null
        binding = null
        super.onDestroyView()
    }
}
