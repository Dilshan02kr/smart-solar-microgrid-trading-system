package com.smartsolar.microgrid.ui.auth

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.fragment.app.Fragment
import androidx.navigation.fragment.findNavController
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.databinding.FragmentLoginSelectionBinding

class LoginSelectionFragment : Fragment() {
    private var binding: FragmentLoginSelectionBinding? = null

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?,
    ): View = FragmentLoginSelectionBinding.inflate(inflater, container, false)
        .also { binding = it }
        .root

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        binding?.prosumerLoginCard?.setOnClickListener {
            findNavController().navigate(R.id.action_loginSelectionFragment_to_loginFragment)
        }
        binding?.operatorLoginCard?.setOnClickListener {
            findNavController().navigate(R.id.action_loginSelectionFragment_to_operatorLoginFragment)
        }
    }

    override fun onDestroyView() {
        binding = null
        super.onDestroyView()
    }
}
