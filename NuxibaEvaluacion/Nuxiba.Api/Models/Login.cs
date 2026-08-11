namespace Nuxiba.Api.Models
{
    public class Login
    {
        
        public int Id { get; set; } // Este lo agrego al modelo porque lo necesito como clave primaria para el EF y cumplir con los endpoints
        public int UserId { get; set; }
        public int Extension {  get; set; }
        public int TipoMov {  get; set; }
        public DateTime Fecha { get; set; }

    }
}
