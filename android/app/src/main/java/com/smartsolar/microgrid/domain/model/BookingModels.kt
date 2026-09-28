package com.smartsolar.microgrid.domain.model

enum class StationStatus {
    ACTIVE,
    INACTIVE;

    companion object {
        fun fromBackendValue(value: String): StationStatus? = entries.firstOrNull { it.name == value }
    }
}

data class Station(
    val id: String,
    val name: String,
    val locationName: String,
    val latitude: Double,
    val longitude: Double,
    val totalCapacityKw: Double,
    val operationalSchedule: String,
    val status: StationStatus,
    val createdAt: String,
    val updatedAt: String,
)

data class EnergySlot(
    val id: String,
    val stationId: String,
    val date: String,
    val startTime: String,
    val endTime: String,
    val capacityKw: Double,
    val isAvailable: Boolean,
    val createdAt: String,
    val updatedAt: String,
)

enum class ReservationStatus {
    PENDING,
    APPROVED,
    COMPLETED,
    CANCELLED;

    companion object {
        fun fromBackendValue(value: String): ReservationStatus? = entries.firstOrNull { it.name == value }
    }
}

data class Reservation(
    val reservationId: String,
    val prosumerId: String,
    val stationId: String,
    val slotId: String,
    val scheduledTime: String,
    val status: ReservationStatus,
    val transactionReference: String?,
    val createdAt: String,
    val updatedAt: String,
    val completedAt: String?,
)

