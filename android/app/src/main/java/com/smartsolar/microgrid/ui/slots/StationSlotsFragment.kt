package com.smartsolar.microgrid.ui.slots

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
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
import com.smartsolar.microgrid.databinding.FragmentListStateBinding
import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
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
        binding?.availabilityFilterGroup?.addOnButtonCheckedListener { _, checkedId, isChecked ->
            if (isChecked) viewModel.setAvailableOnly(checkedId == R.id.available_only_filter)
        }
        binding?.resetFiltersButton?.setOnClickListener { viewModel.resetFilters() }
        viewLifecycleOwner.lifecycleScope.launch { viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) { viewModel.state.collect(::render) } }
    }
    override fun onResume() {
        super.onResume()
        if (hasResumed) viewModel.load() else hasResumed = true
    }
    private fun render(state: StationSlotsState) {
        val b = binding ?: return
        val hasServerSlots = state.allSlots.isNotEmpty()
        val filteredEmpty = hasServerSlots && state.filteredSlots.isEmpty()
        b.progressIndicator.isVisible = state.isLoading
        b.errorContainer.isVisible = state.error != null
        b.slotFilters.isVisible = !state.isLoading && state.error == null && hasServerSlots
        b.emptyMessage.isVisible = !state.isLoading && state.error == null && (!hasServerSlots || filteredEmpty)
        b.items.isVisible = !state.isLoading && state.error == null && state.filteredSlots.isNotEmpty()
        b.emptyMessage.setText(if (filteredEmpty) R.string.no_matching_slots else R.string.no_slots)
        state.error?.let {
            b.errorMessage.text = it.message
            if (it.httpStatus == 401) findNavController().navigate(R.id.action_global_loginFragment)
        }

        val selectedButton = if (state.availableOnly) R.id.available_only_filter else R.id.all_slots_filter
        if (b.availabilityFilterGroup.checkedButtonId != selectedButton) b.availabilityFilterGroup.check(selectedButton)
        val dateValues = listOf<String?>(null) + state.availableDates
        val dateLabels = listOf(getString(R.string.all_dates_filter)) + state.availableDates.map(BookingPresentation::date)
        b.dateFilter.setAdapter(ArrayAdapter(requireContext(), android.R.layout.simple_dropdown_item_1line, dateLabels))
        val selectedIndex = dateValues.indexOf(state.selectedDate).coerceAtLeast(0)
        if (b.dateFilter.text.toString() != dateLabels[selectedIndex]) b.dateFilter.setText(dateLabels[selectedIndex], false)
        b.dateFilter.setOnItemClickListener { _, _, position, _ -> viewModel.setDate(dateValues[position]) }
        adapter.submitList(state.filteredSlots)
    }
    private fun book(slot: EnergySlot){
        if (!slot.isAvailable) return
        findNavController().navigate(R.id.action_stationSlotsFragment_to_createReservationFragment,bundleOf(StationsFragment.ARG_STATION_ID to stationId,ARG_SLOT_ID to slot.id))
    }
    override fun onDestroyView(){binding?.items?.adapter=null;binding=null;super.onDestroyView()}
    companion object{const val ARG_SLOT_ID="slotId"}
}
