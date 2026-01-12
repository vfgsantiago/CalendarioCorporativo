namespace CalendarioCorporativo.Model
{
    public class EventoMOD
    {
        public int CdEvento { get; set; }
        public string TxTitulo { get; set; }
        public string TxDescricao { get; set; }
        public int CdCategoria { get; set; }
        public string TxCategoria { get; set; }
        public string TxDescricaoCategoria { get; set; }
        public DateTime? DtInicioEvento { get; set; }
        public DateTime? DtFimEvento { get; set; }
        public int CdIcone { get; set; }
        public string TxIcone { get; set; }
        public int CdCor { get; set; }
        public string TxCor { get; set; }
        public DateTime DtCadastro { get; set; }
        public int CdUsuarioCadastrou { get; set; }
        public string NoUsuarioCadastrou { get; set; }
        public string NoCentroCustoUsuarioCadastrou { get; set; }
        public string NoUnidadeUsuarioCadastrou { get; set; }
        public DateTime? DtAlteracao { get; set; }
        public int? CdUsuarioAlterou { get; set; }
        public string? NoUsuarioAlterou { get; set; }
        public string? NoCentroCustoUsuarioAlterou { get; set; }
        public string? NoUnidadeUsuarioAlterou { get; set; }
        public string SnAtivo { get; set; }
        public int QtdEventos { get; set; }
    }
}