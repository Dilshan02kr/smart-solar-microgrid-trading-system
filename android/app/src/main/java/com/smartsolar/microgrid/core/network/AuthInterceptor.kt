package com.smartsolar.microgrid.core.network

import com.smartsolar.microgrid.core.session.SessionManager
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(
    private val sessionManager: SessionManager,
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val token = sessionManager.getToken()
        val request = if (token == null) {
            chain.request()
        } else {
            chain.request().newBuilder()
                .header(AUTHORIZATION_HEADER, "Bearer $token")
                .build()
        }
        return chain.proceed(request)
    }

    private companion object {
        const val AUTHORIZATION_HEADER = "Authorization"
    }
}

