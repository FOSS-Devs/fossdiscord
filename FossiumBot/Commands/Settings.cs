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
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
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
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
                }
                /*else
                {
                    JObject overwrite =
                        new JObject(
                            new JProperty("config",
                                new JObject(
                                    new JProperty("loggingchannelid", loggingchannel.Id)
                                )
                            )
                        );
                    string overwriteData = Newtonsoft.Json.JsonConvert.SerializeObject(overwrite, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, overwriteData);
                }*/
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Set `#{loggingchannel.Name}` as the logging channel",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            /*JObject disabledata = new JObject(
                new JProperty($"Loggingchannelid", "0")
                );
            string disablejson = JsonConvert.SerializeObject(disabledata);
            Directory.CreateDirectory(@"Settings/");
            string disablepath = $"Settings/Loggingsettings-{ctx.Guild.Id}.json";
            using (TextWriter tw = new StreamWriter(disablepath))
            {
                tw.WriteLine(disablejson);
            };

            var disableembed = new DiscordEmbedBuilder
            {
                Title = $"Disabled logging",
                Color = new DiscordColor(0x2ECC70)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(disableembed));
            return;*/
            /*JObject data = new JObject(
                new JProperty($"Loggingchannelid", $"{loggingchannel.Id}")
                );

            string json = JsonConvert.SerializeObject(data);
            Directory.CreateDirectory(@"Settings/");
            string path = $"Settings/Loggingsettings-{ctx.Guild.Id}.json";
            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine(json);
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Set `#{loggingchannel.Name}` as the logging channel",
                Color = new DiscordColor(0x2ECC70)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));*/

            [SlashCommand("muterole", "Set the role used by the mute commands")]
            [SlashRequireUserPermissions(Permissions.Administrator)]
            public async Task MuteRoleCommand(InteractionContext ctx, [Option("muterole", "Mention the role used by the mute commands")] DiscordRole muterole)
            {
                string file = $"Settings/guilds/{ctx.Guild.Id}.json";
                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds/");
                if (File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = readData.ReadToEnd();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    jsonData["config"]["muterole"] = muterole.Id;
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
                }
                else
                {
                    JObject overwrite =
                        new JObject(
                            new JProperty("config",
                            new JObject(
                                    new JProperty("muterole", muterole.Id)
                                )
                            )
                        );
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(overwrite, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
                }
                var em = new DiscordEmbedBuilder
                {
                    Title = $"`@{muterole.Name}` has been set as the mute role",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }
        }
    }
}