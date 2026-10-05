package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.EnergySlot

object SlotFilter {
    fun apply(slots: List<EnergySlot>, availableOnly: Boolean = true, selectedDate: String? = null): List<EnergySlot> = slots
        .asSequence()
        .filter { !availableOnly || it.isAvailable }
        .filter { selectedDate == null || dateKey(it.date) == selectedDate }
        .sortedWith(compareBy<EnergySlot>({ dateKey(it.date) }, { it.startTime }))
        .toList()

    fun dates(slots: List<EnergySlot>): List<String> = slots.map { dateKey(it.date) }.distinct().sorted()

    private fun dateKey(value: String): String = value.take(10)
}
