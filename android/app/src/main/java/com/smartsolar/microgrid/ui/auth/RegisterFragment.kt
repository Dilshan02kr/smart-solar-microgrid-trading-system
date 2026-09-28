package com.smartsolar.microgrid.ui.auth

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.view.inputmethod.EditorInfo
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.databinding.FragmentRegisterBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.RegistrationInput
import com.smartsolar.microgrid.ui.common.fieldError
import kotlinx.coroutines.launch

class RegisterFragment : Fragment() {
    private var binding: FragmentRegisterBinding? = null
    private val viewModel: RegisterViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { RegisterViewModel(container.prosumerRepository) }
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentRegisterBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        binding?.registerButton?.setOnClickListener { submit() }
        binding?.confirmPasswordInput?.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_DONE) submit()
            actionId == EditorInfo.IME_ACTION_DONE
        }
        binding?.returnToLoginButton?.setOnClickListener { findNavController().popBackStack() }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    private fun submit() {
        val currentBinding = binding ?: return
        clearErrors(currentBinding)
        viewModel.register(
            RegistrationInput(
                nic = currentBinding.nicInput.text?.toString().orEmpty(),
                firstName = currentBinding.firstNameInput.text?.toString().orEmpty(),
                lastName = currentBinding.lastNameInput.text?.toString().orEmpty(),
                email = currentBinding.emailInput.text?.toString().orEmpty(),
                phone = currentBinding.phoneInput.text?.toString().orEmpty(),
                password = currentBinding.passwordInput.text?.toString().orEmpty(),
                confirmPassword = currentBinding.confirmPasswordInput.text?.toString().orEmpty(),
            ),
        )
    }

    private fun render(state: RegisterUiState) {
        val currentBinding = binding ?: return
        currentBinding.registerButton.isEnabled = !state.isLoading
        currentBinding.progressIndicator.isVisible = state.isLoading
        currentBinding.formContainer.isVisible = !state.registrationComplete
        currentBinding.successContainer.isVisible = state.registrationComplete
        if (state.registrationComplete) {
            currentBinding.passwordInput.text?.clear()
            currentBinding.confirmPasswordInput.text?.clear()
        }

        applyValidationErrors(currentBinding, state.validationErrors)
        state.error?.let { error ->
            currentBinding.formError.isVisible = true
            currentBinding.formError.text = error.message
            currentBinding.nicLayout.error = error.fieldError("Nic") ?: currentBinding.nicLayout.error
            currentBinding.firstNameLayout.error = error.fieldError("FirstName") ?: currentBinding.firstNameLayout.error
            currentBinding.lastNameLayout.error = error.fieldError("LastName") ?: currentBinding.lastNameLayout.error
            currentBinding.emailLayout.error = error.fieldError("Email") ?: currentBinding.emailLayout.error
            currentBinding.phoneLayout.error = error.fieldError("Phone") ?: currentBinding.phoneLayout.error
            currentBinding.passwordLayout.error = error.fieldError("Password") ?: currentBinding.passwordLayout.error
            currentBinding.confirmPasswordLayout.error =
                error.fieldError("ConfirmPassword") ?: currentBinding.confirmPasswordLayout.error
        }
    }

    private fun applyValidationErrors(binding: FragmentRegisterBinding, errors: Set<FormField>) {
        errors.forEach { field ->
            val message = when (field) {
                FormField.EMAIL -> getString(R.string.error_invalid_email)
                FormField.CONFIRM_PASSWORD -> getString(R.string.error_passwords_must_match)
                else -> getString(R.string.error_required)
            }
            when (field) {
                FormField.NIC -> binding.nicLayout.error = message
                FormField.FIRST_NAME -> binding.firstNameLayout.error = message
                FormField.LAST_NAME -> binding.lastNameLayout.error = message
                FormField.EMAIL -> binding.emailLayout.error = message
                FormField.PHONE -> binding.phoneLayout.error = message
                FormField.PASSWORD -> binding.passwordLayout.error = message
                FormField.CONFIRM_PASSWORD -> binding.confirmPasswordLayout.error = message
            }
        }
    }

    private fun clearErrors(binding: FragmentRegisterBinding) {
        listOf(
            binding.nicLayout,
            binding.firstNameLayout,
            binding.lastNameLayout,
            binding.emailLayout,
            binding.phoneLayout,
            binding.passwordLayout,
            binding.confirmPasswordLayout,
        ).forEach { it.error = null }
        binding.formError.isVisible = false
    }

    override fun onDestroyView() {
        binding?.passwordInput?.text?.clear()
        binding?.confirmPasswordInput?.text?.clear()
        binding = null
        super.onDestroyView()
    }
}
