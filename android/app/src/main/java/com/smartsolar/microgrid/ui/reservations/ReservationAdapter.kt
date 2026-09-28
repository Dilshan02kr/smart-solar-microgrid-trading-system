package com.smartsolar.microgrid.ui.reservations

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.core.view.isVisible
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.core.util.ReservationPresentation
import com.smartsolar.microgrid.databinding.ItemReservationBinding
import com.smartsolar.microgrid.domain.model.ReservationStatus

class ReservationAdapter(
    private val onDetails: (String) -> Unit,
    private val onQr: (String) -> Unit,
) : ListAdapter<ReservationListItem, ReservationAdapter.ReservationHolder>(Diff) {
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ReservationHolder = ReservationHolder(
        ItemReservationBinding.inflate(LayoutInflater.from(parent.context), parent, false),
    )

    override fun onBindViewHolder(holder: ReservationHolder, position: Int) = holder.bind(getItem(position))

    inner class ReservationHolder(private val binding: ItemReservationBinding) : RecyclerView.ViewHolder(binding.root) {
        fun bind(item: ReservationListItem) {
            val reservation = item.reservation
            binding.statusValue.text = reservation.status.name
            binding.statusDescription.text = when (reservation.status) {
                ReservationStatus.PENDING -> binding.root.context.getString(R.string.pending_status_description)
                ReservationStatus.APPROVED -> binding.root.context.getString(R.string.approved_status_description)
                ReservationStatus.COMPLETED -> binding.root.context.getString(R.string.completed_status_description)
                ReservationStatus.CANCELLED -> binding.root.context.getString(R.string.cancelled_status_description)
            }
            binding.stationValue.text = item.stationName
            binding.scheduledValue.text = BookingPresentation.timestamp(reservation.scheduledTime)
            binding.reservationIdValue.text = reservation.reservationId
            binding.detailsButton.setOnClickListener { onDetails(reservation.reservationId) }
            binding.qrButton.isVisible = ReservationPresentation.qrPayload(reservation) != null
            binding.qrButton.setOnClickListener { onQr(reservation.reservationId) }
        }
    }

    private object Diff : DiffUtil.ItemCallback<ReservationListItem>() {
        override fun areItemsTheSame(oldItem: ReservationListItem, newItem: ReservationListItem): Boolean =
            oldItem.reservation.reservationId == newItem.reservation.reservationId

        override fun areContentsTheSame(oldItem: ReservationListItem, newItem: ReservationListItem): Boolean =
            oldItem == newItem
    }
}
