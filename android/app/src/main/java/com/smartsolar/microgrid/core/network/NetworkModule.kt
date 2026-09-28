package com.smartsolar.microgrid.core.network

import com.smartsolar.microgrid.BuildConfig
import com.smartsolar.microgrid.core.session.SessionManager
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object NetworkModule {
    fun createRetrofit(sessionManager: SessionManager): Retrofit {
        val baseUrl = requireNotNull(BuildConfig.API_BASE_URL.takeIf { it.endsWith('/') }) {
            "API_BASE_URL must end with a slash."
        }

        return Retrofit.Builder()
            .baseUrl(baseUrl)
            .client(createHttpClient(sessionManager))
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    private fun createHttpClient(sessionManager: SessionManager): OkHttpClient =
        OkHttpClient.Builder()
            .connectTimeout(20, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .addInterceptor(AuthInterceptor(sessionManager))
            .addInterceptor(UnauthorizedInterceptor(sessionManager))
            .apply {
                if (BuildConfig.DEBUG) {
                    addInterceptor(
                        HttpLoggingInterceptor().apply {
                            redactHeader("Authorization")
                            level = HttpLoggingInterceptor.Level.BASIC
                        },
                    )
                }
            }
            .build()
}

