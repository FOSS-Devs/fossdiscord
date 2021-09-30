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
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FossiumBot.Commands
{
    public class Fun : ApplicationCommandModule
    {
        [SlashCommand("rate", "Rate something out of 10")]
        public async Task RateCommand(InteractionContext ctx, [Option("thing", "Thing to rate")] string thing)
        {
            Random r = new Random();
            int randomnum = r.Next(0, 10);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"I rate `{thing}` a {randomnum}/10",
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("cat", "Shows a random picture of a cat")]
        public async Task CatCommand(InteractionContext ctx)
        {
            string content = String.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("FossiumBot", Program.localversion));
                content = await client.GetStringAsync("https://api.thecatapi.com/v1/images/search");
            }
            JArray jsonData = JArray.Parse(content);
            var caturl = jsonData[0]["url"];
            string catpic = (string)caturl;
            var embed = new DiscordEmbedBuilder
            {
                Title = "Cat Picture",
                ImageUrl = catpic,
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("dog", "Shows a random picture of a dog")]
        public async Task DogCommand(InteractionContext ctx)
        {
            string content = String.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("FossiumBot", Program.localversion));
                content = await client.GetStringAsync("https://api.thedogapi.com/v1/images/search   ");
            }
            JArray jsonData = JArray.Parse(content);
            var dogurl = jsonData[0]["url"];
            string dogpic = (string)dogurl;
            var embed = new DiscordEmbedBuilder
            {
                Title = "Dog Picture",
                ImageUrl = dogpic,
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("wikipedia", "Get information about something from Wikipedia")]
        public async Task WikiCommand(InteractionContext ctx, [Option("query", "What you want to get information of")] string query)
        {
            string URL = $"https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro&explaintext&origin=*&format=json&generator=search&gsrnamespace=0&gsrlimit=1&gsrsearch={query}";
            WebRequest wrREQUEST;
            wrREQUEST = WebRequest.Create(URL);
            wrREQUEST.Proxy = null;
            wrREQUEST.Method = "GET";
            WebResponse response = wrREQUEST.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string responseData = streamReader.ReadToEnd();
            streamReader.Close();
            JObject jsonData = JObject.Parse(responseData);
            try
            {
                string pageID = ((JProperty)jsonData["query"]["pages"].First()).Name;
                if (pageID == "-1")
                {
                    var errEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Oops...",
                        Description = "The page you've requested might not exist",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errEmbed));
                    return;
                }
                else
                {
                    string pageTitle = (string)jsonData["query"]["pages"][pageID]["title"];
                    string extract = (string)jsonData["query"]["pages"][pageID]["extract"];
                    string brief = extract.Substring(0, 260);
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{pageTitle}",
                        Description = $"{brief}...",
                        Color = new DiscordColor(0x0080FF)
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                }
            }
            catch (Exception)
            {
                var errEmbed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Page does not exist",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errEmbed));
                return;
            }
        }
    }
}
