using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Cwiczenia_12_API.Data;

namespace Cwiczenia_12_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly FinalContext _context;

        public ClientsController(FinalContext context)
        {
            _context = context;
        }

        // DELETE: api/clients/{idClient}
        [HttpDelete("{idClient}")]
        public IActionResult DeleteClient(int idClient)
        {
            // 1. Sprawdź, czy klient istnieje
            var client = _context.Clients.FirstOrDefault(c => c.IdClient == idClient);
            if (client == null)
                return NotFound(new { message = $"Client id={idClient} not found." });

            // 2. Sprawdź, czy klient ma przypisane wycieczki
            var hasTrips = _context.ClientTrips.Any(ct => ct.IdClient == idClient);
            if (hasTrips)
                return BadRequest(new { message = "Client has assigned trips; cannot delete." });

            // 3. Usuń klienta
            _context.Clients.Remove(client);
            _context.SaveChanges();

            return NoContent();
        }
    }
}