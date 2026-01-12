using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.Repository
{
    public class CategoriaREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public CategoriaREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region Buscar
        /// <summary>
        /// Busca todos as categorias de eventos ativas com centro de custo
        /// </summary>
        /// <param name="cdCentroCusto"></param>
        /// <returns></returns>
        public async Task<List<CategoriaMOD>> Buscar(int cdCentroCusto)
        {
            List<CategoriaMOD> lista = new List<CategoriaMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT C.CD_CATEGORIA,
                                               C.TX_TITULO,
                                               C.TX_DESCRICAO,
                                               C.CD_ICONE,
                                               I.TX_ICONE,
                                               C.CD_COR,
                                               CC.TX_COR,
                                               C.DT_CADASTRO,
                                               C.CD_USUARIO_CADASTROU,
                                               C.DT_ALTERACAO,
                                               C.CD_USUARIO_ALTEROU,
                                               C.SN_ATIVO
                                          FROM CC_EVENTO_CATEGORIA C,
                                               CC_CATEGORIA_COR    CC,
                                               CC_CATEGORIA_ICONE  I,
                                               USUARIO          U
                                         WHERE C.SN_ATIVO             = 'S'
                                           AND C.CD_COR               = CC.CD_COR
                                           AND C.CD_ICONE             = I.CD_ICONE
                                           AND C.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                           AND U.CDCENTROCUSTO        = :CdCentroCusto
                                         ORDER BY C.TX_TITULO ASC";
                    lista = (await con.QueryAsync<CategoriaMOD>(query,new { CdCentroCusto = cdCentroCusto })).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion
        
        #region BuscarTudo
        /// <summary>
        /// Busca todos as categorias por centro de custo
        /// </summary>
        /// <param name="cdCentroCusto"></param>
        /// <returns></returns>
        public async Task<List<CategoriaMOD>> BuscarPorCentroCusto(int? cdCentroCusto)
        {
            List<CategoriaMOD> lista = new List<CategoriaMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT DISTINCT 
                                                C.CD_CATEGORIA,
                                                C.TX_TITULO,
                                                C.TX_DESCRICAO,
                                                CC.TX_COR,
                                                CI.TX_ICONE
                                           FROM CC_EVENTO           E, 
                                                CC_EVENTO_CATEGORIA C,
                                                CC_CATEGORIA_COR    CC, 
                                                CC_CATEGORIA_ICONE  CI, 
                                                USUARIO          U
                                          WHERE C.CD_CATEGORIA     = E.CD_CATEGORIA
                                            AND CC.CD_COR          = C.CD_COR
                                            AND CI.CD_ICONE        = C.CD_ICONE
                                            AND U.CDUSUARIO        = E.CD_USUARIO_CADASTROU
                                            AND E.SN_ATIVO         = 'S'
                                            AND (:CdCentroCusto   IS NULL 
                                                OR U.CDCENTROCUSTO = :CdCentroCusto)
                                          ORDER BY C.TX_TITULO";
                    lista = (await con.QueryAsync<CategoriaMOD>(query, new {CdCentroCusto = cdCentroCusto})).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region BuscarPorCodigo
        /// <summary>
        /// Busca a categoria por código
        /// </summary>
        /// <param name="cdCategoria"></param>
        /// <returns></returns>
        public async Task<CategoriaMOD> BuscarPorCodigo(int cdCategoria)
        {
            CategoriaMOD model = new CategoriaMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT C.CD_CATEGORIA,
                                               C.TX_TITULO,
                                               C.TX_DESCRICAO,
                                               C.CD_ICONE,
                                               I.TX_ICONE,
                                               C.CD_COR,
                                               CC.TX_COR,
                                               C.DT_CADASTRO,
                                               C.CD_USUARIO_CADASTROU,
                                               C.DT_ALTERACAO,
                                               C.CD_USUARIO_ALTEROU,
                                               C.SN_ATIVO
                                        FROM CC_EVENTO_CATEGORIA C,
                                             CC_CATEGORIA_COR    CC,
                                             CC_CATEGORIA_ICONE  I
                                        WHERE C.CD_COR     = CC.CD_COR
                                          AND C.CD_ICONE   = I.CD_ICONE
                                          AND CD_CATEGORIA = :cdCategoria";
                    model = await con.QueryFirstOrDefaultAsync<CategoriaMOD>(query, new { cdCategoria });
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
        /// Busca as categorias de forma paginada, e com filtros
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="itensPorPagina"></param>
        /// <returns>Lista paginada das categorias de eventos</returns>
        public async Task<PaginacaoResposta<CategoriaMOD>> BuscarPaginadoComFiltro(int pagina, int itensPorPagina, int cdCentroCusto, string? filtro)
        {
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                int offset = (pagina - 1) * itensPorPagina;
                var parametros = new DynamicParameters();
                parametros.Add("CdCentroCusto", cdCentroCusto);
                parametros.Add("Offset", offset);
                parametros.Add("ItensPorPagina", itensPorPagina);
                string condicaoFiltro = "";
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    filtro = filtro.Trim().ToUpper();
                    parametros.Add("Filtro", $"%{filtro}%");

                    condicaoFiltro += @"AND   (
                                                  UPPER(C.TX_TITULO) LIKE UPPER(:Filtro)
                                                OR
                                                  C.CD_CATEGORIA LIKE :Filtro
                                                )";
                }
                var query = $@"SELECT C.CD_CATEGORIA,
                                            C.TX_TITULO,
                                            C.TX_DESCRICAO,
                                            C.CD_ICONE,
                                            I.TX_ICONE,
                                            C.CD_COR,
                                            CC.TX_COR,
                                            C.DT_CADASTRO,
                                            C.CD_USUARIO_CADASTROU,
                                            U.NOUSUARIO             AS NoUsuarioCadastrou,
                                            T.NOCENTROCUSTO         AS NoCentroCustoUsuarioCadastrou,
                                            UN.NOUNIDADE            AS NoUnidadeUsuarioCadastrou,
                                            C.DT_ALTERACAO,
                                            C.CD_USUARIO_ALTEROU,
                                            UU.NOUSUARIO            AS NoUsuarioAlterou,
                                            TT.NOCENTROCUSTO        AS NoCentroCustoUsuarioAlterou,
                                            UNN.NOUNIDADE           AS NoUnidadeUsuarioAlterou,
                                            C.SN_ATIVO
                                       FROM CC_EVENTO_CATEGORIA C, 
                                            CC_CATEGORIA_COR    CC, 
                                            CC_CATEGORIA_ICONE  I,
                                            USUARIO          U,
                                            USUARIO          UU,
                                            CENTRO_CUSTO     T,
                                            CENTRO_CUSTO     TT,
                                            UNIDADE          UN,
                                            UNIDADE          UNN
                                      WHERE C.CD_COR               = CC.CD_COR
                                        AND C.CD_ICONE             = I.CD_ICONE
                                        AND C.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                        AND U.CDCENTROCUSTO        = :CdCentroCusto
                                        AND C.CD_USUARIO_ALTEROU   = UU.CDUSUARIO(+)
                                        AND U.CDCENTROCUSTO        = T.CDCENTROCUSTO
                                        AND UU.CDCENTROCUSTO       = TT.CDCENTROCUSTO(+)
                                        AND T.CDUNIDADE            = UN.CDUNIDADE
                                        AND TT.CDUNIDADE           = UNN.CDUNIDADE(+)
                                          {condicaoFiltro}
                                      ORDER BY C.DT_ALTERACAO DESC
                                      OFFSET :Offset ROWS FETCH NEXT :ItensPorPagina ROWS ONLY";
                var lista = (await con.QueryAsync<CategoriaMOD>(query, parametros)).ToList();

                var totalQuery = $@"SELECT COUNT(*)
                                            FROM CC_EVENTO_CATEGORIA     C, 
                                                 CC_CATEGORIA_COR        CC, 
                                                 CC_CATEGORIA_ICONE      I,
                                                 USUARIO              U,
                                                 USUARIO              UU,
                                                 CENTRO_CUSTO         T,
                                                 CENTRO_CUSTO         TT,
                                                 UNIDADE              UN,
                                                 UNIDADE              UNN
                                           WHERE C.CD_COR               = CC.CD_COR
                                             AND C.CD_ICONE             = I.CD_ICONE
                                             AND C.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                             AND U.CDCENTROCUSTO        = :CdCentroCusto
                                             AND C.CD_USUARIO_ALTEROU   = UU.CDUSUARIO(+)
                                             AND U.CDCENTROCUSTO        = T.CDCENTROCUSTO
                                             AND UU.CDCENTROCUSTO       = TT.CDCENTROCUSTO(+)
                                             AND T.CDUNIDADE            = UN.CDUNIDADE
                                             AND TT.CDUNIDADE           = UNN.CDUNIDADE(+)                           
                                               {condicaoFiltro}";
                int totalItens = await con.ExecuteScalarAsync<int>(totalQuery, parametros);

                return new PaginacaoResposta<CategoriaMOD>
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
                throw new Exception("Erro ao buscar categorias paginado com filtro.", ex);
            }
        }
        #endregion

        #region BuscarDiferenteAtual
        /// <summary>
        /// Busca todos as categorias de eventos diferentes da categoria atual
        /// </summary>
        /// <param name="cdCategoria"></param>
        /// <param name="cdCentroCusto"></param>
        /// <returns></returns>
        public async Task<List<CategoriaMOD>> BuscarDiferenteAtual(int cdCategoria, int cdCentroCusto)
        {
            List<CategoriaMOD> lista = new List<CategoriaMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT C.CD_CATEGORIA,
                                               C.TX_TITULO,
                                               C.TX_DESCRICAO,
                                               C.CD_ICONE,
                                               I.TX_ICONE,
                                               C.CD_COR,
                                               CC.TX_COR,
                                               C.DT_CADASTRO,
                                               C.CD_USUARIO_CADASTROU,
                                               C.DT_ALTERACAO,
                                               C.CD_USUARIO_ALTEROU,
                                               C.SN_ATIVO
                                          FROM CC_EVENTO_CATEGORIA C,
                                               CC_CATEGORIA_COR    CC,
                                               CC_CATEGORIA_ICONE  I,
                                               USUARIO          U
                                         WHERE C.SN_ATIVO             = 'S'
                                           AND C.CD_COR               = CC.CD_COR
                                           AND C.CD_ICONE             = I.CD_ICONE
                                           AND C.CD_CATEGORIA         <> :cdCategoria
                                           AND C.CD_USUARIO_CADASTROU = U.CDUSUARIO
                                           AND U.CDCENTROCUSTO        = :CdCentroCusto
                                         ORDER BY C.TX_TITULO DESC";
                    lista = (await con.QueryAsync<CategoriaMOD>(query, new { cdCategoria, CdCentroCusto = cdCentroCusto })).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region Cadastrar
        /// <summary>
        /// Cadastrar a categoria de evento
        /// </summary>
        /// <param name="categoriaMOD"></param>
        /// <returns></returns>
        public bool Cadastrar(CategoriaMOD categoriaMOD)
        {
            bool cadastrou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"INSERT INTO CC_EVENTO_CATEGORIA
                                                 (
                                                  TX_TITULO,
                                                  TX_DESCRICAO,
                                                  CD_COR,
                                                  CD_ICONE,
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
                                                 :CdCor,
                                                 :CdIcone,
                                                 :SnAtivo,
                                                 :CdUsuarioCadastrou,
                                                 :DtCadastro,
                                                 :CdUsuarioAlterou,
                                                 :DtAlteracao
                                                 )";

                    var parametros = new DynamicParameters(categoriaMOD);
                    parametros.Add("TxTitulo", categoriaMOD.TxTitulo);
                    parametros.Add("TxDescricao", categoriaMOD.TxDescricao);
                    parametros.Add("CdCor", categoriaMOD.CdCor);
                    parametros.Add("CdIcone", categoriaMOD.CdIcone);
                    parametros.Add("SnAtivo", categoriaMOD.SnAtivo);
                    parametros.Add("CdUsuarioCadastrou", categoriaMOD.CdUsuarioCadastrou);
                    parametros.Add("DtCadastro", categoriaMOD.DtCadastro);
                    parametros.Add("CdUsuarioAlterou", categoriaMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", categoriaMOD.DtAlteracao);
                    con.Execute(query, parametros);
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
        /// Editar a categoria de evento
        /// </summary>
        /// <param name="categoriaMOD"></param>
        /// <returns></returns>
        public async Task<bool> Editar(CategoriaMOD categoriaMOD)
        {
            bool editou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE CC_EVENTO_CATEGORIA
                                        SET
                                            TX_TITULO          = :TxTitulo,
                                            TX_DESCRICAO       = :TxDescricao,
                                            CD_ICONE           = :CdIcone,
                                            CD_COR             = :CdCor,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO       = :DtAlteracao
                                      WHERE
                                            CD_CATEGORIA       = :CdCategoria";

                    var parametros = new DynamicParameters(categoriaMOD);
                    parametros.Add("TxTitulo", categoriaMOD.TxTitulo);
                    parametros.Add("TxDescricao", categoriaMOD.TxDescricao);
                    parametros.Add("CdIcone", categoriaMOD.CdIcone);
                    parametros.Add("CdCor", categoriaMOD.CdCor);
                    parametros.Add("CdUsuarioAlterou", categoriaMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", categoriaMOD.DtAlteracao);
                    parametros.Add("CdCategoria", categoriaMOD.CdCategoria);
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
        /// Altera o status da categoria de evento
        /// </summary>
        /// <param name="categoriaMOD"></param>
        /// <returns></returns>
        public bool AlterarStatus(CategoriaMOD categoriaMOD)
        {
            bool alterouStatus = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE CC_EVENTO_CATEGORIA
                                        SET
                                            SN_ATIVO           = :SnAtivo,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO       = :DtAlteracao
                                      WHERE
                                            CD_CATEGORIA       = :CdCategoria";

                    var parametros = new DynamicParameters(categoriaMOD);

                    parametros.Add("SnAtivo", categoriaMOD.SnAtivo);
                    parametros.Add("CdUsuarioAlterou", categoriaMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", categoriaMOD.DtAlteracao);
                    parametros.Add("CdCategoria", categoriaMOD.CdCategoria);
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
        /// Conta todas as categorias de eventos ativas
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
                                           FROM CC_EVENTO_CATEGORIA C, 
                                                USUARIO          U
                                          WHERE C.CD_USUARIO_CADASTROU  = U.CDUSUARIO
                                            AND U.CDCENTROCUSTO         = :CdCentroCusto";
                    return await con.ExecuteScalarAsync<int>(query, new { CdCentroCusto = cdCentroCusto });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #endregion
    }
}