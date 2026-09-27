package com.smartsolar.microgrid

import android.content.Context
import com.smartsolar.microgrid.core.network.NetworkModule
import com.smartsolar.microgrid.core.session.SessionManager
import com.smartsolar.microgrid.data.local.AppDatabaseHelper
import com.smartsolar.microgrid.data.local.DatabaseProvider
import com.smartsolar.microgrid.data.local.MetadataDao
import com.smartsolar.microgrid.data.remote.AuthApiService
import com.smartsolar.microgrid.data.remote.ProsumerApiService
import com.smartsolar.microgrid.data.repository.ProsumerRepository
import com.smartsolar.microgrid.data.repository.SessionRepository

class AppContainer(context: Context) {
    val sessionManager = SessionManager(context)
    val databaseHelper: AppDatabaseHelper = DatabaseProvider.getInstance(context)
    val metadataDao = MetadataDao(databaseHelper)

    private val retrofit = NetworkModule.createRetrofit(sessionManager)
    private val authApiService = retrofit.create(AuthApiService::class.java)
    private val prosumerApiService = retrofit.create(ProsumerApiService::class.java)

    val sessionRepository = SessionRepository(authApiService, sessionManager)
    val prosumerRepository = ProsumerRepository(prosumerApiService, sessionRepository)
}

