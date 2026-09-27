package com.smartsolar.microgrid.ui.stations

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
import com.smartsolar.microgrid.databinding.FragmentStationDetailsBinding
import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.domain.model.StationStatus
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.UiState
import kotlinx.coroutines.launch

class StationDetailsFragment : Fragment() {
    private var binding: FragmentStationDetailsBinding? = null
    private val stationId by lazy { requireArguments().getString(StationsFragment.ARG_STATION_ID).orEmpty() }
    private val viewModel: StationDetailsViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { StationDetailsViewModel(stationId, app.stationRepository) }
    }
    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentStationDetailsBinding.inflate(inflater, container, false).also { binding = it }.root
    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        binding?.slotsButton?.setOnClickListener {
            findNavController().navigate(R.id.action_stationDetailsFragment_to_stationSlotsFragment, bundleOf(StationsFragment.ARG_STATION_ID to stationId))
        }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) { viewModel.state.collect(::render) }
        }
    }
    private fun render(state: UiState<Station>) {
        val b = binding ?: return
        b.progressIndicator.isVisible = state is UiState.Loading
        b.errorContainer.isVisible = state is UiState.Error
        b.contentContainer.isVisible = state is UiState.Content
        if (state is UiState.Error) {
            b.errorMessage.text = state.error.message
            if (state.error.httpStatus == 401) findNavController().navigate(R.id.action_global_loginFragment)
        }
        if (state is UiState.Content) with(state.value) {
            b.stationName.text = name; b.locationValue.text = locationName
            b.capacityValue.text = getString(R.string.capacity_value, totalCapacityKw)
            b.scheduleValue.text = operationalSchedule; b.statusValue.text = status.name
            b.coordinatesValue.text = getString(R.string.coordinates_value, latitude, longitude)
            b.slotsButton.isVisible = status == StationStatus.ACTIVE
            b.unavailableMessage.isVisible = status == StationStatus.INACTIVE
        }
    }
    override fun onDestroyView() { binding = null; super.onDestroyView() }
}

