package com.smartsolar.microgrid.domain.model

enum class AccountStatus {
    ACTIVE,
    PENDING,
    DEACTIVATED;

    companion object {
        fun fromBackendValue(value: String): AccountStatus? = entries.firstOrNull { it.name == value }
    }
}

