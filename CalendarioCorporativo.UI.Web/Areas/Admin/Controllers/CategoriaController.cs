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
    public class CategoriaController : Controller
    {
        #region Repositories
        private readonly CategoriaREP _repositorioCategoria;
        private readonly IconeREP _repositorioIcone;
        private readonly CorREP _repositorioCor;
        #endregion

        #region Parameters
        private const int _take = 12;
        private int _numeroPagina = 1;
        private int _pagina;
        private int cdCentroCusto => int.Parse(User.FindFirst("CentroCusto")!.Value);
        #endregion

        #region Constructor
        public CategoriaController(
            CategoriaREP repositorioCategoria,
            IconeREP repositorioIcone,
            CorREP repositorioCor)
        {
            _repositorioCategoria = repositorioCategoria;
            _repositorioIcone = repositorioIcone;
            _repositorioCor = repositorioCor;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index(int? pagina, string? filtro)
        {
            int numeroPagina = pagina ?? 1;
            var resultado = await _repositorioCategoria.BuscarPaginadoComFiltro(numeroPagina, _take, cdCentroCusto, filtro);

            var categoriaViewMOD = new CategoriaViewMOD
            {
                Lista = resultado.Dados,
                QtdTotalDeRegistros = resultado.Paginacao.TotalItens,
                PaginaAtual = resultado.Paginacao.PaginaAtual,
                TotalPaginas = resultado.Paginacao.TotalPaginas
            };
            ViewBag.Filtro = filtro;
            ViewBag.Titulo = "Categorias de Eventos";
            return View("Index", categoriaViewMOD);
        }
        #endregion

        #region Cadastrar
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cadastrar()
        {
            CategoriaMOD categoriaMOD = new CategoriaMOD();
            var categoriaViewMOD = categoriaMOD.Adapt<CategoriaViewMOD>();
            ViewBag.Icones = await _repositorioIcone.BuscarNaoUsados();
            ViewBag.Cores = await _repositorioCor.BuscarNaoUsados();
            return View(categoriaViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(CategoriaViewMOD dadosTela)
        {
            if (dadosTela.Categoria.CdIcone == 0 || dadosTela.Categoria.CdCor == 0)
            {
                TempData["Modal-Erro"] = "Selecione um ícone e uma cor!";
                return View(dadosTela);
            }
            var categoriaMOD = dadosTela.Adapt<CategoriaMOD>();
            categoriaMOD.TxTitulo = dadosTela.Categoria.TxTitulo;
            categoriaMOD.TxDescricao = dadosTela.Categoria.TxDescricao;
            categoriaMOD.CdIcone = dadosTela.Categoria.CdIcone;
            categoriaMOD.CdCor = dadosTela.Categoria.CdCor;
            categoriaMOD.SnAtivo = "S";
            categoriaMOD.CdUsuarioCadastrou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            categoriaMOD.DtCadastro = DateTime.Now;
            categoriaMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            categoriaMOD.DtAlteracao = DateTime.Now;
            
            var cadastrou = _repositorioCategoria.Cadastrar(categoriaMOD);
            if (cadastrou)
            {
                await AtualizarUsoIconeCor(0,0,categoriaMOD.CdIcone,categoriaMOD.CdCor);
                TempData["Modal-Sucesso"] = "Categoria de evento cadastrada com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Modal-Erro"] = "Erro ao cadastrar categoria de evento!";
                return View(dadosTela);
            }
        }
        #endregion

        #region EditarConteudo
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarConteudo(int cdCategoria)
        {
            var categoriaMOD = await _repositorioCategoria.BuscarPorCodigo(cdCategoria);
            if (categoriaMOD == null)
            {
                TempData["Modal-Erro"] = "Categoria de evento não encontrada!";
                return RedirectToAction("Index");
            }
            var categoriaViewMOD = categoriaMOD.Adapt<CategoriaViewMOD>();
            return View(categoriaViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> EditarConteudo(CategoriaViewMOD dadosTela)
        {
            var categoriaMOD = await _repositorioCategoria.BuscarPorCodigo(dadosTela.Categoria.CdCategoria);
            if (categoriaMOD == null)
            {
                TempData["Modal-Erro"] = "Categoria de evento não encontrada!";
                return RedirectToAction("Index");
            }
            categoriaMOD.TxTitulo = dadosTela.Categoria.TxTitulo;
            categoriaMOD.TxDescricao = dadosTela.Categoria.TxDescricao;
            categoriaMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            categoriaMOD.DtAlteracao = DateTime.Now;

            var editou = await _repositorioCategoria.Editar(categoriaMOD);
            if (editou)
                TempData["Modal-Sucesso"] = "Conteúdo da categoria alterado com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar conteúdo da categoria.";

            return RedirectToAction("Index");
        }
        #endregion

        #region EditarMarcador
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarMarcador(int cdCategoria)
        {
            var categoriaMOD = await _repositorioCategoria.BuscarPorCodigo(cdCategoria);
            if (categoriaMOD == null)
            {
                TempData["Modal-Erro"] = "Categoria de evento não encontrada!";
                return RedirectToAction("Index");
            }
            ViewBag.Icones = await _repositorioIcone.BuscarNaoUsados();
            ViewBag.Cores = await _repositorioCor.BuscarNaoUsados();
            var categoriaViewMOD = categoriaMOD.Adapt<CategoriaViewMOD>();
            return View(categoriaViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> EditarMarcador(CategoriaViewMOD dadosTela)
        {
            var categoriaMOD = await _repositorioCategoria.BuscarPorCodigo(dadosTela.Categoria.CdCategoria);
            if (categoriaMOD == null)
            {
                TempData["Modal-Erro"] = "Categoria de evento não encontrada!";
                return RedirectToAction("Index");
            }
            await AtualizarUsoIconeCor(categoriaMOD.CdIcone, categoriaMOD.CdCor, dadosTela.Categoria.CdIcone, dadosTela.Categoria.CdCor);
            categoriaMOD.CdIcone = dadosTela.Categoria.CdIcone;
            categoriaMOD.CdCor = dadosTela.Categoria.CdCor;
            categoriaMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            categoriaMOD.DtAlteracao = DateTime.Now;

            var editou = await _repositorioCategoria.Editar(categoriaMOD);
            if (editou)
                TempData["Modal-Sucesso"] = "Marcador da categoria alterado com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar marcador da categoria.";

            return RedirectToAction("Index");
        }
        #endregion

        #region AlterarStatus
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AlterarStatus(int cdCategoria)
        {
            var categoriaMOD = await _repositorioCategoria.BuscarPorCodigo(cdCategoria);
            if (categoriaMOD == null)
            {
                TempData["Modal-Erro"] = "Categoria não encontrada!";
                return RedirectToAction("Index");
            }
            categoriaMOD.SnAtivo = categoriaMOD.SnAtivo == "S" ? "N" : "S";
            categoriaMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            categoriaMOD.DtAlteracao = DateTime.Now;

            var alterouStatus = _repositorioCategoria.AlterarStatus(categoriaMOD);
            if (alterouStatus)
                TempData["Modal-Sucesso"] = $"Categoria {(categoriaMOD.SnAtivo == "S" ? "ativado" : "desativado")} com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar status da categoria. Tente novamente.";

            return RedirectToAction("Index");
        }
        #endregion

        #region Helpers

        #region AtualizarUsoIconeCor
        private async Task AtualizarUsoIconeCor(int cdIconeAntigo, int cdCorAntiga, int cdIconeNovo, int cdCorNova)
        {
            #region Icone
            if (cdIconeAntigo != cdIconeNovo)
            {
                if (cdIconeAntigo > 0)
                {
                    var iconeAntigo = await _repositorioIcone.BuscarPorCodigo(cdIconeAntigo);
                    if (iconeAntigo != null)
                    {
                        iconeAntigo.SnUsado = "N";
                        _repositorioIcone.Atualizar(iconeAntigo);
                    }
                }

                var iconeNovo = await _repositorioIcone.BuscarPorCodigo(cdIconeNovo);
                if (iconeNovo != null)
                {
                    iconeNovo.SnUsado = "S";
                    _repositorioIcone.Atualizar(iconeNovo);
                }
            }
            #endregion

            #region Cor
            if (cdCorAntiga != cdCorNova)
            {
                if (cdCorAntiga > 0)
                {
                    var corAntiga = await _repositorioCor.BuscarPorCodigo(cdCorAntiga);
                    if (corAntiga != null)
                    {
                        corAntiga.SnUsado = "N";
                        _repositorioCor.Atualizar(corAntiga);
                    }
                }

                var corNova = await _repositorioCor.BuscarPorCodigo(cdCorNova);
                if (corNova != null)
                {
                    corNova.SnUsado = "S";
                    _repositorioCor.Atualizar(corNova);
                }
            }
            #endregion
        }
        #endregion

        #endregion

        #endregion
    }
}