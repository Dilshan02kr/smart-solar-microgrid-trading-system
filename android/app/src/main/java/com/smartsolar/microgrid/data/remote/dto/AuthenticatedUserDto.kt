package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.AccountStatus
import com.smartsolar.microgrid.domain.model.SessionUser
import com.smartsolar.microgrid.domain.model.UserRole

data class AuthenticatedUserDto(
    val userId: String,
    val firstName: String,
    val lastName: String,
    val role: String,
    val accountStatus: String,
    val nic: String?,
    val assignedMicrogridNodeId: String?,
) {
    fun toDomainOrNull(): SessionUser? {
        val parsedRole = UserRole.fromBackendValue(role) ?: return null
        val parsedStatus = AccountStatus.fromBackendValue(accountStatus) ?: return null
        return SessionUser(
            userId = userId,
            firstName = firstName,
            lastName = lastName,
            role = parsedRole,
            accountStatus = parsedStatus,
            nic = nic,
            assignedMicrogridNodeId = assignedMicrogridNodeId,
        )
    }
}

