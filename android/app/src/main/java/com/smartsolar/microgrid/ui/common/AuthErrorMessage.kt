package com.smartsolar.microgrid.ui.common

enum class AuthErrorMessage {
    ACCOUNT_PENDING,
    ACCOUNT_DEACTIVATED,
    INVALID_CREDENTIALS,
    DEFAULT,
}

fun authErrorMessageFor(code: String): AuthErrorMessage = when (code) {
    "ACCOUNT_PENDING" -> AuthErrorMessage.ACCOUNT_PENDING
    "ACCOUNT_DEACTIVATED" -> AuthErrorMessage.ACCOUNT_DEACTIVATED
    "INVALID_CREDENTIALS" -> AuthErrorMessage.INVALID_CREDENTIALS
    else -> AuthErrorMessage.DEFAULT
}

