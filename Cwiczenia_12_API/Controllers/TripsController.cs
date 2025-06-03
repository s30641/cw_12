using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cwiczenia_12_API.Data;
using Cwiczenia_12_API.Models;
using Cwiczenia_12_API.DTOs;

namespace Cwiczenia_12_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly FinalContext _context;

        public TripsController(FinalContext context)
        {
            _context = context;
        }

        // GET: api/trips?page={page}&pageSize={pageSize}
        [HttpGet]
        public IActionResult GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Trips
                .Include(t => t.IdCountries)
                .Include(t => t.ClientTrips)
                    .ThenInclude(ct => ct.IdClientNavigation)
                .OrderByDescending(t => t.DateFrom);

            var totalCount = query.Count();
            var allPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var tripsOnPage = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var tripsDto = tripsOnPage.Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryDto
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            }).ToList();

            var response = new
            {
                pageNum = page,
                pageSize = pageSize,
                allPages = allPages,
                trips = tripsDto
            };

            return Ok(response);
        }

        // POST: api/trips/{idTrip}/clients
        [HttpPost("{idTrip}/clients")]
        public IActionResult AssignClient(int idTrip, [FromBody] NewClientDto dto)
        {
            // 1. Sprawdź, czy wycieczka istnieje
            var trip = _context.Trips.FirstOrDefault(t => t.IdTrip == idTrip);
            if (trip == null)
                return NotFound(new { message = $"Trip id={idTrip} not found." });

            // 2. Czy DateFrom w przyszłości?
            if (trip.DateFrom <= DateTime.UtcNow)
                return BadRequest(new { message = $"Trip id={idTrip} already started or finished." });

            // 3. Sprawdź, czy klient o tym Pesel już istnieje
            var existingClient = _context.Clients.FirstOrDefault(c => c.Pesel == dto.Pesel);
            if (existingClient != null)
            {
                return Conflict(new { message = $"Client with Pesel={dto.Pesel} already exists." });
            }

            // 4. Utwórz nowego klienta
            var client = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Pesel = dto.Pesel
            };
            _context.Clients.Add(client);
            _context.SaveChanges(); // aby uzyskać IdClient

            // 5. Utwórz wpis w ClientTrip
            var clientTrip = new ClientTrip
            {
                IdClient = client.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.UtcNow,
                PaymentDate = dto.PaymentDate
            };
            _context.ClientTrips.Add(clientTrip);
            _context.SaveChanges();

            return StatusCode(201);
        }
    }
}
