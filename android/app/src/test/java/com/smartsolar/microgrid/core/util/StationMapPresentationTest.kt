package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.domain.model.StationStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test

class StationMapPresentationTest {
    @Test
    fun `coordinate validation accepts valid Sri Lankan coordinates`() {
        assertTrue(StationMapPresentation.hasValidCoordinates(7.2906, 80.6337))
    }

    @Test
    fun `coordinate validation rejects invalid and non-finite values`() {
        assertFalse(StationMapPresentation.hasValidCoordinates(91.0, 80.0))
        assertFalse(StationMapPresentation.hasValidCoordinates(7.0, 181.0))
        assertFalse(StationMapPresentation.hasValidCoordinates(Double.NaN, 80.0))
    }

    @Test
    fun `only stations with valid coordinates produce markers`() {
        val markers = StationMapPresentation.markers(
            listOf(station("valid", 7.2906, 80.6337), station("invalid", 91.0, 80.0)),
        )
        assertEquals(listOf("valid"), markers.map { it.stationId })
    }

    @Test
    fun `focus selects known marker and rejects unknown id`() {
        val markers = StationMapPresentation.markers(listOf(station("known", 7.2906, 80.6337)))
        assertEquals("known", StationMapPresentation.focusedMarker(markers, "known")?.stationId)
        assertNull(StationMapPresentation.focusedMarker(markers, "missing"))
        assertNull(StationMapPresentation.focusedMarker(markers, null))
    }

    private fun station(id: String, latitude: Double, longitude: Double) = Station(
        id = id,
        name = "Station $id",
        locationName = "Location",
        latitude = latitude,
        longitude = longitude,
        totalCapacityKw = 100.0,
        operationalSchedule = "06:00 - 18:00",
        status = StationStatus.ACTIVE,
        createdAt = "2026-01-01T00:00:00Z",
        updatedAt = "2026-01-01T00:00:00Z",
    )
}
