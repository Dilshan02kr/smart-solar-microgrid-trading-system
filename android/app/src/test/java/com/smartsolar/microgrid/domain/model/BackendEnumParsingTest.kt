package com.smartsolar.microgrid.domain.model

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test

class BackendEnumParsingTest {
    @Test
    fun `role parsing accepts exact backend values`() {
        assertEquals(UserRole.BACKOFFICE, UserRole.fromBackendValue("BACKOFFICE"))
        assertEquals(UserRole.GRID_OPERATOR, UserRole.fromBackendValue("GRID_OPERATOR"))
        assertEquals(UserRole.PROSUMER, UserRole.fromBackendValue("PROSUMER"))
    }

    @Test
    fun `unknown role is not mapped to a valid role`() {
        assertNull(UserRole.fromBackendValue("UNKNOWN"))
        assertNull(UserRole.fromBackendValue("prosumer"))
    }

    @Test
    fun `account status parsing accepts only exact backend values`() {
        assertEquals(AccountStatus.ACTIVE, AccountStatus.fromBackendValue("ACTIVE"))
        assertEquals(AccountStatus.PENDING, AccountStatus.fromBackendValue("PENDING"))
        assertEquals(AccountStatus.DEACTIVATED, AccountStatus.fromBackendValue("DEACTIVATED"))
        assertNull(AccountStatus.fromBackendValue("DISABLED"))
    }
}

