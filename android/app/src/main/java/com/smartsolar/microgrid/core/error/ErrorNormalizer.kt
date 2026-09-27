package com.smartsolar.microgrid.core.error

import com.google.gson.Gson
import com.google.gson.JsonParseException
import com.smartsolar.microgrid.data.remote.dto.ApiErrorResponseDto
import retrofit2.HttpException
import java.io.IOException
import java.net.SocketTimeoutException

class ErrorNormalizer(
    private val gson: Gson = Gson(),
) {
    fun normalize(throwable: Throwable): AppError {
        if (throwable is SocketTimeoutException) {
            return AppError("The request timed out. Please try again.", "REQUEST_TIMEOUT")
        }
        if (throwable is IOException) {
            return AppError(
                "The service is currently unreachable. Check your connection and try again.",
                "NETWORK_UNAVAILABLE",
            )
        }
        if (throwable is HttpException) {
            return fromHttpException(throwable)
        }
        return AppError("An unexpected error occurred. Please try again.")
    }

    private fun fromHttpException(exception: HttpException): AppError {
        val status = exception.code()
        val parsed = try {
            exception.response()?.errorBody()?.charStream()?.use { reader ->
                gson.fromJson(reader, ApiErrorResponseDto::class.java)
            }
        } catch (_: IOException) {
            null
        } catch (_: JsonParseException) {
            null
        }

        if (!parsed?.code.isNullOrBlank() && !parsed?.message.isNullOrBlank()) {
            return AppError(
                message = parsed!!.message,
                code = parsed.code,
                httpStatus = status,
                fieldErrors = parsed.errors,
            )
        }

        return when (status) {
            401 -> AppError("Authentication is required.", "AUTHENTICATION_REQUIRED", status)
            403 -> AppError(
                "You do not have permission to perform this action.",
                "ACCESS_DENIED",
                status,
            )
            else -> AppError(
                "The server returned an unexpected response. Please try again.",
                "UNEXPECTED_RESPONSE",
                status,
            )
        }
    }
}

