package com.smartsolar.microgrid.data.remote.dto

data class ProsumerLoginRequestDto(
    val nic: String,
    val password: String,
)

data class AuthenticationResponseDto(
    val token: String,
    val expiresAtUtc: String,
    val user: AuthenticatedUserDto,
)

data class RegisterProsumerRequestDto(
    val nic: String,
    val firstName: String,
    val lastName: String,
    val email: String,
    val phone: String,
    val password: String,
    val confirmPassword: String,
)

data class ProsumerRegistrationResponseDto(
    val userId: String,
    val nic: String,
    val firstName: String,
    val lastName: String,
    val email: String,
    val phone: String,
    val role: String,
    val accountStatus: String,
)

