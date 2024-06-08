using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Bot
{
    class ThreadReaction
    {
        public static void Init()
        {
            Program.GetClient().ReactionAdded += ReactionAdded;
        }

        private static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == "thread")
            {
                IUserMessage realMessage = await message.GetOrDownloadAsync();
                if (realMessage.Thread != null) return;

                IMessageChannel realChannel = await channel.GetOrDownloadAsync();

                string name = realMessage.Content;
                if (name == String.Empty) name = "thread";
                if (name.Length > 25) name = name.Substring(0, Math.Min(name.Length, 25));
                
                await ((SocketTextChannel)realChannel).CreateThreadAsync(name:name, message:realMessage);
            }
        }
    }
}