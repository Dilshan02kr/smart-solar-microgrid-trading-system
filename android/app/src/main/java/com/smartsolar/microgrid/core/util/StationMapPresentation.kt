package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.domain.model.StationStatus

data class StationMarkerModel(
    val stationId: String,
    val name: String,
    val locationName: String,
    val latitude: Double,
    val longitude: Double,
    val totalCapacityKw: Double,
    val status: StationStatus,
)

object StationMapPresentation {
    fun hasValidCoordinates(latitude: Double, longitude: Double): Boolean =
        latitude.isFinite() && longitude.isFinite() &&
            latitude in -90.0..90.0 && longitude in -180.0..180.0

    fun markers(stations: List<Station>): List<StationMarkerModel> = stations.mapNotNull { station ->
        if (!hasValidCoordinates(station.latitude, station.longitude)) return@mapNotNull null
        StationMarkerModel(
            stationId = station.id,
            name = station.name,
            locationName = station.locationName,
            latitude = station.latitude,
            longitude = station.longitude,
            totalCapacityKw = station.totalCapacityKw,
            status = station.status,
        )
    }

    fun focusedMarker(markers: List<StationMarkerModel>, stationId: String?): StationMarkerModel? =
        stationId?.let { id -> markers.firstOrNull { it.stationId == id } }
}
