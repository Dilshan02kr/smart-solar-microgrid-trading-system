package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.domain.model.StationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test

class BookingDtoTest {
    @Test
    fun `station mapping rejects unknown status`() {
        val dto = StationResponseDto("id", "Station", "Town", 1.0, 2.0, 10.0, "Always", "UNKNOWN", "created", "updated")
        assertNull(dto.toDomainOrNull())
        assertEquals(StationStatus.ACTIVE, dto.copy(status = "ACTIVE").toDomainOrNull()?.status)
    }

    @Test
    fun `reservation mapping accepts exact statuses and rejects unknown values`() {
        val dto = ReservationResponseDto("r", "p", "s", "slot", "2026-01-02T00:00:00Z", "APPROVED", null, "c", "u", null)
        assertEquals(ReservationStatus.APPROVED, dto.toDomainOrNull()?.status)
        assertNull(dto.copy(status = "UNKNOWN").toDomainOrNull())
    }
}
