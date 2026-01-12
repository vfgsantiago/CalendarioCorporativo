using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using CalendarioCorporativo.Data;
using CalendarioCorporativo.Model;
using CalendarioCorporativo.Repository;
using CalendarioCorporativo.UI.Web.Models;

namespace CalendarioCorporativo.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        #region Conections
        private readonly LoginREP _repositorioLogin;
        private readonly UsuarioREP _repositorioUsuario;
        private readonly SistemaREP _repositorioSistema;
        private readonly AcessaDados _acessaDados;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public LoginController(LoginREP loginService, UsuarioREP usuarioService, SistemaREP sistemaService, IConfiguration configuration, AcessaDados acessaDados)
        {
            _repositorioLogin = loginService;
            _repositorioUsuario = usuarioService;
            _repositorioSistema = sistemaService;
            _configuration = configuration;
            _acessaDados = acessaDados;
        }
        #endregion

        #region Methods

        #region EfeturarLoginAdmin
        [AllowAnonymous]
        public async Task<IActionResult> EfetuarLoginAdmin()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> EfetuarLoginAdmin(LoginAdminViewMOD model, string? returnUrl)
        {
            UsuarioMOD usuario = new UsuarioMOD();
            usuario.TxLogin = model.Login.ToUpper();
            usuario.TxSenha = model.Password;
            bool valido = await _repositorioLogin.ValidarLogin(usuario);

            if (valido)
            {
                usuario = await _repositorioLogin.BuscarPorLogin(usuario);
                var id = _configuration.GetSection("CodigoSistema").Value;

                if (!string.IsNullOrEmpty(usuario.NrCpf))
                {
                    usuario = await _repositorioLogin.BuscarPorCpf(usuario);
                    SistemaMOD sistema = await _repositorioSistema.BuscarPorCodigo(Convert.ToInt32(id));
                    UsuarioSistemaMOD usuarioSistema = await _repositorioUsuario.BuscarAcessoUsuarioSistema(usuario.CdUsuario, sistema.CdSistema);
                    String tpNivelAcesso = _repositorioLogin.ObterTpNivelAcessoUsuario(usuario.CdUsuario);
                    var infoUsuario = await _repositorioUsuario.BuscarPorMatricula(usuario.NrMatricula);

                    if (usuario.CdUsuario > 0 && usuarioSistema.Usuario.CdUsuario > 0)
                    {
                        string role = tpNivelAcesso == "A" ? "Admin" : "Comum";
                        var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, usuario.TxLogin),
                                new Claim("NrCpf", usuario.NrCpf),
                                new Claim("CdUsuario", usuario.CdUsuario.ToString()),
                                new Claim("NmUsuario", usuario.NmUsuario.ToString()),
                                new Claim("Avatar", usuario.Avatar.TxLocalFoto),
                                new Claim("CentroCusto", infoUsuario.CdCentroCusto.ToString()),
                                new Claim("NoCentroCusto", (infoUsuario.TxCentroCusto != null) ? infoUsuario.TxCentroCusto : ""),
                                new Claim(ClaimTypes.Role, role),
                            };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,
                            IsPersistent = true,
                            IssuedUtc = DateTime.Now,
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        if (!string.IsNullOrEmpty(returnUrl))
                            return Redirect(returnUrl);
                        else
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                }
            }
            ModelState.AddModelError<LoginAdminViewMOD>(x => x.Login, "Usuário e/ou Senha Inválidos.");
            return View(model);
        }
        #endregion

        #region Sair
        public async Task<ActionResult> Sair(string? returnUrl = null)
        {
            var CdUsuario = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "CdUsuario").Value);

            await HttpContext.SignOutAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/Login/EfetuarLoginAdmin" });

            return RedirectToAction("EfetuarLoginAdmin");
        }
        #endregion

        #endregion
    }
}