package com.smartsolar.microgrid.ui.auth

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.remote.dto.RegisterProsumerRequestDto
import com.smartsolar.microgrid.data.repository.ProsumerRepository
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.FormValidator
import com.smartsolar.microgrid.ui.common.RegistrationInput
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class RegisterViewModel(
    private val prosumerRepository: ProsumerRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(RegisterUiState())
    val state: StateFlow<RegisterUiState> = mutableState.asStateFlow()

    fun register(input: RegistrationInput) {
        if (mutableState.value.isLoading) return
        val validationErrors = FormValidator.validateRegistration(input)
        if (validationErrors.isNotEmpty()) {
            mutableState.value = RegisterUiState(validationErrors = validationErrors)
            return
        }

        mutableState.value = RegisterUiState(isLoading = true)
        viewModelScope.launch {
            val request = RegisterProsumerRequestDto(
                nic = input.nic.trim(),
                firstName = input.firstName.trim(),
                lastName = input.lastName.trim(),
                email = input.email.trim(),
                phone = input.phone.trim(),
                password = input.password,
                confirmPassword = input.confirmPassword,
            )
            mutableState.value = when (val result = prosumerRepository.register(request)) {
                is AppResult.Success -> RegisterUiState(registrationComplete = true)
                is AppResult.Error -> RegisterUiState(error = result.error)
            }
        }
    }
}

data class RegisterUiState(
    val isLoading: Boolean = false,
    val validationErrors: Set<FormField> = emptySet(),
    val error: AppError? = null,
    val registrationComplete: Boolean = false,
)

