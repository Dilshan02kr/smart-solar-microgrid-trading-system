package com.smartsolar.microgrid.ui.reservations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.ReservationPresentation
import com.smartsolar.microgrid.data.repository.ReservationRepository
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.Reservation
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class ReservationQrContent(
    val reservation: Reservation,
    val payload: String,
    val stationName: String,
)

data class ReservationQrState(
    val isLoading: Boolean = false,
    val content: ReservationQrContent? = null,
    val isUnavailable: Boolean = false,
    val error: AppError? = null,
)

class ReservationQrViewModel(
    private val reservationId: String,
    private val reservations: ReservationRepository,
    private val stations: StationRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(ReservationQrState())
    val state = mutableState.asStateFlow()

    init {
        load()
    }

    fun load() {
        if (mutableState.value.isLoading) return
        mutableState.value = ReservationQrState(isLoading = true)
        viewModelScope.launch {
            when (val result = reservations.get(reservationId)) {
                is AppResult.Error -> mutableState.value = ReservationQrState(error = result.error)
                is AppResult.Success -> {
                    val payload = ReservationPresentation.qrPayload(result.value)
                    if (payload == null) {
                        mutableState.value = ReservationQrState(isUnavailable = true)
                    } else {
                        val station = (stations.getStation(result.value.stationId) as? AppResult.Success)?.value
                        mutableState.value = ReservationQrState(
                            content = ReservationQrContent(
                                reservation = result.value,
                                payload = payload,
                                stationName = station?.name ?: result.value.stationId,
                            ),
                        )
                    }
                }
            }
        }
    }
}
