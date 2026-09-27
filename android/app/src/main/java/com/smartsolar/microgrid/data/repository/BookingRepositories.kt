package com.smartsolar.microgrid.data.repository

import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.error.ErrorNormalizer
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.remote.EnergySlotApiService
import com.smartsolar.microgrid.data.remote.ReservationApiService
import com.smartsolar.microgrid.data.remote.StationApiService
import com.smartsolar.microgrid.data.remote.dto.ReservationRequestDto
import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.Station

private suspend fun <T> networkResult(
    sessionRepository: SessionRepository,
    errorNormalizer: ErrorNormalizer,
    block: suspend () -> T,
): AppResult<T> = try {
    AppResult.Success(block())
} catch (throwable: Throwable) {
    val error = errorNormalizer.normalize(throwable)
    sessionRepository.invalidateIfUnauthorized(error)
    AppResult.Error(error)
}

class StationRepository(
    private val api: StationApiService,
    private val sessionRepository: SessionRepository,
    private val errors: ErrorNormalizer = ErrorNormalizer(),
) {
    suspend fun getStations(): AppResult<List<Station>> = networkResult(sessionRepository, errors) {
        api.getStations().map { it.toDomainOrNull() ?: throw InvalidBookingContractException() }
    }

    suspend fun getStation(id: String): AppResult<Station> = networkResult(sessionRepository, errors) {
        api.getStation(id).toDomainOrNull() ?: throw InvalidBookingContractException()
    }
}

class EnergySlotRepository(
    private val api: EnergySlotApiService,
    private val sessionRepository: SessionRepository,
    private val errors: ErrorNormalizer = ErrorNormalizer(),
) {
    suspend fun getSlot(id: String): AppResult<EnergySlot> = networkResult(sessionRepository, errors) {
        api.getSlot(id).toDomain()
    }

    suspend fun getStationSlots(stationId: String): AppResult<List<EnergySlot>> = networkResult(sessionRepository, errors) {
        api.getStationSlots(stationId).map { it.toDomain() }
    }
}

class ReservationRepository(
    private val api: ReservationApiService,
    private val sessionRepository: SessionRepository,
    private val errors: ErrorNormalizer = ErrorNormalizer(),
) {
    suspend fun create(stationId: String, slotId: String): AppResult<Reservation> = execute {
        api.createReservation(ReservationRequestDto(stationId, slotId))
    }

    suspend fun get(id: String): AppResult<Reservation> = execute { api.getReservation(id) }

    suspend fun update(id: String, stationId: String, slotId: String): AppResult<Reservation> = execute {
        api.updateReservation(id, ReservationRequestDto(stationId, slotId))
    }

    suspend fun cancel(id: String): AppResult<Reservation> = execute { api.cancelReservation(id) }

    private suspend fun execute(block: suspend () -> com.smartsolar.microgrid.data.remote.dto.ReservationResponseDto): AppResult<Reservation> =
        networkResult(sessionRepository, errors) {
            block().toDomainOrNull() ?: throw InvalidBookingContractException()
        }
}

private class InvalidBookingContractException : Exception()
