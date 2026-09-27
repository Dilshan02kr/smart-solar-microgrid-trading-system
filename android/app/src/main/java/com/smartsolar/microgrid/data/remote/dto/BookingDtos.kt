package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.domain.model.StationStatus

data class StationResponseDto(
    val id: String,
    val name: String,
    val locationName: String,
    val latitude: Double,
    val longitude: Double,
    val totalCapacityKw: Double,
    val operationalSchedule: String,
    val status: String,
    val createdAt: String,
    val updatedAt: String,
) {
    fun toDomainOrNull(): Station? = StationStatus.fromBackendValue(status)?.let {
        Station(id, name, locationName, latitude, longitude, totalCapacityKw, operationalSchedule, it, createdAt, updatedAt)
    }
}

data class EnergySlotResponseDto(
    val id: String,
    val stationId: String,
    val date: String,
    val startTime: String,
    val endTime: String,
    val capacityKw: Double,
    val isAvailable: Boolean,
    val createdAt: String,
    val updatedAt: String,
) {
    fun toDomain(): EnergySlot = EnergySlot(id, stationId, date, startTime, endTime, capacityKw, isAvailable, createdAt, updatedAt)
}

data class ReservationRequestDto(
    val stationId: String,
    val slotId: String,
)

data class ReservationResponseDto(
    val reservationId: String,
    val prosumerId: String,
    val stationId: String,
    val slotId: String,
    val scheduledTime: String,
    val status: String,
    val transactionReference: String?,
    val createdAt: String,
    val updatedAt: String,
    val completedAt: String?,
) {
    fun toDomainOrNull(): Reservation? = ReservationStatus.fromBackendValue(status)?.let {
        Reservation(reservationId, prosumerId, stationId, slotId, scheduledTime, it, transactionReference, createdAt, updatedAt, completedAt)
    }
}

