package com.smartsolar.microgrid.data.remote.dto

data class ApiErrorResponseDto(
    val code: String,
    val message: String,
    val errors: Map<String, List<String>>? = null,
)

