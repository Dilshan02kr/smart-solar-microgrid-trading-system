using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IEnergyBookingSlotService
{
    Task<IReadOnlyList<EnergyBookingSlotResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<EnergyBookingSlotManagementResult> GetByIdAsync(
        string slotId,
        CancellationToken cancellationToken = default);

    Task<EnergyBookingSlotManagementResult> GetByStationIdAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    Task<EnergyBookingSlotManagementResult> CreateAsync(
        CreateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken = default);

    Task<EnergyBookingSlotManagementResult> UpdateAsync(
        string slotId,
        UpdateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken = default);
}

public enum EnergyBookingSlotManagementStatus
{
    Success,
    InvalidId,
    NotFound,
    ValidationError
}

public sealed record EnergyBookingSlotManagementResult(
    EnergyBookingSlotManagementStatus Status,
    EnergyBookingSlotResponse? Slot = null,
    IReadOnlyList<EnergyBookingSlotResponse>? Slots = null,
    string? ErrorMessage = null);