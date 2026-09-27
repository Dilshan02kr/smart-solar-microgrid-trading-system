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
import com.smartsolar.microgrid.domain.model.StationStatus
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class EditReservationViewModel(
    private val id: String,
    private val reservations: ReservationRepository,
    private val stations: StationRepository,
    private val slots: EnergySlotRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(EditReservationState(isLoading = true))
    val state = mutableState.asStateFlow()
    init { load() }
    fun load() = viewModelScope.launch {
        mutableState.value = EditReservationState(isLoading = true)
        val reservationResult = reservations.get(id); val stationsResult = stations.getStations()
        if (reservationResult is AppResult.Error) { mutableState.value = EditReservationState(error = reservationResult.error); return@launch }
        if (stationsResult is AppResult.Error) { mutableState.value = EditReservationState(error = stationsResult.error); return@launch }
        val reservation = (reservationResult as AppResult.Success).value
        val stationList = (stationsResult as AppResult.Success).value.filter { it.status == StationStatus.ACTIVE }
        mutableState.value = EditReservationState(reservation = reservation, stations = stationList, selectedStationId = reservation.stationId, isLoadingSlots = true)
        loadSlots(reservation.stationId, reservation.slotId)
    }
    fun selectStation(stationId: String) {
        if (stationId == mutableState.value.selectedStationId) return
        mutableState.value = mutableState.value.copy(selectedStationId = stationId, selectedSlotId = null, isLoadingSlots = true, error = null)
        loadSlots(stationId, null)
    }
    fun selectSlot(slotId: String) { mutableState.value = mutableState.value.copy(selectedSlotId = slotId) }
    private fun loadSlots(stationId: String, preferredSlotId: String?) = viewModelScope.launch {
        mutableState.value = when (val result = slots.getStationSlots(stationId)) {
            is AppResult.Success -> {
                val currentId = mutableState.value.reservation?.slotId
                val usable = result.value.filter { it.isAvailable || it.id == currentId }
                mutableState.value.copy(slots = usable, selectedSlotId = preferredSlotId?.takeIf { id -> usable.any { it.id == id } }, isLoadingSlots = false)
            }
            is AppResult.Error -> mutableState.value.copy(isLoadingSlots = false, error = result.error)
        }
    }
    fun save() {
        val state = mutableState.value
        val stationId = state.selectedStationId ?: return
        val slotId = state.selectedSlotId ?: return
        if (state.isSaving) return
        mutableState.value = state.copy(isSaving = true, error = null)
        viewModelScope.launch {
            mutableState.value = when (val result = reservations.update(id, stationId, slotId)) {
                is AppResult.Success -> mutableState.value.copy(isSaving = false, updated = true, reservation = result.value)
                is AppResult.Error -> mutableState.value.copy(isSaving = false, error = result.error)
            }
        }
    }
}
data class EditReservationState(
    val isLoading: Boolean = false,
    val isLoadingSlots: Boolean = false,
    val isSaving: Boolean = false,
    val reservation: Reservation? = null,
    val stations: List<Station> = emptyList(),
    val slots: List<EnergySlot> = emptyList(),
    val selectedStationId: String? = null,
    val selectedSlotId: String? = null,
    val error: AppError? = null,
    val updated: Boolean = false,
)
