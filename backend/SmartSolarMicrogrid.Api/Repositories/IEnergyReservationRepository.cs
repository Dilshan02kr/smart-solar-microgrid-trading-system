// Defines focused MongoDB persistence operations for reservation workflows.
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public interface IEnergyReservationRepository
{
    // Create reservation indexes needed for transaction-reference safety.
    Task EnsureIndexesAsync(CancellationToken cancellationToken = default);

    // Insert a new server-controlled reservation.
    Task CreateAsync(EnergyReservation reservation, CancellationToken cancellationToken = default);

    // Retrieve a reservation by its validated public identifier.
    Task<EnergyReservation?> GetByIdAsync(string reservationId, CancellationToken cancellationToken = default);

    // Retrieve an approved reservation by its transaction reference.
    Task<EnergyReservation?> GetByTransactionReferenceAsync(
        string transactionReference,
        CancellationToken cancellationToken = default);

    // Retrieve reservations belonging to one Prosumer.
    Task<IReadOnlyList<EnergyReservation>> GetByProsumerAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);

    // Search reservations for Backoffice administration.
    Task<IReadOnlyList<EnergyReservation>> SearchAsync(
        string? prosumerId,
        ReservationStatus? status,
        string? searchTerm,
        CancellationToken cancellationToken = default);

    // Atomically transition a future pending reservation to approved.
    Task<EnergyReservation?> TryApprovePendingAsync(
        string reservationId,
        string transactionReference,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    // Atomically replace booking references for an eligible owned reservation.
    Task<EnergyReservation?> TryUpdateBookingAsync(
        string reservationId,
        string prosumerId,
        string expectedSlotId,
        string stationId,
        string slotId,
        DateTime scheduledTime,
        DateTime cutoffUtc,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken = default);

    // Atomically cancel an eligible owned reservation before the cutoff.
    Task<EnergyReservation?> TryCancelAsync(
        string reservationId,
        string prosumerId,
        DateTime cutoffUtc,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken = default);

    // Atomically complete an approved reservation.
    Task<EnergyReservation?> TryCompleteApprovedAsync(
        string reservationId,
        string expectedStationId,
        DateTime completedAtUtc,
        CancellationToken cancellationToken = default);

    // Count reservations in an authoritative status.
    Task<long> CountByStatusAsync(
        ReservationStatus status,
        CancellationToken cancellationToken = default);

    // Count approved reservations scheduled after the supplied instant.
    Task<long> CountApprovedFutureAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    // Determine whether a station has pending or approved reservations.
    Task<bool> HasActiveReservationsForStationAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    // Count station reservations in an authoritative status.
    Task<long> CountByStationAndStatusAsync(
        string stationId,
        ReservationStatus status,
        CancellationToken cancellationToken = default);

    // Count future approved reservations for one station.
    Task<long> CountApprovedFutureByStationAsync(
        string stationId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
