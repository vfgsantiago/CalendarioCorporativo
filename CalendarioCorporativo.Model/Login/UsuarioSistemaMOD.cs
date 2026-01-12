namespace CalendarioCorporativo.Model
{
    public class UsuarioSistemaMOD
    {
        public UsuarioMOD Usuario { get; set; } = new UsuarioMOD();
        public SistemaMOD Sistema { get; set; } = new SistemaMOD();
        public int CdUsuario { get; set; }
        public int CdSistema { get; set; }
        public string AoAtivo { get; set; }
        public string TpVisibilidade { get; set; }
        public string TpNivelAcesso { get; set; }
    }
}