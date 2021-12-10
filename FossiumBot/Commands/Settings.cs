// Copyright (c) 2021 Fossium-Team
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                                    new JProperty("loggingchannelid", "null"),
                                    new JProperty("muterole", "null"),
                                    new JProperty("welcomer", "off"),
                                    new JProperty("welcomerchannel", "null"),
                                    new JProperty("welcomercustommessage", "null")
                                 }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
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
                    string json = await File.ReadAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json");
                    dynamic jsonData = JsonConvert.DeserializeObject(json);
                    jsonData["config"]["loggingchannelid"] = "null";
                    string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                    await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
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
                    string json = await File.ReadAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json");
                    dynamic jsonData = JsonConvert.DeserializeObject(json);
                    jsonData["config"]["loggingchannelid"] = $"{loggingchannel.Id}";
                    string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                    await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
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
                                    new JProperty("loggingchannelid", "null"),
                                    new JProperty("muterole", "null"),
                                    new JProperty("welcomer", "off"),
                                    new JProperty("welcomerchannel", "null"),
                                    new JProperty("welcomercustommessage", "null")
                                 }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }
                string json = await File.ReadAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                jsonData["config"]["muterole"] = $"{muterole.Id}";
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                var em = new DiscordEmbedBuilder
                {
                    Title = $"`@{muterole.Name}` has been set as the mute role",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }

            [SlashCommand("welcomertoggle", "Toggle the welcomer")]
            public async Task WelcomertoggleCommand(InteractionContext ctx, [Option("option", "on, off, channel or custommessage")] string option)
            {
                if (option.ToLower() != "on" && option.ToLower() != "off")
                {
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"Invalid option",
                        Description = "Valid options: on or off",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                }
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                new JProperty("loggingchannelid", "null"),
                                new JProperty("muterole", "null"),
                                new JProperty("welcomer", "off"),
                                new JProperty("welcomerchannel", "null"),
                                new JProperty("welcomercustommessage", "null")
                                    }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }

                string json = await File.ReadAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                if (option.ToLower() == "on")
                {
                    jsonData["config"]["welcomer"] = "on";
                }
                else
                {
                    jsonData["config"]["welcomer"] = "off";
                }
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                if (option.ToLower() == "on")
                {
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"Enabled the welcomer",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                }
                else
                {
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"Disabled the welcomer",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                }
            }

            [SlashCommand("welcomerchannel", "Set the channel the welcomer sends messages to")]
            public async Task WelcomerchannelCommand(InteractionContext ctx, [Option("channel", "The channel to send the messages to")] DiscordChannel channel)
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                new JProperty("loggingchannelid", "null"),
                                new JProperty("muterole", "null"),
                                new JProperty("welcomer", "off"),
                                new JProperty("welcomerchannel", "null"),
                                new JProperty("welcomercustommessage", "null")
                                    }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }

                string json = await File.ReadAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                jsonData["config"]["welcomerchannel"] = $"{channel.Id}";
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
                var em = new DiscordEmbedBuilder
                {
                    Title = $"`{channel.Name}` is now used to send welcome messages",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }

            [SlashCommand("welcomercustommessage", "Set your own custom welcome message")]
            public async Task WelcomercustommessageCommand(InteractionContext ctx, [Option("custommessage", "Your own custom welcome message. Placeholders: {user}, {usermention}, {servername}")] string custommessage)
            {
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                if (!File.Exists($"Settings/guilds/{ctx.Guild.Id}.json"))
                {
                    JObject newConfig =
                        new JObject(
                            new JProperty("config",
                            new JObject {
                                new JProperty("loggingchannelid", "null"),
                                new JProperty("muterole", "null"),
                                new JProperty("welcomer", "off"),
                                new JProperty("welcomerchannel", "null"),
                                new JProperty("welcomercustommessage", "null")
                                    }
                            )
                        );
                    string nonexistdataWrite = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", nonexistdataWrite);
                }

                string json = await File.ReadAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json");
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                jsonData["config"]["welcomercustommessage"] = $"{custommessage}";
                string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                await File.WriteAllTextAsync($"Settings/guilds/{ctx.Guild.Id}.json", dataWrite);
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
