using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FossiumBot.Commands
{
    public class Update : ApplicationCommandModule
    {
        [SlashCommand("updatecheck", "Check for updates")]
        [SlashRequireOwner]
        public async Task UpdatecheckCommand(InteractionContext ctx)
        {
            var checkingembed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Client.CurrentUser.Username} is checking for updates...",
                Color = new DiscordColor(0xFFA500)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(checkingembed));

            string content = String.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("FossiumBot", Program.localversion));
                content = await client.GetStringAsync("https://api.github.com/repos/Fossium-Team/FossiumBot/releases/latest");
            }
            JObject jsonData = JObject.Parse(content);
            string latestversion = jsonData["tag_name"].ToString();
            string latestreleaseurl = jsonData["html_url"].ToString();
            
            int compare = string.Compare(latestversion, Program.localversion);

            if (compare > 0)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Update available",
                    Description = $"Download the latest release at {latestreleaseurl}",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"No updates available",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
        }
    }
}