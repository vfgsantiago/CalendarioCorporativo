using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.Repository
{
    public class CentroCustoREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoSat;
        #endregion

        #region Constructor
        public CentroCustoREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoSat = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarComEvento
        /// <summary>
        /// Busca todos os Centros de Custo que possuem eventos vinculados
        /// </summary>
        public async Task<List<CentroCustoMOD>> BuscarComEvento()
        {
            using var con = new OracleConnection(_conexaoSat);
            try
            {
                await con.OpenAsync();

                var query = @"SELECT DISTINCT 
                                           C.CDCENTROCUSTO AS CdCentroCusto,
                                           C.NOCENTROCUSTO AS NoCentroCusto
                                      FROM CC_EVENTO       E,
                                           USUARIO      U, 
                                           CENTRO_CUSTO C
                                     WHERE U.CDUSUARIO     = E.CD_USUARIO_CADASTROU
                                       AND C.CDCENTROCUSTO = U.CDCENTROCUSTO
                                       AND E.SN_ATIVO      = 'S'
                                     ORDER BY C.NOCENTROCUSTO";

                var resultado = await con.QueryAsync<CentroCustoMOD>(query);
                return resultado.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar centros de custo com eventos.", ex);
            }
        }
        #endregion

        #endregion
    }
}