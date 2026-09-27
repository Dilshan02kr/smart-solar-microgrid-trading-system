package com.smartsolar.microgrid.ui.common

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.data.repository.SessionState
import com.smartsolar.microgrid.databinding.FragmentFoundationBinding
import kotlinx.coroutines.launch

class FoundationFragment : Fragment() {
    private var binding: FragmentFoundationBinding? = null

    private val viewModel: FoundationViewModel by viewModels {
        val container = (requireActivity().application as SmartSolarApplication).appContainer
        FoundationViewModelFactory(container.sessionRepository)
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentFoundationBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.sessionState.collect(::render)
            }
        }
    }

    private fun render(state: SessionState) {
        val currentBinding = binding ?: return
        currentBinding.progressIndicator.visibility =
            if (state is SessionState.Initializing) View.VISIBLE else View.GONE
        currentBinding.stateMessage.text = when (state) {
            SessionState.Initializing -> getString(R.string.session_checking)
            SessionState.Unauthenticated -> getString(R.string.session_unauthenticated)
            is SessionState.Authenticated -> getString(
                R.string.session_authenticated,
                state.user.firstName,
                state.user.role.name,
            )
            is SessionState.Failed -> state.error.message
        }
    }

    override fun onDestroyView() {
        binding = null
        super.onDestroyView()
    }
}

