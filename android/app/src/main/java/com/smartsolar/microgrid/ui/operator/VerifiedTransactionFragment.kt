package com.smartsolar.microgrid.ui.operator

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.appcompat.app.AlertDialog
import androidx.core.view.isVisible
import androidx.core.content.ContextCompat
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.core.util.OperatorErrorMessage
import com.smartsolar.microgrid.core.util.OperatorPresentation
import com.smartsolar.microgrid.databinding.FragmentVerifiedTransactionBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class VerifiedTransactionFragment : Fragment() {
    private var binding: FragmentVerifiedTransactionBinding? = null
    private var confirmationDialog: AlertDialog? = null
    private val reference by lazy { requireArguments().getString(ARG_TRANSACTION_REFERENCE).orEmpty() }
    private val viewModel: VerifiedTransactionViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory {
            VerifiedTransactionViewModel(reference, app.operatorRepository, app.stationRepository)
        }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentVerifiedTransactionBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.retryButton?.setOnClickListener { viewModel.verify() }
        binding?.completeButton?.setOnClickListener { confirmCompletion() }
        binding?.scanAnotherButton?.setOnClickListener { scanAnother() }
        binding?.errorScanAnotherButton?.setOnClickListener { scanAnother() }
        binding?.dashboardButton?.setOnClickListener {
            findNavController().navigate(R.id.action_verifiedTransactionFragment_to_operatorDashboardFragment)
        }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    private fun render(state: VerifiedTransactionState) {
        val current = binding ?: return
        current.progressIndicator.isVisible = state.isLoading || state.isCompleting
        current.errorContainer.isVisible = state.error != null
        current.contentContainer.isVisible = state.transaction != null
        state.error?.let { error ->
            current.errorMessage.text = operatorErrorText(error.code, error.message)
            if (error.httpStatus == 401) findNavController().navigate(R.id.action_global_loginSelectionFragment)
        }
        state.transaction?.let { transaction ->
            current.resultTitle.setText(
                if (state.completed) R.string.energy_transfer_completed else R.string.verified_reservation,
            )
            current.reservationValue.text = transaction.reservationId
            current.referenceValue.text = transaction.transactionReference
            current.statusValue.text = transaction.status.name
            current.prosumerValue.text = transaction.prosumerId
            current.stationValue.text = state.stationName ?: transaction.stationId
            current.slotValue.text = transaction.slotId
            current.scheduledValue.text = BookingPresentation.timestamp(transaction.scheduledTime)
            current.completeButton.isVisible = OperatorPresentation.canComplete(transaction) &&
                !state.completed && !state.terminalConflict
            current.completeButton.isEnabled = !state.isCompleting
            current.completionMessage.isVisible = state.completed || state.completionError != null
            current.completionMessage.text = when {
                state.completed -> getString(R.string.energy_transfer_completed_message)
                state.completionError != null -> operatorErrorText(
                    state.completionError.code,
                    state.completionError.message,
                )
                else -> ""
            }
            current.completionMessage.setTextColor(
                ContextCompat.getColor(
                    requireContext(),
                    if (state.completed) R.color.status_success else R.color.status_error,
                ),
            )
            if (state.completionError?.httpStatus == 401) {
                findNavController().navigate(R.id.action_global_loginSelectionFragment)
            }
        }
    }

    private fun confirmCompletion() {
        confirmationDialog?.dismiss()
        confirmationDialog = AlertDialog.Builder(requireContext())
            .setTitle(R.string.complete_transfer_title)
            .setMessage(R.string.complete_transfer_confirmation)
            .setNegativeButton(R.string.cancel, null)
            .setPositiveButton(R.string.complete_transfer_action) { _, _ -> viewModel.complete() }
            .show()
    }

    private fun scanAnother() {
        findNavController().previousBackStackEntry?.savedStateHandle?.set(
            QrScannerFragment.SCAN_AGAIN_KEY,
            true,
        )
        findNavController().navigateUp()
    }

    private fun operatorErrorText(code: String, fallback: String): String = when (
        OperatorPresentation.errorMessage(code)
    ) {
        OperatorErrorMessage.STATION_NOT_ASSIGNED -> getString(R.string.operator_station_not_assigned)
        OperatorErrorMessage.WRONG_STATION -> getString(R.string.wrong_station_message)
        OperatorErrorMessage.INVALID_REFERENCE -> getString(R.string.invalid_reference_message)
        OperatorErrorMessage.NOT_APPROVED -> getString(R.string.reservation_not_approved_message)
        OperatorErrorMessage.CANCELLED -> getString(R.string.reservation_cancelled_message)
        OperatorErrorMessage.ALREADY_COMPLETED -> getString(R.string.reservation_already_completed_message)
        OperatorErrorMessage.DEFAULT -> fallback
    }

    override fun onDestroyView() {
        confirmationDialog?.dismiss()
        confirmationDialog = null
        binding = null
        super.onDestroyView()
    }

    companion object {
        const val ARG_TRANSACTION_REFERENCE = "transactionReference"
    }
}
