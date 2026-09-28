package com.smartsolar.microgrid.ui.stations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.StationRepository
import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.ui.common.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class StationDetailsViewModel(private val id: String, private val repository: StationRepository) : ViewModel() {
    private val mutableState = MutableStateFlow<UiState<Station>>(UiState.Loading)
    val state = mutableState.asStateFlow()
    init { load() }
    fun load() {
        mutableState.value = UiState.Loading
        viewModelScope.launch {
            mutableState.value = when (val result = repository.getStation(id)) {
                is AppResult.Success -> UiState.Content(result.value)
                is AppResult.Error -> UiState.Error(result.error)
            }
        }
    }
}

