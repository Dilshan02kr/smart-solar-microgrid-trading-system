package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.OperatorTransaction
import com.smartsolar.microgrid.domain.model.ReservationStatus

enum class OperatorErrorMessage {
    STATION_NOT_ASSIGNED,
    WRONG_STATION,
    INVALID_REFERENCE,
    NOT_APPROVED,
    CANCELLED,
    ALREADY_COMPLETED,
    DEFAULT,
}

object OperatorPresentation {
    fun normalizeReference(value: String): String = value.trim()

    fun canComplete(transaction: OperatorTransaction): Boolean =
        transaction.status == ReservationStatus.APPROVED

    fun errorMessage(code: String): OperatorErrorMessage = when (code) {
        "OPERATOR_STATION_NOT_ASSIGNED" -> OperatorErrorMessage.STATION_NOT_ASSIGNED
        "ACCESS_DENIED" -> OperatorErrorMessage.WRONG_STATION
        "INVALID_TRANSACTION_REFERENCE", "RESERVATION_NOT_FOUND" -> OperatorErrorMessage.INVALID_REFERENCE
        "RESERVATION_NOT_APPROVED" -> OperatorErrorMessage.NOT_APPROVED
        "RESERVATION_CANCELLED" -> OperatorErrorMessage.CANCELLED
        "RESERVATION_ALREADY_COMPLETED" -> OperatorErrorMessage.ALREADY_COMPLETED
        else -> OperatorErrorMessage.DEFAULT
    }
}
