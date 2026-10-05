package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.EnergySlot
import org.junit.Assert.assertEquals
import org.junit.Test

class OperatorSlotPresentationTest {
    @Test
    fun chronological_ordersByDateThenStartTime() {
        val laterDate = slot("later-date", "2026-10-07T00:00:00Z", "08:00:00")
        val laterTime = slot("later-time", "2026-10-06T00:00:00Z", "12:00:00")
        val earlierTime = slot("earlier-time", "2026-10-06T00:00:00Z", "09:00:00")

        val result = OperatorSlotPresentation.chronological(listOf(laterDate, laterTime, earlierTime))

        assertEquals(listOf("earlier-time", "later-time", "later-date"), result.map { it.id })
    }

    @Test
    fun chronological_preservesEmptyList() {
        assertEquals(emptyList<EnergySlot>(), OperatorSlotPresentation.chronological(emptyList()))
    }

    private fun slot(id: String, date: String, start: String) = EnergySlot(
        id = id,
        stationId = "station",
        date = date,
        startTime = start,
        endTime = "13:00:00",
        capacityKw = 5.0,
        isAvailable = true,
        createdAt = "2026-10-01T00:00:00Z",
        updatedAt = "2026-10-01T00:00:00Z",
    )
}
