package com.smartsolar.microgrid.ui.launch

import kotlinx.coroutines.async
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.delay

const val SPLASH_MIN_DURATION_MS = 10_000L

/** Runs session validation and the branded minimum-duration gate concurrently. */
suspend fun <T> awaitSplashReady(
    validateSession: suspend () -> T,
    waitForMinimumDuration: suspend (Long) -> Unit = { delay(it) },
): T = coroutineScope {
    val durationGate = async { waitForMinimumDuration(SPLASH_MIN_DURATION_MS) }
    val validation = async { validateSession() }
    durationGate.await()
    validation.await()
}
