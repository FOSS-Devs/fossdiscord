// Copyright (c) 2021 Fossium-Team
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DSharpPlus.Interactivity.Extensions;
using System.Text.RegularExpressions;

namespace FossiumBot.Commands
{
    public class Moderation : ApplicationCommandModule
    {
        [SlashCommand("kick", "Kick a member")]
        [SlashRequirePermissions(Permissions.KickMembers)]
        public async Task KickCommand(InteractionContext ctx, [Option("member", "mention or an id of a member")] DiscordUser user, [Option("reason", "Reason of kicking")] string reason = "no reason given")
        {
            DiscordMember member = (DiscordMember)user;
            if (member.Id == ctx.Member.Id)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "You cannot kick yourself",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Your role is too low in the role hierarchy to do that",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (ctx.Guild.Id != member.Guild.Id)
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That user does not exist in this server",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                return;
            }
            else if (reason.Length > 350)
            {
                var lengthError = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "Please shorten your reason to within 350 characters",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lengthError));
                return;
            }
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Kicked {member.Username}#{member.Discriminator}",
                Color = new DiscordColor(0xFFA500)
            };
            embed.AddField("Reason", reason);
            await member.RemoveAsync(reason);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/guilds");
            string file = $"Settings/guilds/{ctx.Guild.Id}.json";
            JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
            if ((string)jsonData["config"]["loggingchannelid"] == "null")
            {
                return;
            }
            else
            {
                var loggingembed = new DiscordEmbedBuilder
                {
                    Title = $"{member.Username}#{member.Discriminator} has been kicked",
                    Color = new DiscordColor(0xFFA500),
                    Timestamp = DateTime.Now
                };
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
                DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                await loggingchannel.SendMessageAsync(loggingembed);
                return;
            }
        }

        [SlashCommand("ban", "Ban a member")]
        [SlashRequirePermissions(Permissions.BanMembers)]
        public async Task BanCommand(InteractionContext ctx, [Option("member", "mention or an id of a member")] DiscordUser user, [Option("deletemessagedays", "How many days to delete the messages from")] long deletemessagedays = 5, [Option("reason", "Reason of banning")] string reason = "no reason given")
        {
            DiscordMember member = (DiscordMember)user;
            if (member.Id == ctx.Member.Id)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "You cannot ban yourself",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Your role is too low in the role hierarchy to do that",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (reason.Length > 350)
            {
                var lengthError = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "Please shorten your reason to within 350 characters",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lengthError));
                return;
            }
            var banlist = ctx.Guild.GetBansAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (banlist.Any(x => x.User.Id == member.Id))
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = $"{member.Username}#{member.Discriminator} is already banned",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Banned {member.Username}#{member.Discriminator}",
                    Color = new DiscordColor(0xFF0000)
                };
                embed.AddField("Reason", reason);
                await member.BanAsync((int)deletemessagedays, reason);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }

            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/guilds");
            string file = $"Settings/guilds/{ctx.Guild.Id}.json";
            JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
            if ((string)jsonData["config"]["loggingchannelid"] == "null")
            {
                return;
            }
            else
            {
                var loggingembed = new DiscordEmbedBuilder
                {
                    Title = $"{member.Username}#{member.Discriminator} has been banned",
                    Color = new DiscordColor(0xFF0000),
                    Timestamp = DateTime.Now
                };
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
                DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                await loggingchannel.SendMessageAsync(loggingembed);
                return;
            }
        }

        [SlashCommand("softban", "Ban and unban a user to delete all their messages")]
        [SlashRequirePermissions(Permissions.BanMembers)]
        public async Task SoftbanCommand(InteractionContext ctx, [Option("member", "mention or an id of a member")] DiscordUser user, [Option("deletemessagedays", "How many days to delete the messages from")] long deletemessagedays = 5, [Option("reason", "Reason of banning")] string reason = "no reason given")
        {
            DiscordMember member = (DiscordMember)user;
            if (member.Id == ctx.Member.Id)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "You cannot softban yourself",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Your role is too low in the role hierarchy to do that",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (ctx.Guild.Id != member.Guild.Id)
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That user does not exist in this server",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                return;
            }
            else if (reason.Length > 350)
            {
                var lengthError = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "Please shorten your reason to within 350 characters",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lengthError));
                return;
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Sofbanned {member.Username}#{member.Discriminator}",
                    Color = new DiscordColor(0xFFA500)
                };
                embed.AddField("Reason", reason);
                await member.BanAsync((int)deletemessagedays, reason);
                await ctx.Guild.UnbanMemberAsync(member.Id);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }

            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/guilds");
            string file = $"Settings/guilds/{ctx.Guild.Id}.json";
            JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
            if ((string)jsonData["config"]["loggingchannelid"] == "null")
            {
                return;
            }
            else
            {
                var loggingembed = new DiscordEmbedBuilder
                {
                    Title = $"{member.Username}#{member.Discriminator} has been softbanned",
                    Color = new DiscordColor(0xFFA500),
                    Timestamp = DateTime.Now
                };
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
                DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                await loggingchannel.SendMessageAsync(loggingembed);
                return;
            }
        }

        [SlashCommand("unban", "Unban a user")]
        [SlashRequirePermissions(Permissions.BanMembers)]
        public async Task UnbanCommand(InteractionContext ctx, [Option("user", "The member you want to unban")] DiscordUser user)
        {
            var banlist = ctx.Guild.GetBansAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (banlist.Any(x => x.User.Id == user.Id))
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Unbanned {user.Username}#{user.Discriminator}",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.Guild.UnbanMemberAsync(user);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = $"{user.Username}#{user.Discriminator} is not banned",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
            }

            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/guilds");
            string file = $"Settings/guilds/{ctx.Guild.Id}.json";
            JObject jsonData = JObject.Parse(await File.ReadAllTextAsync(file));
            if ((string)jsonData["config"]["loggingchannelid"] == "null")
            {
                return;
            }
            else
            {
                var loggingembed = new DiscordEmbedBuilder
                {
                    Title = $"{user.Username}#{user.Discriminator} has been unbanned",
                    Color = new DiscordColor(0x2ECC70),
                    Timestamp = DateTime.Now
                };
                ulong loggingchannelid = (ulong)jsonData["config"]["loggingchannelid"];
                DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                await loggingchannel.SendMessageAsync(loggingembed);
                return;
            }
        }

        [SlashCommand("purge", "Purge a certain amount of messages")]
        [SlashRequirePermissions(Permissions.ManageMessages)]
        public async Task PurgeCommands(InteractionContext ctx, [Option("amount", "Amount of messages to delete")] long amount = 10)
        {
            if (amount > 50)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Cannot purge more than 50 messages at one time",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            var messages = await ctx.Channel.GetMessagesAsync((int)amount + 1);
            await ctx.Channel.DeleteMessagesAsync(messages);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Purged {amount} messages",
                Color = new DiscordColor(0x2ECC70)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            await Task.Delay(5000);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("autodelete", "Automatically delete messages in a channel")]
        [SlashRequirePermissions(Permissions.ManageMessages)]
        public async Task AutoDeleteCommand(InteractionContext ctx, [Option("channel", "The channel to automatically delete messages for")] DiscordChannel channel, [Option("time", "The amount of time between deletion of messsages in the channel in hours")] string time)
        {
            if (ctx.Guild.Id != channel.GuildId)
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel does not exist in this server",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                return;
            }
            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/lck/");
            if (time == "off" && !File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel is not configured to auto delete messages",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                return;
            }
            else if (time == "off")
            {
                File.Delete(@$"Settings/lck/{channel.Id}.lck");
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Successfully stopped autodelete for `#{channel.Name}`",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                return;
            }
            if (time != "off" && short.Parse(time) >= 1)
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Autodelete has started...",
                    Description = $"`{channel.Name}` is now configured to auto delete messages every {time} hour(s)",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                await File.Create(@$"Settings/lck/{channel.Id}.lck").DisposeAsync();
                while (File.Exists($"Settings/lck/{channel.Id}.lck"))
                {
                    var messages = await channel.GetMessagesAsync();
                    foreach (var message in messages)
                    {
                        var msgTime = message.Timestamp.UtcDateTime;
                        var sysTime = DateTime.UtcNow;
                        if (sysTime.Subtract(msgTime).TotalHours > short.Parse(time) && sysTime.Subtract(msgTime).TotalHours < 336)
                        {
                        await channel.DeleteMessageAsync(message);
                        await Task.Delay(10000);
                        }
                    }
                }
            }
            else
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "You command syntax is not right",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
            }
            return;
        }

        [SlashCommand("warn", "Warn a user")]
        [SlashRequirePermissions(Permissions.ManageMessages)]
        public async Task WarnCommand(InteractionContext ctx, [Option("user", "The user to warn")] DiscordUser user, [Option("reason", "The reason of the warn")] string reason = "none")
        {
            DiscordMember member = (DiscordMember)user;
            if (ctx.Guild.Id != member.Guild.Id)
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That user is not in this guild",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                return;
            }
            else if (ctx.Member.Id == member.Id)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "You cannot warn yourself",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (reason.Length > 350)
            {
                var lengthError = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "Please make your reason max 350 characters",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lengthError));
                return;
            }
            string file = $"Data/Warnings/{ctx.Guild.Id}.json";
            Directory.CreateDirectory(@"Data/");
            Directory.CreateDirectory(@"Data/Warnings/");
            try
            {
                if (File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = await readData.ReadToEndAsync();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    if (jsonData.GetValue($"{member.Id}") != null)
                    {
                        if (jsonData[$"{member.Id}"].Any())
                        {
                            string lastItem = ((JProperty)jsonData[$"{member.Id}"].Last()).Name;
                            int caseIncrement = int.Parse(lastItem) + 1;
                            jsonData[$"{member.Id}"][$"{caseIncrement}"] = reason;
                        }
                        else
                        {
                            jsonData[$"{member.Id}"]["1"] = reason;
                        }
                        
                    }
                    else 
                    {
                        jsonData.Add(new JProperty($"{member.Id}", new JObject(new JProperty("1", reason))));
                    }
                    string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                    await File.WriteAllTextAsync(file, dataWrite);
                    var firstEM = new DiscordEmbedBuilder
                    {
                        Title = $"`{member.DisplayName}` has been warned",
                        Color = new DiscordColor(0xFFA500)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(firstEM));
                }
                else
                {
                    JObject overwrite =
                    new JObject(
                        new JProperty($"{member.Id}",
                            new JObject(
                                new JProperty("1", reason)
                            )
                        )
                    );
                    string dataWrite = JsonConvert.SerializeObject(overwrite, Formatting.Indented);
                    await File.WriteAllTextAsync(file, dataWrite);
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"`{member.DisplayName}` has been warned",
                        Color = new DiscordColor(0xFFA500)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                }
            }
            catch (Exception)
            {
                JObject overwrite =
                    new JObject(
                        new JProperty($"{member.Id}",
                            new JObject(
                                new JProperty("1", reason)
                            )
                        )
                    );
                string dataWrite = JsonConvert.SerializeObject(overwrite, Formatting.Indented);
                await File.WriteAllTextAsync(file, dataWrite);
                var emNEW = new DiscordEmbedBuilder
                {
                    Title = $"`{member.DisplayName}` has been warned",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(emNEW));
            };

            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/guilds");
            string loggingfile = $"Settings/guilds/{ctx.Guild.Id}.json";
            JObject loggingjsonData = JObject.Parse(await File.ReadAllTextAsync(loggingfile));
            if ((string)loggingjsonData["config"]["loggingchannelid"] == "null")
            {
                return;
            }
            else
            {
                var loggingembed = new DiscordEmbedBuilder
                {
                    Title = $"{member.Username}#{member.Discriminator} has been warned",
                    Color = new DiscordColor(0xFFA500),
                    Timestamp = DateTime.Now
                };
                ulong loggingchannelid = (ulong)loggingjsonData["config"]["loggingchannelid"];
                DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                loggingembed.AddField("Reason", reason);
                loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                await loggingchannel.SendMessageAsync(loggingembed);
                return;
            }
        }

        [SlashCommand("warns", "See all the warnings of a user")]
        [SlashRequirePermissions(Permissions.ManageMessages)]
        public async Task Warnings(InteractionContext ctx, [Option("user", "The user to show the warnings of")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            try
            {
                string file = $"Data/Warnings/{ctx.Guild.Id}.json";
                if (File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = await readData.ReadToEndAsync();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    if (jsonData.GetValue($"{member.Id}") != null && jsonData[$"{member.Id}"].Any())
                    {
                        dynamic iterData = jsonData[$"{member.Id}"];
                        var firstEM = new DiscordEmbedBuilder
                        {
                            Title = $"`{member.DisplayName}`'s warnings:",
                            Color = new DiscordColor(0xFFA500)
                        };
                        int count = 0;
                        foreach (KeyValuePair<string, JToken> items in (JObject)jsonData[$"{member.Id}"])
                        {
                            string caseID = items.Key;
                            var r = items.Value;
                            string brief = (string)r;
                            if (brief.Length > 360)
                            {
                                string reason = brief.Substring(0, 360);
                                firstEM.AddField($"Case {caseID}", $"{reason}...");
                            }
                            else
                            {
                                firstEM.AddField($"Case {caseID}", $"{brief}");
                            }
                            count += 1;
                            if(count == 5)
                            {
                                break;
                            }
                        }
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(firstEM));
                    }
                    else
                    {
                        var nonexistEM = new DiscordEmbedBuilder
                        {
                            Title = "Nice!",
                            Description = $"`{member.DisplayName}` doesn't have any warnings",
                            Color = new DiscordColor(0x2ECC70)
                        };
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nonexistEM));
                    }
                }
                if (!File.Exists(file)) 
                {
                    var nonexistEM = new DiscordEmbedBuilder
                    {
                        Title = "Nice!",
                        Description = "Nobody in this guild has been warned yet",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nonexistEM));
                }
            }
            catch (Exception ex)
            {
                var errorEM = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = $"{ex}",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEM));
                return;
            };
        }

        [SlashCommand("delwarn", "Remove warnings from a user")]
        [SlashRequirePermissions(Permissions.ManageMessages)]
        public async Task Delwarn(InteractionContext ctx, [Option("user", "The user to delete the warning from")] DiscordUser user, [Option("caseID", "The ID of the case you want to delete")] string caseID)
        {
            DiscordMember member = (DiscordMember)user;
            string file = $"Data/Warnings/{ctx.Guild.Id}.json";
            if(!File.Exists(file))
            {
                var nonexistEM = new DiscordEmbedBuilder
                {
                    Title = "Nice!",
                    Description = $"Nobody in this guild has been warned yet",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nonexistEM));
                return;
            }
            else {
                try
                {
                    StreamReader readData = new StreamReader(file);
                    string data = await readData.ReadToEndAsync();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    if(jsonData.GetValue($"{member.Id}") != null && jsonData[$"{member.Id}"].Any())
                    {
                        if (caseID == "all" | caseID == "All")
                        {
                            List<string> temp = new List<string>();
                            foreach (KeyValuePair<string, JToken> items in (JObject)jsonData[$"{member.Id}"])
                            {
                                temp.Add(items.Key);
                            };
                            foreach(string i in temp)
                            {
                                jsonData[$"{member.Id}"][i].Parent.Remove();
                            }
                            string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                            await File.WriteAllTextAsync(file, dataWrite);
                            var EM = new DiscordEmbedBuilder
                            {
                                Title = $"Successfully removed all warnings for `{member.DisplayName}`",
                                Color = new DiscordColor(0x0080FF)
                            };
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(EM));

                            Directory.CreateDirectory(@"Settings/");
                            Directory.CreateDirectory(@"Settings/guilds");
                            string loggingfile = $"Settings/guilds/{ctx.Guild.Id}.json";
                            JObject loggingjsonData = JObject.Parse(await File.ReadAllTextAsync(loggingfile));
                            if ((string)loggingjsonData["config"]["loggingchannelid"] == "null")
                            {
                                return;
                            }
                            else
                            {
                                var loggingembed = new DiscordEmbedBuilder
                                {
                                    Title = $"All warnings from {member.Username}#{member.Discriminator} have been removed",
                                    Color = new DiscordColor(0x2ECC70),
                                    Timestamp = DateTime.Now
                                };
                                ulong loggingchannelid = (ulong)loggingjsonData["config"]["loggingchannelid"];
                                DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                                loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                                loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                                await loggingchannel.SendMessageAsync(loggingembed);
                                return;
                            }
                        }
                        else if (jsonData[$"{member.Id}"][$"{caseID}"] == null)
                        {
                            var nonexistEM = new DiscordEmbedBuilder
                            {
                                Title = "Oops...",
                                Description = "This warning doesn't exist",
                                Color = new DiscordColor(0xFF0000)
                            };
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nonexistEM));
                            return;
                        }
                        else
                        {
                            jsonData[$"{member.Id}"][$"{caseID}"].Parent.Remove();
                            string dataWrite = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                            await File.WriteAllTextAsync(file, dataWrite);
                            var EM = new DiscordEmbedBuilder
                            {
                                Title = $"Successfully removed `Case {caseID}` for `{member.DisplayName}`",
                                Color = new DiscordColor(0x0080FF)
                            };
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(EM));

                            Directory.CreateDirectory(@"Settings/");
                            Directory.CreateDirectory(@"Settings/guilds");
                            string loggingfile = $"Settings/guilds/{ctx.Guild.Id}.json";
                            JObject loggingjsonData = JObject.Parse(await File.ReadAllTextAsync(loggingfile));
                            if ((string)loggingjsonData["config"]["loggingchannelid"] == "null")
                            {
                                return;
                            }
                            else
                            {
                                var loggingembed = new DiscordEmbedBuilder
                                {
                                    Title = $"Case {caseID} has been removed from {member.Username}#{member.Discriminator}",
                                    Color = new DiscordColor(0x2ECC70),
                                    Timestamp = DateTime.Now
                                };
                                ulong loggingchannelid = (ulong)loggingjsonData["config"]["loggingchannelid"];
                                DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                                loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                                loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                                await loggingchannel.SendMessageAsync(loggingembed);
                                return;
                            }
                        }
                    }
                    else
                    {
                        var nonexistEM = new DiscordEmbedBuilder
                        {
                            Title = "Nice!",
                            Description = $"`{member.DisplayName}` doesn't have any warnings",
                            Color = new DiscordColor(0x0080FF)
                        };
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nonexistEM));
                        return;
                    }
                        
                }
                catch (Exception ex)
                {
                    var errorEM = new DiscordEmbedBuilder
                    {
                        Title = "Oops...",
                        Description = $"{ex}",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEM));
                    return;
                };
            }
        }

        [SlashCommand("mute", "Mute a user")]
        [SlashRequirePermissions(Permissions.ManageRoles)]
        public async Task MuteCommand(InteractionContext ctx, [Option("user", "The user to mute")] DiscordUser user, [Option("mutetime", "The amount of time to mute the user for in minutes, default value is 15")] long mutetime = 15)
        {
            DiscordMember member = (DiscordMember)user;
            if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Your role is too low in the role hierarchy to do that",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            else if (member.Id == ctx.Member.Id)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "You cannot mute yourself",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errembed));
                return;
            }
            string file = $"Settings/guilds/{ctx.Guild.Id}.json";
            if (File.Exists(file))
            {
                StreamReader readData = new StreamReader(file);
                string data = await readData.ReadToEndAsync();
                readData.Close();
                JObject jsonData = JObject.Parse(data);
                if(jsonData["config"]["muterole"] != null)
                {
                    ulong roleID = (ulong)jsonData["config"]["muterole"];
                    DiscordRole muteRole = ctx.Guild.GetRole(roleID);
                    await member.GrantRoleAsync(muteRole);
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"`{member.DisplayName}` has been muted for {mutetime} minute(s)",
                        Color = new DiscordColor(0xFFA500)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(mutetime));
                    //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(mutetime));
                    await member.RevokeRoleAsync(muteRole);

                    Directory.CreateDirectory(@"Settings/");
                    Directory.CreateDirectory(@"Settings/guilds");
                    string loggingfile = $"Settings/guilds/{ctx.Guild.Id}.json";
                    JObject loggingjsonData = JObject.Parse(await File.ReadAllTextAsync(loggingfile));
                    if ((string)loggingjsonData["config"]["loggingchannelid"] == "null")
                    {
                        return;
                    }
                    else
                    {
                        var loggingembed = new DiscordEmbedBuilder
                        {
                            Title = $"{member.Username}#{member.Discriminator} has been muted",
                            Color = new DiscordColor(0xFFA500),
                            Timestamp = DateTime.Now
                        };
                        ulong loggingchannelid = (ulong)loggingjsonData["config"]["loggingchannelid"];
                        DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                        loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                        loggingembed.AddField("Time in minutes", mutetime.ToString());
                        loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                        await loggingchannel.SendMessageAsync(loggingembed);
                        return;
                    }
                }
            }
            else
            {
                var roleNotExist = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = $"You haven't set the role for muted members yet\nSet the muted role with `/settings muterole <mention role>`",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(roleNotExist));
                return;
            }
        }

        [SlashCommand("unmute", "Unmute a user")]
        [SlashRequirePermissions(Permissions.ManageRoles)]
        public async Task UnmuteCommand(InteractionContext ctx, [Option("user", "The user to unmute")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            string file = $"Settings/guilds/{ctx.Guild.Id}.json";
            if (File.Exists(file))
            {
                StreamReader readData = new StreamReader(file);
                string data = await readData.ReadToEndAsync();
                readData.Close();
                JObject jsonData = JObject.Parse(data);
                ulong roleID = (ulong)jsonData["config"]["muterole"];
                DiscordRole muteRole = ctx.Guild.GetRole(roleID);
                try
                {
                    await member.RevokeRoleAsync(muteRole);
                }
                catch (Exception ex)
                {
                    var errEM = new DiscordEmbedBuilder
                    {
                        Title = "Oops...",
                        Description = $"{ex}",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errEM));
                    return;
                }
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Unmuted `{member.DisplayName}`",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));

                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/guilds");
                string loggingfile = $"Settings/guilds/{ctx.Guild.Id}.json";
                JObject loggingjsonData = JObject.Parse(await File.ReadAllTextAsync(loggingfile));
                if ((string)loggingjsonData["config"]["loggingchannelid"] == "null")
                {
                    return;
                }
                else
                {
                    var loggingembed = new DiscordEmbedBuilder
                    {
                        Title = $"{member.Username}#{member.Discriminator} has been unmuted",
                        Color = new DiscordColor(0x2ECC70),
                        Timestamp = DateTime.Now
                    };
                    ulong loggingchannelid = (ulong)loggingjsonData["config"]["loggingchannelid"];
                    DiscordChannel loggingchannel = ctx.Guild.GetChannel(loggingchannelid);
                    loggingembed.WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl);
                    loggingembed.AddField("Moderator", $"{ctx.Member.Username}#{ctx.Member.Discriminator}");
                    await loggingchannel.SendMessageAsync(loggingembed);
                    return;
                }
            }
            else
            {
                var roleNotExist = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = $"You haven't set the role for muted members yet\nSet the muted role with `/settings muterole <mention role>`",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(roleNotExist));
                return;
            }
        }

        [SlashCommand("reactionroles", "Reaction roles")]
        public async Task ReactionrolesCommand(InteractionContext ctx, [Option("text", "The text in the message, new line with \"\\n\"")]string text, [Option("channel", "The channel to make the reaction roles message")] DiscordChannel channel)
        {
            DiscordEmoji discordemoji = null;
            DiscordRole discordrole = null;
            List<string> discordemojis = new List<string>();
            List<ulong> discordroles = new List<ulong>();
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                var addrole = new DiscordEmbedBuilder
                {
                    Title = "Reaction roles",
                    Description = $"To add a role: `<role id> | <emoji name>`, `done`, `list` or `cancel`\nTimeout in 30 seconds",
                    Color = new DiscordColor(0x0080FF)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(addrole));
            while (true)
            {
                var response = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User, TimeSpan.FromSeconds(30));
                if (response.TimedOut)
                {
                    var errorembed = new DiscordEmbedBuilder
                    {
                        Title = "Oops...",
                        Description = $"Timed out",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.Channel.SendMessageAsync(errorembed);
                    return;
                }

                if (response.Result.Content.ToLower() == "cancel")
                {
                    var errorembed = new DiscordEmbedBuilder
                    {
                        Title = "Oops...",
                        Description = $"Cancelled",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.Channel.SendMessageAsync(errorembed);
                    return;
                }
                if (response.Result.Content.ToLower() == "done")
                {
                    if (!discordroles.Any()) 
                    {
                        var emptyembed = new DiscordEmbedBuilder
                        {
                            Title = $"Oops...",
                            Description = $"Please add at least one role",
                            Color = new DiscordColor(0xFF0000)
                        };
                        await ctx.Channel.SendMessageAsync(emptyembed);
                        continue;
                    }
                    break;
                }
                if (response.Result.Content.ToLower() == "list")
                {
                    if (!discordroles.Any())
                    {
                        var emptyembed = new DiscordEmbedBuilder
                        {
                            Title = $"Oops...",
                            Description = $"Please add at least one role",
                            Color = new DiscordColor(0xFF0000)
                        };
                        await ctx.Channel.SendMessageAsync(emptyembed);
                        continue;
                    }

                    string listdiscordemojis = string.Empty;
                    string[] listdiscordemojisarray = discordemojis.ToArray();
                    ulong[] listdiscordrolesarray = discordroles.ToArray();
                    string listreactionroles = String.Empty;;
                    long repeatlistforeach = 0;
                    foreach (string emoji in discordemojis)
                    {
                        listreactionroles = $"{listreactionroles}\nRole: {ctx.Guild.GetRole(listdiscordrolesarray[repeatlistforeach]).Mention} Emoji: {emoji}";
                        repeatlistforeach += 1;
                    }

                    var listembed = new DiscordEmbedBuilder
                    {
                        Title = $"Added roles",
                        Description = listreactionroles,
                        Color = new DiscordColor(0x0080FF)
                    }; 
                    await ctx.Channel.SendMessageAsync(listembed);
                    continue;
                }

                if (!Regex.IsMatch(response.Result.Content, @"\d* \| [a-z_\-A-Z]*"))
                {
                    continue;
                }

                Directory.CreateDirectory(@"Settings/");
                Directory.CreateDirectory(@"Settings/reactionroles/");

                string[] responsesplit = response.Result.Content.Split(" | ");
                try
                {
                    discordemoji = DiscordEmoji.FromName(ctx.Client, $":{responsesplit[1]}:");
                    discordrole = ctx.Guild.GetRole(ulong.Parse(responsesplit[0]));
                }
                catch
                {
                    var errorembed = new DiscordEmbedBuilder
                    {
                        Title = $"Oops...",
                        Description = $"That is not a valid role and/or emoji",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.Channel.SendMessageAsync(errorembed);
                    continue;
                }
                discordemojis.Add(DiscordEmoji.FromName(ctx.Client, $":{responsesplit[1]}:").GetDiscordName());
                discordroles.Add(ctx.Guild.GetRole(ulong.Parse(responsesplit[0])).Id);

                var addedembed = new DiscordEmbedBuilder
                {
                    Title = $"Reaction roles",
                    Description = $"Added role {discordrole.Mention} with emoij {discordemoji.Name}",
                    Color = new DiscordColor(0x0080FF)
                };
                await ctx.Channel.SendMessageAsync(addedembed);
            }
            text = text.Replace("\\n", "\r\n");
            var msg = await channel.SendMessageAsync(text);
            string[] discordemojisarray = discordemojis.ToArray();

            ulong repeatforeach = 0;
            foreach (string emoji in discordemojis)
            {
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, $"{discordemojisarray[repeatforeach]}"));
                repeatforeach += 1;
            }

            JObject reactionrolesConfig =
                    new JObject(
                        new JProperty("reactionroles",
                        new JObject {
                                    new JProperty("emojis", $"{string.Join(", ", discordemojis)}"),
                                    new JProperty("roles", $"{string.Join(", ", discordroles)}")
                                }
                        )
                    );
            string reactionrolesWrite = JsonConvert.SerializeObject(reactionrolesConfig, Formatting.Indented);
            await File.WriteAllTextAsync($"Settings/reactionroles/{ctx.Guild.Id}-{channel.Id}-{msg.Id}.json", reactionrolesWrite);
        }
    }
}
