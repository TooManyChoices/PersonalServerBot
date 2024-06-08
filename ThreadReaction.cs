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
                IMessageChannel realChannel = await channel.GetOrDownloadAsync();
                string content = realMessage.Content;
                content = content.Substring(0, Math.Min(content.Length, 99));
                await ((SocketTextChannel)realChannel).CreateThreadAsync(name:content, message:realMessage).Result.SendMessageAsync("thread");
            }
        }
    }
}