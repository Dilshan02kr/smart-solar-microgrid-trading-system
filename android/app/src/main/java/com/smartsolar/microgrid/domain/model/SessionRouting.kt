package com.smartsolar.microgrid.domain.model

enum class SessionDestination {
    PROSUMER_HOME,
    OPERATOR_DASHBOARD,
    UNSUPPORTED,
}

object SessionRouting {
    fun destination(user: SessionUser): SessionDestination = when {
        user.accountStatus != AccountStatus.ACTIVE -> SessionDestination.UNSUPPORTED
        user.role == UserRole.PROSUMER -> SessionDestination.PROSUMER_HOME
        user.role == UserRole.GRID_OPERATOR -> SessionDestination.OPERATOR_DASHBOARD
        else -> SessionDestination.UNSUPPORTED
    }
}
