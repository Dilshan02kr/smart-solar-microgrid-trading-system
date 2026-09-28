package com.smartsolar.microgrid.data.repository

import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.error.ErrorNormalizer
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.remote.ProsumerApiService
import com.smartsolar.microgrid.data.remote.dto.RegisterProsumerRequestDto
import com.smartsolar.microgrid.data.remote.dto.UpdateProsumerProfileRequestDto
import com.smartsolar.microgrid.domain.model.ProsumerProfile
import com.smartsolar.microgrid.domain.model.AccountStatus

class ProsumerRepository(
    private val apiService: ProsumerApiService,
    private val sessionRepository: SessionRepository,
    private val errorNormalizer: ErrorNormalizer = ErrorNormalizer(),
) {
    suspend fun register(request: RegisterProsumerRequestDto): AppResult<Unit> =
        execute {
            val response = apiService.registerProsumer(request)
            if (response.role != "PROSUMER" || response.accountStatus != "PENDING") {
                throw InvalidServerContractException()
            }
            Unit
        }

    suspend fun loadProfile(): AppResult<ProsumerProfile> = execute {
        apiService.getMyProfile().toDomainOrNull() ?: throw InvalidServerContractException()
    }

    suspend fun updateProfile(
        request: UpdateProsumerProfileRequestDto,
    ): AppResult<ProsumerProfile> = execute {
        apiService.updateMyProfile(request).toDomainOrNull()
            ?: throw InvalidServerContractException()
    }

    suspend fun deactivate(): AppResult<ProsumerProfile> = execute {
        apiService.deactivateMyProfile().toDomainOrNull()
            ?: throw InvalidServerContractException()
    }.requireDeactivated()

    private fun AppResult<ProsumerProfile>.requireDeactivated(): AppResult<ProsumerProfile> =
        when (this) {
            is AppResult.Success -> if (value.accountStatus == AccountStatus.DEACTIVATED) {
                this
            } else {
                AppResult.Error(
                    AppError(
                        message = "The server did not confirm account deactivation.",
                        code = "DEACTIVATION_NOT_CONFIRMED",
                    ),
                )
            }
            is AppResult.Error -> this
        }

    private suspend fun <T> execute(block: suspend () -> T): AppResult<T> = try {
        AppResult.Success(block())
    } catch (_: InvalidServerContractException) {
        AppResult.Error(
            AppError(
                message = "The server returned unsupported Prosumer information.",
                code = "INVALID_PROSUMER_RESPONSE",
            ),
        )
    } catch (throwable: Throwable) {
        val error = errorNormalizer.normalize(throwable)
        sessionRepository.invalidateIfUnauthorized(error)
        AppResult.Error(error)
    }

    private class InvalidServerContractException : Exception()
}
