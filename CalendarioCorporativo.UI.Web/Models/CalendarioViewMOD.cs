using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.UI.Web.Models
{
    public class CalendarioViewMOD
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string MesDescricao { get; set; } = string.Empty;
        public string DiaSelecionado { get; set; } = string.Empty;
        public DateTime SemanaInicio { get; set; }
        public DateTime SemanaFim { get; set; }
        public CalendarioViewTipo Tipo { get; set; }
        public bool SomenteComEventos { get; set; }
        public List<EventoMOD> Eventos { get; set; } = new();
        public List<CategoriaMOD> Categorias { get; set; } = new();
        public List<CentroCustoMOD> CentroCustos { get; set; } = new();
        public List<CalendarioDiaViewMOD> DiasDoMes { get; set; } = new();
        public List<EventoMOD> ProximosEventos { get; set; } = new();
    }
    public class CalendarioDiaViewMOD
    {
        public DateTime? Data { get; set; }
        public List<EventoMOD> Eventos { get; set; } = new();
    }
    public enum CalendarioViewTipo
    {
        Mensal,
        Semanal,
        Diaria
    }
}