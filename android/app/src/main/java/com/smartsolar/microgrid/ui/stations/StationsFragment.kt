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
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.databinding.FragmentStationsBinding
import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.UiState
import kotlinx.coroutines.launch

class StationsFragment : Fragment() {
    private var binding: FragmentStationsBinding? = null
    private val adapter = StationAdapter(::openStation)
    private val viewModel: StationsViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { StationsViewModel(app.stationRepository) }
    }
    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentStationsBinding.inflate(inflater, container, false).also { binding = it }.root
    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.items?.adapter = adapter
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        binding?.viewMapButton?.setOnClickListener {
            findNavController().navigate(R.id.action_stationsFragment_to_stationsMapFragment)
        }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                launch { viewModel.state.collect(::render) }
                launch { viewModel.lastSuccessfulSync.collect(::renderLastSuccessfulSync) }
            }
        }
    }

    private fun renderLastSuccessfulSync(epochMillis: Long?) {
        val current = binding ?: return
        current.lastSyncMessage.isVisible = epochMillis != null
        current.lastSyncMessage.text = epochMillis?.let {
            getString(
                R.string.last_station_sync,
                BookingPresentation.timestamp(java.time.Instant.ofEpochMilli(it).toString()),
            )
        }
    }
    private fun render(state: UiState<List<Station>>) {
        val b = binding ?: return
        b.progressIndicator.isVisible = state is UiState.Loading
        b.errorContainer.isVisible = state is UiState.Error
        b.emptyMessage.isVisible = state is UiState.Empty
        b.items.isVisible = state is UiState.Content
        if (state is UiState.Error) {
            b.errorMessage.text = state.error.message
            if (state.error.httpStatus == 401) findNavController().navigate(R.id.action_global_loginSelectionFragment)
        }
        if (state is UiState.Content) adapter.submitList(state.value)
    }
    private fun openStation(id: String) {
        findNavController().navigate(R.id.action_stationsFragment_to_stationDetailsFragment, bundleOf(ARG_STATION_ID to id))
    }
    override fun onDestroyView() { binding?.items?.adapter = null; binding = null; super.onDestroyView() }
    companion object { const val ARG_STATION_ID = "stationId" }
}

