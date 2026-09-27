package com.smartsolar.microgrid.domain.model

data class ProsumerProfile(
    val userId: String,
    val nic: String,
    val firstName: String,
    val lastName: String,
    val email: String,
    val phone: String,
    val accountStatus: AccountStatus,
    val createdAt: String,
    val updatedAt: String,
)

