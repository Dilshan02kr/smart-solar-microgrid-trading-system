package com.smartsolar.microgrid.data.local

import android.content.Context

object DatabaseProvider {
    @Volatile
    private var instance: AppDatabaseHelper? = null

    fun getInstance(context: Context): AppDatabaseHelper = instance ?: synchronized(this) {
        instance ?: AppDatabaseHelper(context.applicationContext).also { instance = it }
    }
}

