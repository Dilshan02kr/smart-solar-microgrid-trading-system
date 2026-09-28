package com.smartsolar.microgrid.ui.reservations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.EnergySlotRepository
import com.smartsolar.microgrid.data.repository.ReservationRepository
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.domain.model.Station
import kotlinx.coroutines.async
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class CreateReservationViewModel(
    private val stationId: String,
    private val slotId: String,
    private val stations: StationRepository,
    private val slots: EnergySlotRepository,
    private val reservations: ReservationRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(CreateReservationState(isLoading = true))
    val state = mutableState.asStateFlow()
    init { load() }
    fun load() = viewModelScope.launch {
        mutableState.value = CreateReservationState(isLoading = true)
        val stationResult = async { stations.getStation(stationId) }
        val slotResult = async { slots.getSlot(slotId) }
        val station = stationResult.await(); val slot = slotResult.await()
        mutableState.value = when {
            station is AppResult.Error -> CreateReservationState(error = station.error)
            slot is AppResult.Error -> CreateReservationState(error = slot.error)
            station is AppResult.Success && slot is AppResult.Success && slot.value.stationId == station.value.id ->
                CreateReservationState(context = BookingContext(station.value, slot.value))
            else -> CreateReservationState(error = AppError("The selected slot does not belong to this station.", "SLOT_STATION_MISMATCH"))
        }
    }
    fun submit() {
        if (mutableState.value.isSubmitting) return
        val context = mutableState.value.context ?: return
        mutableState.value = mutableState.value.copy(isSubmitting = true, error = null)
        viewModelScope.launch {
            mutableState.value = when (val result = reservations.create(context.station.id, context.slot.id)) {
                is AppResult.Success -> mutableState.value.copy(isSubmitting = false, createdReservationId = result.value.reservationId)
                is AppResult.Error -> mutableState.value.copy(isSubmitting = false, error = result.error)
            }
        }
    }
}
data class BookingContext(val station: Station, val slot: EnergySlot)
data class CreateReservationState(
    val isLoading: Boolean = false,
    val isSubmitting: Boolean = false,
    val context: BookingContext? = null,
    val error: AppError? = null,
    val createdReservationId: String? = null,
)

