package com.smartsolar.microgrid.ui.reservations

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.appcompat.app.AlertDialog
import androidx.core.os.bundleOf
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.google.android.material.snackbar.Snackbar
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.core.util.ReservationPresentation
import com.smartsolar.microgrid.databinding.FragmentReservationDetailsBinding
import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class ReservationDetailsFragment : Fragment() {
    private var binding:FragmentReservationDetailsBinding?=null
    private var cancelDialog:AlertDialog?=null
    private var hasResumed=false
    private val reservationId by lazy{requireArguments().getString(CreateReservationFragment.ARG_RESERVATION_ID).orEmpty()}
    private val viewModel:ReservationDetailsViewModel by viewModels{
        val app=(requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory{ReservationDetailsViewModel(reservationId,app.reservationRepository,app.stationRepository,app.energySlotRepository)}
    }
    override fun onCreateView(inflater:LayoutInflater,container:ViewGroup?,state:Bundle?):View=FragmentReservationDetailsBinding.inflate(inflater,container,false).also{binding=it}.root
    override fun onViewCreated(view:View,state:Bundle?){
        binding?.retryButton?.setOnClickListener{viewModel.load()};binding?.cancelButton?.setOnClickListener{confirmCancel()}
        binding?.refreshButton?.setOnClickListener{viewModel.load()}
        binding?.showQrButton?.setOnClickListener{findNavController().navigate(R.id.action_reservationDetailsFragment_to_reservationQrFragment,bundleOf(CreateReservationFragment.ARG_RESERVATION_ID to reservationId))}
        binding?.editButton?.setOnClickListener{findNavController().navigate(R.id.action_reservationDetailsFragment_to_editReservationFragment,bundleOf(CreateReservationFragment.ARG_RESERVATION_ID to reservationId))}
        findNavController().currentBackStackEntry?.savedStateHandle?.getLiveData<Boolean>(UPDATED_KEY)?.observe(viewLifecycleOwner){if(it==true){findNavController().currentBackStackEntry?.savedStateHandle?.remove<Boolean>(UPDATED_KEY);viewModel.load();binding?.root?.let{root->Snackbar.make(root,R.string.reservation_updated,Snackbar.LENGTH_SHORT).show()}}}
        viewLifecycleOwner.lifecycleScope.launch{viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED){viewModel.state.collect(::render)}}
    }
    private fun render(state:ReservationDetailsState){
        val b=binding?:return;b.progressIndicator.isVisible=state.isLoading||state.isCancelling;b.errorContainer.isVisible=state.error!=null;b.contentContainer.isVisible=state.content!=null
        state.error?.let{b.errorMessage.text=it.message;if(it.httpStatus==401)findNavController().navigate(R.id.action_global_loginFragment)}
        state.actionError?.let{cancelDialog?.dismiss();cancelDialog=null;Snackbar.make(b.root,it.message,Snackbar.LENGTH_LONG).show()}
        state.content?.let{content->val r=content.reservation;b.reservationIdValue.text=r.reservationId;b.stationValue.text=content.station?.name?:r.stationId;b.slotValue.text=content.slot?.let{getString(R.string.slot_summary,BookingPresentation.date(it.date),BookingPresentation.time(it.startTime),BookingPresentation.time(it.endTime))}?:r.slotId
            b.scheduledValue.text=BookingPresentation.timestamp(r.scheduledTime);b.statusValue.text=r.status.name;b.referenceValue.text=r.transactionReference?:getString(R.string.reference_not_issued);b.createdValue.text=BookingPresentation.timestamp(r.createdAt);b.updatedValue.text=BookingPresentation.timestamp(r.updatedAt);b.completedValue.text=r.completedAt?.let{BookingPresentation.timestamp(it)}?:getString(R.string.not_applicable)
            val mutable=BookingPresentation.canModify(r);b.editButton.isVisible=mutable;b.cancelButton.isVisible=mutable;b.pendingMessage.isVisible=r.status==ReservationStatus.PENDING;b.showQrButton.isVisible=ReservationPresentation.qrPayload(r)!=null
            if(r.status==ReservationStatus.CANCELLED)cancelDialog?.dismiss()
        }
    }
    override fun onResume(){super.onResume();if(hasResumed)viewModel.load() else hasResumed=true}
    private fun confirmCancel(){cancelDialog=AlertDialog.Builder(requireContext()).setTitle(R.string.cancel_reservation_title).setMessage(R.string.cancel_reservation_message).setNegativeButton(R.string.keep_reservation,null).setPositiveButton(R.string.cancel_reservation_confirm,null).create().also{d->d.setOnShowListener{d.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener{d.getButton(AlertDialog.BUTTON_POSITIVE).isEnabled=false;d.getButton(AlertDialog.BUTTON_NEGATIVE).isEnabled=false;d.getButton(AlertDialog.BUTTON_POSITIVE).setText(R.string.cancelling);viewModel.cancel()}};d.show()}}
    override fun onDestroyView(){cancelDialog?.dismiss();cancelDialog=null;binding=null;super.onDestroyView()}
    companion object{const val UPDATED_KEY="reservation_updated"}
}
