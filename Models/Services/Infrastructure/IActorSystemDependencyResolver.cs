using System;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Persistence;
using AkkanetFsmDemo.Models.Commands;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public interface IActorSystemDependencyResolver
    {
        void RegisterDependencyResolver(ActorSystem actorSystem);
        Props CreateActorProps<TActor>(ActorSystem actorSystem) where TActor : ActorBase;
        Props CreateActorProps(Type actorType, ActorSystem actorSystem);
    }
}