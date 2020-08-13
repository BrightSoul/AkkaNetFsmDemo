
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;

namespace AkkanetFsmDemo.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddActorsFromAssembly(this IServiceCollection serviceCollection, Assembly assembly)
        {
            var actorTypes = assembly.GetTypes().Where(type => typeof(ActorBase).IsAssignableFrom(type)).ToArray();
            foreach (var actorType in actorTypes)
            {
                serviceCollection.AddScoped(actorType);
            }
        }
    }
}