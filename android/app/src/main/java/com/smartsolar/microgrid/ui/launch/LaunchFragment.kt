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
import kotlinx.coroutines.launch

class LaunchFragment : Fragment() {
    private var binding: FragmentLaunchBinding? = null

    private val viewModel: LaunchViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        LaunchViewModelFactory(container.sessionRepository)
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
                viewModel.state.collect { state ->
                    if (state == LaunchUiState.Ready && findNavController().currentDestination?.id == R.id.launchFragment) {
                        findNavController().navigate(R.id.action_launchFragment_to_foundationFragment)
                    }
                }
            }
        }
    }

    override fun onDestroyView() {
        binding = null
        super.onDestroyView()
    }
}

