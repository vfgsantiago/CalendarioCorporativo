using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.Repository
{
    public class EventoREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public EventoREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarPorCodigo
        /// <summary>
        /// Busca o evento por código
        /// </summary>
        /// <param name="cdEvento"></param>
        /// <returns></returns>
        public async Task<EventoMOD> BuscarPorCodigo(int cdEvento)
        {
            EventoMOD model = new EventoMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT E.CD_EVENTO,
                                               E.TX_TITULO,
                                               E.TX_DESCRICAO,
                                               E.DT_INICIO_EVENTO,
                                               E.DT_FIM_EVENTO,
                                               E.CD_CATEGORIA,
                                               C.TX_TITULO              AS TxCategoria,
                                               C.TX_DESCRICAO           AS TxDescricaoCategoria,
                                               C.CD_ICONE,
                                               I.TX_ICONE,
                                               C.CD_COR,
                                               CC.TX_COR,
                                               E.DT_CADASTRO,
                                               E.CD_USUARIO_CADASTROU,
                                               E.DT_ALTERACAO,
                                               E.CD_USUARIO_ALTEROU,
                                               E.SN_ATIVO
                                          FROM CC_EVENTO           E, 
                                               CC_EVENTO_CATEGORIA C, 
                                               CC_CATEGORIA_COR    CC, 
                                               CC_CATEGORIA_ICONE  I
                                         WHERE E.CD_CATEGORIA = C.CD_CATEGORIA
                                           AND C.CD_COR       = CC.CD_COR
                                           AND C.CD_ICONE     = I.CD_ICONE
                                           AND E.CD_EVENTO    = :cdEvento
                                         ORDER BY E.TX_TITULO DESC";
                    model = await con.QueryFirstOrDefaultAsync<EventoMOD>(query, new { cdEvento });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return model;
        }
        #endregion

        #region BuscarPaginadoComFiltro
        /// <summary>
        /// Busca os eventos de forma paginada, e com filtros
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="itensPorPagina"></param>
        /// <returns>Lista paginada dos eventos</returns>
        public async Task<PaginacaoResposta<EventoMOD>> BuscarPaginadoComFiltro(int pagina, int itensPorPagina, int cdCentroCusto, int? cdEvento, string? txTitulo, int? cdCategoria, DateTime? dtInicioPeriodo, DateTime? dtFimPeriodo)
        {
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                int offset = (pagina - 1) * itensPorPagina;
                var parametros = new DynamicParameters();
                string filtros = "";
                parametros.Add("CdCentroCusto", cdCentroCusto);
                if (cdEvento.HasValue)
                {
                    filtros += " AND E.CD_EVENTO = :CdEvento ";
                    parametros.Add("CdEvento", cdEvento.Value);
                }
                if (txTitulo != null)
                {
                    filtros += " AND UPPER(E.TX_TITULO) LIKE UPPER(:TxTitulo) ";
                    parametros.Add("TxTitulo", $"%{txTitulo}%");
                }
                if (cdCategoria.HasValue)
                {
                    filtros += " AND E.CD_CATEGORIA = :CdCategoria ";
                    parametros.Add("CdCategoria", cdCategoria.Value);
                }
                if (dtInicioPeriodo.HasValue && dtFimPeriodo.HasValue)
                {
                    filtros += @" AND E.DT_INICIO_EVENTO <= :DtFimPeriodo
                                  AND E.DT_FIM_EVENTO   >= :DtInicioPeriodo";
                    parametros.Add("DtInicioPeriodo", dtInicioPeriodo.Value);
                    parametros.Add("DtFimPeriodo", dtFimPeriodo.Value);
                }
                parametros.Add("Offset", offset);
                parametros.Add("ItensPorPagina", itensPorPagina);

                var query = $@"SELECT E.CD_EVENTO,
                                             E.TX_TITULO,
                                             E.TX_DESCRICAO,
                                             E.DT_INICIO_EVENTO,
                                             E.DT_FIM_EVENTO,
                                             E.CD_CATEGORIA,
                                             C.TX_TITULO             AS TxCategoria,
                                             C.TX_DESCRICAO          AS TxDescricaoCategoria,
                                             C.CD_ICONE,             
                                             I.TX_ICONE,             
                                             C.CD_COR,               
                                             CC.TX_COR,              
                                             E.DT_CADASTRO,          
                                             E.CD_USUARIO_CADASTROU, 
                                             U.NOUSUARIO             AS NoUsuarioCadastrou,
                                             T.NOCENTROCUSTO         AS NoCentroCustoUsuarioCadastrou,
                                             UN.NOUNIDADE            AS NoUnidadeUsuarioCadastrou,
                                             E.DT_ALTERACAO,         
                                             E.CD_USUARIO_ALTEROU,   
                                             UU.NOUSUARIO            AS NoUsuarioAlterou,
                                             TT.NOCENTROCUSTO        AS NoCentroCustoUsuarioAlterou,
                                             UNN.NOUNIDADE           AS NoUnidadeUsuarioAlterou,
                                             E.SN_ATIVO
                                        FROM CC_EVENTO           E,
                                             CC_EVENTO_CATEGORIA C,
                                             CC_CATEGORIA_COR    CC,
                                             CC_CATEGORIA_ICONE  I,
                                             USUARIO          U,
                                             USUARIO          UU,
                                             CENTRO_CUSTO     T,
                                             CENTRO_CUSTO     TT,
                                             UNIDADE          UN,
                                             UNIDADE          UNN
                                       WHERE E.CD_CATEGORIA         = C.CD_CATEGORIA
                                         AND C.CD_COR               = CC.CD_COR
                                         AND C.CD_ICONE             = I.CD_ICONE
                                         AND E.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                         AND E.CD_USUARIO_ALTEROU   = UU.CDUSUARIO(+)
                                         AND U.CDCENTROCUSTO        = T.CDCENTROCUSTO
                                         AND U.CDCENTROCUSTO        = :CdCentroCusto
                                         AND UU.CDCENTROCUSTO       = TT.CDCENTROCUSTO(+)
                                         AND T.CDUNIDADE            = UN.CDUNIDADE
                                         AND TT.CDUNIDADE           = UNN.CDUNIDADE(+)
                                          {filtros}
                                       ORDER BY E.DT_ALTERACAO DESC
                                       OFFSET :Offset ROWS FETCH NEXT :ItensPorPagina ROWS ONLY";
                var lista = (await con.QueryAsync<EventoMOD>(query, parametros)).ToList();

                var totalQuery = $@"SELECT
                                              COUNT(*)
                                        FROM CC_EVENTO           E,
                                             CC_EVENTO_CATEGORIA C,
                                             CC_CATEGORIA_COR    CC,
                                             CC_CATEGORIA_ICONE  I,
                                             USUARIO          U,
                                             USUARIO          UU,
                                             CENTRO_CUSTO     T,
                                             CENTRO_CUSTO     TT,
                                             UNIDADE          UN,
                                             UNIDADE          UNN
                                       WHERE E.CD_CATEGORIA         = C.CD_CATEGORIA
                                         AND C.CD_COR               = CC.CD_COR
                                         AND C.CD_ICONE             = I.CD_ICONE
                                         AND E.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                         AND E.CD_USUARIO_ALTEROU   = UU.CDUSUARIO(+)
                                         AND U.CDCENTROCUSTO        = T.CDCENTROCUSTO
                                         AND U.CDCENTROCUSTO        = :CdCentroCusto
                                         AND UU.CDCENTROCUSTO       = TT.CDCENTROCUSTO(+)
                                         AND T.CDUNIDADE            = UN.CDUNIDADE
                                         AND TT.CDUNIDADE           = UNN.CDUNIDADE(+)
                                          {filtros}";
                int totalItens = await con.ExecuteScalarAsync<int>(totalQuery, parametros);

                return new PaginacaoResposta<EventoMOD>
                {
                    Dados = lista,
                    Paginacao = new Paginacao
                    {
                        PaginaAtual = pagina,
                        QuantidadePorPagina = itensPorPagina,
                        TotalItens = totalItens,
                        TotalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina)
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar eventos paginado com filtro.", ex);
            }
        }
        #endregion

        #region BuscarEventosMes
        public async Task<List<EventoMOD>> BuscarEventosMes(int cdCentroCusto)
        {
            using var con = new OracleConnection(_conexaoOracle);
            var query = @"SELECT E.CD_EVENTO,
                                             E.TX_TITULO,
                                             E.TX_DESCRICAO,
                                             E.DT_INICIO_EVENTO,
                                             E.DT_FIM_EVENTO,
                                             E.CD_CATEGORIA,
                                             C.TX_TITULO             AS TxCategoria,
                                             C.TX_DESCRICAO          AS TxDescricaoCategoria,
                                             C.CD_ICONE,             
                                             I.TX_ICONE,             
                                             C.CD_COR,               
                                             CC.TX_COR,              
                                             E.DT_CADASTRO,          
                                             E.CD_USUARIO_CADASTROU, 
                                             U.NOUSUARIO             AS NoUsuarioCadastrou,
                                             T.NOCENTROCUSTO         AS NoCentroCustoUsuarioCadastrou,
                                             UN.NOUNIDADE            AS NoUnidadeUsuarioCadastrou,
                                             E.DT_ALTERACAO,         
                                             E.CD_USUARIO_ALTEROU,   
                                             UU.NOUSUARIO            AS NoUsuarioAlterou,
                                             TT.NOCENTROCUSTO        AS NoCentroCustoUsuarioAlterou,
                                             UNN.NOUNIDADE           AS NoUnidadeUsuarioAlterou,
                                             E.SN_ATIVO
                                        FROM CC_EVENTO           E,
                                             CC_EVENTO_CATEGORIA C,
                                             CC_CATEGORIA_COR    CC,
                                             CC_CATEGORIA_ICONE  I,
                                             USUARIO          U,
                                             USUARIO          UU,
                                             CENTRO_CUSTO     T,
                                             CENTRO_CUSTO     TT,
                                             UNIDADE          UN,
                                             UNIDADE          UNN
                                       WHERE E.CD_CATEGORIA                  = C.CD_CATEGORIA
                                         AND C.CD_COR                        = CC.CD_COR
                                         AND C.CD_ICONE                      = I.CD_ICONE
                                         AND E.CD_USUARIO_CADASTROU          = U.CDUSUARIO
                                         AND E.CD_USUARIO_ALTEROU            = UU.CDUSUARIO(+)
                                         AND U.CDCENTROCUSTO                 = T.CDCENTROCUSTO
                                         AND U.CDCENTROCUSTO                 = :CdCentroCusto
                                         AND UU.CDCENTROCUSTO                = TT.CDCENTROCUSTO(+)
                                         AND T.CDUNIDADE                     = UN.CDUNIDADE
                                         AND TT.CDUNIDADE                    = UNN.CDUNIDADE(+)
                                        AND (TRUNC(E.DT_INICIO_EVENTO, 'MM') = TRUNC(SYSDATE, 'MM')
                                             OR
                                             TRUNC(E.DT_FIM_EVENTO, 'MM')    = TRUNC(SYSDATE, 'MM'))
                                       ORDER BY E.DT_INICIO_EVENTO";

            return (await con.QueryAsync<EventoMOD>(query, new { CdCentroCusto = cdCentroCusto })).ToList();
        }
        #endregion

        #region BuscarEventosPorCategoria
        public async Task<List<EventoMOD>> BuscarEventosPorCategoria(int cdCentroCusto)
        {
            using var con = new OracleConnection(_conexaoOracle);

            var query = @"SELECT C.CD_CATEGORIA     AS CdCategoria,
                                       C.TX_TITULO        AS TxCategoria,
                                       CC.TX_COR          AS TxCor,
                                       CI.TX_ICONE        AS TxIcone,
                                       COUNT(E.CD_EVENTO) AS QtdEventos
                                  FROM CC_EVENTO           E, 
                                       CC_EVENTO_CATEGORIA C, 
                                       CC_CATEGORIA_COR    CC, 
                                       CC_CATEGORIA_ICONE  CI,
                                       USUARIO          U
                                 WHERE C.CD_CATEGORIA         = E.CD_CATEGORIA
                                   AND C.CD_ICONE             = CI.CD_ICONE
                                   AND C.CD_COR               = CC.CD_COR
                                   AND C.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                   AND U.CDCENTROCUSTO        = :CdCentroCusto
                                   AND E.SN_ATIVO             = 'S'
                                 GROUP BY C.CD_CATEGORIA, 
                                          C.TX_TITULO, 
                                          CC.TX_COR, 
                                          CI.TX_ICONE
                                 ORDER BY C.TX_TITULO";

            return (await con.QueryAsync<EventoMOD>(query, new { CdCentroCusto = cdCentroCusto })).ToList();
        }

        #endregion

        #region BuscarCalendario
        public async Task<List<EventoMOD>> BuscarCalendario(DateTime mesReferencia, string? categorias, int? cdCentroCusto)
        {
            var inicioMes = new DateTime(mesReferencia.Year, mesReferencia.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            using var con = new OracleConnection(_conexaoOracle);

            var parametros = new DynamicParameters();
            string filtros = "";
            parametros.Add("DT_INICIO_MES", inicioMes);
            parametros.Add("DT_FIM_MES", fimMes);
            if (cdCentroCusto.HasValue)
            {
                filtros += " AND U.CDCENTROCUSTO = :CdCentroCusto ";
                parametros.Add("CdCentroCusto", cdCentroCusto.Value);
            }
            if (!string.IsNullOrWhiteSpace(categorias))
            {
                var listaCategorias = categorias
                    .Split(',')
                    .Select((c, i) => new { Valor = c.Trim(), Index = i })
                    .ToList();

                var inParams = new List<string>();

                foreach (var cetegoria in listaCategorias)
                {
                    string paramName = $"CAT{cetegoria.Index}";
                    inParams.Add($":{paramName}");
                    parametros.Add(paramName, int.Parse(cetegoria.Valor));
                }

                filtros = $" AND E.CD_CATEGORIA IN ({string.Join(",", inParams)}) ";
            }

            var query = $@"SELECT E.CD_EVENTO,
                                         E.TX_TITULO,
                                         E.TX_DESCRICAO,
                                         E.DT_INICIO_EVENTO,
                                         E.DT_FIM_EVENTO,
                                         E.CD_CATEGORIA,
                                         C.TX_TITULO             AS TxCategoria,
                                         C.TX_DESCRICAO          AS TxDescricaoCategoria,
                                         C.CD_ICONE,             
                                         I.TX_ICONE,             
                                         C.CD_COR,               
                                         CC.TX_COR,              
                                         E.DT_CADASTRO,          
                                         E.CD_USUARIO_CADASTROU, 
                                         U.NOUSUARIO             AS NoUsuarioCadastrou,
                                         T.NOCENTROCUSTO         AS NoCentroCustoUsuarioCadastrou,
                                         UN.NOUNIDADE            AS NoUnidadeUsuarioCadastrou,
                                         E.DT_ALTERACAO,         
                                         E.CD_USUARIO_ALTEROU,   
                                         UU.NOUSUARIO            AS NoUsuarioAlterou,
                                         TT.NOCENTROCUSTO        AS NoCentroCustoUsuarioAlterou,
                                         UNN.NOUNIDADE           AS NoUnidadeUsuarioAlterou,
                                         E.SN_ATIVO
                                    FROM CC_EVENTO           E,
                                         CC_EVENTO_CATEGORIA C,
                                         CC_CATEGORIA_COR    CC,
                                         CC_CATEGORIA_ICONE  I,
                                         USUARIO          U,
                                         USUARIO          UU,
                                         CENTRO_CUSTO     T,
                                         CENTRO_CUSTO     TT,
                                         UNIDADE          UN,
                                         UNIDADE          UNN
                                   WHERE E.CD_CATEGORIA         = C.CD_CATEGORIA
                                     AND C.CD_COR               = CC.CD_COR
                                     AND C.CD_ICONE             = I.CD_ICONE
                                     AND E.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                     AND E.CD_USUARIO_ALTEROU   = UU.CDUSUARIO(+)
                                     AND U.CDCENTROCUSTO        = T.CDCENTROCUSTO
                                     AND UU.CDCENTROCUSTO       = TT.CDCENTROCUSTO(+)
                                     AND T.CDUNIDADE            = UN.CDUNIDADE
                                     AND TT.CDUNIDADE           = UNN.CDUNIDADE(+)
                                     AND E.SN_ATIVO             = 'S'
                                     AND E.DT_INICIO_EVENTO     <= :DT_FIM_MES
                                     AND E.DT_FIM_EVENTO        >= :DT_INICIO_MES
                                     {filtros}
                                ORDER BY E.DT_INICIO_EVENTO, 
                                         E.TX_TITULO";

            var resultado = await con.QueryAsync<EventoMOD>(query, parametros);
            return resultado.ToList();
        }
        #endregion

        #region Cadastrar
        /// <summary>
        /// Cadastrar o evento
        /// </summary>
        /// <param name="eventoMOD"></param>
        /// <returns></returns>
        public async Task<bool> Cadastrar(EventoMOD eventoMOD)
        {
            bool cadastrou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"INSERT INTO CC_EVENTO
                                                 (
                                                  TX_TITULO,
                                                  TX_DESCRICAO,
                                                  CD_CATEGORIA,
                                                  DT_INICIO_EVENTO,
                                                  DT_FIM_EVENTO,
                                                  SN_ATIVO,
                                                  CD_USUARIO_CADASTROU,
                                                  DT_CADASTRO,
                                                  CD_USUARIO_ALTEROU,
                                                  DT_ALTERACAO
                                                 )
                                           VALUES
                                                 (
                                                 :TxTitulo,
                                                 :TxDescricao,
                                                 :CdCategoria,
                                                 :DtInicioEvento,
                                                 :DtFimEvento,
                                                 :SnAtivo,
                                                 :CdUsuarioCadastrou,
                                                 :DtCadastro,
                                                 :CdUsuarioAlterou,
                                                 :DtAlteracao
                                                 )";
                    var parametros = new DynamicParameters(eventoMOD);
                    parametros.Add("TxTitulo", eventoMOD.TxTitulo);
                    parametros.Add("TxDescricao", eventoMOD.TxDescricao);
                    parametros.Add("CdCategoria", eventoMOD.CdCategoria);
                    parametros.Add("DtInicioEvento", eventoMOD.DtInicioEvento);
                    parametros.Add("DtFimEvento", eventoMOD.DtFimEvento);
                    parametros.Add("SnAtivo", eventoMOD.SnAtivo);
                    parametros.Add("CdUsuarioCadastrou", eventoMOD.CdUsuarioCadastrou);
                    parametros.Add("DtCadastro", eventoMOD.DtCadastro);
                    parametros.Add("CdUsuarioAlterou", eventoMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", eventoMOD.DtAlteracao);
                    await con.ExecuteAsync(query, parametros);
                    transacao.Commit();
                    cadastrou = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return cadastrou;
        }
        #endregion

        #region Editar
        /// <summary>
        /// Editar o evento
        /// </summary>
        /// <param name="eventoMOD"></param>
        /// <returns></returns>
        public async Task<bool> Editar(EventoMOD eventoMOD)
        {
            bool editou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE CC_EVENTO
                                        SET
                                            TX_TITULO          = :TxTitulo,
                                            TX_DESCRICAO       = :TxDescricao,
                                            DT_INICIO_EVENTO   = :DtInicioEvento,
                                            DT_FIM_EVENTO      = :DtFimEvento,
                                            CD_CATEGORIA       = :CdCategoria,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO       = :DtAlteracao
                                      WHERE
                                            CD_EVENTO          = :CdEvento";

                    var parametros = new DynamicParameters(eventoMOD);
                    parametros.Add("TxTitulo", eventoMOD.TxTitulo);
                    parametros.Add("TxDescricao", eventoMOD.TxDescricao);
                    parametros.Add("DtInicioEvento", eventoMOD.DtInicioEvento);
                    parametros.Add("DtFimEvento", eventoMOD.DtFimEvento);
                    parametros.Add("CdCategoria", eventoMOD.CdCategoria);
                    parametros.Add("CdUsuarioAlterou", eventoMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", eventoMOD.DtAlteracao);
                    parametros.Add("CdEvento", eventoMOD.CdEvento);
                    await con.ExecuteAsync(query, parametros);
                    transacao.Commit();
                    editou = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return editou;
        }
        #endregion

        #region AlterarStatus
        /// <summary>
        /// Altera o status do evento
        /// </summary>
        /// <param name="eventoMOD"></param>
        /// <returns></returns>
        public bool AlterarStatus(EventoMOD eventoMOD)
        {
            bool alterouStatus = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE CC_EVENTO
                                        SET
                                            SN_ATIVO           = :SnAtivo,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO       = :DtAlteracao
                                      WHERE
                                            CD_EVENTO          = :CdEvento";

                    var parametros = new DynamicParameters(eventoMOD);

                    parametros.Add("SnAtivo", eventoMOD.SnAtivo);
                    parametros.Add("CdUsuarioAlterou", eventoMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", eventoMOD.DtAlteracao);
                    parametros.Add("CdEvento", eventoMOD.CdEvento);
                    con.Execute(query, parametros);
                    transacao.Commit();
                    alterouStatus = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return alterouStatus;
        }
        #endregion

        #region Contar
        /// <summary>
        /// Conta todos os eventos ativos
        /// </summary>
        /// <returns>Total de registros ativos</returns>
        public async Task<int> Contar(int cdCentroCusto)
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT COUNT(*)
                                           FROM CC_EVENTO           E, 
                                                CC_EVENTO_CATEGORIA C, 
                                                USUARIO          U, 
                                                USUARIO          UU
                                          WHERE E.CD_CATEGORIA         = C.CD_CATEGORIA 
                                            AND C.CD_USUARIO_CADASTROU = UU.CDUSUARIO
                                            AND E.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                            AND (U.CDCENTROCUSTO       = :CdCentroCusto 
                                                OR UU.CDCENTROCUSTO    = :CdCentroCusto)";
                    return await con.ExecuteScalarAsync<int>(query, new { CdCentroCusto = cdCentroCusto });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region ContarEventosMes
        public async Task<int> ContarEventosMes(int cdCentroCusto)
        {
            using var con = new OracleConnection(_conexaoOracle);

            var query = @"SELECT COUNT(*)
                                  FROM CC_EVENTO  E, 
                                       USUARIO U
                                 WHERE E.CD_USUARIO_CADASTROU         = U.CDUSUARIO
                                   AND U.CDCENTROCUSTO                = :CdCentroCusto
                                   AND SN_ATIVO                       = 'S'
                                   AND (
                                        TRUNC(DT_INICIO_EVENTO, 'MM') = TRUNC(SYSDATE, 'MM')
                                        OR TRUNC(DT_FIM_EVENTO, 'MM') = TRUNC(SYSDATE, 'MM')
                                   )";

            return await con.ExecuteScalarAsync<int>(query, new { CdCentroCusto = cdCentroCusto });
        }
        #endregion

        #region ContarEventosHoje
        public async Task<int> ContarEventosHoje(int cdCentroCusto)
        {
            using var con = new OracleConnection(_conexaoOracle);

            var query = @"SELECT COUNT(*)
                                   FROM CC_EVENTO  E, 
                                        USUARIO U
                                 WHERE E.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                   AND U.CDCENTROCUSTO        = :CdCentroCusto
                                   AND SN_ATIVO               = 'S'
                                   AND DT_INICIO_EVENTO       < TRUNC(SYSDATE) + 1
                                   AND DT_FIM_EVENTO          >= TRUNC(SYSDATE)";

            return await con.ExecuteScalarAsync<int>(query, new { CdCentroCusto = cdCentroCusto });
        }
        #endregion

        #endregion
    }
}