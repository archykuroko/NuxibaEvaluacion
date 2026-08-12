using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nuxiba.Api.Data;
using Nuxiba.Api.DTOs;
using System.Text;
using System.Globalization;

namespace Nuxiba.Api.Controllers
{
    [ApiController]
    [Route("reports")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("worked-hours/csv")]
        public async Task<IActionResult> GetWorkedHoursCsv()
        {
          
            var movements = await _context.Logins
                .AsNoTracking()
                .OrderBy(l => l.UserId)
                .ThenBy(l => l.Fecha)
                .ToListAsync();


            var hoursByUser = new Dictionary<int, double>();

            foreach (var userGroup in movements.GroupBy(l => l.UserId))
            {
                var orderedMovements = userGroup
                    .OrderBy(l => l.Fecha)
                    .ToList();

                double totalHours = 0;

                for (int i = 0; i < orderedMovements.Count - 1; i++)
                {
                    var current = orderedMovements[i];
                    var next = orderedMovements[i + 1];

                    if (current.TipoMov == 1 && next.TipoMov == 0)
                    {
                        totalHours += (next.Fecha - current.Fecha).TotalHours;
                    }
                }

                hoursByUser[userGroup.Key] = totalHours;
            }

            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            var areas = await _context.Areas
                .AsNoTracking()
                .ToListAsync();

            // Construimos el reporte

            var report = users
            .Where(u => hoursByUser.ContainsKey(u.UserId))
            .Select(u =>
            {
        var areaName = areas
            .Where(a => a.AreaId == u.AreaId)
            .Select(a => a.AreaName)
            .FirstOrDefault() ?? "Sin área";

        var fullName = string.Join(" ",
            new[]
            {
                u.Nombres,
                u.ApellidoPaterno,
                u.ApellidoMaterno
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        return new WorkedHoursReportDto
        {
            Login = u.Login,
            NombreCompleto = fullName,
            Area = areaName,
            TotalHoras = Math.Round(hoursByUser[u.UserId], 2)
        };
    })
    .ToList();


            // Generamos el CSV

            var csv = new StringBuilder();

            csv.AppendLine("Login,NombreCompleto,Area,TotalHoras");

            foreach (var row in report)
            {
                csv.AppendLine(
                    $"{EscapeCsv(row.Login)}," +
                    $"{EscapeCsv(row.NombreCompleto)}," +
                    $"{EscapeCsv(row.Area)}," +
                    $"{row.TotalHoras.ToString(CultureInfo.InvariantCulture)}");
            }




            var bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(
                bytes,
                "text/csv",
                "reporte-horas-trabajadas.csv");

        }



        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

    }


}