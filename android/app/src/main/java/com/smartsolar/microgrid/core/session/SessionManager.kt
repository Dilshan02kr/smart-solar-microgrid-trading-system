package com.smartsolar.microgrid.core.session

import android.content.Context
import androidx.core.content.edit

class SessionManager(context: Context) {
    private val preferences = context.applicationContext.getSharedPreferences(
        PREFERENCES_NAME,
        Context.MODE_PRIVATE,
    )

    fun saveToken(token: String) {
        require(token.isNotBlank()) { "Token must not be blank." }
        preferences.edit { putString(KEY_ACCESS_TOKEN, token) }
    }

    fun getToken(): String? = preferences.getString(KEY_ACCESS_TOKEN, null)
        ?.takeIf(String::isNotBlank)

    fun clearToken() {
        preferences.edit { remove(KEY_ACCESS_TOKEN) }
    }

    companion object {
        private const val PREFERENCES_NAME = "authenticated_session"
        private const val KEY_ACCESS_TOKEN = "access_token"
    }
}
