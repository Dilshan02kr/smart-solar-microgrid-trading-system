package com.smartsolar.microgrid.ui.slots

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.data.repository.EnergySlotRepository
import com.smartsolar.microgrid.domain.model.EnergySlot
import com.smartsolar.microgrid.ui.common.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class StationSlotsViewModel(private val stationId: String, private val repository: EnergySlotRepository) : ViewModel() {
    private val mutableState = MutableStateFlow<UiState<List<EnergySlot>>>(UiState.Loading)
    val state = mutableState.asStateFlow()
    init { load() }
    fun load() {
        mutableState.value = UiState.Loading
        viewModelScope.launch {
            mutableState.value = when (val result = repository.getStationSlots(stationId)) {
                is AppResult.Success -> if (result.value.isEmpty()) UiState.Empty else UiState.Content(result.value)
                is AppResult.Error -> UiState.Error(result.error)
            }
        }
    }
}

