package com.smartsolar.microgrid.ui.reservations

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.core.os.bundleOf
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.core.util.ReservationGroup
import com.smartsolar.microgrid.databinding.FragmentMyReservationsBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class MyReservationsFragment : Fragment() {
    private var binding: FragmentMyReservationsBinding? = null
    private var hasResumed = false
    private val adapter = ReservationAdapter(::openDetails, ::openQr)
    private val viewModel: MyReservationsViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { MyReservationsViewModel(app.reservationRepository, app.stationRepository) }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentMyReservationsBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.reservationList?.adapter = adapter
        binding?.refreshButton?.setOnClickListener { viewModel.load(refresh = true) }
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        binding?.filterGroup?.addOnButtonCheckedListener { _, checkedId, isChecked ->
            if (isChecked) viewModel.select(
                when (checkedId) {
                    R.id.approved_filter -> ReservationGroup.APPROVED
                    R.id.history_filter -> ReservationGroup.HISTORY
                    else -> ReservationGroup.PENDING
                },
            )
        }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    override fun onResume() {
        super.onResume()
        if (hasResumed) viewModel.load(refresh = true) else hasResumed = true
    }

    private fun render(state: MyReservationsState) {
        val currentBinding = binding ?: return
        currentBinding.progressIndicator.isVisible = state.isLoading || state.isRefreshing
        currentBinding.errorContainer.isVisible = state.error != null
        currentBinding.contentContainer.isVisible = !state.isLoading && state.error == null
        state.error?.let {
            currentBinding.errorMessage.text = it.message
            if (it.httpStatus == 401) findNavController().navigate(R.id.action_global_loginFragment)
        }
        currentBinding.stationWarning.isVisible = state.stationNamesUnavailable
        adapter.submitList(state.visibleItems)
        currentBinding.emptyMessage.isVisible = state.visibleItems.isEmpty()
        currentBinding.emptyMessage.setText(
            when (state.selectedGroup) {
                ReservationGroup.PENDING -> R.string.no_pending_reservations
                ReservationGroup.APPROVED -> R.string.no_approved_reservations
                ReservationGroup.HISTORY -> R.string.no_reservation_history
            },
        )
    }

    private fun openDetails(id: String) {
        findNavController().navigate(
            R.id.action_myReservationsFragment_to_reservationDetailsFragment,
            bundleOf(CreateReservationFragment.ARG_RESERVATION_ID to id),
        )
    }

    private fun openQr(id: String) {
        findNavController().navigate(
            R.id.action_myReservationsFragment_to_reservationQrFragment,
            bundleOf(CreateReservationFragment.ARG_RESERVATION_ID to id),
        )
    }

    override fun onDestroyView() {
        binding?.reservationList?.adapter = null
        binding = null
        super.onDestroyView()
    }
}
