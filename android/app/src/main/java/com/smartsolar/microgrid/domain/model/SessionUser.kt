package com.smartsolar.microgrid.domain.model

data class SessionUser(
    val userId: String,
    val firstName: String,
    val lastName: String,
    val role: UserRole,
    val accountStatus: AccountStatus,
    val nic: String?,
    val assignedMicrogridNodeId: String?,
)

