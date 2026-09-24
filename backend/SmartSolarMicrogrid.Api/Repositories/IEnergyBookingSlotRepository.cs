using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public interface IEnergyBookingSlotRepository
{
    Task CreateAsync(
        EnergyBookingSlot slot,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnergyBookingSlot>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<EnergyBookingSlot?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnergyBookingSlot>> GetByStationIdAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    Task<EnergyBookingSlot?> UpdateAsync(
        string id,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        double capacityKw,
        CancellationToken cancellationToken = default);
}
