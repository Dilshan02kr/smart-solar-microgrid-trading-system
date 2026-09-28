package com.smartsolar.microgrid.ui.auth

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.SessionRepository
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.FormValidator
import com.smartsolar.microgrid.ui.common.LoginInput
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class LoginViewModel(
    private val sessionRepository: SessionRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(LoginUiState())
    val state: StateFlow<LoginUiState> = mutableState.asStateFlow()

    fun login(nic: String, password: String) {
        if (mutableState.value.isLoading) return
        val input = LoginInput(nic, password)
        val validationErrors = FormValidator.validateLogin(input)
        if (validationErrors.isNotEmpty()) {
            mutableState.value = LoginUiState(validationErrors = validationErrors)
            return
        }

        mutableState.value = LoginUiState(isLoading = true)
        viewModelScope.launch {
            mutableState.value = when (val result = sessionRepository.loginProsumer(nic, password)) {
                is AppResult.Success -> LoginUiState(loginComplete = true)
                is AppResult.Error -> LoginUiState(error = result.error)
            }
        }
    }
}

data class LoginUiState(
    val isLoading: Boolean = false,
    val validationErrors: Set<FormField> = emptySet(),
    val error: AppError? = null,
    val loginComplete: Boolean = false,
)

