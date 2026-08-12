using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nuxiba.Api.Data;
using Nuxiba.Api.Models;
using Nuxiba.Api.DTOs;

namespace Nuxiba.Api.Controllers
{
    [ApiController]
    [Route("logins")]
    public class LoginsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoginsController(AppDbContext context)
        {
            _context = context;
        }
         
        // Usamos el GET para traer todos los registros de la base de datos "ccloglogin"

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Login>>> GetLogins()
        {
            var logins = await _context.Logins
                .AsNoTracking()
                .ToListAsync();

            return Ok(logins);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Login>> GetLoginById(int id)
        {
            var login = await _context.Logins
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (login == null)
            {
                return NotFound($"No se encontró el registro con Id {id}");
            }

            return Ok(login);
        }



        // Método POST para ingresar registros a la tabla "ccloglogin"

        [HttpPost]
        public async Task<ActionResult<Login>> CreateLogin(CreateLoginDto request)
        {
            // TipoMov solo puede ser 0 = logout o 1 = login
            if (request.TipoMov != 0 && request.TipoMov != 1)
            {
                return BadRequest("TipoMov debe ser 0 (logout) o 1 (login)");
            }

            // Verificamos que la fecha sea correcta
            if (request.Fecha == default)
            {
                return BadRequest("La fecha es obligatoria y debe ser válida");
            }

            // Verificamos que la fecha no sea futura
            if (request.Fecha > DateTime.Now)
            {
                return BadRequest("La fecha no puede ser posterior a la fecha actual");
            }

            // El usuario debe existir en ccUsers
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == request.UserId);

            if (!userExists)
            {
                return BadRequest($"El usuario con UserId {request.UserId} no existe");
            }

            // Buscar el último movimiento del usuario
            var lastMovement = await _context.Logins
                .Where(l => l.UserId == request.UserId)
                .OrderByDescending(l => l.Fecha)
                .FirstOrDefaultAsync();

            if (lastMovement != null)
            {
                // Evitar dos logins consecutivos
                if (request.TipoMov == 1 && lastMovement.TipoMov == 1)
                {
                    return BadRequest("El usuario ya tiene un login activo. Debe registrar un logout antes de otro login");
                }

                // Evitar dos logouts consecutivos
                if (request.TipoMov == 0 && lastMovement.TipoMov == 0)
                {
                    return BadRequest("No se puede registrar un logout porque el último movimiento del usuario ya es un logout");
                }
            }
            else
            {
                // Si nunca ha tenido movimientos, no puede empezar con logout
                if (request.TipoMov == 0)
                {
                    return BadRequest("No se puede registrar un logout sin un login previo");
                }
            }

            var login = new Login
            {
                UserId = request.UserId,
                Extension = request.Extension,
                TipoMov = request.TipoMov,
                Fecha = request.Fecha
            };

            _context.Logins.Add(login);
            await _context.SaveChangesAsync();

        
            return CreatedAtAction(
                nameof(GetLoginById),
                new { id = login.Id },
                login);

        }



        // Método PUT para actualizar un registro 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLogin(int id, UpdateLoginDto request)
        {
            var login = await _context.Logins.FindAsync(id);

            if (login == null)
            {
                return NotFound($"No se encontró el registro con Id {id}");
            }

            if (request.TipoMov != 0 && request.TipoMov != 1)
            {
                return BadRequest("TipoMov debe ser 0 (logout) o 1 (login)");
            }

            if (request.Fecha == default)
            {
                return BadRequest("La fecha es obligatoria y debe ser válida");
            }

            if (request.Fecha > DateTime.Now)
            {
                return BadRequest("La fecha no puede ser posterior a la fecha actual");
            }

            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == request.UserId);

            if (!userExists)
            {
                return BadRequest($"El usuario con UserId {request.UserId} no existe");
            }

            var previousMovement = await _context.Logins
                .AsNoTracking()
                .Where(l =>
                    l.UserId == request.UserId &&
                    l.Id != id &&
                    l.Fecha < request.Fecha)
                .OrderByDescending(l => l.Fecha)
                .FirstOrDefaultAsync();

            var nextMovement = await _context.Logins
                .AsNoTracking()
                .Where(l =>
                    l.UserId == request.UserId &&
                    l.Id != id &&
                    l.Fecha > request.Fecha)
                .OrderBy(l => l.Fecha)
                .FirstOrDefaultAsync();

            if (previousMovement == null && request.TipoMov == 0)
            {
                return BadRequest("El primer movimiento de un usuario no puede ser un logout");
            }

            if (previousMovement != null &&
                previousMovement.TipoMov == request.TipoMov)
            {
                return BadRequest(
                    "El movimiento no puede ser del mismo tipo que el movimiento anterior");
            }

            if (nextMovement != null &&
                nextMovement.TipoMov == request.TipoMov)
            {
                return BadRequest(
                    "El movimiento no puede ser del mismo tipo que el movimiento siguiente");
            }

            login.UserId = request.UserId;
            login.Extension = request.Extension;
            login.TipoMov = request.TipoMov;
            login.Fecha = request.Fecha;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // Método DELETE para borrar algun registro de la BD

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogin(int id)
        {
            var login = await _context.Logins.FindAsync(id);

            if (login == null)
            {
                return NotFound($"No se encontró el registro con Id {id}");
            }

            _context.Logins.Remove(login);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}