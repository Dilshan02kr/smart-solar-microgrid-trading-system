package com.smartsolar.microgrid.data.remote

import com.smartsolar.microgrid.data.remote.dto.ReservationRequestDto
import com.smartsolar.microgrid.data.remote.dto.ReservationResponseDto
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path

interface ReservationApiService {
    @POST("api/reservations")
    suspend fun createReservation(@Body request: ReservationRequestDto): ReservationResponseDto

    @GET("api/reservations/{reservationId}")
    suspend fun getReservation(@Path("reservationId") reservationId: String): ReservationResponseDto

    @PUT("api/reservations/{reservationId}")
    suspend fun updateReservation(
        @Path("reservationId") reservationId: String,
        @Body request: ReservationRequestDto,
    ): ReservationResponseDto

    @PUT("api/reservations/{reservationId}/cancel")
    suspend fun cancelReservation(@Path("reservationId") reservationId: String): ReservationResponseDto
}

