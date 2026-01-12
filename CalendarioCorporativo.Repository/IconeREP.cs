using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.Repository
{
    public class IconeREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public IconeREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region Buscar
        /// <summary>
        /// Busca todos os icones ativos
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
                    var query = @"SELECT I.CD_ICONE, 
                                               I.TX_ICONE, 
                                               I.SN_USADO, 
                                               I.SN_ATIVO
                                          FROM CC_CATEGORIA_ICONE I
                                         WHERE I.SN_ATIVO = 'S'
                                         ORDER BY I.TX_ICONE";
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
        /// Busca os ícones não usados em categorias
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<IconeMOD>> BuscarNaoUsados()
        {
            List<IconeMOD> lista = new List<IconeMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT I.CD_ICONE, 
                                               I.TX_ICONE, 
                                               I.SN_USADO, 
                                               I.SN_ATIVO
                                          FROM CC_CATEGORIA_ICONE I
                                         WHERE I.SN_ATIVO = 'S'
                                           AND I.SN_USADO = 'N'
                                         ORDER BY I.TX_ICONE";
                    lista = (await con.QueryAsync<IconeMOD>(query)).ToList();
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
        /// Busca o ícone por código
        /// </summary>
        /// <param name="cdIcone"></param>
        /// <returns></returns>
        public async Task<IconeMOD> BuscarPorCodigo(int cdIcone)
        {
            IconeMOD model = new IconeMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT I.CD_ICONE, 
                                               I.TX_ICONE, 
                                               I.SN_USADO, 
                                               I.SN_ATIVO
                                          FROM CC_CATEGORIA_ICONE I
                                         WHERE I.SN_ATIVO = 'S'
                                           AND I.CD_ICONE = :cdIcone
                                         ORDER BY I.TX_ICONE";
                    model = await con.QueryFirstOrDefaultAsync<IconeMOD>(query, new { cdIcone });
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
        /// Atualizar o ícone
        /// </summary>
        /// <param name="iconeMOD"></param>
        /// <returns></returns>
        public bool Atualizar(IconeMOD iconeMOD)
        {
            bool atualizou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE CC_CATEGORIA_ICONE
                                        SET SN_USADO = :SnUsado
                                      WHERE CD_ICONE = :CdIcone";

                    var parametros = new DynamicParameters(iconeMOD);
                    parametros.Add("SnUsado", iconeMOD.SnUsado);
                    parametros.Add("CdIcone", iconeMOD.CdIcone);
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