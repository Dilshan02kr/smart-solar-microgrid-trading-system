package com.smartsolar.microgrid.ui.operator

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
import com.smartsolar.microgrid.databinding.FragmentOperatorLoginBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.AuthErrorMessage
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.authErrorMessageFor
import kotlinx.coroutines.launch

class OperatorLoginFragment : Fragment() {
    private var binding: FragmentOperatorLoginBinding? = null
    private val viewModel: OperatorLoginViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { OperatorLoginViewModel(app.sessionRepository) }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentOperatorLoginBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.loginButton?.setOnClickListener { submit() }
        binding?.passwordInput?.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_DONE) submit()
            actionId == EditorInfo.IME_ACTION_DONE
        }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    private fun submit() {
        val current = binding ?: return
        current.emailLayout.error = null
        current.passwordLayout.error = null
        current.errorMessage.isVisible = false
        viewModel.login(
            current.emailInput.text?.toString().orEmpty(),
            current.passwordInput.text?.toString().orEmpty(),
        )
    }

    private fun render(state: OperatorLoginState) {
        val current = binding ?: return
        current.progressIndicator.isVisible = state.isLoading
        current.loginButton.isEnabled = !state.isLoading
        state.validationErrors.forEach {
            when (it) {
                FormField.EMAIL -> current.emailLayout.error = getString(R.string.error_invalid_email)
                FormField.PASSWORD -> current.passwordLayout.error = getString(R.string.error_required)
                else -> Unit
            }
        }
        current.errorMessage.isVisible = state.error != null
        current.errorMessage.text = state.error?.let { error ->
            when (authErrorMessageFor(error.code)) {
                AuthErrorMessage.ACCOUNT_PENDING -> getString(R.string.error_account_pending)
                AuthErrorMessage.ACCOUNT_DEACTIVATED -> getString(R.string.error_account_deactivated)
                AuthErrorMessage.INVALID_CREDENTIALS -> getString(R.string.operator_invalid_credentials)
                AuthErrorMessage.DEFAULT -> error.message
            }
        }
        if (state.loginComplete && findNavController().currentDestination?.id == R.id.operatorLoginFragment) {
            current.passwordInput.text?.clear()
            findNavController().navigate(R.id.action_operatorLoginFragment_to_operatorDashboardFragment)
        }
    }

    override fun onDestroyView() {
        binding?.passwordInput?.text?.clear()
        binding = null
        super.onDestroyView()
    }
}
