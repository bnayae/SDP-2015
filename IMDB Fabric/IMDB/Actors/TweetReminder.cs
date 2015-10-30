
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
        private const int TIMER_DURATION_SEC = 30; 

        public async Task Initialize()
        {
            ActorEventSource.Current.ActorMessage(this, $"INITIALIZE: {Host.ActivationContext.WorkDirectory}, {this.Id}");
            if (State.Inilialized)
                return;                  
            State.Inilialized = true;

            IActorReminder reminderRegistration = 
                await RegisterReminder(
                            REMINDER_NAME,
                            new byte[0],
                            TimeSpan.FromSeconds(TIMER_DURATION_SEC),
                            TimeSpan.FromSeconds(TIMER_DURATION_SEC),
                            ActorReminderAttributes.None);
        }

        public async Task ReceiveReminderAsync(
            string reminderName, 
            byte[] context, 
            TimeSpan dueTime,
            TimeSpan period)
        {
            ActorEventSource.Current.ActorMessage(this, "Reminder");

            var id = ActorId.NewId();
            var tweetLoader = ActorProxy.Create<ITweetLoader>(id);
            await tweetLoader.LoadAsync();
        }
    }
}
