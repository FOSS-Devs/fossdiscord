using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using FossiumBot.Commands;
using System.Net.Sockets;

namespace FossiumBot
{
    class Program
    {
        // Set the local version, change when making a release
        public static string localversion = "v1.1-Dev";

        // For the uptime command
        public static DateTimeOffset StartTime = DateTime.Now;
        private static LogLevel log;

        public InteractivityExtension Interactivity {  get; set; }
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }
        internal static async Task MainAsync()
        {
            if (!File.Exists("config.json"))
            {
                Console.WriteLine("No config found\nStarting the config creator...\n");
                WriteConfig();
                Console.Clear();
                Console.WriteLine("Starting the bot...");
            }

            JObject cfgjson = JObject.Parse(await File.ReadAllTextAsync("config.json"));

            if(cfgjson["debug"].ToString() == "yes")
            {
                log = LogLevel.Debug;
            }
            else
            {
                log = LogLevel.None;
            }

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = cfgjson["token"].ToString(),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,

                // Remove this when making a release
                MinimumLogLevel = log
            });
            //discord.ComponentInteractionCreated += async (s, e) =>
            //{
            //    if (e.Id == "help_moderation")
            //    {
            //        var embed = new DiscordEmbedBuilder
            //        {
            //            Title = $"Help",
            //            Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
            //            Color = new DiscordColor(0x0080FF)
            //        };
            //        embed.AddField("Moderation", "Autodelete\nBan\nDelwarn\nKick\nMute\nPurge\nSoftban\nUnban\nUnmute\nWarn\nWarns");
            //        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            //        new DiscordInteractionResponseBuilder()
            //            .AddEmbed(embed)
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", true),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
            //            })
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
            //            }));
            //        return;
            //    }
            //    else if (e.Id == "help_utils")
            //    {
            //        var embed = new DiscordEmbedBuilder
            //        {
            //            Title = $"Help",
            //            Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
            //            Color = new DiscordColor(0x0080FF)
            //        };
            //        embed.AddField("Moderation", "Avatar\nEmoji\nPing\nPoll\nQuickpoll\nServerinfo\nUptime\nUserinfo");
            //        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            //        new DiscordInteractionResponseBuilder()
            //            .AddEmbed(embed)
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", true),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
            //            })
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
            //            }));
            //        return;
            //    }
            //    else if (e.Id == "help_fun")
            //    {
            //        var embed = new DiscordEmbedBuilder
            //        {
            //            Title = $"Help",
            //            Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
            //            Color = new DiscordColor(0x0080FF)
            //        };
            //        embed.AddField("Fun", "Cat\nDog\nRate\nWikipedia");
            //        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            //        new DiscordInteractionResponseBuilder()
            //            .AddEmbed(embed)
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", true),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
            //            })
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
            //            }));
            //        return;
            //    }
            //    else if (e.Id == "help_music")
            //    {
            //        var embed = new DiscordEmbedBuilder
            //        {
            //            Title = $"Help",
            //            Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
            //            Color = new DiscordColor(0x0080FF)
            //        };
            //        embed.AddField("Music", "Nowplaying\nPlay\nStop");
            //        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            //        new DiscordInteractionResponseBuilder()
            //            .AddEmbed(embed)
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", true),
            //            })
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
            //            }));
            //        return;
            //    }
            //    else if (e.Id == "help_owner")
            //    {
            //        var embed = new DiscordEmbedBuilder
            //        {
            //            Title = $"Help",
            //            Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
            //            Color = new DiscordColor(0x0080FF)
            //        };
            //        embed.AddField("Owner", "Leaveserver\nServers\nShutdown");
            //        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            //        new DiscordInteractionResponseBuilder()
            //            .AddEmbed(embed)
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
            //            })
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", true),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
            //            }));
            //        return;
            //    }
            //    else if (e.Id == "help_settings")
            //    {
            //        var embed = new DiscordEmbedBuilder
            //        {
            //            Title = $"Help",
            //            Description = $"Use `{cfgjson["prefix"]}help <command>` for extended information on a command",
            //            Color = new DiscordColor(0x0080FF)
            //        };
            //        embed.AddField("Settings", "Loggingchannel");
            //        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            //        new DiscordInteractionResponseBuilder()
            //            .AddEmbed(embed)
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
            //            })
            //            .AddComponents(new DiscordComponent[]
            //            {
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
            //                new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", true)
            //            }));
            //        return;
            //    }
            //};
            discord.GuildAvailable += async (s, e) =>
            {
                string jsonfile = $"Settings/guilds/{e.Guild.Id}.json";
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (!File.Exists(jsonfile))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                    new JProperty("loggingchannelid", "null"),
                                    new JProperty("muterole", "null"),
                                    new JProperty("welcomer", "off"),
                                    new JProperty("welcomercustommessage", "null")
                                 }
                            )
                        );
                    string dataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    await File.WriteAllTextAsync(jsonfile, dataWrite);
                    await Task.CompletedTask;
                }

                List<string> guildslist = new List<string>();
                foreach (DiscordGuild guild in discord.Guilds.Values)
                {
                    guildslist.Add(guild.Id.ToString());
                }

                List<string> fileslist = new List<string>();
                foreach (string file in Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Settings/guilds/", "*.json"))
                {
                    fileslist.Add(file);
                }
                foreach (string file in fileslist)
                {
                    if (!guildslist.Contains(Path.GetFileNameWithoutExtension(file)))
                    {
                        File.Delete(file);
                    }
                }
                return;
            };
            discord.GuildCreated += async (s, e) =>
            {
                string jsonfile = $"Settings/guilds/{e.Guild.Id}.json";
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (!File.Exists(jsonfile))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                    new JProperty("loggingchannelid", "null"),
                                    new JProperty("muterole", "null"),
                                    new JProperty("welcomer", "off"),
                                    new JProperty("welcomercustommessage", "null")
                                 }
                            )
                        );
                    string dataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    await File.WriteAllTextAsync(jsonfile, dataWrite);
                }
                await Task.CompletedTask;
            };
            discord.GuildDeleted += async (s, e) =>
            {
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                try
                {
                    File.Delete(file);
                }
                catch (FileNotFoundException)
                {
                    await Task.CompletedTask;
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
                Directory.CreateDirectory(@"Settings/guilds");
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
                if((string)jsonData["config"]["loggingchannelid"] == "null" || e.Message.Author == null || discord.CurrentUser.Id == e.Message.Author.Id)
                {
                    return;
                }
                else 
                {
                    ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
                    DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                    embed.WithAuthor($"{e.Message.Author.Username}#{e.Message.Author.Discriminator}", null, e.Message.Author.AvatarUrl);
                    embed.AddField("Content", e.Message.Content);
                    embed.AddField("ID", $"```TOML\nUser = {e.Message.Author.Id}\nMessage = {e.Message.Id}\n```");
                    await loggingchannel.SendMessageAsync(embed);
                    return;
                }
            };
            discord.MessageUpdated += async (s, e) =>
            {
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                if (e.Message.IsEdited == false || e.Message.Embeds.Count >= 1)
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
                    Directory.CreateDirectory(@"Settings/guilds/");
                    if (!File.Exists(file))
                    {
                        return;
                    }
                    JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
                    if ((string)jsonData["config"]["loggingchannelid"] == "null")
                    {
                        return;
                    }
                    ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
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
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Member joined",
                    Color = new DiscordColor(0x2ECC70),
                    Timestamp = e.Member.JoinedAt
                };
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (!File.Exists(file))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
                if ((string)jsonData["config"]["loggingchannelid"] == "null")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.WithAuthor($"{e.Member.Username}#{e.Member.Discriminator}", null, e.Member.AvatarUrl);
                embed.AddField("ID", e.Member.Id.ToString());
                long membercreation = e.Member.CreationTimestamp.ToUnixTimeSeconds();
                embed.AddField("Registered", $"<t:{membercreation}:F>");
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.GuildMemberRemoved += async (s, e) =>
            {
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Member left",
                    Color = new DiscordColor(0xFF0000),
                    Timestamp = DateTime.Now
                };
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (!File.Exists(file))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
                if ((string)jsonData["config"]["loggingchannelid"] == "null")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
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
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Channel created",
                    Description = e.Channel.Mention,
                    Color = new DiscordColor(0x2ECC70),
                    Timestamp = e.Channel.CreationTimestamp
                };
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (!File.Exists(file))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
                if ((string)jsonData["config"]["loggingchannelid"] == "null")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
                DiscordChannel loggingchannel = e.Guild.GetChannel(loggingchannelid);
                embed.AddField("Type", e.Channel.Type.ToString());
                embed.AddField("ID", e.Channel.Id.ToString());
                await loggingchannel.SendMessageAsync(embed);
            };
            discord.ChannelUpdated += async (s, e) =>
            {
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Channel updated",
                    Description = e.ChannelAfter.Mention,
                    Color = new DiscordColor(0x2ECC70),
                    Timestamp = DateTime.Now
                };
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (!File.Exists(file))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
                if ((string)jsonData["config"]["loggingchannelid"] == "null")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
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
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Channel deleted",
                    Color = new DiscordColor(0xFF0000),
                    Timestamp = DateTime.Now
                };
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (!File.Exists(file))
                {
                    return;
                }
                JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
                if ((string)jsonData["config"]["loggingchannelid"] == "null")
                {
                    return;
                }
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
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
            discord.GuildMemberAdded += async (s, e) =>
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                string file = $"Settings/guilds/{e.Guild.Id}.json";
                if (!File.Exists(file))
                {
                    return;
                }

                string json = await File.ReadAllTextAsync(file);
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                    if (jsonData["config"]["welcomer"] == "off")
                    {
                        return;
                    }

                    if (jsonData["config"]["welcomerchannel"] == "null")
                    {
                        return;
                    }

                    if (jsonData["config"]["welcomercustommessage"] == "null")
                    {
                        ulong welcomerchannelID = (ulong)jsonData["config"]["welcomerchannel"];
                        DiscordChannel welcomerchannel = e.Guild.GetChannel(welcomerchannelID);
                        await welcomerchannel.SendMessageAsync($"Welcome {e.Member.Mention}, to {e.Guild.Name}!");
                    }
                    else
                    {
                        ulong welcomerchannelID = (ulong)jsonData["config"]["welcomerchannel"];
                        DiscordChannel welcomerchannel = e.Guild.GetChannel(welcomerchannelID);
                        string custommessage = jsonData["config"]["welcomercustommessage"];
                        string custommessagereplaced = custommessage.Replace("{user}", e.Member.Username).Replace("{usermention}", e.Member.Mention).Replace("{servername}", e.Guild.Name);
                        await welcomerchannel.SendMessageAsync(custommessagereplaced);
                    }   
            };
            discord.MessageReactionAdded += async (s, e) =>
            {
                if (e.User.IsBot)
                {
                    return;
                }
                if (!File.Exists(@$"Settings/reactionroles/{e.Guild.Id}-{e.Channel.Id}-{e.Message.Id}.json"))
                {
                    return;
                }
        
                JObject jsonData =
                    JObject.Parse(
                        await File.ReadAllTextAsync(
                            @$"Settings/reactionroles/{e.Guild.Id}-{e.Channel.Id}-{e.Message.Id}.json"));
                string[] splitreactionroles = jsonData["reactionroles"]["roles"].ToString().Split(", ");
                string[] splitreactionemojis = jsonData["reactionroles"]["emojis"].ToString().Split(", ");

                if (Array.IndexOf(splitreactionemojis, e.Emoji.GetDiscordName()) == -1)
                {
                    return;
                }
                
                string reactionrole = splitreactionroles[Array.IndexOf(splitreactionemojis, e.Emoji.GetDiscordName())];
                DiscordRole role = e.Guild.GetRole(ulong.Parse(reactionrole));
                DiscordMember member = (DiscordMember)e.User;
                try
                {
                    await member.GrantRoleAsync(role);
                }
                catch
                {
                    var errorembed = new DiscordEmbedBuilder
                    {
                        Title = $"Oops...",
                        Description = $"Reaction roles message ID: {e.Message.Id}\nI don't have the permission to give role `{role.Name}` to `{e.User.Username}#{e.User.Discriminator}`",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await e.Guild.Owner.SendMessageAsync(errorembed);
                }

                await e.Message.DeleteReactionAsync(e.Emoji, e.User);
            };

            discord.UseInteractivity();
            var slash = discord.UseSlashCommands();
            //slash.RegisterCommands<Fun>(848464241219338250);
            slash.RegisterCommands<Fun>();
            //slash.RegisterCommands<Music>(848464241219338250);
            slash.RegisterCommands<Music>();
            //slash.RegisterCommands<Owner>(848464241219338250);
            slash.RegisterCommands<Owner>();
            //slash.RegisterCommands<Moderation>(848464241219338250);
            slash.RegisterCommands<Moderation>();
            //slash.RegisterCommands<Settings>(848464241219338250);
            slash.RegisterCommands<Settings>();
            //slash.RegisterCommands<Utils>(848464241219338250);
            slash.RegisterCommands<Utils>();
            //slash.RegisterCommands<Update>(848464241219338250);
            slash.RegisterCommands<Update>();
            DiscordActivity discordActivity = new DiscordActivity
            {
                Name = $"for commands | {localversion}",
                ActivityType = ActivityType.Watching
            };

            var localendpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };
            var locallavalinkConfig = new LavalinkConfiguration
            {
                Password = "whatdidyousayagain",
                RestEndpoint = localendpoint,
                SocketEndpoint = localendpoint
            };
            var lavalink = discord.UseLavalink();
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Oops...\nSomething went wrong");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("In most cases this means that the token is invalid");
                goto Ask;
            Ask:
                Console.Write("Do you want to\n(r)ewrite the config\n(s)how more info\n(q)uit\n");
                string answer = Console.ReadLine();
                if (answer == "r")
                {
                    Console.Clear();
                    Console.WriteLine("Starting the config creator...\n");
                    WriteConfig();
                    Console.Clear();
                    Console.WriteLine("Starting the bot...");
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

            //Remove some session specific files/dirs
            if (Directory.Exists("Data/playback/"))
            {
                Directory.Delete("Data/playback/", true);
            }

            Console.WriteLine("----------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connected!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Please use /shutdown to properly shut down the bot");
            Console.WriteLine("----------------------------------------");
            TcpClient tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(localendpoint.Hostname, localendpoint.Port);
                tcpClient.Dispose();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Connected to Lavalink");
                Console.ForegroundColor = ConsoleColor.White;
                await lavalink.ConnectAsync(locallavalinkConfig);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No local Lavalink instance found.\nIf you do, make sure the port is \"2333\" and the password is \"whatdidyousayagain\"");
                Console.ForegroundColor = ConsoleColor.White;
                var mainendpoint = new ConnectionEndpoint
                {
                    Hostname = "fossiumbot-lavalink.herokuapp.com",
                    Port = 80
                };
                var mainlavalinkConfig = new LavalinkConfiguration
                {
                    Password = "whatdidyousayagain",
                    RestEndpoint = mainendpoint,
                    SocketEndpoint = mainendpoint
                };
            
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Connected to Lavalink");
                    Console.ForegroundColor = ConsoleColor.White;
                    await lavalink.ConnectAsync(mainlavalinkConfig);
                }
                catch
                {
                    Console.WriteLine("----------------------------------------");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not find a working local or hosted Lavalink instance");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("If you continue you won't have any music functionality");
                    Console.WriteLine("----------------------------------------");
                }
            }

            await Task.Delay(-1);
        }

        private static void WriteConfig()
        {
            Console.Write("Enter your bot token: ");
            string token = Console.ReadLine();
            Console.Write($"Is this correct? \"{token}\"\n(y)es\n(n)o\n");
            string confirmation = Console.ReadLine();
            Console.Write("Debug mode: \n(y)es\n(n)o\n");
            string debug = Console.ReadLine();
            if (debug.ToLower() != "y" || debug.ToLower() != "yes")
            {
                debug = "no";
            }
            else
            {
                debug = "yes";
            }
            if (confirmation != "y")
            {
                Console.Clear();
                Console.WriteLine("Re-running config creator\n");
                WriteConfig();
            }
            Console.WriteLine("Writing config...");
            JObject data = new JObject(
                new JProperty("token", $"{token}"),
                new JProperty("debug", $"{debug}")
                );
            string configjson = JsonConvert.SerializeObject(data, Formatting.Indented);
            string path = @"config.json";
            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine(configjson);
            };
        }
    }
}
