using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.Repository
{
    public class CorREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public CorREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region Buscar
        /// <summary>
        /// Busca todos as cores ativas
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<CorMOD>> Buscar()
        {
            List<CorMOD> lista = new List<CorMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT C.CD_COR,
                                               C.TX_COR,
                                               C.SN_USADO,
                                               C.SN_ATIVO
                                          FROM CC_CATEGORIA_COR C
                                         WHERE C.SN_ATIVO = 'S'
                                         ORDER BY C.TX_COR";
                    lista = (await con.QueryAsync<CorMOD>(query)).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region BuscarNaoUsados
        /// <summary>
        /// Busca as cores não usadas em categorias
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<CorMOD>> BuscarNaoUsados()
        {
            List<CorMOD> lista = new List<CorMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT C.CD_COR,
                                                C.TX_COR,
                                                C.SN_USADO,
                                                C.SN_ATIVO
                                           FROM CC_CATEGORIA_COR C
                                          WHERE C.SN_ATIVO = 'S'
                                            AND C.SN_USADO = 'N'
                                          ORDER BY C.TX_COR";
                    lista = (await con.QueryAsync<CorMOD>(query)).ToList();
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
        /// Busca a cor por código
        /// </summary>
        /// <param name="cdCor"></param>
        /// <returns></returns>
        public async Task<CorMOD> BuscarPorCodigo(int cdCor)
        {
            CorMOD model = new CorMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT C.CD_COR,
                                                C.TX_COR,
                                                C.SN_USADO,
                                                C.SN_ATIVO
                                           FROM CC_CATEGORIA_COR C
                                          WHERE C.SN_ATIVO = 'S'
                                            AND C.CD_COR   = :cdCor
                                          ORDER BY C.TX_COR";
                    model = await con.QueryFirstOrDefaultAsync<CorMOD>(query, new { cdCor });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return model;
        }
        #endregion

        #region Atualizar
        /// <summary>
        /// Atualizar a cor
        /// </summary>
        /// <param name="corMOD"></param>
        /// <returns></returns>
        public bool Atualizar(CorMOD corMOD)
        {
            bool atualizou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE CC_CATEGORIA_COR
                                        SET SN_USADO = :SnUsado
                                      WHERE CD_COR   = :CdCor";

                    var parametros = new DynamicParameters(corMOD);
                    parametros.Add("SnUsado", corMOD.SnUsado);
                    parametros.Add("CdCor", corMOD.CdCor);
                    con.Execute(query, parametros);
                    transacao.Commit();
                    atualizou = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return atualizou;
        }
        #endregion

        #endregion
    }
}