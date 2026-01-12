using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.UI.Web.Models
{
    public class CategoriaViewMOD
    {
        public CategoriaMOD Categoria { get; set; } = new CategoriaMOD();
        public List<CategoriaMOD> Lista { get; set; } = new List<CategoriaMOD>();
        public int QtdTotalDeRegistros { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
    }
}