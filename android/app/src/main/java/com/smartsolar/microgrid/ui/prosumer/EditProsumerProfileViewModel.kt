package com.smartsolar.microgrid.ui.prosumer

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.remote.dto.UpdateProsumerProfileRequestDto
import com.smartsolar.microgrid.data.repository.ProsumerRepository
import com.smartsolar.microgrid.domain.model.ProsumerProfile
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.FormValidator
import com.smartsolar.microgrid.ui.common.ProfileInput
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class EditProsumerProfileViewModel(
    private val prosumerRepository: ProsumerRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(EditProfileUiState(isLoadingProfile = true))
    val state: StateFlow<EditProfileUiState> = mutableState.asStateFlow()

    init {
        loadProfile()
    }

    fun save(input: ProfileInput) {
        if (mutableState.value.isSaving) return
        val validationErrors = FormValidator.validateProfile(input)
        if (validationErrors.isNotEmpty()) {
            mutableState.value = mutableState.value.copy(validationErrors = validationErrors)
            return
        }

        mutableState.value = mutableState.value.copy(
            isSaving = true,
            error = null,
            validationErrors = emptySet(),
        )
        viewModelScope.launch {
            val request = UpdateProsumerProfileRequestDto(
                firstName = input.firstName.trim(),
                lastName = input.lastName.trim(),
                email = input.email.trim(),
                phone = input.phone.trim(),
            )
            mutableState.value = when (val result = prosumerRepository.updateProfile(request)) {
                is AppResult.Success -> mutableState.value.copy(
                    isSaving = false,
                    profile = result.value,
                    saveComplete = true,
                )
                is AppResult.Error -> mutableState.value.copy(
                    isSaving = false,
                    error = result.error,
                )
            }
        }
    }

    fun loadProfile() {
        mutableState.value = EditProfileUiState(isLoadingProfile = true)
        viewModelScope.launch {
            mutableState.value = when (val result = prosumerRepository.loadProfile()) {
                is AppResult.Success -> EditProfileUiState(profile = result.value)
                is AppResult.Error -> EditProfileUiState(error = result.error)
            }
        }
    }
}

data class EditProfileUiState(
    val isLoadingProfile: Boolean = false,
    val isSaving: Boolean = false,
    val profile: ProsumerProfile? = null,
    val validationErrors: Set<FormField> = emptySet(),
    val error: AppError? = null,
    val saveComplete: Boolean = false,
)
