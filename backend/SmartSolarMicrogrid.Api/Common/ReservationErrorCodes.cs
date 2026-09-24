// Defines stable error codes for reservation and transaction workflows.
namespace SmartSolarMicrogrid.Api.Common;

public static class ReservationErrorCodes
{
    public const string InvalidTransactionReference = "INVALID_TRANSACTION_REFERENCE";
    public const string ReservationNotFound = "RESERVATION_NOT_FOUND";
    public const string InvalidReservationId = "INVALID_RESERVATION_ID";
    public const string ReservationNotApproved = "RESERVATION_NOT_APPROVED";
    public const string ReservationCancelled = "RESERVATION_CANCELLED";
    public const string ReservationAlreadyCompleted = "RESERVATION_ALREADY_COMPLETED";
    public const string InvalidProsumerId = "INVALID_PROSUMER_ID";
    public const string InvalidStationId = "INVALID_STATION_ID";
    public const string InvalidSlotId = "INVALID_SLOT_ID";
    public const string InvalidReservationStatus = "INVALID_RESERVATION_STATUS";
    public const string StationNotFound = "STATION_NOT_FOUND";
    public const string SlotNotFound = "SLOT_NOT_FOUND";
    public const string StationInactive = "STATION_INACTIVE";
    public const string SlotNotAvailable = "SLOT_NOT_AVAILABLE";
    public const string SlotStationMismatch = "SLOT_STATION_MISMATCH";
    public const string ReservationOutsideAllowedWindow = "RESERVATION_OUTSIDE_ALLOWED_WINDOW";
    public const string ReservationChangeTooLate = "RESERVATION_CHANGE_TOO_LATE";
    public const string InvalidReservationState = "INVALID_RESERVATION_STATE";
}
