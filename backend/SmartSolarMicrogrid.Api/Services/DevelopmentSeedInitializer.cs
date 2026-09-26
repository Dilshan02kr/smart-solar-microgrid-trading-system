// Seeds initial development-only station, users, slots, and reservations when explicitly enabled.
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.Configuration;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class DevelopmentSeedInitializer(
    IHostEnvironment hostEnvironment,
    IOptions<DevelopmentSeedSettings> seedOptions,
    ISolarStationRepository stationRepository,
    IEnergyBookingSlotRepository slotRepository,
    IEnergyReservationRepository reservationRepository,
    IUserDetailsRepository userRepository,
    IPasswordHasher<UserDetails> passwordHasher,
    ILogger<DevelopmentSeedInitializer> logger) : IHostedService
{
    private const string SeedStationName = "Negombo Test Solar Hub";
    private const string SeedStationLocation = "Negombo";
    private const double SeedLatitude = 7.2083;
    private const double SeedLongitude = 79.8358;
    private const double SeedCapacityKw = 150.0;
    private const string SeedSchedule = "06:00 - 18:00";

    private const string OperatorEmail = "operator@smartsolar.local";
    private const string ProsumerEmail = "prosumer@smartsolar.local";
    private const string ProsumerNic = "200012345678";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Execute seeding strictly when running in Development and explicitly enabled.
        if (!hostEnvironment.IsDevelopment() || !seedOptions.Value.Enabled)
        {
            return;
        }

        var settings = seedOptions.Value;
        if (string.IsNullOrWhiteSpace(settings.OperatorPassword) ||
            string.IsNullOrWhiteSpace(settings.ProsumerPassword))
        {
            throw new InvalidOperationException(
                "Development seed is enabled, but DevelopmentSeed:OperatorPassword and DevelopmentSeed:ProsumerPassword must be configured via environment or local settings.");
        }

        logger.LogInformation("Starting development seed data initialization...");

        // 1. Seed or retrieve test station (SolarStationInfo)
        var station = await EnsureStationAsync(cancellationToken);

        // 2. Seed or retrieve Grid Operator (UserDetails)
        var operatorUser = await EnsureOperatorAsync(station.Id, settings.OperatorPassword, cancellationToken);

        // 3. Seed or retrieve Prosumer (UserDetails)
        var prosumerUser = await EnsureProsumerAsync(settings.ProsumerPassword, cancellationToken);

        // 4. Seed energy booking slots (EnergyBookingSlot)
        var slots = await EnsureSlotsAsync(station.Id, cancellationToken);

        // 5. Seed test reservations in all 4 states (EnergyReservation)
        await EnsureReservationsAsync(prosumerUser.Id.ToString(), station.Id.ToString(), slots, cancellationToken);

        logger.LogInformation("Development seed data initialization completed successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<SolarStationInfo> EnsureStationAsync(CancellationToken cancellationToken)
    {
        var existingStations = await stationRepository.GetAllAsync(cancellationToken);
        var station = existingStations.FirstOrDefault(s =>
            string.Equals(s.Name, SeedStationName, StringComparison.OrdinalIgnoreCase));

        if (station is not null)
        {
            logger.LogInformation(
                "Development seed: Station '{Name}' already exists (ID: {StationId}).",
                station.Name,
                station.Id);
            return station;
        }

        var now = DateTime.UtcNow;
        station = new SolarStationInfo
        {
            Name = SeedStationName,
            LocationName = SeedStationLocation,
            Latitude = SeedLatitude,
            Longitude = SeedLongitude,
            TotalCapacityKw = SeedCapacityKw,
            OperationalSchedule = SeedSchedule,
            Status = StationStatus.ACTIVE,
            CreatedAt = now,
            UpdatedAt = now
        };

        await stationRepository.CreateAsync(station, cancellationToken);
        logger.LogInformation(
            "Development seed: Created station '{Name}' (ID: {StationId}, Status: {Status}).",
            station.Name,
            station.Id,
            station.Status);

        return station;
    }

    private async Task<UserDetails> EnsureOperatorAsync(
        ObjectId stationId,
        string password,
        CancellationToken cancellationToken)
    {
        var operatorUser = await userRepository.GetByEmailAsync(OperatorEmail, cancellationToken);
        if (operatorUser is null)
        {
            var now = DateTime.UtcNow;
            operatorUser = new UserDetails
            {
                FirstName = "Grid",
                LastName = "Operator",
                Email = OperatorEmail,
                Phone = "+94771111111",
                PasswordHash = string.Empty,
                Role = UserRole.GRID_OPERATOR,
                AccountStatus = AccountStatus.ACTIVE,
                Nic = null,
                AssignedMicrogridNodeId = stationId,
                CreatedAt = now,
                UpdatedAt = now
            };
            operatorUser.PasswordHash = passwordHasher.HashPassword(operatorUser, password);

            await userRepository.CreateAsync(operatorUser, cancellationToken);
            logger.LogInformation(
                "Development seed: Created Grid Operator '{Email}' (ID: {UserId}, AssignedStationId: {StationId}).",
                operatorUser.Email,
                operatorUser.Id,
                operatorUser.AssignedMicrogridNodeId);
        }
        else
        {
            if (operatorUser.AssignedMicrogridNodeId != stationId)
            {
                await userRepository.UpdateWebUserAsync(
                    operatorUser.Id.ToString(),
                    operatorUser.Role,
                    operatorUser.FirstName,
                    operatorUser.LastName,
                    operatorUser.Email,
                    operatorUser.Phone,
                    stationId,
                    cancellationToken);
                operatorUser.AssignedMicrogridNodeId = stationId;
                logger.LogInformation(
                    "Development seed: Updated Grid Operator '{Email}' station assignment to {StationId}.",
                    operatorUser.Email,
                    stationId);
            }
            else
            {
                logger.LogInformation(
                    "Development seed: Grid Operator '{Email}' already exists (ID: {UserId}, AssignedStationId: {StationId}).",
                    operatorUser.Email,
                    operatorUser.Id,
                    operatorUser.AssignedMicrogridNodeId);
            }
        }

        return operatorUser;
    }

    private async Task<UserDetails> EnsureProsumerAsync(
        string password,
        CancellationToken cancellationToken)
    {
        var prosumerUser = await userRepository.GetByEmailAsync(ProsumerEmail, cancellationToken)
                           ?? await userRepository.GetByNicAsync(ProsumerNic, cancellationToken);

        if (prosumerUser is null)
        {
            var now = DateTime.UtcNow;
            prosumerUser = new UserDetails
            {
                FirstName = "Test",
                LastName = "Prosumer",
                Email = ProsumerEmail,
                Phone = "+94772222222",
                PasswordHash = string.Empty,
                Role = UserRole.PROSUMER,
                AccountStatus = AccountStatus.ACTIVE,
                Nic = ProsumerNic,
                AssignedMicrogridNodeId = null,
                CreatedAt = now,
                UpdatedAt = now
            };
            prosumerUser.PasswordHash = passwordHasher.HashPassword(prosumerUser, password);

            await userRepository.CreateAsync(prosumerUser, cancellationToken);
            logger.LogInformation(
                "Development seed: Created Prosumer '{Email}' (ID: {UserId}, NIC: {Nic}).",
                prosumerUser.Email,
                prosumerUser.Id,
                prosumerUser.Nic);
        }
        else
        {
            logger.LogInformation(
                "Development seed: Prosumer '{Email}' already exists (ID: {UserId}, NIC: {Nic}).",
                prosumerUser.Email,
                prosumerUser.Id,
                prosumerUser.Nic);
        }

        return prosumerUser;
    }

    private async Task<IReadOnlyList<EnergyBookingSlot>> EnsureSlotsAsync(
        ObjectId stationId,
        CancellationToken cancellationToken)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        var yesterdayUtc = todayUtc.AddDays(-1);

        var existingSlots = await slotRepository.GetByStationIdAsync(stationId.ToString(), cancellationToken);

        // Slot 1: Tomorrow 10:00 AM - 11:00 AM (for Pending reservation)
        var slot1 = await GetOrCreateSlotAsync(
            existingSlots,
            stationId,
            tomorrowUtc,
            TimeSpan.FromHours(10),
            TimeSpan.FromHours(11),
            50.0,
            cancellationToken);

        // Slot 2: Tomorrow 02:00 PM - 03:00 PM (for Approved reservation)
        var slot2 = await GetOrCreateSlotAsync(
            existingSlots,
            stationId,
            tomorrowUtc,
            TimeSpan.FromHours(14),
            TimeSpan.FromHours(15),
            50.0,
            cancellationToken);

        // Slot 3: Tomorrow 04:00 PM - 05:00 PM (for Cancelled reservation)
        var slot3 = await GetOrCreateSlotAsync(
            existingSlots,
            stationId,
            tomorrowUtc,
            TimeSpan.FromHours(16),
            TimeSpan.FromHours(17),
            50.0,
            cancellationToken);

        // Slot 4: Yesterday 10:00 AM - 11:00 AM (for Completed reservation)
        var slot4 = await GetOrCreateSlotAsync(
            existingSlots,
            stationId,
            yesterdayUtc,
            TimeSpan.FromHours(10),
            TimeSpan.FromHours(11),
            50.0,
            cancellationToken);

        return [slot1, slot2, slot3, slot4];
    }

    private async Task<EnergyBookingSlot> GetOrCreateSlotAsync(
        IReadOnlyList<EnergyBookingSlot> existingSlots,
        ObjectId stationId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        double capacityKw,
        CancellationToken cancellationToken)
    {
        var existing = existingSlots.FirstOrDefault(s =>
            s.Date.Date == date.Date &&
            s.StartTime == startTime &&
            s.EndTime == endTime);

        if (existing is not null)
        {
            logger.LogInformation(
                "Development seed: Slot already exists for {Date:yyyy-MM-dd} {StartTime}-{EndTime} (ID: {SlotId}).",
                date,
                startTime,
                endTime,
                existing.Id);
            return existing;
        }

        var now = DateTime.UtcNow;
        var slot = new EnergyBookingSlot
        {
            StationId = stationId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            CapacityKw = capacityKw,
            IsAvailable = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await slotRepository.CreateAsync(slot, cancellationToken);
        logger.LogInformation(
            "Development seed: Created slot for {Date:yyyy-MM-dd} {StartTime}-{EndTime} (ID: {SlotId}, Capacity: {Capacity} kW).",
            date,
            startTime,
            endTime,
            slot.Id,
            capacityKw);

        return slot;
    }

    private async Task EnsureReservationsAsync(
        string prosumerId,
        string stationId,
        IReadOnlyList<EnergyBookingSlot> slots,
        CancellationToken cancellationToken)
    {
        var existingReservations = await reservationRepository.GetByProsumerAsync(prosumerId, cancellationToken);

        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        var yesterdayUtc = todayUtc.AddDays(-1);

        // 1. PENDING reservation (future - slot 1)
        var pendingSlot = slots[0];
        var pendingRes = existingReservations.FirstOrDefault(r =>
            r.StationId == stationId &&
            r.SlotId == pendingSlot.Id.ToString() &&
            r.Status == ReservationStatus.PENDING);

        if (pendingRes is null)
        {
            var now = DateTime.UtcNow;
            pendingRes = new EnergyReservation
            {
                ProsumerId = prosumerId,
                StationId = stationId,
                SlotId = pendingSlot.Id.ToString(),
                ScheduledTime = tomorrowUtc.Add(pendingSlot.StartTime),
                Status = ReservationStatus.PENDING,
                TransactionReference = GenerateTransactionReference(),
                CreatedAt = now,
                UpdatedAt = now
            };
            await reservationRepository.CreateAsync(pendingRes, cancellationToken);
            logger.LogInformation(
                "Development seed: Created PENDING reservation (ID: {ReservationId}, Ref: {Ref}, Scheduled: {ScheduledTime:u}).",
                pendingRes.Id,
                pendingRes.TransactionReference,
                pendingRes.ScheduledTime);
        }
        else
        {
            logger.LogInformation(
                "Development seed: PENDING reservation already exists (ID: {ReservationId}, Ref: {Ref}).",
                pendingRes.Id,
                pendingRes.TransactionReference);
        }

        // 2. APPROVED reservation (future - slot 2)
        var approvedSlot = slots[1];
        var approvedRes = existingReservations.FirstOrDefault(r =>
            r.StationId == stationId &&
            r.SlotId == approvedSlot.Id.ToString() &&
            r.Status == ReservationStatus.APPROVED);

        if (approvedRes is null)
        {
            var now = DateTime.UtcNow;
            approvedRes = new EnergyReservation
            {
                ProsumerId = prosumerId,
                StationId = stationId,
                SlotId = approvedSlot.Id.ToString(),
                ScheduledTime = tomorrowUtc.Add(approvedSlot.StartTime),
                Status = ReservationStatus.APPROVED,
                TransactionReference = GenerateTransactionReference(),
                CreatedAt = now,
                UpdatedAt = now
            };
            await reservationRepository.CreateAsync(approvedRes, cancellationToken);
            logger.LogInformation(
                "Development seed: Created APPROVED reservation (ID: {ReservationId}, Ref: {Ref}, Scheduled: {ScheduledTime:u}).",
                approvedRes.Id,
                approvedRes.TransactionReference,
                approvedRes.ScheduledTime);
        }
        else
        {
            logger.LogInformation(
                "Development seed: APPROVED reservation already exists (ID: {ReservationId}, Ref: {Ref}).",
                approvedRes.Id,
                approvedRes.TransactionReference);
        }

        // 3. CANCELLED reservation (future - slot 3)
        var cancelledSlot = slots[2];
        var cancelledRes = existingReservations.FirstOrDefault(r =>
            r.StationId == stationId &&
            r.SlotId == cancelledSlot.Id.ToString() &&
            r.Status == ReservationStatus.CANCELLED);

        if (cancelledRes is null)
        {
            var now = DateTime.UtcNow;
            cancelledRes = new EnergyReservation
            {
                ProsumerId = prosumerId,
                StationId = stationId,
                SlotId = cancelledSlot.Id.ToString(),
                ScheduledTime = tomorrowUtc.Add(cancelledSlot.StartTime),
                Status = ReservationStatus.CANCELLED,
                TransactionReference = GenerateTransactionReference(),
                CreatedAt = now,
                UpdatedAt = now
            };
            await reservationRepository.CreateAsync(cancelledRes, cancellationToken);
            logger.LogInformation(
                "Development seed: Created CANCELLED reservation (ID: {ReservationId}, Ref: {Ref}, Scheduled: {ScheduledTime:u}).",
                cancelledRes.Id,
                cancelledRes.TransactionReference,
                cancelledRes.ScheduledTime);
        }
        else
        {
            logger.LogInformation(
                "Development seed: CANCELLED reservation already exists (ID: {ReservationId}, Ref: {Ref}).",
                cancelledRes.Id,
                cancelledRes.TransactionReference);
        }

        // 4. COMPLETED reservation (past - slot 4)
        var completedSlot = slots[3];
        var completedRes = existingReservations.FirstOrDefault(r =>
            r.StationId == stationId &&
            r.SlotId == completedSlot.Id.ToString() &&
            r.Status == ReservationStatus.COMPLETED);

        if (completedRes is null)
        {
            var now = DateTime.UtcNow;
            completedRes = new EnergyReservation
            {
                ProsumerId = prosumerId,
                StationId = stationId,
                SlotId = completedSlot.Id.ToString(),
                ScheduledTime = yesterdayUtc.Add(completedSlot.StartTime),
                Status = ReservationStatus.COMPLETED,
                TransactionReference = GenerateTransactionReference(),
                CreatedAt = now,
                UpdatedAt = now,
                CompletedAt = yesterdayUtc.Add(completedSlot.EndTime)
            };
            await reservationRepository.CreateAsync(completedRes, cancellationToken);
            logger.LogInformation(
                "Development seed: Created COMPLETED reservation (ID: {ReservationId}, Ref: {Ref}, Scheduled: {ScheduledTime:u}).",
                completedRes.Id,
                completedRes.TransactionReference,
                completedRes.ScheduledTime);
        }
        else
        {
            logger.LogInformation(
                "Development seed: COMPLETED reservation already exists (ID: {ReservationId}, Ref: {Ref}).",
                completedRes.Id,
                completedRes.TransactionReference);
        }
    }

    private static string GenerateTransactionReference() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
}
