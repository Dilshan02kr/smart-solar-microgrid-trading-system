package com.smartsolar.microgrid.ui.operator

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.OperatorReservationPresentation
import com.smartsolar.microgrid.data.repository.OperatorRepository
import com.smartsolar.microgrid.domain.model.OperatorReservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class OperatorReservationsState(
    val isLoading: Boolean = false,
    val reservations: List<OperatorReservation> = emptyList(),
    val selectedStatus: ReservationStatus = ReservationStatus.PENDING,
    val error: AppError? = null,
) {
    val visibleReservations: List<OperatorReservation>
        get() = OperatorReservationPresentation.visible(reservations, selectedStatus)
}

class OperatorReservationsViewModel(private val repository: OperatorRepository) : ViewModel() {
    private val mutableState = MutableStateFlow(OperatorReservationsState())
    val state = mutableState.asStateFlow()

    init { load() }

    fun load() {
        if (mutableState.value.isLoading) return
        mutableState.value = mutableState.value.copy(isLoading = true, error = null)
        viewModelScope.launch {
            mutableState.value = when (val result = repository.getReservations()) {
                is AppResult.Success -> mutableState.value.copy(isLoading = false, reservations = result.value)
                is AppResult.Error -> mutableState.value.copy(isLoading = false, error = result.error)
            }
        }
    }

    fun selectStatus(status: ReservationStatus) {
        require(status == ReservationStatus.PENDING || status == ReservationStatus.APPROVED)
        mutableState.value = mutableState.value.copy(selectedStatus = status)
    }
}
