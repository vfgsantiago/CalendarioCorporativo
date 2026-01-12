using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.Repository
{
    public class UsuarioREP
    {
        #region Services
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoSat;
        #endregion

        #region Constructor
        public UsuarioREP(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _acessaDados = new AcessaDados(_configuration);
            _httpClient = _acessaDados.conexaoWebApiIntranet();
            _conexaoSat = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarPorCodigo
        /// <summary>
        /// Buscar 1 usuario por código
        /// </summary>
        /// <param name="CdUsuario"></param>
        /// <returns></returns>
        public async Task<UsuarioMOD> BuscarPorCodigo(int CdUsuario)
        {
            UsuarioMOD usuario = new UsuarioMOD();
            String url = $"Usuarios/api/Usuario/BuscarUsuarioPorCodigo?CdUsuario={CdUsuario}";

            using (var response = await _httpClient.GetAsync(url))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                usuario = JsonConvert.DeserializeObject<UsuarioMOD>(apiResponse);
            }

            return usuario;
        }
        #endregion

        #region BuscarPorLogin
        /// <summary>
        /// Buscar 1 usuário por login
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public async Task<UsuarioMOD> BuscarPorLogin(UsuarioMOD usuario)
        {
            String url = $"Usuarios/api/Usuario/BuscarUsuarioLoginPorLogin?TxLogin={usuario.TxLogin}";

            using (var response = await _httpClient.GetAsync(url))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                usuario = JsonConvert.DeserializeObject<UsuarioMOD>(apiResponse);
            }

            return usuario;
        }
        #endregion

        #region BuscarPorCpf
        /// <summary>
        /// Buscar 1 usuario por cpf
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public async Task<UsuarioMOD> BuscarPorCpf(UsuarioMOD usuario)
        {
            String url = $"Usuarios/api/Usuario/BuscarUsuarioPorCpf?NrCpf={usuario.NrCpf}";

            using (var response = await _httpClient.GetAsync(url))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                usuario = JsonConvert.DeserializeObject<UsuarioMOD>(apiResponse);
            }

            return usuario;
        }
        #endregion

        #region Buscar
        /// <summary>
        /// Buscar todos usuarios
        /// </summary>
        /// <returns></returns>
        public async Task<List<UsuarioMOD>> Buscar()
        {
            List<UsuarioMOD> ListaUsuario = new List<UsuarioMOD>();


            using (var response = await _httpClient.GetAsync("Usuarios/api/Usuario/BuscarUsuarios"))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                ListaUsuario = JsonConvert.DeserializeObject<List<UsuarioMOD>>(apiResponse);
            }

            return ListaUsuario;
        }
        #endregion

        #region BuscarAcessoUsuarioSistema
        /// <summary>
        /// Buscar o acesso do usuário ao sistema
        /// </summary>
        /// <param name="CdUsuario"></param>
        /// <param name="CdSistema"></param>
        /// <returns></returns>
        public async Task<UsuarioSistemaMOD> BuscarAcessoUsuarioSistema(Int32 CdUsuario, Int32 CdSistema)
        {
            UsuarioSistemaMOD usuarioSistema = new UsuarioSistemaMOD();

            String url = $"Usuarios/api/Usuario/BuscarAcessoPorCodigoUsuarioSistema?cdUsuario={CdUsuario}&cdSistema={CdSistema}";

            using (var response = await _httpClient.GetAsync(url))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                usuarioSistema = JsonConvert.DeserializeObject<UsuarioSistemaMOD>(apiResponse);
            }

            return usuarioSistema;
        }
        #endregion

        #region BuscarPorNomeOuMatricula
        /// <summary>
        /// Busca os usuários por nome ou matrícula
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public async Task<List<UsuarioMOD>> BuscarPorNomeOuMatricula(string? filtro)
        {
            using var con = new OracleConnection(_conexaoSat);
            try
            {
                await con.OpenAsync();
                var parametros = new DynamicParameters();
                string condicaoFiltro = "";
                if (!string.IsNullOrWhiteSpace(filtro) && filtro.Trim().Length >= 3)
                {
                    filtro = filtro.Trim().ToUpper();
                    parametros.Add("Filtro", $"%{filtro}%");

                    condicaoFiltro = @" AND (
                                              UPPER(U.NOUSUARIO) LIKE UPPER(:Filtro)
                                           OR U.NRMATRICULA     LIKE :Filtro
                                           )";
                }

                string query = $@"SELECT DISTINCT 
                                         U.CDUSUARIO,
                                         U.NOUSUARIO      AS NmUsuario,
                                         U.NRMATRICULA,
                                         U.CDCENTROCUSTO,
                                         CC.NOCENTROCUSTO AS TxCentroCusto,
                                         U.CDUNIDADE,
                                         UN.NOUNIDADE     AS TxUnidade,
                                         U.CDCARGO,
                                         CA.NMCARGO       AS TxCargo,
                                         U.CDFUNCAO,
                                         FU.NMFUNCAO      AS TxFuncao
                                    FROM USUARIO      U,
                                         CENTRO_CUSTO CC,
                                         UNIDADE      UN,
                                         CARGO        CA,
                                         FUNCAO       FU
                                   WHERE U.CDCENTROCUSTO    = CC.CDCENTROCUSTO
                                         AND U.CDUNIDADE    = UN.CDUNIDADE
                                         AND U.CDCARGO      = CA.CDCARGO
                                         AND U.CDFUNCAO     = FU.CDFUNCAO
                                         AND U.AOATIVO      = 'S'
                                         {condicaoFiltro}
                                     ORDER BY U.NOUSUARIO ASC";

                var lista = await con.QueryAsync<UsuarioMOD>(query, parametros);
                return lista.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar usuários por nome ou matrícula.", ex);
            }
        }
        #endregion

        #region BuscarPorMatricula
        /// <summary>
        /// Busca o usuário por matrícula
        /// </summary>
        /// <param name="nrMatricula"></param>
        /// <returns></returns>
        public async Task<UsuarioMOD> BuscarPorMatricula(int nrMatricula)
        {
            UsuarioMOD model = new UsuarioMOD();
            using (var con = new OracleConnection(_conexaoSat))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT U.CDUSUARIO,
                                               U.NOUSUARIO      AS NmUsuario,
                                               U.NRMATRICULA,
                                               U.CDCENTROCUSTO,
                                               CC.NOCENTROCUSTO AS TxCentroCusto,
                                               U.CDUNIDADE,
                                               UN.NOUNIDADE     AS TxUnidade,
                                               U.CDCARGO,
                                               CA.NMCARGO       AS TxCargo,
                                               U.CDFUNCAO,
                                               FU.NMFUNCAO      AS TxFuncao
                                          FROM USUARIO      U,
                                               CENTRO_CUSTO CC,
                                               UNIDADE      UN,
                                               CARGO        CA,
                                               FUNCAO       FU
                                         WHERE U.CDCENTROCUSTO    = CC.CDCENTROCUSTO
                                               AND U.CDUNIDADE    = UN.CDUNIDADE
                                               AND U.CDCARGO      = CA.CDCARGO
                                               AND U.CDFUNCAO     = FU.CDFUNCAO
                                               AND U.AOATIVO      = 'S'
                                               AND U.NRMATRICULA  = :nrMatricula";
                    model = await con.QueryFirstOrDefaultAsync<UsuarioMOD>(query, new { nrMatricula });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return model;
        }
        #endregion

        #endregion
    }
}