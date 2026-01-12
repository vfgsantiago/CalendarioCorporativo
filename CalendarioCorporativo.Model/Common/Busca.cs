namespace CalendarioCorporativo.Model
{
    public class Busca
    {
        public int? Id { get; set; }
        public string? Texto { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public int? Status { get; set; }
        public int? Usuario { get; set; }
    }
}