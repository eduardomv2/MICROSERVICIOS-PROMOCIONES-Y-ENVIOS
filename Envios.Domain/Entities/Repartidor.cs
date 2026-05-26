namespace Envios.Domain.Entities
{
    public class Repartidor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool EstaDisponible { get; set; } = true;
        public int Status { get; set; } = 1;
    }
}