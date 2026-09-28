package com.smartsolar.microgrid.data.remote.dto

import com.smartsolar.microgrid.domain.model.AccountStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test

class ProsumerProfileDtoTest {
    private val profile = ProsumerProfileDto(
        userId = "507f1f77bcf86cd799439011",
        nic = "200012345678",
        firstName = "Asha",
        lastName = "Perera",
        email = "asha@example.test",
        phone = "0712345678",
        accountStatus = "ACTIVE",
        createdAt = "2026-09-27T10:00:00Z",
        updatedAt = "2026-09-27T10:00:00Z",
    )

    @Test
    fun `profile maps exact backend values`() {
        val mapped = profile.toDomainOrNull()

        assertEquals(AccountStatus.ACTIVE, mapped?.accountStatus)
        assertEquals(profile.userId, mapped?.userId)
        assertEquals(profile.nic, mapped?.nic)
    }

    @Test
    fun `profile rejects unknown account status`() {
        assertNull(profile.copy(accountStatus = "UNKNOWN").toDomainOrNull())
    }
}
