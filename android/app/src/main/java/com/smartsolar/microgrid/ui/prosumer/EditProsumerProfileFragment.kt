package com.smartsolar.microgrid.ui.prosumer

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
import com.smartsolar.microgrid.databinding.FragmentEditProsumerProfileBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.FormField
import com.smartsolar.microgrid.ui.common.ProfileInput
import com.smartsolar.microgrid.ui.common.fieldError
import kotlinx.coroutines.launch

class EditProsumerProfileFragment : Fragment() {
    private var binding: FragmentEditProsumerProfileBinding? = null
    private var populatedProfileId: String? = null
    private val viewModel: EditProsumerProfileViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { EditProsumerProfileViewModel(container.prosumerRepository) }
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentEditProsumerProfileBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        binding?.saveButton?.setOnClickListener { save() }
        binding?.phoneInput?.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_DONE) save()
            actionId == EditorInfo.IME_ACTION_DONE
        }
        binding?.retryButton?.setOnClickListener { viewModel.loadProfile() }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    private fun save() {
        val currentBinding = binding ?: return
        clearErrors(currentBinding)
        viewModel.save(
            ProfileInput(
                firstName = currentBinding.firstNameInput.text?.toString().orEmpty(),
                lastName = currentBinding.lastNameInput.text?.toString().orEmpty(),
                email = currentBinding.emailInput.text?.toString().orEmpty(),
                phone = currentBinding.phoneInput.text?.toString().orEmpty(),
            ),
        )
    }

    private fun render(state: EditProfileUiState) {
        val currentBinding = binding ?: return
        currentBinding.progressIndicator.isVisible = state.isLoadingProfile || state.isSaving
        currentBinding.formContainer.isVisible = !state.isLoadingProfile && state.profile != null
        currentBinding.errorContainer.isVisible = !state.isLoadingProfile && state.profile == null && state.error != null
        currentBinding.saveButton.isEnabled = !state.isSaving

        state.profile?.let { profile ->
            if (populatedProfileId != profile.userId) {
                populatedProfileId = profile.userId
                currentBinding.firstNameInput.setText(profile.firstName)
                currentBinding.lastNameInput.setText(profile.lastName)
                currentBinding.emailInput.setText(profile.email)
                currentBinding.phoneInput.setText(profile.phone)
            }
        }
        applyValidationErrors(currentBinding, state.validationErrors)
        state.error?.let { error ->
            currentBinding.errorMessage.text = error.message
            currentBinding.rootError.text = error.message
            if (state.profile != null) currentBinding.rootError.isVisible = true
            currentBinding.firstNameLayout.error = error.fieldError("FirstName")
            currentBinding.lastNameLayout.error = error.fieldError("LastName")
            currentBinding.emailLayout.error = error.fieldError("Email")
            currentBinding.phoneLayout.error = error.fieldError("Phone")
            if (error.httpStatus == 401) navigateToLogin()
        }
        if (state.saveComplete && findNavController().currentDestination?.id == R.id.editProsumerProfileFragment) {
            findNavController().previousBackStackEntry?.savedStateHandle
                ?.set(ProsumerProfileFragment.PROFILE_UPDATED_KEY, true)
            findNavController().popBackStack()
        }
    }

    private fun applyValidationErrors(
        binding: FragmentEditProsumerProfileBinding,
        errors: Set<FormField>,
    ) {
        errors.forEach { field ->
            val message = if (field == FormField.EMAIL) {
                getString(R.string.error_invalid_email)
            } else {
                getString(R.string.error_required)
            }
            when (field) {
                FormField.FIRST_NAME -> binding.firstNameLayout.error = message
                FormField.LAST_NAME -> binding.lastNameLayout.error = message
                FormField.EMAIL -> binding.emailLayout.error = message
                FormField.PHONE -> binding.phoneLayout.error = message
                else -> Unit
            }
        }
    }

    private fun clearErrors(binding: FragmentEditProsumerProfileBinding) {
        listOf(
            binding.firstNameLayout,
            binding.lastNameLayout,
            binding.emailLayout,
            binding.phoneLayout,
        ).forEach { it.error = null }
        binding.rootError.isVisible = false
    }

    private fun navigateToLogin() {
        if (findNavController().currentDestination?.id == R.id.editProsumerProfileFragment) {
            findNavController().navigate(R.id.action_editProsumerProfileFragment_to_loginSelectionFragment)
        }
    }

    override fun onDestroyView() {
        binding = null
        super.onDestroyView()
    }
}
