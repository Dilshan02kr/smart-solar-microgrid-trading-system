package com.smartsolar.microgrid.data.remote

import com.smartsolar.microgrid.data.remote.dto.StationResponseDto
import retrofit2.http.GET
import retrofit2.http.Path

interface StationApiService {
    @GET("api/stations")
    suspend fun getStations(): List<StationResponseDto>

    @GET("api/stations/{id}")
    suspend fun getStation(@Path("id") id: String): StationResponseDto
}

