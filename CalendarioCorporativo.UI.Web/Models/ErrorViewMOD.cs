namespace CalendarioCorporativo.UI.Web.Models
{
    public class ErrorViewMOD
    {
        public string? RequestId { get; set; }
        public string? ErrorMessage { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}