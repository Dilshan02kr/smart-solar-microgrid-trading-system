package com.smartsolar.microgrid.ui.prosumer

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.ReservationRepository
import com.smartsolar.microgrid.data.repository.SessionRepository
import com.smartsolar.microgrid.domain.model.ReservationStatus
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class ProsumerDashboardState(
    val isLoading: Boolean = false,
    val pendingCount: Int = 0,
    val approvedCount: Int = 0,
    val completedCount: Int = 0,
    val error: AppError? = null,
)

class ProsumerHomeViewModel(
    private val sessionRepository: SessionRepository,
    private val reservationRepository: ReservationRepository,
) : ViewModel() {
    val sessionState = sessionRepository.sessionState
    private val mutableDashboardState = MutableStateFlow(ProsumerDashboardState())
    val dashboardState = mutableDashboardState.asStateFlow()

    init { refresh() }

    fun refresh() {
        if (mutableDashboardState.value.isLoading) return
        mutableDashboardState.value = mutableDashboardState.value.copy(isLoading = true, error = null)
        viewModelScope.launch {
            when (val result = reservationRepository.getMine()) {
                is AppResult.Error -> mutableDashboardState.value = mutableDashboardState.value.copy(isLoading = false, error = result.error)
                is AppResult.Success -> mutableDashboardState.value = ProsumerDashboardState(
                    isLoading = false,
                    pendingCount = result.value.count { it.status == ReservationStatus.PENDING },
                    approvedCount = result.value.count { it.status == ReservationStatus.APPROVED },
                    completedCount = result.value.count { it.status == ReservationStatus.COMPLETED },
                )
            }
        }
    }

    fun logout() { sessionRepository.clearSession() }
}
