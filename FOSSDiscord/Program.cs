using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace FOSSDiscord
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }
        internal static async Task MainAsync()
        {
            var json = "";
            // Remove ..\..\..\ when making a release
            using (var fs = File.OpenRead(@"..\..\..\config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,

                // Remove this when making a release
                MinimumLogLevel = LogLevel.Debug

            });
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { cfgjson.CommandPrefix },
                EnableMentionPrefix = true
            });
            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            // Logging
            discord.MessageDeleted += async (s, e) =>
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Message deleted in #{e.Channel.Name}",
                    Color = new DiscordColor(0xFF0000),
                    Timestamp = e.Message.Timestamp
                };
                ulong loggingchannelid = 848826372390518805;
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.WithAuthor($"{e.Message.Author.Username}#{e.Message.Author.Discriminator}", null, e.Message.Author.AvatarUrl);
                embed.AddField("Content", e.Message.Content);
                embed.AddField("ID", $"```TOML\nUser = {e.Message.Author.Id}\nMessage = {e.Message.Id}\n```");
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.MessageUpdated += async (s, e) =>
            {
                if(e.MessageBefore.Content == e.Message.Content)
                {
                    return;
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Message edited in #{e.Channel.Name}",
                        Description = $"[Jump To Message]({e.Message.JumpLink})",
                        Color = new DiscordColor(0xFFA500),
                        Timestamp = e.Message.Timestamp
                    };
                    ulong loggingchannelid = 848826372390518805;
                    DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                    embed.WithAuthor($"{e.Message.Author.Username}#{e.Message.Author.Discriminator}", null, e.Message.Author.AvatarUrl);
                    embed.AddField("Before", e.MessageBefore.Content);
                    embed.AddField("After", e.Message.Content);
                    embed.AddField("ID", $"```TOML\nUser = {e.Message.Author.Id}\nMessage = {e.Message.Id}\n```");
                    await loggingchannel.SendMessageAsync(embed);
                }
            };
            discord.GuildMemberAdded += async (s, e) =>
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Member joined",
                    Color = new DiscordColor(0xFFA500),
                    Timestamp = e.Member.JoinedAt
                };
                ulong loggingchannelid = 848826372390518805;
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.WithAuthor($"{e.Member.Username}#{e.Member.Discriminator}", null, e.Member.AvatarUrl);
                embed.AddField("ID", e.Member.Id.ToString());
                long membercreation = e.Member.CreationTimestamp.ToUnixTimeSeconds();
                embed.AddField("Registered", $"<t:{membercreation}:F>");
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.GuildMemberRemoved += async (s, e) =>
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Member left",
                    Color = new DiscordColor(0xFF0000),
                    Timestamp = DateTime.Now
                };
                ulong loggingchannelid = 848826372390518805;
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.WithAuthor($"{e.Member.Username}#{e.Member.Discriminator}", null, e.Member.AvatarUrl);
                embed.AddField("ID", e.Member.Id.ToString());
                long memberjoinedat = e.Member.JoinedAt.ToUnixTimeSeconds();
                embed.AddField("Joined Guild", $"<t:{memberjoinedat}:F>");
                await loggingchannel.SendMessageAsync(embed);
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        public struct ConfigJson
        {
            [JsonProperty("token")]
            public string Token { get; private set; }

            [JsonProperty("prefix")]
            public string CommandPrefix { get; private set; }
        }
    }
}
