package com.smartsolar.microgrid

import android.app.Application

class SmartSolarApplication : Application() {
    val appContainer: AppContainer by lazy { AppContainer(this) }
}

