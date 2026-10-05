package com.smartsolar.microgrid.ui.operator

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.core.util.BookingPresentation
import com.smartsolar.microgrid.databinding.ItemOperatorSlotBinding
import com.smartsolar.microgrid.domain.model.EnergySlot

class OperatorSlotAdapter(
    private val onAvailabilityClick: (EnergySlot) -> Unit,
) : ListAdapter<EnergySlot, OperatorSlotAdapter.Holder>(Diff) {
    var mutatingSlotId: String? = null
        set(value) {
            if (field == value) return
            val changedIds = setOfNotNull(field, value)
            field = value
            changedIds.forEach { id ->
                val index = currentList.indexOfFirst { it.id == id }
                if (index >= 0) notifyItemChanged(index)
            }
        }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int) = Holder(
        ItemOperatorSlotBinding.inflate(LayoutInflater.from(parent.context), parent, false),
    )

    override fun onBindViewHolder(holder: Holder, position: Int) =
        holder.bind(getItem(position), mutatingSlotId, onAvailabilityClick)

    class Holder(private val binding: ItemOperatorSlotBinding) : RecyclerView.ViewHolder(binding.root) {
        fun bind(slot: EnergySlot, mutatingSlotId: String?, onAvailabilityClick: (EnergySlot) -> Unit) = with(binding) {
            dateValue.text = BookingPresentation.date(slot.date)
            timeValue.text = root.context.getString(
                R.string.slot_time_value,
                BookingPresentation.time(slot.startTime),
                BookingPresentation.time(slot.endTime),
            )
            capacityValue.text = root.context.getString(R.string.capacity_value, slot.capacityKw)
            availabilityValue.text = root.context.getString(if (slot.isAvailable) R.string.available else R.string.unavailable)
            availabilityAction.text = root.context.getString(
                if (slot.isAvailable) R.string.mark_unavailable else R.string.mark_available,
            )
            availabilityAction.isEnabled = mutatingSlotId == null
            availabilityAction.setOnClickListener { onAvailabilityClick(slot) }
        }
    }

    private object Diff : DiffUtil.ItemCallback<EnergySlot>() {
        override fun areItemsTheSame(old: EnergySlot, new: EnergySlot) = old.id == new.id
        override fun areContentsTheSame(old: EnergySlot, new: EnergySlot) = old == new
    }
}
