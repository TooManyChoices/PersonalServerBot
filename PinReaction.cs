using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Bot
{
    class PinReaction
    {
        public static void Init()
        {
            Program.GetClient().ReactionAdded += ReactionAdded;
            Program.GetClient().ReactionRemoved += ReactionRemoved;
        }

        private static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            IUserMessage realMessage = await message.GetOrDownloadAsync();
            if (reaction.Emote.Name == "pin")
            {
                await realMessage.PinAsync();
            }
        }
        
        private static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == "pin")
            {
                IUserMessage realMessage = await message.GetOrDownloadAsync();
                if (!realMessage.Reactions.ContainsKey(reaction.Emote))
                {
                    await realMessage.UnpinAsync();
                }
            }
        }
    }
}