package com.smartsolar.microgrid.ui.reservations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.EnergySlotRepository
import com.smartsolar.microgrid.data.repository.ReservationRepository
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.Station
import kotlinx.coroutines.async
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class ReservationDetailsViewModel(
    private val id: String,
    private val reservations: ReservationRepository,
    private val stations: StationRepository,
    private val slots: EnergySlotRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(ReservationDetailsState(isLoading = true))
    val state = mutableState.asStateFlow()
    init { load() }
    fun load() = viewModelScope.launch {
        mutableState.value = ReservationDetailsState(isLoading = true)
        when (val reservationResult = reservations.get(id)) {
            is AppResult.Error -> mutableState.value = ReservationDetailsState(error = reservationResult.error)
            is AppResult.Success -> {
                val reservation = reservationResult.value
                val stationCall = async { stations.getStation(reservation.stationId) }
                val slotCall = async { slots.getSlot(reservation.slotId) }
                val station = (stationCall.await() as? AppResult.Success)?.value
                val slot = (slotCall.await() as? AppResult.Success)?.value
                mutableState.value = ReservationDetailsState(content = ReservationDetailsContent(reservation, station, slot))
            }
        }
    }
    fun cancel() {
        if (mutableState.value.isCancelling) return
        mutableState.value = mutableState.value.copy(isCancelling = true, actionError = null)
        viewModelScope.launch {
            mutableState.value = when (val result = reservations.cancel(id)) {
                is AppResult.Success -> mutableState.value.copy(
                    isCancelling = false,
                    content = mutableState.value.content?.copy(reservation = result.value),
                )
                is AppResult.Error -> mutableState.value.copy(isCancelling = false, actionError = result.error)
            }
        }
    }
}
data class ReservationDetailsContent(val reservation: Reservation, val station: Station?, val slot: EnergySlot?)
data class ReservationDetailsState(
    val isLoading: Boolean = false,
    val isCancelling: Boolean = false,
    val content: ReservationDetailsContent? = null,
    val error: AppError? = null,
    val actionError: AppError? = null,
)

