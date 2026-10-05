package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.domain.model.Station

data class ReservationFilterCriteria(
    val searchQuery: String = "",
    val status: ReservationStatus? = null,
)

object ReservationFilter {
    fun apply(
        reservations: List<Reservation>,
        stationsById: Map<String, Station>,
        criteria: ReservationFilterCriteria = ReservationFilterCriteria(),
    ): List<Reservation> {
        val query = criteria.searchQuery.trim()
        return reservations.filter { reservation ->
            val station = stationsById[reservation.stationId]
            val matchesStatus = criteria.status == null || reservation.status == criteria.status
            val matchesSearch = query.isEmpty() || listOfNotNull(
                station?.name,
                station?.locationName,
                reservation.transactionReference,
            ).any { it.contains(query, ignoreCase = true) }
            matchesStatus && matchesSearch
        }
    }

    fun reset(): ReservationFilterCriteria = ReservationFilterCriteria()
}
