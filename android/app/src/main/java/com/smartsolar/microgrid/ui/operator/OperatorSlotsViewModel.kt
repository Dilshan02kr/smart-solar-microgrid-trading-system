package com.smartsolar.microgrid.ui.operator

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.smartsolar.microgrid.core.error.AppError
import com.smartsolar.microgrid.core.util.AppResult
import com.smartsolar.microgrid.core.util.OperatorSlotPresentation
import com.smartsolar.microgrid.data.repository.OperatorRepository
import com.smartsolar.microgrid.domain.model.EnergySlot
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class OperatorSlotsState(
    val isLoading: Boolean = false,
    val slots: List<EnergySlot> = emptyList(),
    val mutatingSlotId: String? = null,
    val error: AppError? = null,
    val mutationError: AppError? = null,
)

class OperatorSlotsViewModel(private val repository: OperatorRepository) : ViewModel() {
    private val mutableState = MutableStateFlow(OperatorSlotsState())
    val state = mutableState.asStateFlow()

    init { load() }

    fun load() {
        if (mutableState.value.isLoading || mutableState.value.mutatingSlotId != null) return
        mutableState.value = mutableState.value.copy(isLoading = true, error = null)
        viewModelScope.launch {
            mutableState.value = when (val result = repository.getSlots()) {
                is AppResult.Success -> mutableState.value.copy(
                    isLoading = false,
                    slots = OperatorSlotPresentation.chronological(result.value),
                )
                is AppResult.Error -> mutableState.value.copy(isLoading = false, error = result.error)
            }
        }
    }

    fun setAvailability(slot: EnergySlot, isAvailable: Boolean) {
        if (mutableState.value.mutatingSlotId != null) return
        mutableState.value = mutableState.value.copy(mutatingSlotId = slot.id, mutationError = null)
        viewModelScope.launch {
            when (val result = repository.updateSlotAvailability(slot.id, isAvailable)) {
                is AppResult.Success -> {
                    val updated = mutableState.value.slots.map { if (it.id == result.value.id) result.value else it }
                    mutableState.value = mutableState.value.copy(
                        slots = OperatorSlotPresentation.chronological(updated),
                        mutatingSlotId = null,
                    )
                    load()
                }
                is AppResult.Error -> mutableState.value = mutableState.value.copy(
                    mutatingSlotId = null,
                    mutationError = result.error,
                )
            }
        }
    }

    fun clearMutationError() {
        mutableState.value = mutableState.value.copy(mutationError = null)
    }
}
