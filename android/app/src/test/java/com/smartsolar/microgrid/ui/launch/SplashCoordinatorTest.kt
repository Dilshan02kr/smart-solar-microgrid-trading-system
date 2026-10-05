package com.smartsolar.microgrid.ui.launch

import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.launch
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.yield
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class SplashCoordinatorTest {
    @Test
    fun `uses centralized ten second minimum`() = runBlocking {
        var requestedDelay = 0L
        val result = awaitSplashReady(
            validateSession = { "LOGIN_SELECTION" },
            waitForMinimumDuration = { requestedDelay = it },
        )

        assertEquals(10_000L, SPLASH_MIN_DURATION_MS)
        assertEquals(SPLASH_MIN_DURATION_MS, requestedDelay)
        assertEquals("LOGIN_SELECTION", result)
    }

    @Test
    fun `early validation waits for duration gate`() = runBlocking {
        val durationGate = CompletableDeferred<Unit>()
        var completed = false
        val job = launch {
            awaitSplashReady(
                validateSession = { "PROSUMER_HOME" },
                waitForMinimumDuration = { durationGate.await() },
            )
            completed = true
        }

        yield()
        assertFalse(completed)
        durationGate.complete(Unit)
        job.join()
        assertTrue(completed)
    }

    @Test
    fun `late validation is awaited after duration gate`() = runBlocking {
        val validation = CompletableDeferred<String>()
        var completionCount = 0
        val job = launch {
            awaitSplashReady(
                validateSession = { validation.await() },
                waitForMinimumDuration = { },
            )
            completionCount++
        }

        yield()
        assertEquals(0, completionCount)
        validation.complete("OPERATOR_DASHBOARD")
        job.join()
        assertEquals(1, completionCount)
    }
}
