package com.smartsolar.microgrid.data.repository

import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.error.ErrorNormalizer
import com.smartsolar.microgrid.core.session.SessionManager
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.remote.AuthApiService
import com.smartsolar.microgrid.data.remote.dto.ProsumerLoginRequestDto
import com.smartsolar.microgrid.domain.model.AccountStatus
import com.smartsolar.microgrid.domain.model.SessionUser
import com.smartsolar.microgrid.domain.model.UserRole
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

    suspend fun loginProsumer(nic: String, password: String): AppResult<SessionUser> {
        return try {
            val response = authApiService.loginProsumer(
                ProsumerLoginRequestDto(nic = nic.trim(), password = password),
            )
            val user = response.user.toDomainOrNull()
                ?: return invalidContractResult()
            if (!user.isActiveProsumer()) {
                return unsupportedSessionResult()
            }
            sessionManager.saveToken(response.token)
            mutableSessionState.value = SessionState.Authenticated(user)
            AppResult.Success(user)
        } catch (throwable: Throwable) {
            AppResult.Error(errorNormalizer.normalize(throwable))
        }
    }

    suspend fun restoreSession(): AppResult<SessionUser?> {
        if (sessionManager.getToken() == null) {
            mutableSessionState.value = SessionState.Unauthenticated
            return AppResult.Success(null)
        }

        return try {
            val user = authApiService.getCurrentUser().toDomainOrNull()
                ?: return invalidContractResult()
            if (!user.isActiveProsumer()) {
                return unsupportedSessionResult()
            }
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

    fun invalidateIfUnauthorized(error: AppError) {
        if (error.httpStatus == 401) {
            clearSession()
        }
    }

    private fun invalidContractResult(): AppResult.Error {
        val error = AppError(
            message = "The server returned unsupported account information.",
            code = "INVALID_SESSION_RESPONSE",
        )
        mutableSessionState.value = SessionState.Failed(error)
        return AppResult.Error(error)
    }

    private fun unsupportedSessionResult(): AppResult.Error {
        val error = AppError(
            message = "This Android application supports active Prosumer accounts only.",
            code = "UNSUPPORTED_ANDROID_SESSION",
        )
        mutableSessionState.value = SessionState.Failed(error)
        return AppResult.Error(error)
    }

    private fun SessionUser.isActiveProsumer(): Boolean =
        role == UserRole.PROSUMER && accountStatus == AccountStatus.ACTIVE
}

sealed interface SessionState {
    data object Initializing : SessionState
    data object Unauthenticated : SessionState
    data class Authenticated(val user: SessionUser) : SessionState
    data class Failed(val error: AppError) : SessionState
}

