using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace FossiumBot.Commands
{
    public class Settings : ApplicationCommandModule
    {
        [SlashCommandGroup("settings", "A group with all the settings")]
        public class SettingsGroup : ApplicationCommandModule
        {
            [SlashCommand("loggingchannel", "Set the channel to log events to, disable logging by running the command without any arguments")]
            [SlashRequireUserPermissions(Permissions.Administrator)]
            public async Task LoggingchannelCommand(InteractionContext ctx, [Option("loggingchannel", "Mention the channel to log events to")] DiscordChannel loggingchannel = null)
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                    new JProperty("loggingchannelid", null),
                                    new JProperty("muterole", null),
                                    new JProperty("welcomer", null),
                                    new JProperty("welcomercustommessage", null)
                                 }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }
                string file = $"Settings/guilds/{ctx.Guild.Id}.json";
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (loggingchannel != null && loggingchannel.GuildId != ctx.Guild.Id)
                {
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"Oops...",
                        Description = "That channel is not in this server",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                    return;
                }
                else if (loggingchannel == null && File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = readData.ReadToEnd();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    jsonData["config"]["loggingchannelid"] = null;
                    string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                    File.WriteAllText(file, dataWrite);
                    var disableembed = new DiscordEmbedBuilder
                    {
                        Title = $"Disabled logging",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(disableembed));
                    return;
                }
                else if (loggingchannel != null && File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = readData.ReadToEnd();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    jsonData["config"]["loggingchannelid"] = loggingchannel.Id;
                    string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                    File.WriteAllText(file, dataWrite);
                }
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Set `#{loggingchannel.Name}` as the logging channel",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }

            [SlashCommand("muterole", "Set the role used by the mute commands")]
            [SlashRequireUserPermissions(Permissions.Administrator)]
            public async Task MuteroleCommand(InteractionContext ctx, [Option("muterole", "Mention the role used by the mute commands")] DiscordRole muterole)
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                    new JProperty("loggingchannelid", null),
                                    new JProperty("muterole", null),
                                    new JProperty("welcomer", null),
                                    new JProperty("welcomercustommessage", null)
                                 }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }
                string json = File.ReadAllText($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                jsonData["config"]["muterole"] = $"{muterole.Id}";
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                var em = new DiscordEmbedBuilder
                {
                    Title = $"`@{muterole.Name}` has been set as the mute role",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }
        }
        [SlashCommandGroup("welcomersettings", "A group with all the settings")]
        public class WelcomersettingsGroup : ApplicationCommandModule
        {
            [SlashCommand("on", "Enable the welcomer")]
            [SlashRequireUserPermissions(Permissions.Administrator)]
            public async Task WelcomerOnCommand(InteractionContext ctx)
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                new JProperty("loggingchannelid", null),
                                new JProperty("muterole", null),
                                new JProperty("welcomer", null),
                                new JProperty("welcomerchannel", null),
                                new JProperty("welcomercustommessage", null)
                                    }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }

                string json = File.ReadAllText($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                jsonData["config"]["welcomer"] = "on";
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Enabled the welcomer",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }

            [SlashCommand("off", "Disable the welcomer")]
            [SlashRequireUserPermissions(Permissions.Administrator)]
            public async Task WelcomerOffCommand(InteractionContext ctx)
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                new JProperty("loggingchannelid", null),
                                new JProperty("muterole", null),
                                new JProperty("welcomer", null),
                                new JProperty("welcomerchannel", null),
                                new JProperty("welcomercustommessage", null)
                                    }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }

                string json = File.ReadAllText($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                jsonData["config"]["welcomer"] = "off";
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Disabled the welcomer",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }

            [SlashCommand("channel", "Set the channel where the welcome message should be sent")]
            [SlashRequireUserPermissions(Permissions.Administrator)]
            public async Task WelcomerChannelCommand(InteractionContext ctx, [Option("channel", "The channel where the welcome message should be sent")] DiscordChannel channel)
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                new JProperty("loggingchannelid", null),
                                new JProperty("muterole", null),
                                new JProperty("welcomer", null),
                                new JProperty("welcomerchannel", null),
                                new JProperty("welcomercustommessage", null)
                                    }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }

                string json = File.ReadAllText($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                jsonData["config"]["welcomerchannel"] = $"{channel.Id}";
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                var em = new DiscordEmbedBuilder
                {
                    Title = $"{channel.Name} is now used to send welcome messages",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }

            [SlashCommand("custommessage", "Set your own custom welcome message")]
            [SlashRequireUserPermissions(Permissions.Administrator)]
            public async Task WelcomerCustommessageCommand(InteractionContext ctx, [Option("custommessage", "Your own custom welcome message. Placeholders: {user}, {usermention}, {servername}")] string custommessage)
            {
                string file = $"Settings/guilds/{ctx.Guild.Id}.json";
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (File.Exists(file))
                {
                    string json = File.ReadAllText($"Settings/guilds/{ctx.Guild.Id}.json");
                    dynamic jsonData = JsonConvert.DeserializeObject(json);
                    jsonData["config"]["welcomercustommessage"] = $"{custommessage}";
                    string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                    File.WriteAllText($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"The welcome message is now:",
                        Description = $"`{custommessage}`",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                }
            }
        }
    }
}