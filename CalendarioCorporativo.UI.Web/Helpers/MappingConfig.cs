using Mapster;
using System.Reflection;
using CalendarioCorporativo.Model;
using CalendarioCorporativo.UI.Web.Models;

namespace CalendarioCorporativo.UI.Web.Helpers
{
    public class MappingConfig
    {
        public static void RegisterMaps(IServiceCollection services)
        {
            #region Objects

            #region Evento
            TypeAdapterConfig<EventoViewMOD, EventoMOD>
            .NewConfig()
            .Map(dest => dest, src => src.Evento);

            TypeAdapterConfig<EventoMOD, EventoViewMOD>
            .NewConfig()
            .Map(dest => dest.Evento, src => src);
            #endregion

            #region Categoria
            TypeAdapterConfig<CategoriaViewMOD, CategoriaMOD>
            .NewConfig()
            .Map(dest => dest, src => src.Categoria);

            TypeAdapterConfig<CategoriaMOD, CategoriaViewMOD>
            .NewConfig()
            .Map(dest => dest.Categoria, src => src);
            #endregion

            #endregion

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }
    }
}