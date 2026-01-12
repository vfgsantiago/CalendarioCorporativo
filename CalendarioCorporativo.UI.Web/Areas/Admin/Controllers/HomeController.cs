using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalendarioCorporativo.Repository;
using CalendarioCorporativo.UI.Web.Models;

namespace CalendarioCorporativo.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        #region Repositories
        private readonly EventoREP _repositorioEvento;
        private readonly CategoriaREP _repositorioCategoria;
        #endregion

        #region Parameters
        private int cdCentroCusto => int.Parse(User.FindFirst("CentroCusto")!.Value);
        #endregion

        #region Constructor
        public HomeController(EventoREP repositorioEvento,
            CategoriaREP repositorioCategoria)
        {
            _repositorioEvento = repositorioEvento;
            _repositorioCategoria = repositorioCategoria;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index()
        {
            var viewMOD = new HomeAdminViewMOD();
            viewMOD.QtdEventos = await _repositorioEvento.Contar(cdCentroCusto);
            viewMOD.QtdCategorias = await _repositorioCategoria.Contar(cdCentroCusto);
            viewMOD.QtdEventosMes = await _repositorioEvento.ContarEventosMes(cdCentroCusto);
            viewMOD.QtdEventosDia = await _repositorioEvento.ContarEventosHoje(cdCentroCusto);
            viewMOD.ListaEventosMes = await _repositorioEvento.BuscarEventosMes(cdCentroCusto);
            viewMOD.EventosPorCategoria = await _repositorioEvento.BuscarEventosPorCategoria(cdCentroCusto);
            return View(viewMOD);
        }
        #endregion

        #endregion
    }
}