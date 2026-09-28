package com.smartsolar.microgrid.ui.common

enum class FormField {
    NIC,
    FIRST_NAME,
    LAST_NAME,
    EMAIL,
    PHONE,
    PASSWORD,
    CONFIRM_PASSWORD,
}

data class LoginInput(
    val nic: String,
    val password: String,
)

data class OperatorLoginInput(
    val email: String,
    val password: String,
)

data class RegistrationInput(
    val nic: String,
    val firstName: String,
    val lastName: String,
    val email: String,
    val phone: String,
    val password: String,
    val confirmPassword: String,
)

data class ProfileInput(
    val firstName: String,
    val lastName: String,
    val email: String,
    val phone: String,
)

object FormValidator {
    private val emailPattern = Regex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")

    fun validateLogin(input: LoginInput): Set<FormField> = buildSet {
        if (input.nic.isBlank()) add(FormField.NIC)
        if (input.password.isBlank()) add(FormField.PASSWORD)
    }

    fun validateOperatorLogin(input: OperatorLoginInput): Set<FormField> = buildSet {
        if (!emailPattern.matches(input.email.trim())) add(FormField.EMAIL)
        if (input.password.isBlank()) add(FormField.PASSWORD)
    }

    fun validateRegistration(input: RegistrationInput): Set<FormField> = buildSet {
        if (input.nic.isBlank()) add(FormField.NIC)
        if (input.firstName.isBlank()) add(FormField.FIRST_NAME)
        if (input.lastName.isBlank()) add(FormField.LAST_NAME)
        if (!emailPattern.matches(input.email.trim())) add(FormField.EMAIL)
        if (input.phone.isBlank()) add(FormField.PHONE)
        if (input.password.isBlank()) add(FormField.PASSWORD)
        if (input.confirmPassword.isBlank() || input.password != input.confirmPassword) {
            add(FormField.CONFIRM_PASSWORD)
        }
    }

    fun validateProfile(input: ProfileInput): Set<FormField> = buildSet {
        if (input.firstName.isBlank()) add(FormField.FIRST_NAME)
        if (input.lastName.isBlank()) add(FormField.LAST_NAME)
        if (!emailPattern.matches(input.email.trim())) add(FormField.EMAIL)
        if (input.phone.isBlank()) add(FormField.PHONE)
    }
}

