using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Persistence.Sqlite;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkanetFsmDemo.Models.Actors.CommandHandlers;
using AkkanetFsmDemo.Models.Actors.EventHandlers;
using AkkanetFsmDemo.Models.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public class ActorSystemAccessor : IActorSystemAccessor
    {
        private readonly ActorSystem actorSystem;
        private readonly IActorRef primaryCommandHandler;
        private readonly IActorRef primaryEventHandler;
        private readonly ActorMaterializer actorMaterializer;
        public readonly IActorSystemDependencyResolver dependencyResolver;
        public ActorSystemAccessor(IHostEnvironment env, IActorSystemDependencyResolver dependencyResolver, IOptionsMonitor<ActorSystemOptions> actorSystemOptions)
        {
            var options = actorSystemOptions.CurrentValue;
            var configFileName = File.ReadAllText(Path.Combine(env.ContentRootPath, "config.hocon"));
            var config = LoadConfiguration(configFileName);
            this.dependencyResolver = dependencyResolver;
            actorSystem = CreateActorSystem(config, dependencyResolver);
            primaryCommandHandler = CreatePrimaryCommandHandler(actorSystem, options);
            primaryEventHandler = CreatePrimaryEventHandler(actorSystem, options);
            //TODO: here we assume only one Persistence Query will be needed throughout the application life cycle
            //If this is not the case, then we should not create it here. Instead, it should be created by a command handler actor and live in its scope.
            actorMaterializer = ConfigureEventHandling(actorSystem, primaryEventHandler, options);
        }

        private Config LoadConfiguration(string configFileName)
        {
            var defaultConfig = ConfigurationFactory.Default();
            var persistenceConfig = SqlitePersistence.DefaultConfiguration();
            var actualConfiguration = ConfigurationFactory.ParseString(configFileName);
            var config = actualConfiguration.WithFallback(persistenceConfig.WithFallback(defaultConfig));
            return config;
        }

        private ActorSystem CreateActorSystem(Config config, IActorSystemDependencyResolver dependencyResolver)
        {
            var actorSystem = ActorSystem.Create("actor-system", config);
            dependencyResolver.RegisterDependencyResolver(actorSystem);
            SqlitePersistence.Get(actorSystem);
            return actorSystem;
        }

        private IActorRef CreatePrimaryCommandHandler(ActorSystem actorSystem, ActorSystemOptions options)
        {
            return CreateActor(options.PrimaryCommandHandler, "primary-commnand-handler");
        }

        private IActorRef CreatePrimaryEventHandler(ActorSystem actorSystem, ActorSystemOptions options)
        {
            return CreateActor(options.PrimaryEventHandler, "primary-event-handler");
        }

        private IActorRef CreateActor(string actorTypeName, string? name = null)
        {
            var actorType = Type.GetType(actorTypeName);
            if (actorType == null)
            {
                throw new InvalidOperationException($"Couldn't get a Type for type name '{actorTypeName}'");
            }
            var props = dependencyResolver.CreateActorProps(actorType, actorSystem);
            return actorSystem.ActorOf(props, name);
        }

        private ActorMaterializer ConfigureEventHandling(ActorSystem actorSystem, IActorRef primaryEventHandler, ActorSystemOptions options)
        {
            var actorMaterializer = actorSystem.Materializer();
            var readJournal = PersistenceQuery.Get(actorSystem).ReadJournalFor<SqlReadJournal>("akka.persistence.query.journal.sql");
            readJournal
                    .EventsByPersistenceId(options.PersistenceId, 0, long.MaxValue)
                    .Collect(envelope => envelope.Event)
                    .RunWith(Sink.ActorRef<object>(primaryEventHandler, new object()
                    //TODO: complete here
                    //.RunWith(Sink.ActorRefWithAck<ItemAdded>(writer, 
                    //onInitMessage: CreateViewsActor.Init.Instance,
                    //ackMessage: CreateViewsActor.Ack.Instance,
                    //onCompleteMessage: CreateViewsActor.Done.Instance), materializer);
                    ), actorMaterializer);
            return actorMaterializer;
        }

        public IActorRefFactory ActorRefFactory => actorSystem;
        public IActorRef PrimaryCommandHandler => primaryCommandHandler;

        public void Dispose()
        {
            actorMaterializer.Dispose();
            actorSystem.Dispose();
        }
    }
}