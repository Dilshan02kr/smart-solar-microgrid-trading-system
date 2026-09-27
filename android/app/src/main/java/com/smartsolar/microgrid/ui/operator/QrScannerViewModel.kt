package com.smartsolar.microgrid.ui.operator

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.OperatorPresentation
import com.smartsolar.microgrid.data.repository.OperatorRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class QrScannerState(
    val isVerifying: Boolean = false,
    val error: AppError? = null,
    val verifiedReference: String? = null,
)

class QrScannerViewModel(
    private val operators: OperatorRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(QrScannerState())
    val state = mutableState.asStateFlow()

    fun verify(decodedValue: String) {
        if (mutableState.value.isVerifying) return
        val reference = OperatorPresentation.normalizeReference(decodedValue)
        if (reference.isBlank()) {
            mutableState.value = QrScannerState(
                error = AppError("The scanned QR code does not contain a transaction reference.", "INVALID_TRANSACTION_REFERENCE"),
            )
            return
        }
        mutableState.value = QrScannerState(isVerifying = true)
        viewModelScope.launch {
            mutableState.value = when (val result = operators.verifyTransaction(reference)) {
                is AppResult.Success -> QrScannerState(
                    verifiedReference = result.value.transactionReference,
                )
                is AppResult.Error -> QrScannerState(error = result.error)
            }
        }
    }

    fun consumeNavigation() {
        mutableState.value = QrScannerState()
    }

    fun scanAgain() {
        mutableState.value = QrScannerState()
    }
}
