package com.smartsolar.microgrid.ui.prosumer

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
import com.smartsolar.microgrid.data.repository.SessionState
import com.smartsolar.microgrid.databinding.FragmentProsumerHomeBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class ProsumerHomeFragment : Fragment() {
    private var binding: FragmentProsumerHomeBinding? = null
    private val viewModel: ProsumerHomeViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { ProsumerHomeViewModel(container.sessionRepository) }
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentProsumerHomeBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        binding?.profileButton?.setOnClickListener {
            findNavController().navigate(R.id.action_prosumerHomeFragment_to_prosumerProfileFragment)
        }
        binding?.logoutButton?.setOnClickListener { viewModel.logout() }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.sessionState.collect(::render)
            }
        }
    }

    private fun render(state: SessionState) {
        val currentBinding = binding ?: return
        when (state) {
            is SessionState.Authenticated -> {
                currentBinding.welcomeMessage.text = getString(
                    R.string.home_welcome,
                    state.user.firstName,
                )
                currentBinding.roleValue.text = state.user.role.name
            }
            SessionState.Unauthenticated -> navigateToLogin()
            is SessionState.Failed -> currentBinding.welcomeMessage.text = state.error.message
            SessionState.Initializing -> Unit
        }
    }

    private fun navigateToLogin() {
        if (findNavController().currentDestination?.id == R.id.prosumerHomeFragment) {
            findNavController().navigate(R.id.action_prosumerHomeFragment_to_loginFragment)
        }
    }

    override fun onDestroyView() {
        binding = null
        super.onDestroyView()
    }
}

