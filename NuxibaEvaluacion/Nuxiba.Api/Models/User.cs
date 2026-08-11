namespace Nuxiba.Api.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Login { get; set; } = string.Empty;

        public string Nombres { get; set; } = string.Empty;

        public string ApellidoPaterno { get; set; } = string.Empty;

        public string? ApellidoMaterno { get; set; }

        public int AreaId { get; set; }
    }
}