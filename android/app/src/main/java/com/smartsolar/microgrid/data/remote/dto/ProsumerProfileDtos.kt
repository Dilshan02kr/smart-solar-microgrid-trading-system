package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.AccountStatus
import com.smartsolar.microgrid.domain.model.ProsumerProfile

data class ProsumerProfileDto(
    val userId: String,
    val nic: String,
    val firstName: String,
    val lastName: String,
    val email: String,
    val phone: String,
    val accountStatus: String,
    val createdAt: String,
    val updatedAt: String,
) {
    fun toDomainOrNull(): ProsumerProfile? {
        val parsedStatus = AccountStatus.fromBackendValue(accountStatus) ?: return null
        return ProsumerProfile(
            userId = userId,
            nic = nic,
            firstName = firstName,
            lastName = lastName,
            email = email,
            phone = phone,
            accountStatus = parsedStatus,
            createdAt = createdAt,
            updatedAt = updatedAt,
        )
    }
}

data class UpdateProsumerProfileRequestDto(
    val firstName: String,
    val lastName: String,
    val email: String,
    val phone: String,
)

