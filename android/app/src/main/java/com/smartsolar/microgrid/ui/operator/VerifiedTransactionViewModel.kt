package com.smartsolar.microgrid.ui.operator

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.OperatorPresentation
import com.smartsolar.microgrid.data.repository.OperatorRepository
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.OperatorTransaction
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class VerifiedTransactionState(
    val isLoading: Boolean = false,
    val transaction: OperatorTransaction? = null,
    val stationName: String? = null,
    val isCompleting: Boolean = false,
    val completed: Boolean = false,
    val terminalConflict: Boolean = false,
    val error: AppError? = null,
    val completionError: AppError? = null,
)

class VerifiedTransactionViewModel(
    private val transactionReference: String,
    private val operators: OperatorRepository,
    private val stations: StationRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(VerifiedTransactionState())
    val state = mutableState.asStateFlow()

    init {
        verify()
    }

    fun verify() {
        if (mutableState.value.isLoading) return
        mutableState.value = VerifiedTransactionState(isLoading = true)
        viewModelScope.launch {
            mutableState.value = when (val result = operators.verifyTransaction(transactionReference)) {
                is AppResult.Error -> VerifiedTransactionState(error = result.error)
                is AppResult.Success -> {
                    val station = (stations.getStation(result.value.stationId) as? AppResult.Success)?.value
                    VerifiedTransactionState(
                        transaction = result.value,
                        stationName = station?.name ?: result.value.stationId,
                    )
                }
            }
        }
    }

    fun complete() {
        val current = mutableState.value
        val transaction = current.transaction ?: return
        if (current.isCompleting || current.completed || current.terminalConflict ||
            !OperatorPresentation.canComplete(transaction)
        ) return
        mutableState.value = current.copy(isCompleting = true, completionError = null)
        viewModelScope.launch {
            mutableState.value = when (val result = operators.completeReservation(transaction.reservationId)) {
                is AppResult.Success -> mutableState.value.copy(
                    isCompleting = false,
                    completed = true,
                    transaction = result.value,
                    completionError = null,
                )
                is AppResult.Error -> mutableState.value.copy(
                    isCompleting = false,
                    terminalConflict = result.error.code in setOf(
                        "RESERVATION_ALREADY_COMPLETED",
                        "RESERVATION_CANCELLED",
                        "RESERVATION_NOT_APPROVED",
                    ),
                    completionError = result.error,
                )
            }
        }
    }
}
