package com.smartsolar.microgrid.ui.operator

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import android.os.Bundle
import android.provider.Settings
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AlertDialog
import androidx.core.content.ContextCompat
import androidx.core.os.bundleOf
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.google.zxing.BarcodeFormat
import com.journeyapps.barcodescanner.BarcodeCallback
import com.journeyapps.barcodescanner.BarcodeResult
import com.journeyapps.barcodescanner.DefaultDecoderFactory
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.core.util.OperatorErrorMessage
import com.smartsolar.microgrid.core.util.OperatorPresentation
import com.smartsolar.microgrid.databinding.FragmentQrScannerBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class QrScannerFragment : Fragment() {
    private var binding: FragmentQrScannerBinding? = null
    private var hasRequestedCamera = false
    private var permissionDialog: AlertDialog? = null
    private val viewModel: QrScannerViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { QrScannerViewModel(app.operatorRepository) }
    }
    private val permissionLauncher = registerForActivityResult(
        ActivityResultContracts.RequestPermission(),
    ) { granted ->
        if (granted) startScanning() else showPermissionState()
    }
    private val callback = object : BarcodeCallback {
        override fun barcodeResult(result: BarcodeResult) {
            binding?.barcodeView?.pause()
            viewModel.verify(result.text.orEmpty())
        }
    }

    override fun onCreate(state: Bundle?) {
        super.onCreate(state)
        hasRequestedCamera = state?.getBoolean(KEY_REQUESTED_CAMERA) ?: false
    }

    override fun onSaveInstanceState(outState: Bundle) {
        outState.putBoolean(KEY_REQUESTED_CAMERA, hasRequestedCamera)
        super.onSaveInstanceState(outState)
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentQrScannerBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.barcodeView?.barcodeView?.decoderFactory = DefaultDecoderFactory(
            listOf(BarcodeFormat.QR_CODE),
        )
        binding?.permissionButton?.setOnClickListener { requestCameraAccess() }
        binding?.settingsButton?.setOnClickListener { openSettings() }
        binding?.scanAgainButton?.setOnClickListener {
            viewModel.scanAgain()
            startScanning()
        }
        binding?.manualVerifyButton?.setOnClickListener {
            binding?.barcodeView?.pause()
            viewModel.verify(binding?.manualReferenceInput?.text?.toString().orEmpty())
        }
        binding?.cancelButton?.setOnClickListener { findNavController().navigateUp() }
        findNavController().currentBackStackEntry?.savedStateHandle
            ?.getLiveData<Boolean>(SCAN_AGAIN_KEY)
            ?.observe(viewLifecycleOwner) {
                if (it == true) {
                    findNavController().currentBackStackEntry?.savedStateHandle?.remove<Boolean>(SCAN_AGAIN_KEY)
                    viewModel.scanAgain()
                    startScanning()
                }
            }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    override fun onResume() {
        super.onResume()
        if (viewModel.state.value.error == null && !viewModel.state.value.isVerifying) startScanning()
    }

    override fun onPause() {
        binding?.barcodeView?.pause()
        super.onPause()
    }

    private fun startScanning() {
        val current = binding ?: return
        if (!hasCameraPermission()) {
            showPermissionState()
            return
        }
        current.permissionContainer.isVisible = false
        current.scannerContainer.isVisible = true
        current.verificationContainer.isVisible = false
        current.barcodeView.resume()
        current.barcodeView.decodeSingle(callback)
    }

    private fun requestCameraAccess() {
        if (hasCameraPermission()) {
            startScanning()
        } else if (shouldShowRequestPermissionRationale(Manifest.permission.CAMERA)) {
            permissionDialog?.dismiss()
            permissionDialog = AlertDialog.Builder(requireContext())
                .setTitle(R.string.camera_permission_title)
                .setMessage(R.string.camera_permission_rationale)
                .setNegativeButton(R.string.cancel, null)
                .setPositiveButton(R.string.grant_camera_permission) { _, _ -> launchPermissionRequest() }
                .show()
        } else if (hasRequestedCamera) {
            showPermissionState(permanentlyDenied = true)
        } else {
            launchPermissionRequest()
        }
    }

    private fun launchPermissionRequest() {
        hasRequestedCamera = true
        permissionLauncher.launch(Manifest.permission.CAMERA)
    }

    private fun showPermissionState(permanentlyDenied: Boolean = false) {
        val current = binding ?: return
        current.barcodeView.pause()
        current.scannerContainer.isVisible = false
        current.verificationContainer.isVisible = false
        current.permissionContainer.isVisible = true
        val deniedPermanently = permanentlyDenied ||
            (hasRequestedCamera && !shouldShowRequestPermissionRationale(Manifest.permission.CAMERA))
        current.permissionMessage.setText(
            if (deniedPermanently) R.string.camera_permission_settings_message
            else R.string.camera_permission_rationale,
        )
        current.permissionButton.isVisible = !deniedPermanently
        current.settingsButton.isVisible = deniedPermanently
    }

    private fun render(state: QrScannerState) {
        val current = binding ?: return
        if (state.isVerifying || state.error != null) {
            current.barcodeView.pause()
            current.scannerContainer.isVisible = false
            current.permissionContainer.isVisible = false
            current.verificationContainer.isVisible = true
        }
        current.verifyingIndicator.isVisible = state.isVerifying
        current.verifyingMessage.isVisible = state.isVerifying
        current.manualVerifyButton.isEnabled = !state.isVerifying
        current.verificationError.isVisible = state.error != null
        current.scanAgainButton.isVisible = state.error != null
        state.error?.let { error ->
            current.verificationError.text = operatorErrorText(error.code, error.message)
            if (error.httpStatus == 401) findNavController().navigate(R.id.action_global_loginFragment)
        }
        state.verifiedReference?.let { reference ->
            if (findNavController().currentDestination?.id == R.id.qrScannerFragment) {
                current.manualReferenceInput.text?.clear()
                viewModel.consumeNavigation()
                findNavController().navigate(
                    R.id.action_qrScannerFragment_to_verifiedTransactionFragment,
                    bundleOf(VerifiedTransactionFragment.ARG_TRANSACTION_REFERENCE to reference),
                )
            }
        }
    }

    private fun operatorErrorText(code: String, fallback: String): String = when (
        OperatorPresentation.errorMessage(code)
    ) {
        OperatorErrorMessage.STATION_NOT_ASSIGNED -> getString(R.string.operator_station_not_assigned)
        OperatorErrorMessage.WRONG_STATION -> getString(R.string.wrong_station_message)
        OperatorErrorMessage.INVALID_REFERENCE -> getString(R.string.invalid_reference_message)
        OperatorErrorMessage.NOT_APPROVED -> getString(R.string.reservation_not_approved_message)
        OperatorErrorMessage.CANCELLED -> getString(R.string.reservation_cancelled_message)
        OperatorErrorMessage.ALREADY_COMPLETED -> getString(R.string.reservation_already_completed_message)
        OperatorErrorMessage.DEFAULT -> fallback
    }

    private fun hasCameraPermission(): Boolean = ContextCompat.checkSelfPermission(
        requireContext(),
        Manifest.permission.CAMERA,
    ) == PackageManager.PERMISSION_GRANTED

    private fun openSettings() {
        startActivity(
            Intent(
                Settings.ACTION_APPLICATION_DETAILS_SETTINGS,
                Uri.fromParts("package", requireContext().packageName, null),
            ),
        )
    }

    override fun onDestroyView() {
        permissionDialog?.dismiss()
        permissionDialog = null
        binding?.barcodeView?.pause()
        binding?.manualReferenceInput?.text?.clear()
        binding = null
        super.onDestroyView()
    }

    companion object {
        const val SCAN_AGAIN_KEY = "operator_scan_again"
        private const val KEY_REQUESTED_CAMERA = "requested_camera"
    }
}
