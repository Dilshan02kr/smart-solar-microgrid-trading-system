package com.smartsolar.microgrid.ui.slots

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.core.view.isVisible
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.databinding.ItemEnergySlotBinding
import com.smartsolar.microgrid.domain.model.EnergySlot

class EnergySlotAdapter(private val onBook: (EnergySlot) -> Unit) : ListAdapter<EnergySlot, EnergySlotAdapter.Holder>(Diff) {
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int) = Holder(ItemEnergySlotBinding.inflate(LayoutInflater.from(parent.context), parent, false))
    override fun onBindViewHolder(holder: Holder, position: Int) = holder.bind(getItem(position))
    inner class Holder(private val binding: ItemEnergySlotBinding) : RecyclerView.ViewHolder(binding.root) {
        fun bind(slot: EnergySlot) = with(binding) {
            dateValue.text = BookingPresentation.date(slot.date)
            timeValue.text = root.context.getString(R.string.slot_time_value, BookingPresentation.time(slot.startTime), BookingPresentation.time(slot.endTime))
            capacityValue.text = root.context.getString(R.string.capacity_value, slot.capacityKw)
            availabilityValue.setText(if (slot.isAvailable) R.string.available else R.string.unavailable)
            bookButton.isVisible = slot.isAvailable
            bookButton.setOnClickListener { onBook(slot) }
        }
    }
    private object Diff : DiffUtil.ItemCallback<EnergySlot>() {
        override fun areItemsTheSame(old: EnergySlot, new: EnergySlot) = old.id == new.id
        override fun areContentsTheSame(old: EnergySlot, new: EnergySlot) = old == new
    }
}

