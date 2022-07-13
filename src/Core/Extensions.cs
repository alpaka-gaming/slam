using System.Reflection.Metadata;
using Core.Interfaces;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    public static class Extensions
    {
        public static IServiceCollection AddCore(this IServiceCollection @this)
        {
            @this.AddSingleton<SteamService>();
            @this.AddScoped<IGameService, GameService>();

            return @this;
        }
    }
}
