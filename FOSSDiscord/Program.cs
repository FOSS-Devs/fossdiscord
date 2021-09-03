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
using Newtonsoft.Json.Linq;

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
                Directory.CreateDirectory(@"Settings/");
                if(!File.Exists($"Settings/Loggingsettings-{e.Guild.Id}.json"))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(File.ReadAllText($"Settings/Loggingsettings-{e.Guild.Id}.json"));
                if(jsonData["Loggingchannelid"].ToString() == "0")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["Loggingchannelid"];
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.WithAuthor($"{e.Message.Author.Username}#{e.Message.Author.Discriminator}", null, e.Message.Author.AvatarUrl);
                embed.AddField("Content", e.Message.Content);
                embed.AddField("ID", $"```TOML\nUser = {e.Message.Author.Id}\nMessage = {e.Message.Id}\n```");
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.MessageUpdated += async (s, e) =>
            {
                if(e.Message.IsEdited == false)
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
                    Directory.CreateDirectory(@"Settings/");
                    if (!File.Exists($"Settings/Loggingsettings-{e.Guild.Id}.json"))
                    {
                        return;
                    }
                    JObject jsonData = JObject.Parse(File.ReadAllText($"Settings/Loggingsettings-{e.Guild.Id}.json"));
                    if (jsonData["Loggingchannelid"].ToString() == "0")
                    {
                        return;
                    }
                    ulong loggingchannelid = (ulong)jsonData["Loggingchannelid"];
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
                    Color = new DiscordColor(0x2ECC70),
                    Timestamp = e.Member.JoinedAt
                };
                Directory.CreateDirectory(@"Settings/");
                if (!File.Exists($"Settings/Loggingsettings-{e.Guild.Id}.json"))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(File.ReadAllText($"Settings/Loggingsettings-{e.Guild.Id}.json"));
                if (jsonData["Loggingchannelid"].ToString() == "0")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["Loggingchannelid"];
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
                Directory.CreateDirectory(@"Settings/");
                if (!File.Exists($"Settings/Loggingsettings-{e.Guild.Id}.json"))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(File.ReadAllText($"Settings/Loggingsettings-{e.Guild.Id}.json"));
                if (jsonData["Loggingchannelid"].ToString() == "0")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["Loggingchannelid"];
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.WithAuthor($"{e.Member.Username}#{e.Member.Discriminator}", null, e.Member.AvatarUrl);
                embed.AddField("ID", e.Member.Id.ToString());
                long memberjoinedat = e.Member.JoinedAt.ToUnixTimeSeconds();
                embed.AddField("Joined Guild", $"<t:{memberjoinedat}:F>");
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.ChannelCreated += async (s, e) =>
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Channel created",
                    Description = e.Channel.Mention,
                    Color = new DiscordColor(0x2ECC70),
                    Timestamp = e.Channel.CreationTimestamp
                };
                Directory.CreateDirectory(@"Settings/");
                if (!File.Exists($"Settings/Loggingsettings-{e.Guild.Id}.json"))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(File.ReadAllText($"Settings/Loggingsettings-{e.Guild.Id}.json"));
                if (jsonData["Loggingchannelid"].ToString() == "0")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["Loggingchannelid"];
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.AddField("Type", e.Channel.Type.ToString());
                embed.AddField("ID", e.Channel.Id.ToString());
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.ChannelUpdated += async (s, e) =>
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Channel updated",
                    Description = e.ChannelAfter.Mention,
                    Color = new DiscordColor(0x2ECC70),
                    Timestamp = DateTime.Now
                };
                Directory.CreateDirectory(@"Settings/");
                if (!File.Exists($"Settings/Loggingsettings-{e.Guild.Id}.json"))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(File.ReadAllText($"Settings/Loggingsettings-{e.Guild.Id}.json"));
                if (jsonData["Loggingchannelid"].ToString() == "0")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["Loggingchannelid"];
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                if(e.ChannelBefore.Name != e.ChannelAfter.Name)
                {
                    embed.AddField("Name", $"**Before**: {e.ChannelBefore.Name}\n**After**: {e.ChannelAfter.Name}");
                }
                if(e.ChannelAfter.Type == ChannelType.Text && e.ChannelBefore.Topic != e.ChannelAfter.Topic)
                {
                    if(e.ChannelBefore.Topic == null)
                    {
                        embed.AddField("Topic", $"**Before**: <none>\n**After**: `{e.ChannelAfter.Topic}`");
                    }
                    else if(e.ChannelAfter.Topic == null)
                    {
                        embed.AddField("Topic", $"**Before**: `{e.ChannelBefore.Topic}`\n**After**: <none>");
                    }
                    else
                    {
                        embed.AddField("Topic", $"**Before**: `{e.ChannelBefore.Topic}`\n**After**: `{e.ChannelAfter.Topic}`");
                    }
                }
                long channelcreation = e.ChannelBefore.CreationTimestamp.ToUnixTimeSeconds();
                embed.AddField("Type", e.ChannelAfter.Type.ToString());
                embed.AddField("Creation date", $"<t:{channelcreation}:F>");
                embed.AddField("ID", e.ChannelAfter.Id.ToString());
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.ChannelDeleted += async (s, e) =>
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Channel deleted",
                    Color = new DiscordColor(0xFF0000),
                    Timestamp = DateTime.Now
                };
                Directory.CreateDirectory(@"Settings/");
                if (!File.Exists($"Settings/Loggingsettings-{e.Guild.Id}.json"))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(File.ReadAllText($"Settings/Loggingsettings-{e.Guild.Id}.json"));
                if (jsonData["Loggingchannelid"].ToString() == "0")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["Loggingchannelid"];
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                long channelcreation = e.Channel.CreationTimestamp.ToUnixTimeSeconds();
                embed.AddField("Name", e.Channel.Name.ToString());
                if(e.Channel.Type == ChannelType.Text && e.Channel.Topic != null)
                {
                    embed.AddField("Topic", e.Channel.Topic);
                }
                embed.AddField("Type", e.Channel.Type.ToString());
                embed.AddField("Creation date", $"<t:{channelcreation}:F>");
                embed.AddField("ID", e.Channel.Id.ToString());
                await loggingchannel.SendMessageAsync(embed);
            };


            DiscordActivity discordActivity = new DiscordActivity
            {
                Name = $"{cfgjson.CommandPrefix}help | v1.0-Dev",
                ActivityType = ActivityType.Playing
            };
            if (Directory.Exists(@"Settings/lck/"))
            {
                Directory.Delete("Settings/lck/");
            }
            await discord.ConnectAsync(discordActivity);
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
