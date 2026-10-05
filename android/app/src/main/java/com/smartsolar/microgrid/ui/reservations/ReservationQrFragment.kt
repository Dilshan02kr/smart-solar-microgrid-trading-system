package com.smartsolar.microgrid.ui.reservations

import android.content.ClipData
import android.content.ClipboardManager
import android.content.Context
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import androidx.core.content.ContextCompat
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.core.util.QrCodeGenerator
import com.smartsolar.microgrid.databinding.FragmentReservationQrBinding
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class ReservationQrFragment : Fragment() {
    private var binding: FragmentReservationQrBinding? = null
    private var currentPayload: String? = null
    private val reservationId by lazy {
        requireArguments().getString(CreateReservationFragment.ARG_RESERVATION_ID).orEmpty()
    }
    private val viewModel: ReservationQrViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory {
            ReservationQrViewModel(reservationId, app.reservationRepository, app.stationRepository)
        }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentReservationQrBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        binding?.backButton?.setOnClickListener { findNavController().navigateUp() }
        binding?.copyButton?.setOnClickListener { copyReference() }
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    private fun render(state: ReservationQrState) {
        val currentBinding = binding ?: return
        currentBinding.progressIndicator.isVisible = state.isLoading
        currentBinding.errorContainer.isVisible = state.error != null || state.isUnavailable
        currentBinding.contentContainer.isVisible = state.content != null
        currentBinding.retryButton.isVisible = state.error != null
        currentBinding.errorMessage.setText(
            if (state.isUnavailable) R.string.qr_unavailable_message else R.string.qr_load_error,
        )
        state.error?.let {
            currentBinding.errorMessage.text = it.message
            if (it.httpStatus == 401) findNavController().navigate(R.id.action_global_loginSelectionFragment)
        }
        state.content?.let { content ->
            currentPayload = content.payload
            currentBinding.referenceValue.text = content.payload
            currentBinding.stationValue.text = content.stationName
            currentBinding.statusValue.text = content.reservation.status.name
            currentBinding.scheduledValue.text = BookingPresentation.timestamp(content.reservation.scheduledTime)
            currentBinding.qrImage.contentDescription = getString(R.string.qr_content_description, content.payload)
            val horizontalPadding = resources.getDimensionPixelSize(R.dimen.screen_padding) * 2
            val size = minOf(
                resources.displayMetrics.widthPixels - horizontalPadding,
                resources.getDimensionPixelSize(R.dimen.qr_max_size),
            ).coerceAtLeast(1)
            val bitmap = runCatching {
                QrCodeGenerator.create(
                    content.payload,
                    size,
                    ContextCompat.getColor(requireContext(), R.color.qr_foreground),
                    ContextCompat.getColor(requireContext(), R.color.qr_background),
                )
            }.getOrNull()
            currentBinding.qrImage.setImageBitmap(bitmap)
            if (bitmap == null) {
                currentBinding.contentContainer.isVisible = false
                currentBinding.errorContainer.isVisible = true
                currentBinding.retryButton.isVisible = true
                currentBinding.errorMessage.setText(R.string.qr_generation_error)
            }
        } ?: run { currentPayload = null }
    }

    private fun copyReference() {
        val payload = currentPayload ?: return
        val clipboard = requireContext().getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
        clipboard.setPrimaryClip(ClipData.newPlainText(getString(R.string.reference_label), payload))
        Toast.makeText(requireContext(), R.string.reference_copied, Toast.LENGTH_SHORT).show()
    }

    override fun onDestroyView() {
        currentPayload = null
        binding?.qrImage?.setImageDrawable(null)
        binding = null
        super.onDestroyView()
    }
}
