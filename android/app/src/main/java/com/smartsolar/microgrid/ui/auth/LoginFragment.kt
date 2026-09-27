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
import com.smartsolar.microgrid.databinding.FragmentLoginBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.AuthErrorMessage
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.authErrorMessageFor
import kotlinx.coroutines.launch

class LoginFragment : Fragment() {
    private var binding: FragmentLoginBinding? = null
    private val viewModel: LoginViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { LoginViewModel(container.sessionRepository) }
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentLoginBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        binding?.loginButton?.setOnClickListener { submit() }
        binding?.passwordInput?.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_DONE) submit()
            actionId == EditorInfo.IME_ACTION_DONE
        }
        binding?.createAccountButton?.setOnClickListener {
            findNavController().navigate(R.id.action_loginFragment_to_registerFragment)
        }

        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    private fun submit() {
        val currentBinding = binding ?: return
        clearErrors(currentBinding)
        viewModel.login(
            currentBinding.nicInput.text?.toString().orEmpty(),
            currentBinding.passwordInput.text?.toString().orEmpty(),
        )
    }

    private fun render(state: LoginUiState) {
        val currentBinding = binding ?: return
        currentBinding.loginButton.isEnabled = !state.isLoading
        currentBinding.createAccountButton.isEnabled = !state.isLoading
        currentBinding.progressIndicator.isVisible = state.isLoading

        state.validationErrors.forEach { field ->
            when (field) {
                FormField.NIC -> currentBinding.nicLayout.error = getString(R.string.error_required)
                FormField.PASSWORD -> currentBinding.passwordLayout.error = getString(R.string.error_required)
                else -> Unit
            }
        }

        currentBinding.errorMessage.isVisible = state.error != null
        currentBinding.errorMessage.text = state.error?.let { error ->
            when (authErrorMessageFor(error.code)) {
                AuthErrorMessage.ACCOUNT_PENDING -> getString(R.string.error_account_pending)
                AuthErrorMessage.ACCOUNT_DEACTIVATED -> getString(R.string.error_account_deactivated)
                AuthErrorMessage.INVALID_CREDENTIALS -> getString(R.string.error_invalid_credentials)
                AuthErrorMessage.DEFAULT -> error.message
            }
        }

        if (state.loginComplete && findNavController().currentDestination?.id == R.id.loginFragment) {
            currentBinding.passwordInput.text?.clear()
            findNavController().navigate(R.id.action_loginFragment_to_prosumerHomeFragment)
        }
    }

    private fun clearErrors(currentBinding: FragmentLoginBinding) {
        currentBinding.nicLayout.error = null
        currentBinding.passwordLayout.error = null
        currentBinding.errorMessage.isVisible = false
    }

    override fun onDestroyView() {
        binding?.passwordInput?.text?.clear()
        binding = null
        super.onDestroyView()
    }
}
