using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.UI.Web.Models
{
    public class EventoViewMOD
    {
        public EventoMOD Evento { get; set; } = new EventoMOD();
        public List<EventoMOD> Lista { get; set; } = new List<EventoMOD>();
        public int QtdTotalDeRegistros { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
    }
}