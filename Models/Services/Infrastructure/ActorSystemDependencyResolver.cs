using System;
using System.Collections.Concurrent;
using Akka.Actor;
using Akka.DI.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public class ActorSystemDependencyResolver : IActorSystemDependencyResolver
    {
        private readonly IServiceProvider serviceProvider;
        public ActorSystemDependencyResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void RegisterDependencyResolver(ActorSystem actorSystem)
        {
            actorSystem.AddDependencyResolver(new ActorDependencyResolver(actorSystem, serviceProvider));
        }

        public Props CreateActorProps<TActor>(ActorSystem actorSystem) where TActor : ActorBase
        {
            return CreateActorProps(typeof(TActor), actorSystem);
        }

        public Props CreateActorProps(Type actorType, ActorSystem actorSystem)
        {
            if (!typeof(ActorBase).IsAssignableFrom(actorType))
            {
                throw new InvalidOperationException($"Couldn't get props for actor type '{actorType.FullName}' because it was not an ActorBase.");
            }

            var props = actorSystem.GetExtension<DIExt>().Props(actorType);
            if (props == null)
            {
                throw new InvalidOperationException($"Could not create actor of type '{actorType.FullName}'");
            }
            return props;
        }

        public class ActorDependencyResolver : IDependencyResolver
        {
            private readonly ActorSystem actorSystem;
            private readonly IServiceProvider serviceProvider;
            private readonly ConcurrentDictionary<ActorBase, IServiceScope> scopes = new ConcurrentDictionary<ActorBase, IServiceScope>();

            public ActorDependencyResolver(ActorSystem actorSystem, IServiceProvider serviceProvider)
            {
                this.actorSystem = actorSystem;
                this.serviceProvider = serviceProvider;
            }

            public Props Create<TActor>() where TActor : ActorBase
            {
                return Create(typeof(TActor));
            }

            public Props Create(Type actorType)
            {
                return actorSystem.GetExtension<DIExt>().Props(actorType);
            }

            public Func<ActorBase> CreateActorFactory(Type actorType)
            {
                return () => {
                    var scope = serviceProvider.CreateScope();
                    var instance = scope.ServiceProvider.GetService(actorType);
                    if (instance == null) {
                        throw new InvalidOperationException($"Couldn't resolve actor type '{actorType.FullName}'");
                    }
                    var actor = instance as ActorBase;
                    if (actor == null) {
                        throw new InvalidOperationException($"Resolved actor type '{actorType.FullName}' must be of base type ActorBase");
                    }
                    if (!scopes.TryAdd(actor, scope)) {
                        throw new InvalidOperationException($"Couldn't track actor '{actorType.FullName}' in its scope");
                    }
                    return actor;
                };
            }

            public Type GetType(string actorName)
            {
                var type = Type.GetType(actorName);
                if (type == null) {
                    throw new InvalidOperationException($"Couldn't get Type for actor name $'{actorName}'");
                }
                return type;
            }

            public void Release(ActorBase actor)
            {
                if (!scopes.TryRemove(actor, out IServiceScope? scope)) {
                    throw new InvalidOperationException($"Actor '${actor.GetType().FullName}' was not tracked in a scope");
                }
                if (scope == null) {
                    throw new InvalidOperationException($"Actor '${actor.GetType().FullName}' had a null scope");
                }
                scope.Dispose();
            }
        }
    }
}