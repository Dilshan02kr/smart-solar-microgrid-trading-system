package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.ReservationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test

class OperatorDtoTest {
    @Test
    fun `operator response maps exact backend status and fields`() {
        val mapped = response("APPROVED").toDomainOrNull()!!
        assertEquals("reservation", mapped.reservationId)
        assertEquals("server-reference", mapped.transactionReference)
        assertEquals(ReservationStatus.APPROVED, mapped.status)
        assertEquals("station", mapped.stationId)
    }

    @Test
    fun `unknown operator transaction status is rejected`() {
        assertNull(response("VERIFIED").toDomainOrNull())
    }

    private fun response(status: String) = OperatorTransactionResponseDto(
        reservationId = "reservation",
        transactionReference = "server-reference",
        status = status,
        prosumerId = "prosumer",
        stationId = "station",
        slotId = "slot",
        scheduledTime = "2026-01-01T12:00:00Z",
    )
}
