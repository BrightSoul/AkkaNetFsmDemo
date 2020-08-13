using System;
using Akka.Actor;
using Akka.Persistence;
using AkkanetFsmDemo.Models.Commands;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public interface IActorSystemAccessor : IDisposable
    {
        IActorRefFactory ActorRefFactory { get; }
        IActorRef PrimaryCommandHandler { get; }
    }
}