
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMDB.Interfaces;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Actors;

namespace IMDB
{
    // Reminder must be stateful actor

    public class TweetReminder : Actor<Status>, ITweetReminder, IRemindable
    {
        private const string REMINDER_NAME = "TweetReminder";

        public Task Initialize()
        {
            if (State.Inilialized)
                return Task.FromResult(1);
                     
            Task<IActorReminder> reminderRegistration = RegisterReminder(
                                                            REMINDER_NAME,
                                                            new byte[0],
                                                            TimeSpan.FromSeconds(10),
                                                            TimeSpan.FromDays(10),
                                                            ActorReminderAttributes.None);
            State.Inilialized = true;
            return Task.FromResult(1);
        }


        public Task ReceiveReminderAsync(
            string reminderName, 
            byte[] context, 
            TimeSpan dueTime,
            TimeSpan period)
        {
            ActorEventSource.Current.ActorMessage(this, "Reminder");

            var id = ActorId.NewId();
            var tweetLoader = ActorProxy.Create<ITweetLoader>(id);
            tweetLoader.Load();

            return Task.FromResult(1);
        }
    }
}
