/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ReservationService.cs
 * Component: Reservation and Booking Management
 * Component Owner: N A Illangasinghe (IT23391536)
 *
 * Purpose:
 * Coordinates reservation lifecycle rules across users, stations, slots, and MongoDB.
 */
using System.Security.Cryptography;
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class ReservationService(
    IEnergyReservationRepository reservationRepository,
    ISolarStationRepository stationRepository,
    IEnergyBookingSlotRepository slotRepository) : IReservationService
{
    private static readonly TimeSpan ChangeNotice = TimeSpan.FromHours(12);
    private static readonly TimeSpan ReservationWindow = TimeSpan.FromDays(7);

    public async Task<ReservationOperationResult> CreateAsync(
        string userId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate JWT identity and atomically claim the selected slot before inserting.
        if (!ObjectId.TryParse(userId, out _))
        {
            return Result(ReservationOperationStatus.InvalidUserId);
        }

        var now = DateTime.UtcNow;
        var selection = await ValidateSelectionAsync(
            request.StationId,
            request.SlotId,
            now,
            requireAvailable: true,
            cancellationToken);
        if (selection.Status != ReservationOperationStatus.Success)
        {
            return Result(selection.Status);
        }

        if (!await slotRepository.TryClaimAvailableAsync(selection.Slot!.Id.ToString(), cancellationToken))
        {
            return Result(ReservationOperationStatus.SlotNotAvailable);
        }

        var reservation = new EnergyReservation
        {
            ProsumerId = userId,
            StationId = selection.Station!.Id.ToString(),
            SlotId = selection.Slot.Id.ToString(),
            ScheduledTime = selection.ScheduledTime,
            Status = ReservationStatus.PENDING,
            TransactionReference = null
        };

        try
        {
            await reservationRepository.CreateAsync(reservation, cancellationToken);
        }
        catch
        {
            await slotRepository.ReleaseAsync(selection.Slot.Id.ToString(), cancellationToken);
            throw;
        }

        return Result(ReservationOperationStatus.Success, reservation: MapReservation(reservation));
    }

    public async Task<ReservationOperationResult> GetMineAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Return only reservations whose Prosumer reference matches the JWT user ID.
        if (!ObjectId.TryParse(userId, out _))
        {
            return Result(ReservationOperationStatus.InvalidUserId);
        }

        var reservations = await reservationRepository.GetByProsumerAsync(userId, cancellationToken);
        return Result(
            ReservationOperationStatus.Success,
            reservations: reservations.Select(MapReservation).ToList());
    }

    public async Task<ReservationOperationResult> GetByProsumerAsync(
        string prosumerId,
        CancellationToken cancellationToken = default)
    {
        // Return an administrative list after validating the Prosumer ObjectId.
        if (!ObjectId.TryParse(prosumerId, out _))
        {
            return Result(ReservationOperationStatus.InvalidProsumerId);
        }

        var reservations = await reservationRepository.GetByProsumerAsync(prosumerId, cancellationToken);
        return Result(
            ReservationOperationStatus.Success,
            reservations: reservations.Select(MapReservation).ToList());
    }

    public async Task<ReservationOperationResult> GetByIdAsync(
        string reservationId,
        string userId,
        bool isBackoffice,
        CancellationToken cancellationToken = default)
    {
        // Enforce Backoffice-or-owner access without exposing the persistence model.
        if (!ObjectId.TryParse(reservationId, out _))
        {
            return Result(ReservationOperationStatus.InvalidReservationId);
        }

        if (!isBackoffice && !ObjectId.TryParse(userId, out _))
        {
            return Result(ReservationOperationStatus.InvalidUserId);
        }

        var reservation = await reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return Result(ReservationOperationStatus.ReservationNotFound);
        }

        if (!isBackoffice && !string.Equals(reservation.ProsumerId, userId, StringComparison.Ordinal))
        {
            return Result(ReservationOperationStatus.AccessDenied);
        }

        return Result(ReservationOperationStatus.Success, reservation: MapReservation(reservation));
    }

    public async Task<ReservationOperationResult> SearchAsync(
        string? prosumerId,
        string? status,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        // Validate administrative filters and execute an escaped repository search.
        if (!string.IsNullOrWhiteSpace(prosumerId) && !ObjectId.TryParse(prosumerId, out _))
        {
            return Result(ReservationOperationStatus.InvalidProsumerId);
        }

        ReservationStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ReservationStatus>(status.Trim(), ignoreCase: true, out var value) ||
                !Enum.IsDefined(value))
            {
                return Result(ReservationOperationStatus.InvalidReservationStatus);
            }

            parsedStatus = value;
        }

        var reservations = await reservationRepository.SearchAsync(
            prosumerId?.Trim(),
            parsedStatus,
            searchTerm,
            cancellationToken);
        return Result(
            ReservationOperationStatus.Success,
            reservations: reservations.Select(MapReservation).ToList());
    }

    public async Task<ReservationOperationResult> ApproveAsync(
        string reservationId,
        CancellationToken cancellationToken = default)
    {
        // Validate references and atomically transition a future pending reservation to approved.
        if (!ObjectId.TryParse(reservationId, out _))
        {
            return Result(ReservationOperationStatus.InvalidReservationId);
        }

        var reservation = await reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return Result(ReservationOperationStatus.ReservationNotFound);
        }

        if (reservation.Status != ReservationStatus.PENDING)
        {
            return Result(ReservationOperationStatus.InvalidReservationState);
        }

        var now = DateTime.UtcNow;
        if (reservation.ScheduledTime <= now)
        {
            return Result(ReservationOperationStatus.InvalidReservationState);
        }

        var referenceStatus = await ValidateExistingReferencesAsync(reservation, cancellationToken);
        if (referenceStatus != ReservationOperationStatus.Success)
        {
            return Result(referenceStatus);
        }

        var transactionReference = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var approved = await reservationRepository.TryApprovePendingAsync(
            reservationId,
            transactionReference,
            now,
            cancellationToken);
        return approved is not null
            ? Result(ReservationOperationStatus.Success, reservation: MapReservation(approved))
            : Result(ReservationOperationStatus.InvalidReservationState);
    }

    public async Task<ReservationOperationResult> UpdateAsync(
        string reservationId,
        string userId,
        UpdateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Safely switch an owned nonterminal reservation to a validated slot before cutoff.
        var lookup = await GetOwnedMutableReservationAsync(
            reservationId,
            userId,
            cancellationToken);
        if (lookup.Status != ReservationOperationStatus.Success)
        {
            return Result(lookup.Status);
        }

        var existing = lookup.Reservation!;
        var now = DateTime.UtcNow;
        if (existing.ScheduledTime < now.Add(ChangeNotice))
        {
            return Result(ReservationOperationStatus.ChangeTooLate);
        }

        var sameSlot = string.Equals(existing.SlotId, request.SlotId, StringComparison.Ordinal) &&
                       string.Equals(existing.StationId, request.StationId, StringComparison.Ordinal);
        var selection = await ValidateSelectionAsync(
            request.StationId,
            request.SlotId,
            now,
            requireAvailable: !sameSlot,
            cancellationToken);
        if (selection.Status != ReservationOperationStatus.Success)
        {
            return Result(selection.Status);
        }

        if (sameSlot && existing.ScheduledTime == selection.ScheduledTime)
        {
            return Result(ReservationOperationStatus.Success, reservation: MapReservation(existing));
        }

        var newSlotClaimed = false;
        if (!sameSlot)
        {
            newSlotClaimed = await slotRepository.TryClaimAvailableAsync(
                selection.Slot!.Id.ToString(),
                cancellationToken);
            if (!newSlotClaimed)
            {
                return Result(ReservationOperationStatus.SlotNotAvailable);
            }
        }

        var updated = await reservationRepository.TryUpdateBookingAsync(
            reservationId,
            userId,
            existing.SlotId,
            selection.Station!.Id.ToString(),
            selection.Slot!.Id.ToString(),
            selection.ScheduledTime,
            now.Add(ChangeNotice),
            now,
            cancellationToken);

        if (updated is null)
        {
            if (newSlotClaimed)
            {
                await slotRepository.ReleaseAsync(selection.Slot!.Id.ToString(), cancellationToken);
            }

            return Result(ReservationOperationStatus.InvalidReservationState);
        }

        if (!sameSlot)
        {
            await slotRepository.ReleaseAsync(existing.SlotId, cancellationToken);
        }

        return Result(ReservationOperationStatus.Success, reservation: MapReservation(updated));
    }

    public async Task<ReservationOperationResult> CancelAsync(
        string reservationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Cancel an owned pending or approved reservation before the inclusive cutoff.
        var lookup = await GetOwnedMutableReservationAsync(
            reservationId,
            userId,
            cancellationToken);
        if (lookup.Status != ReservationOperationStatus.Success)
        {
            return Result(lookup.Status);
        }

        var existing = lookup.Reservation!;
        var now = DateTime.UtcNow;
        if (existing.ScheduledTime < now.Add(ChangeNotice))
        {
            return Result(ReservationOperationStatus.ChangeTooLate);
        }

        var cancelled = await reservationRepository.TryCancelAsync(
            reservationId,
            userId,
            now.Add(ChangeNotice),
            now,
            cancellationToken);
        if (cancelled is null)
        {
            return Result(ReservationOperationStatus.InvalidReservationState);
        }

        await slotRepository.ReleaseAsync(cancelled.SlotId, cancellationToken);
        return Result(ReservationOperationStatus.Success, reservation: MapReservation(cancelled));
    }

    private async Task<SelectionValidation> ValidateSelectionAsync(
        string? stationId,
        string? slotId,
        DateTime nowUtc,
        bool requireAvailable,
        CancellationToken cancellationToken)
    {
        // Validate station/slot identity, relationship, state, availability, and seven-day timing.
        if (string.IsNullOrWhiteSpace(stationId) || !ObjectId.TryParse(stationId, out _))
        {
            return new SelectionValidation(ReservationOperationStatus.InvalidStationId);
        }

        if (string.IsNullOrWhiteSpace(slotId) || !ObjectId.TryParse(slotId, out _))
        {
            return new SelectionValidation(ReservationOperationStatus.InvalidSlotId);
        }

        var station = await stationRepository.GetByIdAsync(stationId, cancellationToken);
        if (station is null)
        {
            return new SelectionValidation(ReservationOperationStatus.StationNotFound);
        }

        if (station.Status != StationStatus.ACTIVE)
        {
            return new SelectionValidation(ReservationOperationStatus.StationInactive);
        }

        var slot = await slotRepository.GetByIdAsync(slotId, cancellationToken);
        if (slot is null)
        {
            return new SelectionValidation(ReservationOperationStatus.SlotNotFound);
        }

        if (slot.StationId != station.Id)
        {
            return new SelectionValidation(ReservationOperationStatus.SlotStationMismatch);
        }

        if (requireAvailable && !slot.IsAvailable)
        {
            return new SelectionValidation(ReservationOperationStatus.SlotNotAvailable);
        }

        var scheduledTime = DateTime.SpecifyKind(
            slot.Date.Date + slot.StartTime,
            DateTimeKind.Utc);
        if (scheduledTime <= nowUtc || scheduledTime > nowUtc.Add(ReservationWindow))
        {
            return new SelectionValidation(ReservationOperationStatus.OutsideAllowedWindow);
        }

        return new SelectionValidation(
            ReservationOperationStatus.Success,
            station,
            slot,
            scheduledTime);
    }

    private async Task<ReservationOperationStatus> ValidateExistingReferencesAsync(
        EnergyReservation reservation,
        CancellationToken cancellationToken)
    {
        // Revalidate station and slot integrity immediately before approval.
        var station = await stationRepository.GetByIdAsync(reservation.StationId, cancellationToken);
        if (station is null)
        {
            return ReservationOperationStatus.StationNotFound;
        }

        if (station.Status != StationStatus.ACTIVE)
        {
            return ReservationOperationStatus.StationInactive;
        }

        var slot = await slotRepository.GetByIdAsync(reservation.SlotId, cancellationToken);
        if (slot is null)
        {
            return ReservationOperationStatus.SlotNotFound;
        }

        return slot.StationId == station.Id
            ? ReservationOperationStatus.Success
            : ReservationOperationStatus.SlotStationMismatch;
    }

    private async Task<OwnedReservationLookup> GetOwnedMutableReservationAsync(
        string reservationId,
        string userId,
        CancellationToken cancellationToken)
    {
        // Validate identifiers, ownership, and eligibility for update or cancellation.
        if (!ObjectId.TryParse(reservationId, out _))
        {
            return new OwnedReservationLookup(ReservationOperationStatus.InvalidReservationId);
        }

        if (!ObjectId.TryParse(userId, out _))
        {
            return new OwnedReservationLookup(ReservationOperationStatus.InvalidUserId);
        }

        var reservation = await reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return new OwnedReservationLookup(ReservationOperationStatus.ReservationNotFound);
        }

        if (!string.Equals(reservation.ProsumerId, userId, StringComparison.Ordinal))
        {
            return new OwnedReservationLookup(ReservationOperationStatus.AccessDenied);
        }

        if (reservation.Status is not (ReservationStatus.PENDING or ReservationStatus.APPROVED))
        {
            return new OwnedReservationLookup(ReservationOperationStatus.InvalidReservationState);
        }

        return new OwnedReservationLookup(ReservationOperationStatus.Success, reservation);
    }

    private static ReservationOperationResult Result(
        ReservationOperationStatus status,
        ReservationResponse? reservation = null,
        IReadOnlyList<ReservationResponse>? reservations = null)
    {
        // Construct a consistent service outcome for controller mapping.
        return new ReservationOperationResult(status, reservation, reservations);
    }

    private static ReservationResponse MapReservation(EnergyReservation reservation)
    {
        // Map MongoDB-backed identifiers and lifecycle data to the public DTO.
        return new ReservationResponse(
            reservation.Id,
            reservation.ProsumerId,
            reservation.StationId,
            reservation.SlotId,
            reservation.ScheduledTime,
            reservation.Status.ToString(),
            reservation.TransactionReference,
            reservation.CreatedAt,
            reservation.UpdatedAt,
            reservation.CompletedAt);
    }

    private sealed record SelectionValidation(
        ReservationOperationStatus Status,
        SolarStationInfo? Station = null,
        EnergyBookingSlot? Slot = null,
        DateTime ScheduledTime = default);

    private sealed record OwnedReservationLookup(
        ReservationOperationStatus Status,
        EnergyReservation? Reservation = null);
}
