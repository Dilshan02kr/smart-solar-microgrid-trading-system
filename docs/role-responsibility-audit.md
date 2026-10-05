# Role Responsibility Audit

## Grid Operator

| ID | Capability | Status | Evidence |
| --- | --- | --- | --- |
| GO-11 | Slot availability management | PASS | Grid Operator can update availability of existing Energy Booking Slots for the operator's assigned Microgrid Node only. Station scope is derived from the current database assignment; the operator cannot create slots or edit station, date, time, or capacity. |

## Responsibility summary

- Backoffice creates and configures microgrid nodes and base energy booking slots, and approves reservations.
- Grid Operator views existing slots for one assigned microgrid node, changes availability only, monitors operational reservations, verifies QR transactions, and completes approved transfers.
- Prosumer views station slots and creates, changes, or cancels reservations under server-authoritative availability and lifecycle rules.
