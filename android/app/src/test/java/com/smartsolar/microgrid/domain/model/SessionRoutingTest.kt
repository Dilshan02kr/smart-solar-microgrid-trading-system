package com.smartsolar.microgrid.domain.model

import org.junit.Assert.assertEquals
import org.junit.Test

class SessionRoutingTest {
    @Test
    fun `no session routes to login selection`() {
        assertEquals(SessionDestination.LOGIN_SELECTION, SessionRouting.destination(null))
    }

    @Test
    fun `active supported roles route to their own homes`() {
        assertEquals(SessionDestination.PROSUMER_HOME, SessionRouting.destination(user(UserRole.PROSUMER)))
        assertEquals(SessionDestination.OPERATOR_DASHBOARD, SessionRouting.destination(user(UserRole.GRID_OPERATOR)))
    }

    @Test
    fun `backoffice and non-active accounts are unsupported`() {
        assertEquals(SessionDestination.UNSUPPORTED, SessionRouting.destination(user(UserRole.BACKOFFICE)))
        assertEquals(
            SessionDestination.UNSUPPORTED,
            SessionRouting.destination(user(UserRole.GRID_OPERATOR, AccountStatus.PENDING)),
        )
        assertEquals(
            SessionDestination.UNSUPPORTED,
            SessionRouting.destination(user(UserRole.PROSUMER, AccountStatus.DEACTIVATED)),
        )
    }

    private fun user(role: UserRole, status: AccountStatus = AccountStatus.ACTIVE) = SessionUser(
        userId = "user",
        firstName = "Test",
        lastName = "User",
        role = role,
        accountStatus = status,
        nic = null,
        assignedMicrogridNodeId = if (role == UserRole.GRID_OPERATOR) "station" else null,
    )
}
