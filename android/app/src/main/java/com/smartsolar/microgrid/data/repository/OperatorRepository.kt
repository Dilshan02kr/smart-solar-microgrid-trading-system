package com.smartsolar.microgrid.data.repository

import com.smartsolar.microgrid.core.error.ErrorNormalizer
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.remote.OperatorApiService
import com.smartsolar.microgrid.data.remote.dto.VerifyTransactionRequestDto
import com.smartsolar.microgrid.domain.model.OperatorDashboardSummary
import com.smartsolar.microgrid.domain.model.OperatorReservation
import com.smartsolar.microgrid.domain.model.OperatorTransaction

class OperatorRepository(
    private val api: OperatorApiService,
    private val sessionRepository: SessionRepository,
    private val errors: ErrorNormalizer = ErrorNormalizer(),
) {
    suspend fun getDashboardSummary(): AppResult<OperatorDashboardSummary> = execute {
        api.getDashboardSummary().toDomain()
    }

    suspend fun getReservations(): AppResult<List<OperatorReservation>> = execute {
        api.getReservations().map { it.toDomainOrNull() ?: throw InvalidOperatorContractException() }
    }

    suspend fun verifyTransaction(transactionReference: String): AppResult<OperatorTransaction> = execute {
        api.verifyTransaction(
            VerifyTransactionRequestDto(transactionReference.trim()),
        ).toDomainOrNull() ?: throw InvalidOperatorContractException()
    }

    suspend fun completeReservation(reservationId: String): AppResult<OperatorTransaction> = execute {
        api.completeReservation(reservationId).toDomainOrNull()
            ?: throw InvalidOperatorContractException()
    }

    private suspend fun <T> execute(block: suspend () -> T): AppResult<T> = try {
        AppResult.Success(block())
    } catch (throwable: Throwable) {
        val error = errors.normalize(throwable)
        sessionRepository.invalidateIfUnauthorized(error)
        AppResult.Error(error)
    }
}

private class InvalidOperatorContractException : Exception()
