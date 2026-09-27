package com.smartsolar.microgrid.data.remote

import com.smartsolar.microgrid.data.remote.dto.AuthenticatedUserDto
import com.smartsolar.microgrid.data.remote.dto.AuthenticationResponseDto
import com.smartsolar.microgrid.data.remote.dto.ProsumerLoginRequestDto
import retrofit2.http.GET
import retrofit2.http.Body
import retrofit2.http.POST

interface AuthApiService {
    @POST("api/auth/prosumer-login")
    suspend fun loginProsumer(
        @Body request: ProsumerLoginRequestDto,
    ): AuthenticationResponseDto

    @GET("api/auth/me")
    suspend fun getCurrentUser(): AuthenticatedUserDto
}

