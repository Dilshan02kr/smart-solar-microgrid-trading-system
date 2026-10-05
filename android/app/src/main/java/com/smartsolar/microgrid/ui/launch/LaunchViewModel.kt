package com.smartsolar.microgrid.ui.launch

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.data.repository.SessionRepository
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.Job
import com.smartsolar.microgrid.domain.model.SessionDestination
import com.smartsolar.microgrid.domain.model.SessionRouting

class LaunchViewModel(
    private val sessionRepository: SessionRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow<LaunchUiState>(LaunchUiState.Loading)
    val state: StateFlow<LaunchUiState> = mutableState.asStateFlow()
    private var restoreJob: Job? = null

    init {
        restoreSession(enforceMinimumDuration = true)
    }

    fun retry() {
        restoreSession(enforceMinimumDuration = false)
    }

    fun continueToLogin() {
        sessionRepository.clearSession()
        mutableState.value = LaunchUiState.NavigateToLoginSelection
    }

    private fun restoreSession(enforceMinimumDuration: Boolean) {
        if (restoreJob?.isActive == true) return
        mutableState.value = LaunchUiState.Loading
        restoreJob = viewModelScope.launch {
            val restore = suspend { sessionRepository.restoreSession() }
            val result = if (enforceMinimumDuration) awaitSplashReady(restore) else restore()
            mutableState.value = when (result) {
                is AppResult.Success -> when (SessionRouting.destination(result.value)) {
                    SessionDestination.LOGIN_SELECTION -> LaunchUiState.NavigateToLoginSelection
                    SessionDestination.PROSUMER_HOME -> LaunchUiState.NavigateToProsumerHome
                    SessionDestination.OPERATOR_DASHBOARD -> LaunchUiState.NavigateToOperatorDashboard
                    SessionDestination.UNSUPPORTED -> LaunchUiState.Error(
                        AppError("This account role is not supported by the Android application.", "UNSUPPORTED_ANDROID_SESSION"),
                    )
                }
                is AppResult.Error -> if (result.error.httpStatus == 401) {
                    LaunchUiState.NavigateToLoginSelection
                } else {
                    LaunchUiState.Error(result.error)
                }
            }
        }
    }
}

sealed interface LaunchUiState {
    data object Loading : LaunchUiState
    data object NavigateToLoginSelection : LaunchUiState
    data object NavigateToProsumerHome : LaunchUiState
    data object NavigateToOperatorDashboard : LaunchUiState
    data class Error(val error: AppError) : LaunchUiState
}

