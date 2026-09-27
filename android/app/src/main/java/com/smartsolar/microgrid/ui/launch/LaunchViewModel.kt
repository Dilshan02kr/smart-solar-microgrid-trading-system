package com.smartsolar.microgrid.ui.launch

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.data.repository.SessionRepository
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

    private fun restoreSession() {
        mutableState.value = LaunchUiState.Loading
        viewModelScope.launch {
            sessionRepository.restoreSession()
            mutableState.value = LaunchUiState.Ready
        }
    }
}

sealed interface LaunchUiState {
    data object Loading : LaunchUiState
    data object Ready : LaunchUiState
}

