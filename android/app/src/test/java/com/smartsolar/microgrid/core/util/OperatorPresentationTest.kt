package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.OperatorTransaction
import com.smartsolar.microgrid.domain.model.ReservationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class OperatorPresentationTest {
    @Test
    fun `only approved transaction is completion eligible`() {
        assertTrue(OperatorPresentation.canComplete(transaction(ReservationStatus.APPROVED)))
        assertFalse(OperatorPresentation.canComplete(transaction(ReservationStatus.PENDING)))
        assertFalse(OperatorPresentation.canComplete(transaction(ReservationStatus.COMPLETED)))
        assertFalse(OperatorPresentation.canComplete(transaction(ReservationStatus.CANCELLED)))
    }

    @Test
    fun `reference normalization trims only surrounding whitespace`() {
        assertEquals("Ab C-123", OperatorPresentation.normalizeReference("  Ab C-123\n"))
    }

    @Test
    fun `operator errors map to safe presentation categories`() {
        assertEquals(OperatorErrorMessage.STATION_NOT_ASSIGNED, OperatorPresentation.errorMessage("OPERATOR_STATION_NOT_ASSIGNED"))
        assertEquals(OperatorErrorMessage.WRONG_STATION, OperatorPresentation.errorMessage("ACCESS_DENIED"))
        assertEquals(OperatorErrorMessage.INVALID_REFERENCE, OperatorPresentation.errorMessage("RESERVATION_NOT_FOUND"))
        assertEquals(OperatorErrorMessage.ALREADY_COMPLETED, OperatorPresentation.errorMessage("RESERVATION_ALREADY_COMPLETED"))
        assertEquals(OperatorErrorMessage.DEFAULT, OperatorPresentation.errorMessage("UNKNOWN"))
    }

    private fun transaction(status: ReservationStatus) = OperatorTransaction(
        reservationId = "reservation",
        transactionReference = "reference",
        status = status,
        prosumerId = "prosumer",
        stationId = "station",
        slotId = "slot",
        scheduledTime = "2026-01-01T12:00:00Z",
    )
}
