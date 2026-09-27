package com.smartsolar.microgrid.ui.stations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.StationMapPresentation
import com.smartsolar.microgrid.core.util.StationMarkerModel
import com.smartsolar.microgrid.data.repository.StationRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed interface StationsMapUiState {
    data object Loading : StationsMapUiState
    data class Error(val error: AppError) : StationsMapUiState
    data class Content(
        val markers: List<StationMarkerModel>,
        val skippedStationCount: Int,
    ) : StationsMapUiState
}

class StationsMapViewModel(
    private val repository: StationRepository,
) : ViewModel() {
    private val mutableState = MutableStateFlow<StationsMapUiState>(StationsMapUiState.Loading)
    val state: StateFlow<StationsMapUiState> = mutableState.asStateFlow()

    init {
        requestStations()
    }

    fun load() {
        if (mutableState.value is StationsMapUiState.Loading) return
        requestStations()
    }

    private fun requestStations() {
        mutableState.value = StationsMapUiState.Loading
        viewModelScope.launch {
            mutableState.value = when (val result = repository.getStations()) {
                is AppResult.Error -> StationsMapUiState.Error(result.error)
                is AppResult.Success -> {
                    val markers = StationMapPresentation.markers(result.value)
                    StationsMapUiState.Content(
                        markers = markers,
                        skippedStationCount = result.value.size - markers.size,
                    )
                }
            }
        }
    }
}
