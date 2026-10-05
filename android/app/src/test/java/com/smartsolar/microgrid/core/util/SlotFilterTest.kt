package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.EnergySlot
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class SlotFilterTest {
    private val earlyAvailable = slot("early", "2026-10-05", "08:00:00", true)
    private val lateUnavailable = slot("late", "2026-10-05", "12:00:00", false)
    private val nextDayAvailable = slot("next", "2026-10-06", "09:00:00", true)

    @Test fun `available only includes available slots and excludes unavailable slots`() {
        val result = SlotFilter.apply(listOf(lateUnavailable, earlyAvailable))
        assertTrue(earlyAvailable in result)
        assertFalse(lateUnavailable in result)
    }

    @Test fun `all slots includes available and unavailable slots`() {
        assertEquals(2, SlotFilter.apply(listOf(earlyAvailable, lateUnavailable), availableOnly = false).size)
    }

    @Test fun `date filter includes only the matching date`() {
        assertEquals(listOf(earlyAvailable), SlotFilter.apply(listOf(nextDayAvailable, earlyAvailable), selectedDate = "2026-10-05"))
    }

    @Test fun `slots are ordered by date then start time`() {
        assertEquals(listOf(earlyAvailable, lateUnavailable, nextDayAvailable), SlotFilter.apply(listOf(nextDayAvailable, lateUnavailable, earlyAvailable), availableOnly = false))
    }

    @Test fun `reset defaults are available only and all dates`() {
        assertEquals(listOf(earlyAvailable, nextDayAvailable), SlotFilter.apply(listOf(nextDayAvailable, lateUnavailable, earlyAvailable)))
    }

    private fun slot(id: String, date: String, start: String, available: Boolean) = EnergySlot(
        id = id,
        stationId = "station",
        date = date,
        startTime = start,
        endTime = "13:00:00",
        capacityKw = 5.0,
        isAvailable = available,
        createdAt = "",
        updatedAt = "",
    )
}
