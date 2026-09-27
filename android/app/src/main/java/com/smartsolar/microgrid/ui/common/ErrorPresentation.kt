package com.smartsolar.microgrid.ui.common

import android.view.View
import com.google.android.material.snackbar.Snackbar
import com.smartsolar.microgrid.core.error.AppError

fun View.showError(error: AppError) {
    Snackbar.make(this, error.message, Snackbar.LENGTH_LONG).show()
}

fun AppError.fieldError(fieldName: String): String? = fieldErrors
    ?.entries
    ?.firstOrNull { (key, _) -> key.substringAfterLast('.').equals(fieldName, ignoreCase = true) }
    ?.value
    ?.firstOrNull()
