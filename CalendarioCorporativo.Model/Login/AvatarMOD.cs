namespace CalendarioCorporativo.Model
{
    public class AvatarMOD
    {
        public int CdAvatar { get; set; }
        public string TxLocalFoto { get; set; }
        public string TpAvatar { get; set; }
        public int? CdUsuario { get; set; }
        public virtual ICollection<UsuarioMOD> Usuarios { get; set; } = new List<UsuarioMOD>();
    }
}