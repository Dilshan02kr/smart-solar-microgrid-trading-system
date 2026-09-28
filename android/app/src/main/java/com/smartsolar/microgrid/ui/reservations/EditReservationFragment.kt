package com.smartsolar.microgrid.ui.reservations

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import androidx.appcompat.app.AlertDialog
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
import com.smartsolar.microgrid.databinding.FragmentEditReservationBinding
import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class EditReservationFragment:Fragment(){
    private var binding:FragmentEditReservationBinding?=null
    private val id by lazy{requireArguments().getString(CreateReservationFragment.ARG_RESERVATION_ID).orEmpty()}
    private val viewModel:EditReservationViewModel by viewModels{val app=(requireActivity().application as SmartSolarApplication).appContainer;AppViewModelFactory{EditReservationViewModel(id,app.reservationRepository,app.stationRepository,app.energySlotRepository)}}
    override fun onCreateView(inflater:LayoutInflater,container:ViewGroup?,state:Bundle?):View=FragmentEditReservationBinding.inflate(inflater,container,false).also{binding=it}.root
    override fun onViewCreated(view:View,state:Bundle?){binding?.retryButton?.setOnClickListener{viewModel.load()};binding?.saveButton?.setOnClickListener{confirmOrSave()};viewLifecycleOwner.lifecycleScope.launch{viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED){viewModel.state.collect(::render)}}}
    private fun render(state:EditReservationState){val b=binding?:return;b.progressIndicator.isVisible=state.isLoading||state.isLoadingSlots||state.isSaving;b.errorMessage.isVisible=state.error!=null;b.formContainer.isVisible=state.reservation!=null;b.noSlotsMessage.isVisible=!state.isLoadingSlots&&state.reservation!=null&&state.slots.isEmpty();b.saveButton.isEnabled=!state.isSaving&&!state.isLoadingSlots&&state.selectedSlotId!=null
        state.error?.let{b.errorMessage.text=it.message;if(it.httpStatus==401)findNavController().navigate(R.id.action_global_loginFragment)}
        val stationNames=state.stations.map{it.name};b.stationInput.setAdapter(ArrayAdapter(requireContext(),android.R.layout.simple_dropdown_item_1line,stationNames));state.selectedStationId?.let{sid->state.stations.indexOfFirst{it.id==sid}.takeIf{it>=0}?.let{b.stationInput.setText(stationNames[it],false)}}
        b.stationInput.setOnItemClickListener{_,_,position,_->viewModel.selectStation(state.stations[position].id)}
        val slotNames=state.slots.map{getString(R.string.slot_summary,BookingPresentation.date(it.date),BookingPresentation.time(it.startTime),BookingPresentation.time(it.endTime))};b.slotInput.setAdapter(ArrayAdapter(requireContext(),android.R.layout.simple_dropdown_item_1line,slotNames));state.selectedSlotId?.let{sid->state.slots.indexOfFirst{it.id==sid}.takeIf{it>=0}?.let{b.slotInput.setText(slotNames[it],false)}};b.slotInput.setOnItemClickListener{_,_,position,_->viewModel.selectSlot(state.slots[position].id)}
        if(state.updated&&findNavController().currentDestination?.id==R.id.editReservationFragment){findNavController().previousBackStackEntry?.savedStateHandle?.set(ReservationDetailsFragment.UPDATED_KEY,true);findNavController().popBackStack()}
    }
    private fun confirmOrSave(){if(viewModel.state.value.reservation?.status==ReservationStatus.APPROVED)AlertDialog.Builder(requireContext()).setTitle(R.string.update_approved_title).setMessage(R.string.update_approved_message).setNegativeButton(R.string.cancel,null).setPositiveButton(R.string.update_confirm){_,_->viewModel.save()}.show() else viewModel.save()}
    override fun onDestroyView(){binding=null;super.onDestroyView()}
}
