package com.smartsolar.microgrid.ui.reservations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.ReservationGroup
import com.smartsolar.microgrid.core.util.ReservationFilter
import com.smartsolar.microgrid.core.util.ReservationFilterCriteria
import com.smartsolar.microgrid.core.util.ReservationPresentation
import com.smartsolar.microgrid.data.repository.ReservationRepository
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import com.smartsolar.microgrid.domain.model.Station
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class ReservationListItem(
    val reservation: Reservation,
    val stationName: String,
)

data class MyReservationsState(
    val isLoading: Boolean = false,
    val isRefreshing: Boolean = false,
    val reservations: List<Reservation> = emptyList(),
    val stationsById: Map<String, Station> = emptyMap(),
    val selectedGroup: ReservationGroup = ReservationGroup.PENDING,
    val filterCriteria: ReservationFilterCriteria = ReservationFilterCriteria(),
    val stationNamesUnavailable: Boolean = false,
    val error: AppError? = null,
) {
    val visibleItems: List<ReservationListItem>
        get() = ReservationPresentation.inGroup(
            ReservationFilter.apply(reservations, stationsById, filterCriteria),
            selectedGroup,
        ).map {
            ReservationListItem(it, stationsById[it.stationId]?.name ?: it.stationId)
        }

    val hasActiveFilters: Boolean
        get() = filterCriteria.searchQuery.isNotBlank() || filterCriteria.status != null
}

class MyReservationsViewModel(
    private val reservations: ReservationRepository,
    private val stations: StationRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(MyReservationsState())
    val state = mutableState.asStateFlow()

    init {
        load()
    }

    fun select(group: ReservationGroup) {
        mutableState.value = mutableState.value.copy(selectedGroup = group)
    }

    fun setSearch(query: String) {
        mutableState.value = mutableState.value.copy(
            filterCriteria = mutableState.value.filterCriteria.copy(searchQuery = query),
        )
    }

    fun setStatus(status: ReservationStatus?) {
        mutableState.value = mutableState.value.copy(
            filterCriteria = mutableState.value.filterCriteria.copy(status = status),
        )
    }

    fun resetFilters() {
        mutableState.value = mutableState.value.copy(filterCriteria = ReservationFilter.reset())
    }

    fun load(refresh: Boolean = false) {
        if (mutableState.value.isLoading || mutableState.value.isRefreshing) return
        mutableState.value = mutableState.value.copy(
            isLoading = !refresh,
            isRefreshing = refresh,
            error = null,
        )
        viewModelScope.launch {
            when (val reservationResult = reservations.getMine()) {
                is AppResult.Error -> mutableState.value = mutableState.value.copy(
                    isLoading = false,
                    isRefreshing = false,
                    error = reservationResult.error,
                )
                is AppResult.Success -> {
                    val stationResult = stations.getStations()
                    val stationsById = (stationResult as? AppResult.Success)
                        ?.value
                        ?.associateBy { it.id }
                        .orEmpty()
                    mutableState.value = mutableState.value.copy(
                        isLoading = false,
                        isRefreshing = false,
                        reservations = reservationResult.value,
                        stationsById = stationsById,
                        stationNamesUnavailable = stationResult is AppResult.Error,
                        error = null,
                    )
                }
            }
        }
    }
}
