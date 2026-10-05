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
import com.smartsolar.microgrid.core.util.OperatorErrorMessage
import com.smartsolar.microgrid.core.util.OperatorPresentation
import com.smartsolar.microgrid.databinding.FragmentOperatorDashboardBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class OperatorDashboardFragment : Fragment() {
    private var binding: FragmentOperatorDashboardBinding? = null
    private var hasResumed = false
    private val viewModel: OperatorDashboardViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory {
            OperatorDashboardViewModel(
                app.operatorRepository,
                app.stationRepository,
                app.sessionRepository,
            )
        }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentOperatorDashboardBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        binding?.refreshButton?.setOnClickListener { viewModel.load() }
        binding?.scanButton?.setOnClickListener {
            findNavController().navigate(R.id.action_operatorDashboardFragment_to_qrScannerFragment)
        }
        binding?.reservationsButton?.setOnClickListener {
            findNavController().navigate(R.id.action_operatorDashboardFragment_to_operatorReservationsFragment)
        }
        binding?.logoutButton?.setOnClickListener {
            viewModel.logout()
            findNavController().navigate(R.id.action_global_loginFragment)
        }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    override fun onResume() {
        super.onResume()
        if (hasResumed) viewModel.load() else hasResumed = true
    }

    private fun render(state: OperatorDashboardState) {
        val current = binding ?: return
        current.progressIndicator.isVisible = state.isLoading
        current.errorContainer.isVisible = state.error != null
        current.contentContainer.isVisible = state.summary != null
        state.error?.let { error ->
            current.errorMessage.text = when (OperatorPresentation.errorMessage(error.code)) {
                OperatorErrorMessage.STATION_NOT_ASSIGNED -> getString(R.string.operator_station_not_assigned)
                else -> error.message
            }
            if (error.httpStatus == 401) findNavController().navigate(R.id.action_global_loginFragment)
        }
        state.summary?.let {
            current.pendingCount.text = getString(R.string.count_value, it.pendingReservationCount)
            current.approvedCount.text = getString(R.string.count_value, it.approvedFutureReservationCount)
            current.stationValue.text = state.stationLabel ?: getString(R.string.station_context_unavailable)
        }
    }

    override fun onDestroyView() {
        binding = null
        super.onDestroyView()
    }
}
