package com.smartsolar.microgrid.ui.operator

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.OperatorRepository
import com.smartsolar.microgrid.data.repository.SessionRepository
import com.smartsolar.microgrid.data.repository.SessionState
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.OperatorDashboardSummary
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class OperatorDashboardState(
    val isLoading: Boolean = false,
    val summary: OperatorDashboardSummary? = null,
    val stationLabel: String? = null,
    val error: AppError? = null,
)

class OperatorDashboardViewModel(
    private val operators: OperatorRepository,
    private val stations: StationRepository,
    private val sessions: SessionRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(OperatorDashboardState())
    val state = mutableState.asStateFlow()

    init {
        load()
    }

    fun load() {
        if (mutableState.value.isLoading) return
        mutableState.value = OperatorDashboardState(isLoading = true)
        viewModelScope.launch {
            when (val result = operators.getDashboardSummary()) {
                is AppResult.Error -> mutableState.value = OperatorDashboardState(error = result.error)
                is AppResult.Success -> {
                    val stationId = (sessions.sessionState.value as? SessionState.Authenticated)
                        ?.user
                        ?.assignedMicrogridNodeId
                    val stationName = stationId?.let {
                        (stations.getStation(it) as? AppResult.Success)?.value?.name
                    }
                    mutableState.value = OperatorDashboardState(
                        summary = result.value,
                        stationLabel = stationName ?: stationId,
                    )
                }
            }
        }
    }

    fun logout() = sessions.clearSession()
}
