namespace CalendarioCorporativo.Model
{
    public class UsuarioMOD
    {
        public int CdUsuario { get; set; }
        public string NmUsuario { get; set; }
        public int NrMatricula { get; set; }
        public int CdCentroCusto { get; set; }
        public string TxCentroCusto { get; set; }
        public int CdUnidade { get; set; }
        public string TxUnidade { get; set; }
        public int CdCargo { get; set; }
        public string TxCargo { get; set; }
        public int CdFuncao { get; set; }
        public string TxFuncao { get; set; }
        public string NrCpf { get; set; }
        public string TxEmail { get; set; }
        public string TxEmailOpcional { get; set; }
        public string NrCelular { get; set; }
        public string AoAtivo { get; set; }
        public DateTime? DtCadastro { get; set; }
        public string? TxLogin { get; set; }
        public string? TxSenha { get; set; }
        public AvatarMOD Avatar { get; set; } = new AvatarMOD();
    }
}