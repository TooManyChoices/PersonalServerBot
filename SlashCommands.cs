using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Bot
{
    public class SlashCommands 
    {
        public static void Init()
        {
            SlashCommandRegistrar.RegisterCommandEvent("set", set);
            SlashCommandRegistrar.RegisterCommandEvent("git", git);
            SlashCommandRegistrar.RegisterCommandEvent("level", level);
            SlashCommandRegistrar.RegisterCommandEvent("channel", channel);
            SlashCommandRegistrar.RegisterCommandEvent("role", role);
            SlashCommandRegistrar.RegisterCommandEvent("purge", purge);
        }

        private static async Task purge(SocketSlashCommand command)
        {
            string condition = command.Data.Options.First().Name;
            var conditionValue = command.Data.Options.First().Value;
            switch (condition)
            {
                case "the-last-x":
                    var returnedMessages = command.Channel.GetMessagesAsync(limit:(int)(long)conditionValue);
                    List<IMessage> toPurge = new List<IMessage>();
                    await foreach (var batch in returnedMessages)
                    {
                        foreach (var message in batch)
                        {
                            toPurge.Add(message);
                        }
                    }
                    await command.RespondAsync(Person.GetRandomItem("slash_purge"));
                    foreach (var message in toPurge)
                    {
                        await message.DeleteAsync();
                    }
                    break;
                case "until-x":
                    await command.RespondAsync(Person.GetRandomItem("not_implemented"));
                    break;
            }
        }

        private static async Task role(SocketSlashCommand command)
        {
            string linkInput = command.Data.Options.First().Name;
            ServerData serverData = Database.ServerDataFromId((ulong)command.GuildId);
            switch (linkInput)
            {
                case "member":
                    serverData.linked_roles.member = (command.Data.Options.First().Value as SocketRole).Id;
                    break;
            }
            Database.UpdateServerData((ulong)command.GuildId, serverData);
            await command.RespondAsync(Person.GetRandomItem("slash_configs"), ephemeral: true);
        }

        private static async Task channel(SocketSlashCommand command)
        {
            string linkInput = command.Data.Options.First().Name;
            ServerData serverData = Database.ServerDataFromId((ulong)command.GuildId);
            switch (linkInput)
            {
                case "onconnect":
                    serverData.linked_channels.on_connect = (ulong)command.ChannelId;
                    break;
                case "insaneramblings":
                    serverData.linked_channels.insane_ramblings = (ulong)command.ChannelId;
                    break;
            }
            Database.UpdateServerData((ulong)command.GuildId, serverData);
            await command.RespondAsync(Person.GetRandomItem("slash_configs"), ephemeral: true);
        }

        private static async Task level(SocketSlashCommand command)
        {
            await command.RespondAsync(Person.GetRandomItem("slash_level"));
        }

        private static async Task git(SocketSlashCommand command)
        {
            var embed = new EmbedBuilder()
                .WithAuthor("TooManyChoices", "https://avatars.githubusercontent.com/u/81335262", "https://github.com/TooManyChoices")
                .AddField("discord-bot-for-personal-server", 
                    "[Link to Repository](https://github.com/TooManyChoices/discord-bot-for-personal-server)"
                )
                .WithThumbnailUrl("https://github.com/TooManyChoices/discord-bot-for-a-server/raw/git-command-and-not-me-making-an-excuse-to-use-branches/assets/GitHub_Invertocat_Light.png")
                .WithUrl("https://github.com/TooManyChoices/discord-bot-for-personal-server");
            await command.RespondAsync(embed: embed.Build());
        }

        private static async Task set(SocketSlashCommand command)
        {
            var role = await GetPersonalRoleAsync(command);
            SocketGuild guild = Program.GetClient().GetGuild(command.GuildId ?? 0);
            if (role == null) return;
            var subcommand = command.Data.Options.First().Name;
            switch (subcommand)
            {
                case "color":
                    string colorInput = (string)command.Data.Options.First().Options.First().Value;
                    if (colorInput[0] != '#') colorInput = '#'+colorInput;
                    System.Drawing.Color newColor = ColorTranslator.FromHtml(colorInput);
                    Discord.Color roleColor = new(newColor.R, newColor.G, newColor.B);
                    await role.ModifyAsync(x => {x.Color = roleColor;});
                    await command.RespondAsync(Person.GetRandomItem("slash_set_color"), ephemeral: true);
                    break;
                case "title":
                    string titleInput = (string)command.Data.Options.First().Options.First().Value;
                    await role.ModifyAsync(x => {x.Name = titleInput;});
                    await command.RespondAsync(Person.GetRandomItem("slash_set_title"), ephemeral: true);
                    break;
            }
        }

        public static async Task<IRole> GetPersonalRoleAsync(IGuild guild, IUser user)
        {
            ServerData serverData = Database.ServerDataFromId(guild.Id);
            if (serverData.personal_roles.ContainsKey(Convert.ToString(user.Id)))
            {
                IRole foundRole = guild.GetRole(serverData.personal_roles[Convert.ToString(user.Id)]);
                if (foundRole != null) return foundRole;
            }
            
            var role = await guild.CreateRoleAsync(user.Username, GuildPermissions.None, null, false, null);
            await (user as IGuildUser).AddRoleAsync(
                role.Id
            );
            serverData.personal_roles[Convert.ToString(user.Id)] = role.Id;
            Database.UpdateServerData(guild.Id, serverData);

            return role;
        }
        public static async Task<IRole> GetPersonalRoleAsync(SocketGuildUser guildUser)
        {
            return await GetPersonalRoleAsync(guildUser.Guild, guildUser);
        }
        public static async Task<IRole> GetPersonalRoleAsync(SocketSlashCommand command)
        {
            return await GetPersonalRoleAsync(Program.GetClient().GetGuild(command.GuildId ?? 0), command.User);
        }
    }
}