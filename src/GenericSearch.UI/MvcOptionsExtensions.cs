using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GenericSearch.UI
{
    public static class MvcOptionsExtensions
    {
        public static IServiceCollection AddGenericSearchUI(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(x =>
                                           {
                                               x.ModelBinderProviders.Insert(0, new AbstractSearchModelBinderProvider());
                                           });
            services.AddHttpContextAccessor()
                    .AddScoped<SortLinkTagHelper>()
                    .AddScoped<PagerTagHelper>();

            return services;
        }
    }
}
