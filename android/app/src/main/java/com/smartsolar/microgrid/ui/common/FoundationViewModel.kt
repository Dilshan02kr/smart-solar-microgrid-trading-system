package com.smartsolar.microgrid.ui.common

import androidx.lifecycle.ViewModel
import com.smartsolar.microgrid.data.repository.SessionRepository

class FoundationViewModel(
    sessionRepository: SessionRepository,
) : ViewModel() {
    val sessionState = sessionRepository.sessionState
}

