package com.smartsolar.microgrid.ui.common

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.smartsolar.microgrid.data.repository.SessionRepository

class FoundationViewModelFactory(
    private val sessionRepository: SessionRepository,
) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        require(modelClass.isAssignableFrom(FoundationViewModel::class.java))
        return FoundationViewModel(sessionRepository) as T
    }
}

