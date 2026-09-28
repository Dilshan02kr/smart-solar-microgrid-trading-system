package com.smartsolar.microgrid.ui.prosumer

import androidx.lifecycle.ViewModel
import com.smartsolar.microgrid.data.repository.SessionRepository

class ProsumerHomeViewModel(
    private val sessionRepository: SessionRepository,
) : ViewModel() {
    val sessionState = sessionRepository.sessionState

    fun logout() {
        sessionRepository.clearSession()
    }
}

