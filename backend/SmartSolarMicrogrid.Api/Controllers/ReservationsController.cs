using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public ReservationsController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // GET: api/reservations/prosumer/123
        [HttpGet("prosumer/{prosumerId}")]
        public async Task<IActionResult> GetProsumerReservations(string prosumerId)
        {
            var reservations = await _mongoDbService.GetByProsumerAsync(prosumerId);
            return Ok(reservations);
        }

        // POST: api/reservations
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] EnergyReservation reservation)
        {
            DateTime now = DateTime.UtcNow;
            DateTime maxAllowedDate = now.AddDays(7);

            // 7-day rule validation
            if (reservation.ScheduledTime < now || reservation.ScheduledTime > maxAllowedDate)
            {
                return BadRequest(new { message = "Reservations must be scheduled within the allowed 7-day window." });
            }

            reservation.CreatedAt = DateTime.UtcNow;

            await _mongoDbService.CreateAsync(reservation);

            return Ok(new { message = "Reservation created successfully.", data = reservation });
        }

        // PUT: api/reservations/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(string id)
        {
            var existing = await _mongoDbService.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = "Reservation not found." });

            DateTime now = DateTime.UtcNow;
            TimeSpan timeDifference = existing.ScheduledTime - now;

            // 12-hour rule validation
            if (timeDifference.TotalHours < 12)
            {
                return BadRequest(new { message = "Updates and cancellations require at least 12 hours notice." });
            }

            existing.Status = "Cancelled";
            await _mongoDbService.UpdateAsync(id, existing);

            return Ok(new { message = "Reservation cancelled successfully." });
        }

        // GET: api/reservations/search?prosumerId=123&status=Approved&searchTerm=station1
[HttpGet("search")]
public async Task<IActionResult> SearchReservations([FromQuery] string prosumerId, [FromQuery] string? status, [FromQuery] string? searchTerm)
{
    var results = await _mongoDbService.GetFilteredAsync(prosumerId, status, searchTerm);
    return Ok(results);
}

// PUT: api/reservations/{id}
[HttpPut("{id}")]
public async Task<IActionResult> UpdateReservation(string id, [FromBody] EnergyReservation updatedData)
{
    var existing = await _mongoDbService.GetByIdAsync(id);
    if (existing == null) return NotFound(new { message = "Reservation not found." });

    DateTime now = DateTime.UtcNow;
    TimeSpan timeDifference = existing.ScheduledTime - now;

    // 12-hour rule validation for modification
    if (timeDifference.TotalHours < 12)
    {
        return BadRequest(new { message = "Updates require at least 12 hours notice prior to scheduled time." });
    }

    // 7-day rule validation for new requested time
    DateTime maxAllowedDate = now.AddDays(7);
    if (updatedData.ScheduledTime < now || updatedData.ScheduledTime > maxAllowedDate)
    {
        return BadRequest(new { message = "New time must be within the allowed 7-day window." });
    }

    existing.ScheduledTime = updatedData.ScheduledTime;
    existing.SlotId = updatedData.SlotId;

    await _mongoDbService.UpdateAsync(id, existing);

    return Ok(new { message = "Reservation updated successfully.", data = existing });
}
    }
}