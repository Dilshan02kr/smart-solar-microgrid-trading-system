package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.core.error.AppError

sealed interface AppResult<out T> {
    data class Success<T>(val value: T) : AppResult<T>
    data class Error(val error: AppError) : AppResult<Nothing>
}

