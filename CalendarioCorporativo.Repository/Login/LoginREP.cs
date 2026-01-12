using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System.Net.Http.Json;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;

namespace CalendarioCorporativo.Repository
{
    public class LoginREP
    {
        #region Services
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoSat;
        #endregion

        #region Constructor
        public LoginREP(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _acessaDados = new AcessaDados(_configuration);
            _httpClient = _acessaDados.conexaoWebApiIntranet();
            _conexaoSat = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region Buscar
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

        #region BuscarPorCodigo
        /// <summary>
        /// Busca o usuário por código
        /// async
        /// </summary>
        /// <param name="CdUsuario"></param>
        /// <returns>UsuarioMOD</returns>
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
        /// Busca um usuário por login
        /// async
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
        /// Busca um usuário por cpf 
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns>UsuarioMOD</returns>
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

        #region ValidarLogin
        public async Task<Boolean> ValidarLogin(UsuarioMOD usuario)
        {
            var valido = false;
            using (var response = await _httpClient.PostAsJsonAsync("Usuarios/api/Usuario/ValidarUsuarioLogin", usuario))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                valido = JsonConvert.DeserializeObject<Boolean>(apiResponse);
            }

            return valido;
        }
        #endregion

        #region ObterTpNivelAcessoUsuario
        public String ObterTpNivelAcessoUsuario(Int32 cdUsuario)
        {
            String tpNivelAcesso = "";

            using (OracleConnection con = new OracleConnection(_conexaoSat))
            {
                con.Open();
                OracleCommand com = con.CreateCommand();
                com.CommandText = String.Format(@"SELECT US.TPNIVELACESSO
                                                           FROM USUARIO_SISTEMA US
                                                          WHERE US.CDUSUARIO = {0}
                                                            AND US.CDSISTEMA = 123456789", cdUsuario);
                using (OracleDataReader registros = com.ExecuteReader())
                {
                    while (registros.Read())
                    {
                        if (!registros.IsDBNull(0))
                            tpNivelAcesso = registros.GetString(0);
                    }
                }
            }
            return tpNivelAcesso;
        }
        #endregion

        #endregion
    }
}