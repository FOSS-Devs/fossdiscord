using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.CommandsNext.Exceptions;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using DSharpPlus.VoiceNext;
using FossiumBot.Commands;

namespace FossiumBot
{
    class Program
    {
        // Set the local version, change when making a release
        public static string localversion = "v1.0-Beta";
        public VoiceNextExtension Voice { get; set; }
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }
        internal static async Task MainAsync()
        {
            if (!File.Exists("config.json"))
            {
                Console.WriteLine("No config found\nStarting config creator...\n");
                WriteConfig();
                Console.WriteLine("Starting the bot...");
            }

            JObject cfgjson = JObject.Parse(File.ReadAllText("config.json"));

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = cfgjson["token"].ToString(),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,

                // Remove this when making a release
                MinimumLogLevel = LogLevel.Debug
            });
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { cfgjson["prefix"].ToString() },
                EnableMentionPrefix = true,
                EnableDms = false,
                EnableDefaultHelp = false
            });
            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            discord.ComponentInteractionCreated += async (s, e) =>
            {
                if (e.Id == "help_moderation")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Help",
                        Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
                        Color = new DiscordColor(0x0080FF)
                    };
                    embed.AddField("Moderation", "Autodelete\nBan\nDelwarn\nKick\nMute\nPurge\nSoftban\nUnban\nUnmute\nWarn\nWarns");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", true),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
                        })
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
                        }));
                    return;
                }
                else if (e.Id == "help_utils")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Help",
                        Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
                        Color = new DiscordColor(0x0080FF)
                    };
                    embed.AddField("Moderation", "Avatar\nEmoji\nPing\nPoll\nQuickpoll\nServerinfo\nUptime\nUserinfo");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", true),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
                        })
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
                        }));
                    return;
                }
                else if (e.Id == "help_fun")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Help",
                        Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
                        Color = new DiscordColor(0x0080FF)
                    };
                    embed.AddField("Fun", "Cat\nDog\nRate\nWikipedia");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", true),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
                        })
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
                        }));
                    return;
                }
                else if (e.Id == "help_music")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Help",
                        Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
                        Color = new DiscordColor(0x0080FF)
                    };
                    embed.AddField("Music", "Nowplaying\nPlay\nStop");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", true),
                        })
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
                        }));
                    return;
                }
                else if (e.Id == "help_owner")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Help",
                        Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
                        Color = new DiscordColor(0x0080FF)
                    };
                    embed.AddField("Owner", "Leaveserver\nServers\nShutdown");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
                        })
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", true),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
                        }));
                    return;
                }
                else if (e.Id == "help_settings")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Help",
                        Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
                        Color = new DiscordColor(0x0080FF)
                    };
                    embed.AddField("Settings", "Loggingchannel");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
                        })
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
                            new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", true)
                        }));
                    return;
                }
            };

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
                if (e.Message.Author == null)
                {
                    return;
                }
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
                else if (e.Message.Embeds.Count >= 1)
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
                long membercreation = e.Member.CreationTimestamp.ToUnixTimeSeconds();
                embed.AddField("Registered", $"<t:{membercreation}:F>");
                long memberjoinedat = e.Member.JoinedAt.ToUnixTimeSeconds();
                embed.AddField("Joined Server", $"<t:{memberjoinedat}:F>");
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

            commands.CommandErrored += async (s, e) =>
            {
                if (e.Exception is CommandNotFoundException)
                {
                    string messagecommand = e.Context.Message.Content.Replace(cfgjson["prefix"].ToString(), "");
                    var commandnotfoundembed = new DiscordEmbedBuilder
                    {
                        Title = "Oops...",
                        Description = $"The command `{messagecommand}` was not found",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await e.Context.RespondAsync(commandnotfoundembed);
                    return;
                }
                else if (e.Exception.Message == "Could not find a suitable overload for the command.")
                {
                    string messagecommand = e.Context.Message.Content.Replace(cfgjson["prefix"].ToString(), "").Split(" ")[0].ToString();
                    var overloadembed = new DiscordEmbedBuilder
                    {
                        Title = "Oops...",
                        Description = $"One or more arguments are not needed or missing\nRun `{cfgjson["prefix"]}help {messagecommand}` to see all the arguments",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await e.Context.RespondAsync(overloadembed);
                    return;
                }
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = $"Something went wrong:\n`{e.Exception.Message}`",
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync(embed);
            };

            discord.UseVoiceNext();
            var slash = discord.UseSlashCommands();
            //slash.RegisterCommands<Fun>(848464241219338250);
            slash.RegisterCommands<Fun>();
            //slash.RegisterCommands<Music>(848464241219338250);
            slash.RegisterCommands<Music>();
            //slash.RegisterCommands<Owner>(848464241219338250);
            slash.RegisterCommands<Owner>();
            //slash.RegisterCommands<Moderation>(848464241219338250);
            slash.RegisterCommands<Moderation>();
            DiscordActivity discordActivity = new DiscordActivity
            {
                Name = $"{cfgjson["prefix"]}help | {localversion}",
                ActivityType = ActivityType.Playing
            };
            if (Directory.Exists(@"Settings/lck/"))
            {
                Directory.Delete("Settings/lck/", true);
            }
            try
            {
            
                await discord.ConnectAsync(discordActivity);
            }
            catch(Exception e)
            {
                Console.Clear();
                Console.WriteLine("Oops...\nSomething went wrong");
                Console.WriteLine("In most cases this means that the token is invalid");
                goto Ask;
            Ask:
                Console.Write("Do you want to\n(r)ewrite the config\n(s)how more info\n(q)uit\n");
                string answer = Console.ReadLine();
                if (answer == "r")
                {
                    Console.Clear();
                    Console.WriteLine("Starting config creator...\n");
                    WriteConfig();
                    Console.Clear();
                    MainAsync().GetAwaiter().GetResult();

                }
                else if (answer == "s")
                {
                    Console.WriteLine("-----Detailed Error-----");
                    Console.WriteLine(e);
                    Console.WriteLine("-------------------------");
                    Console.Write("\n");
                    goto Ask;
                }
                else if (answer == "q")
                {
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("That option does not exist\n");
                    goto Ask;
                }
            }
            Console.WriteLine("--------------------");
            Console.WriteLine("Connected!");
            Console.WriteLine($"Please use {cfgjson["prefix"]}shutdown to properly shut down the bot");
            Console.WriteLine("--------------------");
            await Task.Delay(-1);
        }

        private static void WriteConfig()
        {
            Console.Write("Enter your bot token: ");
            string token = Console.ReadLine();
            Console.Write($"Is this correct? \"{token}\"\n(y)es\n(n)o\n");
            string confirmation = Console.ReadLine();
            if (confirmation != "y")
            {
                Console.Clear();
                Console.WriteLine("Re-running config creator\n");
                WriteConfig();
            }
            Console.Clear();
            Console.Write("Enter the prefix you want the bot to use: ");
            string prefix = Console.ReadLine();
            Console.Write($"Is this correct? \"{prefix}\"\n(y)es\n(n)o\n");
            string confirmation2 = Console.ReadLine();
            if (confirmation2 != "y")
            {
                Console.Clear();
                Console.WriteLine("Re-running config creator\n");
                WriteConfig();
            }
            Console.WriteLine("Writing config...");

            JObject data = new JObject(
                new JProperty("token", $"{token}"),
                new JProperty("prefix", $"{prefix}")
                );

            string configjson = JsonConvert.SerializeObject(data);
            string path = @"config.json";
            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine(configjson);
            };
        }
    }
}
