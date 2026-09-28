package com.smartsolar.microgrid.ui.prosumer

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.ProsumerRepository
import com.smartsolar.microgrid.data.repository.SessionRepository
import com.smartsolar.microgrid.domain.model.ProsumerProfile
import com.smartsolar.microgrid.ui.common.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class ProsumerProfileViewModel(
    private val prosumerRepository: ProsumerRepository,
    private val sessionRepository: SessionRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow<UiState<ProsumerProfile>>(UiState.Loading)
    val state: StateFlow<UiState<ProsumerProfile>> = mutableState.asStateFlow()

    private val mutableDeactivation = MutableStateFlow(DeactivationState())
    val deactivation: StateFlow<DeactivationState> = mutableDeactivation.asStateFlow()

    init {
        loadProfile()
    }

    fun loadProfile() {
        mutableState.value = UiState.Loading
        viewModelScope.launch {
            mutableState.value = when (val result = prosumerRepository.loadProfile()) {
                is AppResult.Success -> UiState.Content(result.value)
                is AppResult.Error -> UiState.Error(result.error)
            }
        }
    }

    fun deactivate() {
        if (mutableDeactivation.value.isLoading) return
        mutableDeactivation.value = DeactivationState(isLoading = true)
        viewModelScope.launch {
            mutableDeactivation.value = when (val result = prosumerRepository.deactivate()) {
                is AppResult.Success -> {
                    sessionRepository.clearSession()
                    DeactivationState(complete = true)
                }
                is AppResult.Error -> DeactivationState(error = result.error)
            }
        }
    }
}

data class DeactivationState(
    val isLoading: Boolean = false,
    val error: AppError? = null,
    val complete: Boolean = false,
)

