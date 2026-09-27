package com.smartsolar.microgrid.ui.launch

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.databinding.FragmentLaunchBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class LaunchFragment : Fragment() {
    private var binding: FragmentLaunchBinding? = null

    private val viewModel: LaunchViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { LaunchViewModel(container.sessionRepository) }
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentLaunchBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
        binding?.retryButton?.setOnClickListener { viewModel.retry() }
        binding?.continueButton?.setOnClickListener { viewModel.continueToLogin() }
    }

    private fun render(state: LaunchUiState) {
        val currentBinding = binding ?: return
        currentBinding.progressIndicator.visibility =
            if (state is LaunchUiState.Loading) View.VISIBLE else View.GONE
        currentBinding.errorMessage.visibility =
            if (state is LaunchUiState.Error) View.VISIBLE else View.GONE
        currentBinding.retryButton.visibility = currentBinding.errorMessage.visibility
        currentBinding.continueButton.visibility = currentBinding.errorMessage.visibility
        if (state is LaunchUiState.Error) currentBinding.errorMessage.text = state.error.message

        if (findNavController().currentDestination?.id != R.id.launchFragment) return
        when (state) {
            LaunchUiState.NavigateToLogin ->
                findNavController().navigate(R.id.action_launchFragment_to_loginFragment)
            LaunchUiState.NavigateToProsumerHome ->
                findNavController().navigate(R.id.action_launchFragment_to_prosumerHomeFragment)
            LaunchUiState.NavigateToOperatorDashboard ->
                findNavController().navigate(R.id.action_launchFragment_to_operatorDashboardFragment)
            else -> Unit
        }
    }

    override fun onDestroyView() {
        binding = null
        super.onDestroyView()
    }
}

