namespace SmartSolarMicrogrid.Api.Common;

public static class ReservationErrorCodes
{
    public const string InvalidTransactionReference = "INVALID_TRANSACTION_REFERENCE";
    public const string ReservationNotFound = "RESERVATION_NOT_FOUND";
    public const string ReservationNotApproved = "RESERVATION_NOT_APPROVED";
    public const string ReservationCancelled = "RESERVATION_CANCELLED";
    public const string ReservationAlreadyCompleted = "RESERVATION_ALREADY_COMPLETED";
}
