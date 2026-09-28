package com.smartsolar.microgrid.data.remote

import com.smartsolar.microgrid.data.remote.dto.ProsumerProfileDto
import com.smartsolar.microgrid.data.remote.dto.ProsumerRegistrationResponseDto
import com.smartsolar.microgrid.data.remote.dto.RegisterProsumerRequestDto
import com.smartsolar.microgrid.data.remote.dto.UpdateProsumerProfileRequestDto
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.PATCH
import retrofit2.http.POST
import retrofit2.http.PUT

interface ProsumerApiService {
    @POST("api/prosumers/register")
    suspend fun registerProsumer(
        @Body request: RegisterProsumerRequestDto,
    ): ProsumerRegistrationResponseDto

    @GET("api/prosumers/me")
    suspend fun getMyProfile(): ProsumerProfileDto

    @PUT("api/prosumers/me")
    suspend fun updateMyProfile(
        @Body request: UpdateProsumerProfileRequestDto,
    ): ProsumerProfileDto

    @PATCH("api/prosumers/me/deactivate")
    suspend fun deactivateMyProfile(): ProsumerProfileDto
}

