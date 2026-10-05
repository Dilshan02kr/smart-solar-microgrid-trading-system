package com.smartsolar.microgrid.ui.slots

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.SlotFilter
import com.smartsolar.microgrid.data.repository.EnergySlotRepository
import com.smartsolar.microgrid.domain.model.EnergySlot
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class StationSlotsViewModel(private val stationId: String, private val repository: EnergySlotRepository) : ViewModel() {
    private val mutableState = MutableStateFlow(StationSlotsState(isLoading = true))
    val state = mutableState.asStateFlow()
    init { load() }

    fun load() {
        mutableState.value = mutableState.value.copy(isLoading = true, error = null)
        viewModelScope.launch {
            mutableState.value = when (val result = repository.getStationSlots(stationId)) {
                is AppResult.Success -> filteredState(mutableState.value.copy(isLoading = false), result.value)
                is AppResult.Error -> mutableState.value.copy(isLoading = false, error = result.error)
            }
        }
    }

    fun setAvailableOnly(value: Boolean) {
        mutableState.value = filteredState(mutableState.value.copy(availableOnly = value))
    }

    fun setDate(value: String?) {
        mutableState.value = filteredState(mutableState.value.copy(selectedDate = value))
    }

    fun resetFilters() {
        mutableState.value = filteredState(mutableState.value.copy(availableOnly = true, selectedDate = null))
    }

    private fun filteredState(state: StationSlotsState, slots: List<EnergySlot> = state.allSlots): StationSlotsState {
        val dates = SlotFilter.dates(slots)
        val validDate = state.selectedDate?.takeIf { it in dates }
        return state.copy(
            allSlots = slots,
            filteredSlots = SlotFilter.apply(slots, state.availableOnly, validDate),
            availableDates = dates,
            selectedDate = validDate,
        )
    }
}

data class StationSlotsState(
    val isLoading: Boolean = false,
    val error: AppError? = null,
    val allSlots: List<EnergySlot> = emptyList(),
    val filteredSlots: List<EnergySlot> = emptyList(),
    val availableDates: List<String> = emptyList(),
    val availableOnly: Boolean = true,
    val selectedDate: String? = null,
)

