package com.smartsolar.microgrid.ui.stations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.ui.common.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class StationsViewModel(private val repository: StationRepository) : ViewModel() {
    private val mutableState = MutableStateFlow<UiState<List<Station>>>(UiState.Loading)
    val state: StateFlow<UiState<List<Station>>> = mutableState.asStateFlow()
    private val mutableLastSuccessfulSync = MutableStateFlow<Long?>(null)
    val lastSuccessfulSync: StateFlow<Long?> = mutableLastSuccessfulSync.asStateFlow()

    init { load() }

    fun load() {
        mutableState.value = UiState.Loading
        viewModelScope.launch {
            mutableState.value = when (val result = repository.getStations()) {
                is AppResult.Success -> {
                    mutableLastSuccessfulSync.value = repository.getLastSuccessfulSyncEpochMillis()
                    if (result.value.isEmpty()) UiState.Empty else UiState.Content(result.value)
                }
                is AppResult.Error -> UiState.Error(result.error)
            }
        }
    }
}

