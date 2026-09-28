package com.smartsolar.microgrid.domain.model

enum class UserRole {
    BACKOFFICE,
    GRID_OPERATOR,
    PROSUMER;

    companion object {
        fun fromBackendValue(value: String): UserRole? = entries.firstOrNull { it.name == value }
    }
}

