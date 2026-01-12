using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.UI.Web.Models
{
    public class HomeAdminViewMOD
    {
        public int QtdCategorias { get; set; }
        public int QtdEventos { get; set; }
        public int QtdEventosMes { get; set; }
        public int QtdEventosDia { get; set; }
        public List<EventoMOD> ListaEventosMes { get; set; } = new();
        public List<EventoMOD> EventosPorCategoria { get; set; } = new();
    }
}