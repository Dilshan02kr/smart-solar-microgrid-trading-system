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
        var slots = await slotRepository.GetAllAsync(cancellationToken);
        return slots.Select(MapSlot).ToList();
    }

    public async Task<EnergyBookingSlotManagementResult> GetByIdAsync(
    string slotId,
    CancellationToken cancellationToken = default)
{
    if (!ObjectId.TryParse(slotId, out _))
    {
        return new EnergyBookingSlotManagementResult(EnergyBookingSlotManagementStatus.InvalidId);
    }

    var slot = await slotRepository.GetByIdAsync(slotId, cancellationToken);
    return slot is not null
        ? new EnergyBookingSlotManagementResult(EnergyBookingSlotManagementStatus.Success, Slot: MapSlot(slot))
        : new EnergyBookingSlotManagementResult(EnergyBookingSlotManagementStatus.NotFound);
}


    private static DateTime NormalizeDate(DateTime date) =>
        DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

    private static EnergyBookingSlotResponse MapSlot(EnergyBookingSlot slot) =>
        new(
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