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

class LaunchViewModel(
    private val sessionRepository: SessionRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow<LaunchUiState>(LaunchUiState.Loading)
    val state: StateFlow<LaunchUiState> = mutableState.asStateFlow()

    init {
        restoreSession()
    }

    fun retry() {
        restoreSession()
    }

    fun continueToLogin() {
        sessionRepository.clearSession()
        mutableState.value = LaunchUiState.NavigateToLogin
    }

    private fun restoreSession() {
        mutableState.value = LaunchUiState.Loading
        viewModelScope.launch {
            mutableState.value = when (val result = sessionRepository.restoreSession()) {
                is AppResult.Success -> if (result.value == null) {
                    LaunchUiState.NavigateToLogin
                } else {
                    LaunchUiState.NavigateToHome
                }
                is AppResult.Error -> if (result.error.httpStatus == 401) {
                    LaunchUiState.NavigateToLogin
                } else {
                    LaunchUiState.Error(result.error)
                }
            }
        }
    }
}

sealed interface LaunchUiState {
    data object Loading : LaunchUiState
    data object NavigateToLogin : LaunchUiState
    data object NavigateToHome : LaunchUiState
    data class Error(val error: AppError) : LaunchUiState
}

