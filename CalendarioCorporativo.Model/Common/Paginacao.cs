namespace CalendarioCorporativo.Model
{
    public class Paginacao : Busca
    {
        public int PaginaAtual { get; set; }
        public int QuantidadePorPagina { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItens { get; set; }
    }
    public class PaginacaoResposta<T>
    {
        public List<T> Dados { get; set; }
        public Paginacao Paginacao { get; set; }
    }
}