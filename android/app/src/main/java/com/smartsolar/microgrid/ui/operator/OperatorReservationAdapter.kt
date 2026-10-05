package com.smartsolar.microgrid.ui.operator

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.databinding.ItemOperatorReservationBinding
import com.smartsolar.microgrid.domain.model.OperatorReservation

class OperatorReservationAdapter : ListAdapter<OperatorReservation, OperatorReservationAdapter.Holder>(Diff) {
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int) = Holder(
        ItemOperatorReservationBinding.inflate(LayoutInflater.from(parent.context), parent, false),
    )

    override fun onBindViewHolder(holder: Holder, position: Int) = holder.bind(getItem(position))

    class Holder(private val binding: ItemOperatorReservationBinding) : RecyclerView.ViewHolder(binding.root) {
        fun bind(reservation: OperatorReservation) = with(binding) {
            statusValue.text = reservation.status.name
            scheduledValue.text = BookingPresentation.timestamp(reservation.scheduledTime)
            reservationValue.text = reservation.reservationId
            slotValue.text = reservation.slotId
        }
    }

    private object Diff : DiffUtil.ItemCallback<OperatorReservation>() {
        override fun areItemsTheSame(old: OperatorReservation, new: OperatorReservation) = old.reservationId == new.reservationId
        override fun areContentsTheSame(old: OperatorReservation, new: OperatorReservation) = old == new
    }
}
