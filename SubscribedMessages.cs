using System;
using System.Timers;
using System.Threading.Tasks;
using Discord;

namespace Bot
{
    class SubscribedMessages
    {
        private static Timer timer;

        public static void Init()
        {
            if (!Config.GetSetting<bool>("disable-ready-messages", false)) Program.GetClient().Ready += ReadyMessage;

            timer = new Timer();
            DateTime nowTime = DateTime.Now;
            DateTime nextTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0, 0);
            if (nowTime > nextTime) nextTime = nextTime.AddDays(1);
            timer.Interval = (nextTime - nowTime).TotalMilliseconds;
            timer.Elapsed += InsaneMessage;
            timer.Start();
        }

        private static async Task ReadyMessage()
        {
            string messageToSend = Person.GetRandomItem("on_connect");
            foreach (var server in Database.GetServersEnumerable())
            {
                if (server.Value.linked_channels.on_connect != 0)
                {
                    var channel = (IMessageChannel) Program.GetClient().GetGuild((ulong)Convert.ToInt64(server.Key)).GetChannel((ulong)Convert.ToInt64(server.Value.linked_channels.on_connect));
                    await channel.SendMessageAsync(messageToSend);
                }
            }
        }

        private static async void InsaneMessage(object sender, ElapsedEventArgs e)
        {
            if (Program.rng.NextSingle() > Config.GetSetting<double>("insane-rambling-chance", 100.0) / 100) return;

            string messageToSend = Person.GetRandomItem("insane_ramblings");
            foreach (var server in Database.GetServersEnumerable())
            {
                if (server.Value.linked_channels.insane_ramblings != 0)
                {
                    var channel = (IMessageChannel) Program.GetClient().GetGuild((ulong)Convert.ToInt64(server.Key)).GetChannel((ulong)Convert.ToInt64(server.Value.linked_channels.insane_ramblings));
                    await channel.SendMessageAsync(messageToSend);
                }
            }

            DateTime nowTime = DateTime.Now;
            DateTime nextTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0, 0);
            if (nowTime > nextTime) nextTime = nextTime.AddDays(1);
            timer.Interval = (nextTime - nowTime).TotalMilliseconds;
        }
    }
}