using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            if (time == "off" && File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                File.Delete($"Settings/lck/{channel.Id}.lck");
                return;
            }
            else if (Int16.Parse(time) >= 1 && File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel already configured to auto delete messages",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                return;
            }
            else if (time == "off" && !File.Exists($"Settings/lck/{channel.Id}.lck"))
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
            if (!File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                if (time != "off" && Int16.Parse(time) >= 1)
                {
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"Autodelete has started...",
                        Description = $"`{channel.Name}` is now configured to auto delete messages every {time} hour(s)",
                        Color = new DiscordColor(0xFFA500)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
                    File.Create($"Settings/lck/{channel.Id}.lck").Dispose();
                    while (File.Exists($"Settings/lck/{channel.Id}.lck"))
                    {
                        var messages = await channel.GetMessagesAsync();
                        foreach (var message in messages)
                        {
                            var msgTime = message.Timestamp.UtcDateTime;
                            var sysTime = System.DateTime.UtcNow;
                            if (sysTime.Subtract(msgTime).TotalHours > Int16.Parse(time) && sysTime.Subtract(msgTime).TotalHours < 336)
                            {
                                await channel.DeleteMessageAsync(message);
                                await Task.Delay(3000);
                            }
                        }
                        await Task.Delay(1000);
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
            string file = $"Data/blacklist/{ctx.Guild.Id}.lst";
            Directory.CreateDirectory(@"Data/");
            Directory.CreateDirectory(@"Data/blacklist/");
            try
            {
                if (File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = readData.ReadToEnd();
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
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
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
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(overwrite, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
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
                string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(overwrite, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(file, dataWrite);
                var emNEW = new DiscordEmbedBuilder
                {
                    Title = $"`{member.DisplayName}` has been warned",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(emNEW));
                return;
            };
        }

        [SlashCommand("warns", "See all the warnings of a user")]
        [SlashRequirePermissions(Permissions.ManageMessages)]
        public async Task Warnings(InteractionContext ctx, [Option("user", "The user to show the warnings of")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            try
            {
                string file = $"Data/blacklist/{ctx.Guild.Id}.lst";
                if (File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = readData.ReadToEnd();
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
            string file = $"Data/blacklist/{ctx.Guild.Id}.lst";
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
                    string data = readData.ReadToEnd();
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
                            string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
                            System.IO.File.WriteAllText(file, dataWrite);
                            var EM = new DiscordEmbedBuilder
                            {
                                Title = $"Successfully removed all warnings for `{member.DisplayName}`",
                                Color = new DiscordColor(0x0080FF)
                            };
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(EM));
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
                            string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
                            System.IO.File.WriteAllText(file, dataWrite);
                            var EM = new DiscordEmbedBuilder
                            {
                                Title = $"Successfully removed `Case {caseID}` for `{member.DisplayName}`",
                                Color = new DiscordColor(0x0080FF)
                            };
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(EM));
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
            string file = $"Settings/guild/{ctx.Guild.Id}.conf";
            if (File.Exists(file))
            {
                StreamReader readData = new StreamReader(file);
                string data = readData.ReadToEnd();
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

        [SlashCommand("unmute", "Unmute a user")]
        [SlashRequirePermissions(Permissions.ManageRoles)]
        public async Task UnmuteCommand(InteractionContext ctx, [Option("user", "The user to unmute")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            string file = $"Settings/guild/{ctx.Guild.Id}.conf";
            if (File.Exists(file))
            {
                StreamReader readData = new StreamReader(file);
                string data = readData.ReadToEnd();
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
                        Title = "Oops..",
                        Description = $"{ex}",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errEM));
                    return;
                }
                var em = new DiscordEmbedBuilder
                {
                    Title = $"`{member.DisplayName}` has been unmuted",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(em));
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
    }
}
