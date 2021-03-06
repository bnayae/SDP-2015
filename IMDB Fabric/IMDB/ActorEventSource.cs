﻿using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB
{
    [EventSource(Name = "IMDB_Fabric-Actors")]
    internal sealed class ActorEventSource : EventSource
    {
        public static ActorEventSource Current = new ActorEventSource();

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        [Event(1, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (this.IsEnabled())
            {
                WriteEvent(1, message);
            }
        }

        [NonEvent]
        public void ActorMessage(StatelessActor actor, string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                ActorMessage(
                    actor.GetType().ToString(),
                    actor.Id.ToString(),
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationTypeName,
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationName,
                    actor.ActorService.ServiceInitializationParameters.ServiceTypeName,
                    actor.ActorService.ServiceInitializationParameters.ServiceName.ToString(),
                    actor.ActorService.ServiceInitializationParameters.PartitionId,
                    actor.ActorService.ServiceInitializationParameters.InstanceId,
                    FabricRuntime.GetNodeContext().NodeName,
                    finalMessage);
            }
        }

        [NonEvent]
        public void ActorMessage<TState>(StatefulActor<TState> actor, string message, params object[] args) where TState : class
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                ActorMessage(
                    actor.GetType().ToString(),
                    actor.Id.ToString(),
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationTypeName,
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationName,
                    actor.ActorService.ServiceInitializationParameters.ServiceTypeName,
                    actor.ActorService.ServiceInitializationParameters.ServiceName.ToString(),
                    actor.ActorService.ServiceInitializationParameters.PartitionId,
                    actor.ActorService.ServiceInitializationParameters.ReplicaId,
                    FabricRuntime.GetNodeContext().NodeName,
                    finalMessage);
            }
        }

        [Event(2, Level = EventLevel.Informational, Message = "{9}")]
        private void ActorMessage(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
            WriteEvent(
                2,
                actorType,
                actorId,
                applicationTypeName,
                applicationName,
                serviceTypeName,
                serviceName,
                partitionId,
                replicaOrInstanceId,
                nodeName,
                message);
        }

        [NonEvent]
        public void ActorHostInitializationFailed(Exception e)
        {
            if (this.IsEnabled())
            {
                ActorHostInitializationFailed(e.ToString());
            }
        }

        [Event(3, Level = EventLevel.Error, Message = "Actor host initialization failed")]
        private void ActorHostInitializationFailed(string exception)
        {
            WriteEvent(3, exception);
        }

        [Event(4, Level = EventLevel.Informational,
        Message = "{0}: {1}")]
        public void Log(string title, string message)
        {
            WriteEvent(4, title, message);
        }
    }
}
