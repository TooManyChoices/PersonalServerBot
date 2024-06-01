using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bot
{
    public class SlashCommands
    {
        public static async Task RegisterCommands()
        {
            var applicationCommandProperties = new ApplicationCommandProperties[] {
                new SlashCommandBuilder()
                    .WithName("set")
                    .WithDescription("Set some values, idfk.")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("color")
                        .WithDescription("Set your color.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("hex", ApplicationCommandOptionType.String, "hex", 
                            isRequired:true
                        )
                    )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("title")
                        .WithDescription("Set your title.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("title", ApplicationCommandOptionType.String, "title", 
                            isRequired:true
                        )
                    )
                    .Build(),
                new SlashCommandBuilder()
                    .WithName("git")
                    .WithDescription("Link to the GitHub repo for this bot.").Build(),
                new SlashCommandBuilder()
                    .WithName("level")
                    .WithDescription("View your level.")
                    .Build(),
                new SlashCommandBuilder()
                    .WithName("channel")
                    .WithDescription("Manage channel links.")
                    .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
                    .AddOptions(
                        new SlashCommandOptionBuilder()
                            .WithName("onconnect")
                            .WithDescription("Receive message when the bot starts up.")
                            .WithType(ApplicationCommandOptionType.SubCommand),
                        new SlashCommandOptionBuilder()
                            .WithName("insaneramblings")
                            .WithDescription("Receive a message in the middle of the night.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                    )
                    .Build(),
                new SlashCommandBuilder()
                    .WithName("role")
                    .WithDescription("Manage role links.")
                    .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
                    .AddOptions(
                        new SlashCommandOptionBuilder()
                            .WithName("member")
                            .WithDescription("Role that any non-bot user is assigned to upon joining.")
                            .WithType(ApplicationCommandOptionType.Role)
                    )
                    .Build()
            };

            try
            {
                await Program.GetClient().BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
            }
            catch(HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public static async Task SlashCommandExecuted(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "role":
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
                    break;
                }
                case "channel":
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
                    break;
                }
                case "level":
                {
                    await command.RespondAsync(Person.GetRandomItem("slash_level"));
                    break;
                }
                case "git":
                {
                    var embed = new EmbedBuilder()
                        .WithTitle("GitHub Repo")
                        .WithUrl("https://github.com/TooManyChoices/discord-bot-for-personal-server")
                        .WithThumbnailUrl("https://github.githubassets.com/assets/GitHub-Mark-ea2971cee799.png")
                        .WithDescription("bot for a personal server in which i do personal things")
                        .WithColor(Discord.Color.DarkBlue);
                    await command.RespondAsync(embed: embed.Build());
                    break;
                }
                case "set":
                {
                    var role = await GetPersonalRoleAsync(command);
                    SocketGuild guild = Program.GetClient().GetGuild(command.GuildId ?? 0);
                    if (role == null) break;
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
                    break;
                }
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