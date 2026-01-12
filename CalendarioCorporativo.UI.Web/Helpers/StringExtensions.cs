using System.Globalization;
using System.Text.RegularExpressions;

namespace CalendarioCorporativo.UI.Web.Helpers
{
    public static class StringExtensions
    {
        private static readonly HashSet<string> Siglas = new()
        {
            "TI",
            "RH"
        };
        public static string ToTitleCasePtBr(this string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }

            var cultura = new CultureInfo("pt-BR");
            texto = cultura.TextInfo.ToTitleCase(texto.ToLower());

            string[] minusculas = { " Da ", " De ", " Do ", " Das ", " Dos ", " E " };
            foreach (var p in minusculas)
            {
                texto = texto.Replace(p, p.ToLower());
            }

            foreach (var sigla in Siglas)
            {
                texto = Regex.Replace(
                    texto,
                    $@"\b{sigla}\b",
                    sigla,
                    RegexOptions.IgnoreCase
                );
            }
            return texto;
        }
    }
}