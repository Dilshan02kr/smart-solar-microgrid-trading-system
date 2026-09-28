package com.smartsolar.microgrid.core.error

data class AppError(
    val message: String,
    val code: String = "UNEXPECTED_ERROR",
    val httpStatus: Int? = null,
    val fieldErrors: Map<String, List<String>>? = null,
)

