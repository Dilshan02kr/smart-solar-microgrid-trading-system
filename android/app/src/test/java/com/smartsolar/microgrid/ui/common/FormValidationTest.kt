package com.smartsolar.microgrid.ui.common

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class FormValidationTest {
    @Test
    fun `valid registration passes client validation`() {
        val result = FormValidator.validateRegistration(
            RegistrationInput(
                nic = "200012345678",
                firstName = "Asha",
                lastName = "Perera",
                email = "asha@example.test",
                phone = "0712345678",
                password = "secret",
                confirmPassword = "secret",
            ),
        )

        assertTrue(result.isEmpty())
    }

    @Test
    fun `registration detects required invalid and mismatched fields`() {
        val result = FormValidator.validateRegistration(
            RegistrationInput("", "", "", "invalid", "", "one", "two"),
        )

        assertEquals(
            setOf(
                FormField.NIC,
                FormField.FIRST_NAME,
                FormField.LAST_NAME,
                FormField.EMAIL,
                FormField.PHONE,
                FormField.CONFIRM_PASSWORD,
            ),
            result,
        )
    }

    @Test
    fun `login requires nic and password`() {
        assertEquals(
            setOf(FormField.NIC, FormField.PASSWORD),
            FormValidator.validateLogin(LoginInput(" ", "")),
        )
    }

    @Test
    fun `authentication error codes map without changing unknown codes`() {
        assertEquals(AuthErrorMessage.ACCOUNT_PENDING, authErrorMessageFor("ACCOUNT_PENDING"))
        assertEquals(AuthErrorMessage.ACCOUNT_DEACTIVATED, authErrorMessageFor("ACCOUNT_DEACTIVATED"))
        assertEquals(AuthErrorMessage.INVALID_CREDENTIALS, authErrorMessageFor("INVALID_CREDENTIALS"))
        assertEquals(AuthErrorMessage.DEFAULT, authErrorMessageFor("SOMETHING_NEW"))
    }
}
