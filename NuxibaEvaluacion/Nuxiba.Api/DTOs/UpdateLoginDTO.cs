namespace Nuxiba.Api.DTOs
{
    public class UpdateLoginDto
    {
        public int UserId { get; set; }

        public int Extension { get; set; }

        public int TipoMov { get; set; }

        public DateTime Fecha { get; set; }
    }
}