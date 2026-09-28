package com.smartsolar.microgrid.ui.slots

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
import com.smartsolar.microgrid.databinding.FragmentListStateBinding
import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.UiState
import com.smartsolar.microgrid.ui.stations.StationsFragment
import kotlinx.coroutines.launch

class StationSlotsFragment : Fragment() {
    private var binding: FragmentListStateBinding? = null
    private var hasResumed = false
    private val stationId by lazy { requireArguments().getString(StationsFragment.ARG_STATION_ID).orEmpty() }
    private val adapter = EnergySlotAdapter(::book)
    private val viewModel: StationSlotsViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { StationSlotsViewModel(stationId, app.energySlotRepository) }
    }
    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View = FragmentListStateBinding.inflate(inflater, container, false).also { binding = it }.root
    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.screenTitle?.setText(R.string.slots_title); binding?.emptyMessage?.setText(R.string.no_slots)
        binding?.items?.adapter = adapter; binding?.retryButton?.setOnClickListener { viewModel.load() }
        viewLifecycleOwner.lifecycleScope.launch { viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) { viewModel.state.collect(::render) } }
    }
    override fun onResume() {
        super.onResume()
        if (hasResumed) viewModel.load() else hasResumed = true
    }
    private fun render(state: UiState<List<EnergySlot>>) {
        val b=binding?:return; b.progressIndicator.isVisible=state is UiState.Loading; b.errorContainer.isVisible=state is UiState.Error
        b.emptyMessage.isVisible=state is UiState.Empty; b.items.isVisible=state is UiState.Content
        if(state is UiState.Error){b.errorMessage.text=state.error.message;if(state.error.httpStatus==401)findNavController().navigate(R.id.action_global_loginFragment)}
        if(state is UiState.Content)adapter.submitList(state.value)
    }
    private fun book(slot: EnergySlot){findNavController().navigate(R.id.action_stationSlotsFragment_to_createReservationFragment,bundleOf(StationsFragment.ARG_STATION_ID to stationId,ARG_SLOT_ID to slot.id))}
    override fun onDestroyView(){binding?.items?.adapter=null;binding=null;super.onDestroyView()}
    companion object{const val ARG_SLOT_ID="slotId"}
}
