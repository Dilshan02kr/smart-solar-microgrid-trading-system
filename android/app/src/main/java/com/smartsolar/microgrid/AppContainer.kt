package com.smartsolar.microgrid

import android.content.Context
import com.smartsolar.microgrid.core.network.NetworkModule
import com.smartsolar.microgrid.core.session.SessionManager
import com.smartsolar.microgrid.data.local.AppDatabaseHelper
import com.smartsolar.microgrid.data.local.DatabaseProvider
import com.smartsolar.microgrid.data.local.MetadataDao
import com.smartsolar.microgrid.data.remote.AuthApiService
import com.smartsolar.microgrid.data.remote.ProsumerApiService
import com.smartsolar.microgrid.data.remote.StationApiService
import com.smartsolar.microgrid.data.remote.EnergySlotApiService
import com.smartsolar.microgrid.data.remote.ReservationApiService
import com.smartsolar.microgrid.data.remote.OperatorApiService
import com.smartsolar.microgrid.data.repository.ProsumerRepository
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.data.repository.EnergySlotRepository
import com.smartsolar.microgrid.data.repository.ReservationRepository
import com.smartsolar.microgrid.data.repository.SessionRepository
import com.smartsolar.microgrid.data.repository.OperatorRepository

class AppContainer(context: Context) {
    val sessionManager = SessionManager(context)
    val databaseHelper: AppDatabaseHelper = DatabaseProvider.getInstance(context)
    val metadataDao = MetadataDao(databaseHelper)

    private val retrofit = NetworkModule.createRetrofit(sessionManager)
    private val authApiService = retrofit.create(AuthApiService::class.java)
    private val prosumerApiService = retrofit.create(ProsumerApiService::class.java)
    private val stationApiService = retrofit.create(StationApiService::class.java)
    private val energySlotApiService = retrofit.create(EnergySlotApiService::class.java)
    private val reservationApiService = retrofit.create(ReservationApiService::class.java)
    private val operatorApiService = retrofit.create(OperatorApiService::class.java)

    val sessionRepository = SessionRepository(authApiService, sessionManager)
    val prosumerRepository = ProsumerRepository(prosumerApiService, sessionRepository)
    val stationRepository = StationRepository(stationApiService, sessionRepository)
    val energySlotRepository = EnergySlotRepository(energySlotApiService, sessionRepository)
    val reservationRepository = ReservationRepository(reservationApiService, sessionRepository)
    val operatorRepository = OperatorRepository(operatorApiService, sessionRepository)
}

