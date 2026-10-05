package com.smartsolar.microgrid.data.remote

import com.smartsolar.microgrid.data.remote.dto.DashboardSummaryResponseDto
import com.smartsolar.microgrid.data.remote.dto.OperatorTransactionResponseDto
import com.smartsolar.microgrid.data.remote.dto.OperatorReservationResponseDto
import com.smartsolar.microgrid.data.remote.dto.VerifyTransactionRequestDto
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

interface OperatorApiService {
    @GET("api/operator/dashboard/summary")
    suspend fun getDashboardSummary(): DashboardSummaryResponseDto

    @GET("api/operator/reservations")
    suspend fun getReservations(): List<OperatorReservationResponseDto>

    @POST("api/operator/verify-transaction")
    suspend fun verifyTransaction(
        @Body request: VerifyTransactionRequestDto,
    ): OperatorTransactionResponseDto

    @POST("api/operator/reservations/{reservationId}/complete")
    suspend fun completeReservation(
        @Path("reservationId") reservationId: String,
    ): OperatorTransactionResponseDto
}
