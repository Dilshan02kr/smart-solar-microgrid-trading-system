package com.smartsolar.microgrid.data.remote

import com.smartsolar.microgrid.data.remote.dto.EnergySlotResponseDto
import retrofit2.http.GET
import retrofit2.http.Path

interface EnergySlotApiService {
    @GET("api/energy-booking-slots/{slotId}")
    suspend fun getSlot(@Path("slotId") slotId: String): EnergySlotResponseDto

    @GET("api/energy-booking-slots/station/{stationId}")
    suspend fun getStationSlots(@Path("stationId") stationId: String): List<EnergySlotResponseDto>
}

