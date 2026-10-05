/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: EnergyBookingSlotService.cs
 * Component: Energy Slot Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Implements slot validation, overlap prevention, and safe DTO mapping.
 */
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class EnergyBookingSlotService(
    IEnergyBookingSlotRepository slotRepository,
    ISolarStationRepository stationRepository)
    : IEnergyBookingSlotService
{
    public async Task<IReadOnlyList<EnergyBookingSlotResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Return all persisted slots as API-safe response DTOs.
        var slots = await slotRepository.GetAllAsync(cancellationToken);
        return slots.Select(MapSlot).ToList();
    }

    public async Task<EnergyBookingSlotManagementResult> GetByIdAsync(
        string slotId,
        CancellationToken cancellationToken = default)
    {
        // Validate and retrieve one slot by its MongoDB identifier.
        if (!ObjectId.TryParse(slotId, out _))
        {
            return Result(EnergyBookingSlotManagementStatus.InvalidSlotId);
        }

        var slot = await slotRepository.GetByIdAsync(slotId, cancellationToken);
        return slot is not null
            ? Result(EnergyBookingSlotManagementStatus.Success, slot: MapSlot(slot))
            : Result(EnergyBookingSlotManagementStatus.SlotNotFound);
    }

    public async Task<EnergyBookingSlotManagementResult> GetByStationIdAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        // Validate the station, confirm it exists, and return only its slots.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return Result(EnergyBookingSlotManagementStatus.InvalidStationId);
        }

        var station = await stationRepository.GetByIdAsync(stationId, cancellationToken);
        if (station is null)
        {
            return Result(EnergyBookingSlotManagementStatus.StationNotFound);
        }

        var slots = await slotRepository.GetByStationIdAsync(stationId, cancellationToken);
        return Result(
            EnergyBookingSlotManagementStatus.Success,
            slots: slots.Select(MapSlot).ToList());
    }

    public async Task<EnergyBookingSlotManagementResult> CreateAsync(
        CreateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate the request and create a server-owned slot for an existing station.
        if (string.IsNullOrWhiteSpace(request.StationId) ||
            !ObjectId.TryParse(request.StationId, out var stationObjectId))
        {
            return Result(EnergyBookingSlotManagementStatus.InvalidStationId);
        }

        var timeValidation = ValidateTiming(request.Date, request.StartTime, request.EndTime);
        if (timeValidation is not null)
        {
            return timeValidation;
        }

        if (!IsValidCapacity(request.CapacityKw))
        {
            return Result(EnergyBookingSlotManagementStatus.InvalidSlotCapacity);
        }

        var station = await stationRepository.GetByIdAsync(request.StationId, cancellationToken);
        if (station is null)
        {
            return Result(EnergyBookingSlotManagementStatus.StationNotFound);
        }

        if (request.CapacityKw!.Value > station.TotalCapacityKw)
        {
            return Result(EnergyBookingSlotManagementStatus.SlotCapacityExceedsStation);
        }

        var date = NormalizeDate(request.Date!.Value);
        if (await slotRepository.HasOverlappingSlotAsync(
                stationObjectId,
                date,
                request.StartTime!.Value,
                request.EndTime!.Value,
                cancellationToken: cancellationToken))
        {
            return Result(EnergyBookingSlotManagementStatus.SlotTimeConflict);
        }

        var slot = new EnergyBookingSlot
        {
            StationId = stationObjectId,
            Date = date,
            StartTime = request.StartTime.Value,
            EndTime = request.EndTime.Value,
            CapacityKw = request.CapacityKw.Value,
            IsAvailable = true
        };

        await slotRepository.CreateAsync(slot, cancellationToken);
        return Result(EnergyBookingSlotManagementStatus.Success, slot: MapSlot(slot));
    }

    public async Task<EnergyBookingSlotManagementResult> UpdateAsync(
        string slotId,
        UpdateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate and update mutable slot fields while preserving server-owned identity fields.
        if (!ObjectId.TryParse(slotId, out var slotObjectId))
        {
            return Result(EnergyBookingSlotManagementStatus.InvalidSlotId);
        }

        var existingSlot = await slotRepository.GetByIdAsync(slotId, cancellationToken);
        if (existingSlot is null)
        {
            return Result(EnergyBookingSlotManagementStatus.SlotNotFound);
        }

        var timeValidation = ValidateTiming(request.Date, request.StartTime, request.EndTime);
        if (timeValidation is not null)
        {
            return timeValidation;
        }

        if (!IsValidCapacity(request.CapacityKw))
        {
            return Result(EnergyBookingSlotManagementStatus.InvalidSlotCapacity);
        }

        var stationId = existingSlot.StationId.ToString();
        var station = await stationRepository.GetByIdAsync(stationId, cancellationToken);
        if (station is null)
        {
            return Result(EnergyBookingSlotManagementStatus.StationNotFound);
        }

        if (request.CapacityKw!.Value > station.TotalCapacityKw)
        {
            return Result(EnergyBookingSlotManagementStatus.SlotCapacityExceedsStation);
        }

        var date = NormalizeDate(request.Date!.Value);
        if (await slotRepository.HasOverlappingSlotAsync(
                existingSlot.StationId,
                date,
                request.StartTime!.Value,
                request.EndTime!.Value,
                slotObjectId,
                cancellationToken))
        {
            return Result(EnergyBookingSlotManagementStatus.SlotTimeConflict);
        }

        var updatedSlot = await slotRepository.UpdateAsync(
            slotId,
            date,
            request.StartTime.Value,
            request.EndTime.Value,
            request.CapacityKw.Value,
            cancellationToken);

        return updatedSlot is not null
            ? Result(EnergyBookingSlotManagementStatus.Success, slot: MapSlot(updatedSlot))
            : Result(EnergyBookingSlotManagementStatus.SlotNotFound);
    }

    private static EnergyBookingSlotManagementResult? ValidateTiming(
        DateTime? date,
        TimeSpan? startTime,
        TimeSpan? endTime)
    {
        // Ensure the date and same-day time range can represent a valid booking slot.
        if (date is null || date.Value == default ||
            startTime is null || endTime is null ||
            startTime.Value < TimeSpan.Zero || startTime.Value >= TimeSpan.FromDays(1) ||
            endTime.Value <= TimeSpan.Zero || endTime.Value > TimeSpan.FromDays(1) ||
            endTime.Value <= startTime.Value)
        {
            return Result(EnergyBookingSlotManagementStatus.InvalidSlotTime);
        }

        return null;
    }

    private static bool IsValidCapacity(double? capacity)
    {
        // Accept only finite, strictly positive capacity values.
        return capacity is > 0 && double.IsFinite(capacity.Value);
    }

    private static DateTime NormalizeDate(DateTime date)
    {
        // Store slot dates at UTC midnight for deterministic equality queries.
        return DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
    }

    private static EnergyBookingSlotManagementResult Result(
        EnergyBookingSlotManagementStatus status,
        EnergyBookingSlotResponse? slot = null,
        IReadOnlyList<EnergyBookingSlotResponse>? slots = null)
    {
        // Construct a consistent service result for controller mapping.
        return new EnergyBookingSlotManagementResult(status, slot, slots);
    }

    private static EnergyBookingSlotResponse MapSlot(EnergyBookingSlot slot)
    {
        // Map persistence identifiers and fields to the public slot response contract.
        return new EnergyBookingSlotResponse(
            slot.Id.ToString(),
            slot.StationId.ToString(),
            slot.Date,
            slot.StartTime,
            slot.EndTime,
            slot.CapacityKw,
            slot.IsAvailable,
            slot.CreatedAt,
            slot.UpdatedAt);
    }
}
