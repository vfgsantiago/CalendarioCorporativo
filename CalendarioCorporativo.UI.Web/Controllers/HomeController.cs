using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using CalendarioCorporativo.Model;
using CalendarioCorporativo.Repository;
using CalendarioCorporativo.UI.Web.Models;

namespace CalendarioCorporativo.UI.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        #region Repositories
        private readonly EventoREP _repositorioEvento;
        private readonly CategoriaREP _repositorioCategoria;
        private readonly CentroCustoREP _repositorioCentroCusto;
        #endregion

        #region Constructor
        public HomeController(
            EventoREP repositorioEvento,
            CategoriaREP repositorioCategoria,
            CentroCustoREP repositorioCentroCusto)
        {
            _repositorioEvento = repositorioEvento;
            _repositorioCategoria = repositorioCategoria;
            _repositorioCentroCusto = repositorioCentroCusto;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index(
            int? ano,
            int? mes,
            DateTime? data,
            CalendarioViewTipo tipo = CalendarioViewTipo.Mensal,
            bool somenteComEventos = false,
            string? categorias = null,
            int? cdCentroCusto = null)
        {
            DateTime dataBase = data ??
                new DateTime(
                    ano ?? DateTime.Today.Year,
                    mes ?? DateTime.Today.Month,
                    1);

            var eventos = await _repositorioEvento.BuscarCalendario(dataBase, categorias, cdCentroCusto);
            var listaCategorias = await _repositorioCategoria.BuscarPorCentroCusto(cdCentroCusto);
            var dias = new List<CalendarioDiaViewMOD>();
            var listraCentroCustos = await _repositorioCentroCusto.BuscarComEvento();

            #region VISÃO SEMANAL
            if (tipo == CalendarioViewTipo.Semanal)
            {
                var inicioSemana = dataBase.AddDays(-(int)dataBase.DayOfWeek);
                var fimSemana = inicioSemana.AddDays(6);

                for (var d = inicioSemana; d <= fimSemana; d = d.AddDays(1))
                {
                    dias.Add(new CalendarioDiaViewMOD
                    {
                        Data = d,
                        Eventos = eventos
                            .Where(e => e.DtInicioEvento!.Value.Date <= d &&
                                        e.DtFimEvento!.Value.Date >= d)
                            .ToList()
                    });
                }

                return View(BuildViewModel(
                    dataBase,
                    tipo,
                    eventos,
                    listaCategorias,
                    listraCentroCustos,
                    dias,
                    somenteComEventos,
                    inicioSemana,
                    fimSemana));
            }
            #endregion

            #region VISÃO DIÁRIA
            if (tipo == CalendarioViewTipo.Diaria)
            {
                var eventosDoDia = eventos
                    .Where(e => e.DtInicioEvento!.Value.Date <= dataBase &&
                                e.DtFimEvento!.Value.Date >= dataBase)
                    .ToList();

                if (!somenteComEventos || eventosDoDia.Any())
                {
                    dias.Add(new CalendarioDiaViewMOD
                    {
                        Data = dataBase,
                        Eventos = eventosDoDia
                    });
                }

                return View(BuildViewModel(
                    dataBase,
                    tipo,
                    eventos,
                    listaCategorias,
                    listraCentroCustos,
                    dias,
                    somenteComEventos));
            }
            #endregion

            #region VISÃO MENSAL
            var primeiroDiaMes = new DateTime(dataBase.Year, dataBase.Month, 1);
            var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);
            int offsetInicio = (int)primeiroDiaMes.DayOfWeek;

            for (int i = 0; i < offsetInicio; i++)
            {
                dias.Add(new CalendarioDiaViewMOD
                {
                    Data = null,
                    Eventos = new List<EventoMOD>()
                });
            }

            for (var dia = primeiroDiaMes; dia <= ultimoDiaMes; dia = dia.AddDays(1))
            {
                var eventosDoDia = eventos
                    .Where(e => e.DtInicioEvento!.Value.Date <= dia &&
                                e.DtFimEvento!.Value.Date >= dia)
                    .ToList();

                dias.Add(new CalendarioDiaViewMOD
                {
                    Data = dia,
                    Eventos = eventosDoDia
                });
            }

            return View(BuildViewModel(
                dataBase,
                tipo,
                eventos,
                listaCategorias,
                listraCentroCustos,
                dias,
                somenteComEventos));
            #endregion
        }
        #endregion

        #region BuildViewModel
        private CalendarioViewMOD BuildViewModel(
            DateTime dataBase,
            CalendarioViewTipo tipo,
            List<EventoMOD> eventos,
            List<CategoriaMOD> categorias,
            List<CentroCustoMOD> centroCustos,
            List<CalendarioDiaViewMOD> dias,
            bool somenteComEventos,
            DateTime? semanaInicio = null,
            DateTime? semanaFim = null)
        {
            return new CalendarioViewMOD
            {
                Ano = dataBase.Year,
                Mes = dataBase.Month,
                DiaSelecionado = dataBase.ToString("dddd, dd MMM yyyy"),
                MesDescricao = dataBase.ToString("MMMM yyyy"),
                Tipo = tipo,
                SomenteComEventos = somenteComEventos,
                Eventos = eventos,
                Categorias = categorias,
                CentroCustos = centroCustos,
                DiasDoMes = dias,
                SemanaInicio = semanaInicio ?? dataBase,
                SemanaFim = semanaFim ?? dataBase,
                ProximosEventos = eventos
                    .Where(e => e.DtInicioEvento >= DateTime.Today &&
                                e.DtInicioEvento <= DateTime.Today.AddDays(10))
                    .OrderBy(e => e.DtInicioEvento)
                    .Take(10)
                    .ToList()
            };
        }
        #endregion
        
        #region Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewMOD
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
        #endregion

        #region ExportarCalendario
        [HttpGet]
        public async Task<IActionResult> ExportarCalendarioIcs(
            int? ano,
            int? mes,
            DateTime? data,
            string? categorias,
            int? cdCentroCusto)
        {
            DateTime dataBase = data ??
                new DateTime(
                    ano ?? DateTime.Today.Year,
                    mes ?? DateTime.Today.Month,
                    1);

            var eventos = await _repositorioEvento.BuscarCalendario(
                dataBase,
                categorias,
                cdCentroCusto);

            if (!eventos.Any())
                return BadRequest("Nenhum evento para exportar.");

            var ics = GerarIcs(eventos);

            return File(
                new System.Text.UTF8Encoding().GetBytes(ics),
                "text/calendar");
        }

        #region GerarIcs
        private string GerarIcs(List<EventoMOD> eventos)
        {
            var sb = new StringBuilder();

            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//Calendario Corporativo//PT-BR");

            foreach (var e in eventos)
            {
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{Guid.NewGuid()}@calendarcorp");
                sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");

                sb.AppendLine($"DTSTART:{e.DtInicioEvento.Value.ToUniversalTime():yyyyMMddTHHmmssZ}");
                sb.AppendLine($"DTEND:{e.DtFimEvento.Value.ToUniversalTime():yyyyMMddTHHmmssZ}");

                sb.AppendLine($"SUMMARY:{EscapeIcs(e.TxTitulo)}");
                sb.AppendLine($"DESCRIPTION:{EscapeIcs(e.TxDescricao)}");

                sb.AppendLine("END:VEVENT");
            }

            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }
        #endregion

        #region EscapeIcs
        private string EscapeIcs(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "";

            return valor
                .Replace(@"\", @"\\")
                .Replace(";", @"\;")
                .Replace(",", @"\,")
                .Replace("\n", @"\n");
        }
        #endregion

        #endregion

        #endregion
    }
}