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
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.databinding.FragmentCreateReservationBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.slots.StationSlotsFragment
import com.smartsolar.microgrid.ui.stations.StationsFragment
import kotlinx.coroutines.launch

class CreateReservationFragment : Fragment() {
    private var binding: FragmentCreateReservationBinding? = null
    private val stationId by lazy { requireArguments().getString(StationsFragment.ARG_STATION_ID).orEmpty() }
    private val slotId by lazy { requireArguments().getString(StationSlotsFragment.ARG_SLOT_ID).orEmpty() }
    private val viewModel: CreateReservationViewModel by viewModels {
        val app=(requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { CreateReservationViewModel(stationId,slotId,app.stationRepository,app.energySlotRepository,app.reservationRepository) }
    }
    override fun onCreateView(inflater: LayoutInflater,container: ViewGroup?,state: Bundle?):View=FragmentCreateReservationBinding.inflate(inflater,container,false).also{binding=it}.root
    override fun onViewCreated(view:View,state:Bundle?){
        binding?.retryButton?.setOnClickListener{viewModel.load()};binding?.confirmButton?.setOnClickListener{viewModel.submit()}
        viewLifecycleOwner.lifecycleScope.launch{viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED){viewModel.state.collect(::render)}}
    }
    private fun render(state:CreateReservationState){
        val b=binding?:return;b.progressIndicator.isVisible=state.isLoading||state.isSubmitting;b.errorContainer.isVisible=state.error!=null
        b.contentContainer.isVisible=state.context!=null;b.confirmButton.isEnabled=!state.isSubmitting
        state.error?.let{b.errorMessage.text=it.message;if(it.httpStatus==401)findNavController().navigate(R.id.action_global_loginSelectionFragment)}
        state.context?.let{c->b.stationValue.text=c.station.name;b.dateValue.text=BookingPresentation.date(c.slot.date);b.timeValue.text=getString(R.string.slot_time_value,BookingPresentation.time(c.slot.startTime),BookingPresentation.time(c.slot.endTime));b.capacityValue.text=getString(R.string.capacity_value,c.slot.capacityKw)}
        state.createdReservationId?.let{id->if(findNavController().currentDestination?.id==R.id.createReservationFragment)findNavController().navigate(R.id.action_createReservationFragment_to_reservationDetailsFragment,bundleOf(ARG_RESERVATION_ID to id))}
    }
    override fun onDestroyView(){binding=null;super.onDestroyView()}
    companion object{const val ARG_RESERVATION_ID="reservationId"}
}

