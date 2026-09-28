package com.smartsolar.microgrid.ui.common

import com.smartsolar.microgrid.core.error.AppError

sealed interface UiState<out T> {
    data object Loading : UiState<Nothing>
    data class Error(val error: AppError) : UiState<Nothing>
    data object Empty : UiState<Nothing>
    data class Content<T>(val value: T) : UiState<T>
}

