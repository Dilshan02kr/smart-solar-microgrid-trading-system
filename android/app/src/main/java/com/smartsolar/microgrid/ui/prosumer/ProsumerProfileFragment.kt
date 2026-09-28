package com.smartsolar.microgrid.ui.prosumer

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import androidx.appcompat.app.AlertDialog
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.databinding.FragmentProsumerProfileBinding
import com.smartsolar.microgrid.domain.model.ProsumerProfile
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import com.smartsolar.microgrid.ui.common.UiState
import com.smartsolar.microgrid.ui.common.showError
import com.google.android.material.snackbar.Snackbar
import kotlinx.coroutines.launch

class ProsumerProfileFragment : Fragment() {
    private var binding: FragmentProsumerProfileBinding? = null
    private var deactivationDialog: AlertDialog? = null
    private val viewModel: ProsumerProfileViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory {
            ProsumerProfileViewModel(container.prosumerRepository, container.sessionRepository)
        }
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentProsumerProfileBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        binding?.retryButton?.setOnClickListener { viewModel.loadProfile() }
        binding?.editProfileButton?.setOnClickListener {
            findNavController().navigate(R.id.action_prosumerProfileFragment_to_editProsumerProfileFragment)
        }
        binding?.deactivateButton?.setOnClickListener { showDeactivationDialog() }
        binding?.homeButton?.setOnClickListener { findNavController().popBackStack() }

        findNavController().currentBackStackEntry?.savedStateHandle
            ?.getLiveData<Boolean>(PROFILE_UPDATED_KEY)
            ?.observe(viewLifecycleOwner) { updated ->
                if (updated == true) {
                    binding?.root?.let {
                        Snackbar.make(it, R.string.profile_updated, Snackbar.LENGTH_SHORT).show()
                    }
                    findNavController().currentBackStackEntry?.savedStateHandle
                        ?.remove<Boolean>(PROFILE_UPDATED_KEY)
                    viewModel.loadProfile()
                }
            }

        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                launch { viewModel.state.collect(::renderProfile) }
                launch { viewModel.deactivation.collect(::renderDeactivation) }
            }
        }
    }

    private fun renderProfile(state: UiState<ProsumerProfile>) {
        val currentBinding = binding ?: return
        currentBinding.progressIndicator.isVisible = state is UiState.Loading
        currentBinding.errorContainer.isVisible = state is UiState.Error
        currentBinding.contentContainer.isVisible = state is UiState.Content
        if (state is UiState.Error) {
            currentBinding.errorMessage.text = state.error.message
            if (state.error.httpStatus == 401) navigateToLogin()
        }
        if (state is UiState.Content) bindProfile(currentBinding, state.value)
    }

    private fun bindProfile(binding: FragmentProsumerProfileBinding, profile: ProsumerProfile) {
        binding.nameValue.text = getString(R.string.full_name_value, profile.firstName, profile.lastName)
        binding.nicValue.text = profile.nic
        binding.emailValue.text = profile.email
        binding.phoneValue.text = profile.phone
        binding.statusValue.text = profile.accountStatus.name
        binding.createdValue.text = profile.createdAt
        binding.updatedValue.text = profile.updatedAt
    }

    private fun showDeactivationDialog() {
        deactivationDialog = AlertDialog.Builder(requireContext())
            .setTitle(R.string.deactivate_title)
            .setMessage(R.string.deactivate_message)
            .setNegativeButton(R.string.cancel, null)
            .setPositiveButton(R.string.deactivate_confirm, null)
            .create()
            .also { dialog ->
                dialog.setOnShowListener {
                    dialog.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener {
                        dialog.getButton(AlertDialog.BUTTON_POSITIVE).isEnabled = false
                        dialog.getButton(AlertDialog.BUTTON_POSITIVE).setText(R.string.deactivating)
                        dialog.getButton(AlertDialog.BUTTON_NEGATIVE).isEnabled = false
                        viewModel.deactivate()
                    }
                }
                dialog.show()
            }
    }

    private fun renderDeactivation(state: DeactivationState) {
        binding?.deactivateButton?.isEnabled = !state.isLoading
        if (state.error != null) {
            deactivationDialog?.dismiss()
            deactivationDialog = null
            binding?.root?.showError(state.error)
            if (state.error.httpStatus == 401) navigateToLogin()
        }
        if (state.complete && findNavController().currentDestination?.id == R.id.prosumerProfileFragment) {
            deactivationDialog?.dismiss()
            Toast.makeText(requireContext(), R.string.deactivation_complete, Toast.LENGTH_LONG).show()
            findNavController().navigate(R.id.action_prosumerProfileFragment_to_loginFragment)
        }
    }

    private fun navigateToLogin() {
        if (findNavController().currentDestination?.id == R.id.prosumerProfileFragment) {
            findNavController().navigate(R.id.action_prosumerProfileFragment_to_loginFragment)
        }
    }

    override fun onDestroyView() {
        deactivationDialog?.dismiss()
        deactivationDialog = null
        binding = null
        super.onDestroyView()
    }

    companion object {
        const val PROFILE_UPDATED_KEY = "profile_updated"
    }
}
