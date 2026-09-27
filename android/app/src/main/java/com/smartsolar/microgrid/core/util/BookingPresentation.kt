package com.smartsolar.microgrid.core.util

import com.smartsolar.microgrid.domain.model.Reservation
import com.smartsolar.microgrid.domain.model.ReservationStatus
import java.time.Instant
import java.time.LocalDate
import java.time.LocalTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.time.format.FormatStyle
import java.util.Locale

object BookingPresentation {
    fun date(apiValue: String, locale: Locale = Locale.getDefault()): String = runCatching {
        LocalDate.parse(apiValue.take(10)).format(DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM).withLocale(locale))
    }.getOrDefault(apiValue.take(10))

    fun time(apiValue: String, locale: Locale = Locale.getDefault()): String = runCatching {
        LocalTime.parse(apiValue).format(DateTimeFormatter.ofLocalizedTime(FormatStyle.SHORT).withLocale(locale))
    }.getOrDefault(apiValue)

    fun timestamp(apiValue: String, locale: Locale = Locale.getDefault()): String = runCatching {
        DateTimeFormatter.ofLocalizedDateTime(FormatStyle.MEDIUM)
            .withLocale(locale)
            .withZone(ZoneId.systemDefault())
            .format(Instant.parse(apiValue))
    }.getOrDefault(apiValue)

    fun canModify(reservation: Reservation, now: Instant = Instant.now()): Boolean {
        if (reservation.status !in setOf(ReservationStatus.PENDING, ReservationStatus.APPROVED)) return false
        val scheduled = runCatching { Instant.parse(reservation.scheduledTime) }.getOrNull() ?: return false
        return !scheduled.isBefore(now.plusSeconds(12 * 60 * 60))
    }
}

