package com.smartsolar.microgrid.data.remote

import com.smartsolar.microgrid.data.remote.dto.AuthenticatedUserDto
import retrofit2.http.GET

interface AuthApiService {
    @GET("api/auth/me")
    suspend fun getCurrentUser(): AuthenticatedUserDto
}

