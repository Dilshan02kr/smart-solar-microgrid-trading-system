package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.EnergySlot

object OperatorSlotPresentation {
    fun chronological(slots: List<EnergySlot>): List<EnergySlot> =
        slots.sortedWith(compareBy<EnergySlot>({ it.date.take(10) }, { it.startTime }))
}
