package com.smartsolar.microgrid.ui.operator

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.SessionRepository
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.FormValidator
import com.smartsolar.microgrid.ui.common.OperatorLoginInput
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class OperatorLoginState(
    val isLoading: Boolean = false,
    val validationErrors: Set<FormField> = emptySet(),
    val error: AppError? = null,
    val loginComplete: Boolean = false,
)

class OperatorLoginViewModel(
    private val sessions: SessionRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow(OperatorLoginState())
    val state = mutableState.asStateFlow()

    fun login(email: String, password: String) {
        if (mutableState.value.isLoading) return
        val errors = FormValidator.validateOperatorLogin(OperatorLoginInput(email, password))
        if (errors.isNotEmpty()) {
            mutableState.value = OperatorLoginState(validationErrors = errors)
            return
        }
        mutableState.value = OperatorLoginState(isLoading = true)
        viewModelScope.launch {
            mutableState.value = when (val result = sessions.loginOperator(email, password)) {
                is AppResult.Success -> OperatorLoginState(loginComplete = true)
                is AppResult.Error -> OperatorLoginState(error = result.error)
            }
        }
    }
}
