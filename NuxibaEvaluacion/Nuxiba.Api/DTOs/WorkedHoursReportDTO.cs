namespace Nuxiba.Api.DTOs
{
    public class WorkedHoursReportDto
    {
        public string Login { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public double TotalHoras { get; set; }
    }
}