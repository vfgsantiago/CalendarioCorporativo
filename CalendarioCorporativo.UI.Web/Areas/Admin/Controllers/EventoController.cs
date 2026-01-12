using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalendarioCorporativo.Model;
using CalendarioCorporativo.Repository;
using CalendarioCorporativo.UI.Web.Models;

namespace CalendarioCorporativo.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class EventoController : Controller
    {
        #region Repositories
        private readonly CategoriaREP _repositorioCategoria;
        private readonly EventoREP _repositorioEvento;
        #endregion

        #region Parameters
        private const int _take = 12;
        private int _numeroPagina = 1;
        private int _pagina;
        private int cdCentroCusto => int.Parse(User.FindFirst("CentroCusto")!.Value);
        #endregion

        #region Constructor
        public EventoController(
            CategoriaREP repositorioCategoria,
            EventoREP repositorioEvento)
        {
            _repositorioCategoria = repositorioCategoria;
            _repositorioEvento = repositorioEvento;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index(int? pagina, int? cdEvento, string? txTitulo, int? cdCategoria, DateTime? dtInicioPeriodo, DateTime? dtFimPeriodo)
        {
            int numeroPagina = pagina ?? 1;
            var resultado = await _repositorioEvento.BuscarPaginadoComFiltro(
                numeroPagina, _take, cdCentroCusto, cdEvento, txTitulo, cdCategoria, dtInicioPeriodo, dtFimPeriodo);

            var eventoViewMOD = new EventoViewMOD
            {
                Lista = resultado.Dados,
                QtdTotalDeRegistros = resultado.Paginacao.TotalItens,
                PaginaAtual = resultado.Paginacao.PaginaAtual,
                TotalPaginas = resultado.Paginacao.TotalPaginas
            };

            ViewBag.ListaCategorias = await _repositorioCategoria.Buscar(cdCentroCusto);
            ViewBag.Evento = cdEvento;
            ViewBag.Titulo = txTitulo;
            ViewBag.Categoria = cdCategoria;
            ViewBag.DataInicio = dtInicioPeriodo;
            ViewBag.DataFim = dtFimPeriodo;
            return View("Index", eventoViewMOD);
        }
        #endregion

        #region Cadastrar
        public async Task<IActionResult> Cadastrar()
        {
            EventoMOD eventoMOD = new EventoMOD();
            var eventoViewMOD = eventoMOD.Adapt<EventoViewMOD>();
            ViewBag.Categorias = await _repositorioCategoria.Buscar(cdCentroCusto);
            return View(eventoViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(EventoViewMOD dadosTela)
        {
            var eventoMOD = dadosTela.Adapt<EventoMOD>();
            eventoMOD.TxTitulo = dadosTela.Evento.TxTitulo;
            eventoMOD.TxDescricao = dadosTela.Evento.TxDescricao;
            eventoMOD.CdCategoria = dadosTela.Evento.CdCategoria;
            eventoMOD.DtInicioEvento = dadosTela.Evento.DtInicioEvento;
            eventoMOD.DtFimEvento = dadosTela.Evento.DtFimEvento;
            eventoMOD.SnAtivo = "S";
            eventoMOD.CdUsuarioCadastrou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            eventoMOD.DtCadastro = DateTime.Now;
            eventoMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            eventoMOD.DtAlteracao = DateTime.Now;

            var cadastrou = await _repositorioEvento.Cadastrar(eventoMOD);
            if (cadastrou)
            {
                TempData["Modal-Sucesso"] = "Evento cadastrado com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Modal-Erro"] = "Erro ao cadastrar evento!";
                return View(dadosTela);
            }
        }
        #endregion

        #region EditarConteudo
        public async Task<IActionResult> EditarConteudo(int cdEvento)
        {
            var eventoMOD = await _repositorioEvento.BuscarPorCodigo(cdEvento);
            if (eventoMOD == null)
            {
                TempData["Modal-Erro"] = "Evento não encontrado!";
                return RedirectToAction("Index");
            }
            var eventoViewMOD = eventoMOD.Adapt<EventoViewMOD>();
            ViewBag.Categorias = await _repositorioCategoria.Buscar(cdCentroCusto);
            return View(eventoViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> EditarConteudo(EventoViewMOD dadosTela)
        {
            var eventoMOD = await _repositorioEvento.BuscarPorCodigo(dadosTela.Evento.CdEvento);
            if (eventoMOD == null)
            {
                TempData["Modal-Erro"] = "Evento não encontrado!";
                return RedirectToAction("Index");
            }
            eventoMOD.TxTitulo = dadosTela.Evento.TxTitulo;
            eventoMOD.TxDescricao = dadosTela.Evento.TxDescricao;
            eventoMOD.DtInicioEvento = dadosTela.Evento.DtInicioEvento;
            eventoMOD.DtFimEvento = dadosTela.Evento.DtFimEvento;
            eventoMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            eventoMOD.DtAlteracao = DateTime.Now;

            var editou = await _repositorioEvento.Editar(eventoMOD);
            if (editou)
                TempData["Modal-Sucesso"] = "Conteúdo do evento alterado com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar conteúdo do evento.";

            return RedirectToAction("Index");
        }
        #endregion

        #region EditarCategoria
        public async Task<IActionResult> EditarCategoria(int cdEvento)
        {
            var eventoMOD = await _repositorioEvento.BuscarPorCodigo(cdEvento);
            if (eventoMOD == null)
            {
                TempData["Modal-Erro"] = "Evento não encontrado!";
                return RedirectToAction("Index");
            }
            var eventoViewMOD = eventoMOD.Adapt<EventoViewMOD>();
            ViewBag.Categorias = await _repositorioCategoria.BuscarDiferenteAtual(eventoMOD.CdCategoria, cdCentroCusto);
            return View(eventoViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> EditarCategoria(EventoViewMOD dadosTela)
        {
            var eventoMOD = await _repositorioEvento.BuscarPorCodigo(dadosTela.Evento.CdEvento);
            if(eventoMOD == null)
            {
                TempData["Modal-Erro"] = "Evento não encontrado!";
                return RedirectToAction("Index");
            }
            eventoMOD.CdCategoria = dadosTela.Evento.CdCategoria;
            eventoMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            eventoMOD.DtAlteracao = DateTime.Now;

            var editou = await _repositorioEvento.Editar(eventoMOD);
            if (editou)
                TempData["Modal-Sucesso"] = "Categoria do evento alterada com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar categoria do evento.";

            return RedirectToAction("Index");

        }
        #endregion

        #region AlterarStatus
        [HttpPost]
        public async Task<IActionResult> AlterarStatus(int cdEvento)
        {
            var eventoMOD = await _repositorioEvento.BuscarPorCodigo(cdEvento);
            if (eventoMOD == null)
            {
                TempData["Modal-Erro"] = "Evento não encontrado!";
                return RedirectToAction("Index");
            }
            eventoMOD.SnAtivo = eventoMOD.SnAtivo == "S" ? "N" : "S";
            eventoMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            eventoMOD.DtAlteracao = DateTime.Now;

            var alterouStatus = _repositorioEvento.AlterarStatus(eventoMOD);
            if (alterouStatus)
                TempData["Modal-Sucesso"] = $"Evento {(eventoMOD.SnAtivo == "S" ? "ativado" : "desativado")} com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar status do evento. Tente novamente.";

            return RedirectToAction("Index");
        }
        #endregion

        #endregion
    }
}