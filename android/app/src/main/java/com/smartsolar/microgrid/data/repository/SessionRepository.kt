package com.smartsolar.microgrid.data.repository

import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.error.ErrorNormalizer
import com.smartsolar.microgrid.core.session.SessionManager
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.remote.AuthApiService
import com.smartsolar.microgrid.domain.model.SessionUser
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

class SessionRepository(
    private val authApiService: AuthApiService,
    private val sessionManager: SessionManager,
    private val errorNormalizer: ErrorNormalizer = ErrorNormalizer(),
) {
    private val mutableSessionState = MutableStateFlow<SessionState>(SessionState.Initializing)
    val sessionState: StateFlow<SessionState> = mutableSessionState.asStateFlow()

    suspend fun restoreSession(): AppResult<SessionUser?> {
        if (sessionManager.getToken() == null) {
            mutableSessionState.value = SessionState.Unauthenticated
            return AppResult.Success(null)
        }

        return try {
            val user = authApiService.getCurrentUser().toDomainOrNull()
                ?: return invalidContractResult()
            mutableSessionState.value = SessionState.Authenticated(user)
            AppResult.Success(user)
        } catch (throwable: Throwable) {
            val error = errorNormalizer.normalize(throwable)
            if (error.httpStatus == 401) {
                mutableSessionState.value = SessionState.Unauthenticated
            } else {
                mutableSessionState.value = SessionState.Failed(error)
            }
            AppResult.Error(error)
        }
    }

    fun clearSession() {
        sessionManager.clearToken()
        mutableSessionState.value = SessionState.Unauthenticated
    }

    private fun invalidContractResult(): AppResult.Error {
        val error = AppError(
            message = "The server returned unsupported account information.",
            code = "INVALID_SESSION_RESPONSE",
        )
        mutableSessionState.value = SessionState.Failed(error)
        return AppResult.Error(error)
    }
}

sealed interface SessionState {
    data object Initializing : SessionState
    data object Unauthenticated : SessionState
    data class Authenticated(val user: SessionUser) : SessionState
    data class Failed(val error: AppError) : SessionState
}

