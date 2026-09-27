package com.smartsolar.microgrid.ui.launch

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.smartsolar.microgrid.data.repository.SessionRepository

class LaunchViewModelFactory(
    private val sessionRepository: SessionRepository,
) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        require(modelClass.isAssignableFrom(LaunchViewModel::class.java))
        return LaunchViewModel(sessionRepository) as T
    }
}

