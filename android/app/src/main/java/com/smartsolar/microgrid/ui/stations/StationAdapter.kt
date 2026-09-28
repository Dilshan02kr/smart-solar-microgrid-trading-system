package com.smartsolar.microgrid.ui.stations

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.databinding.ItemStationBinding
import com.smartsolar.microgrid.domain.model.Station
import com.smartsolar.microgrid.domain.model.StationStatus

class StationAdapter(private val onClick: (String) -> Unit) :
    ListAdapter<Station, StationAdapter.Holder>(Diff) {
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int) = Holder(
        ItemStationBinding.inflate(LayoutInflater.from(parent.context), parent, false),
    )
    override fun onBindViewHolder(holder: Holder, position: Int) = holder.bind(getItem(position))

    inner class Holder(private val binding: ItemStationBinding) : RecyclerView.ViewHolder(binding.root) {
        fun bind(station: Station) = with(binding) {
            stationName.text = station.name
            stationLocation.text = station.locationName
            stationCapacity.text = root.context.getString(R.string.capacity_value, station.totalCapacityKw)
            stationSchedule.text = station.operationalSchedule
            stationStatus.text = station.status.name
            viewStationButton.text = root.context.getString(
                if (station.status == StationStatus.ACTIVE) R.string.view_station else R.string.view_details,
            )
            root.setOnClickListener { onClick(station.id) }
            viewStationButton.setOnClickListener { onClick(station.id) }
        }
    }

    private object Diff : DiffUtil.ItemCallback<Station>() {
        override fun areItemsTheSame(oldItem: Station, newItem: Station) = oldItem.id == newItem.id
        override fun areContentsTheSame(oldItem: Station, newItem: Station) = oldItem == newItem
    }
}
