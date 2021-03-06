﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using PatternSpider_Discord.Config;
using PatternSpider_Discord.Plugins.Plugin_MTG;
using Serilog;


namespace PatternSpider_Discord.Plugins.MTG
{
    public class PluginMTG : IPatternSpiderPlugin
    {
        private HttpClient _httpClient;

        public string Name => "MTG";
        public List<string> Commands => new List<string> { "mtg" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }


        public PluginMTG()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.Timeout = TimeSpan.FromSeconds(2);
        }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var text = message.Trim();
            var messageParts = text.Split(' ');
            var searchString = string.Join(" ", messageParts.Skip(1));

            DiscordMessage resultMesage;

            try
            {
                resultMesage = await SearchMagic(searchString);
                
            }
            catch (Exception e)
            {
                Log.Error(e, $"Plugin-MTG: Encountered an error searching for the following {searchString}");
                await m.Channel.SendMessageAsync($"Encountered an unexpected problem trying to search for card {searchString}.");
                return;
            }

            await resultMesage.SendMessageToChannel(m.Channel);
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }


        protected async Task<DiscordMessage> SearchMagic(string searchString)
        {            
            List<MtgCard> cards;
            var searchTerm = WebUtility.UrlEncode(searchString.ToLower());
            var searchUrl = $"https://api.magicthegathering.io/v1/cards?name={searchTerm}";
            string jsonData;

            var stringTask = _httpClient.GetStringAsync(searchUrl);

            try
            {
                jsonData = await stringTask;
            }
            catch
            {
                return new DiscordMessage("Error Occured trying to search for card.");
            }
                
            if (string.IsNullOrWhiteSpace(jsonData))
            {                
                return new DiscordMessage("No Results found for: " + searchString);
            }
                

            var errorRegex = @"{""error"":""(.+)""}";
            var errorMatch = Regex.Match(jsonData, errorRegex);
            if (errorMatch.Success)
            {
                var errorString = errorMatch.Groups.FirstOrDefault().ToString();

                Log.Warning($"Plugin-MTG: Error while quering magicthegathering.io: {jsonData}");
            
                return new DiscordMessage($"Error while looking up MTG Card: {errorString}");
            }

            try
            {
                cards = ParseJson(jsonData);
            }
            catch (Exception e)
            {
                Log.Warning("Plugin-MTG: Cannot parse magicthegathering.io API Response. Request: {requestSTring}", searchUrl);
                Log.Debug(e, "Plugin-MTG: Cannot parse magicthegathering.io API Response. Request: {requestSTring}", searchUrl);
                throw;
            }

            if (cards.Count == 1)
            {
                return new DiscordMessage(CardToEmbed(cards.LastOrDefault()));                
            }
                

            if (cards.Count == 0)
            {
                return new DiscordMessage($"Could not find any cards named: {searchString}");
            }

            var nameBuffer = cards.FirstOrDefault().name;
            foreach (var card in cards)
            {
                if( card.name != nameBuffer)
                {                
                    return new DiscordMessage($"[<https://scryfall.com/search?q={searchTerm}&unique=cards&as=grid&order=name>] Found {cards.Count} results.");
                }
                
            }
            return new DiscordMessage( CardToEmbed(cards.LastOrDefault()) );
        
        }

        private static List<MtgCard> ParseJson(string data)
        {
            var jsonData = JsonConvert.DeserializeObject<MtgCards>(data);
            return jsonData.cards.ToList();
        }

        private static Embed CardToEmbed(MtgCard card)
        {
            var cardEmbed = new EmbedBuilder();

            var searchTerm = WebUtility.UrlEncode(card.name.ToLower());
            var cardUrl = $"https://scryfall.com/search?q={searchTerm}&unique=cards&as=grid&order=name";

            var cardImage = card.imageUrl;

            cardEmbed.Title = card.name;
            cardEmbed.Url = cardUrl;
            cardEmbed.ThumbnailUrl = cardImage;

            var setField = new EmbedFieldBuilder
            {
                Name = "Set",
                Value = card.setName,
                IsInline = true
            };

            var rarityField = new EmbedFieldBuilder
            {
                Name = "Rarity",
                Value = card.rarity,
                IsInline = true

            };

            var typeField = new EmbedFieldBuilder
            {
                Name = "Type",
                Value = card.type,                
            };

            cardEmbed.Fields.Add(setField);
            cardEmbed.Fields.Add(rarityField);
            cardEmbed.Fields.Add(typeField);

            if (card.loyalty != null)
            {
                //Card is probably a planeswalker
                var loyaltyField = new EmbedFieldBuilder
                {
                    Name = "Loyalty",
                    Value = card.loyalty.ToString(),
                    IsInline = true,
                };

                cardEmbed.Fields.Add(loyaltyField);

            }
            else if (!string.IsNullOrWhiteSpace(card.toughness) && !string.IsNullOrWhiteSpace(card.power))
            {
                //card is probably some form of creature
                var powerField = new EmbedFieldBuilder
                {
                    Name = "Power",
                    Value = card.power,
                    IsInline = true
                };

                var toughnessField = new EmbedFieldBuilder
                {
                    Name = "Toughness",
                    Value = card.toughness,
                    IsInline = true
                };

                cardEmbed.Fields.Add(powerField);
                cardEmbed.Fields.Add(toughnessField);
            }

            var costField = new EmbedFieldBuilder
            {
                Name = "Cost",
                Value = MTG_EmoteTable.ReplaceSymbols(card.manaCost),               

            };

            var textField = new EmbedFieldBuilder
            {
                Name = "Text",
                Value = MTG_EmoteTable.ReplaceSymbols(card.text),
            };

            cardEmbed.Fields.Add(costField);
            cardEmbed.Fields.Add(textField);

            cardEmbed.Color = new Color(0x68c7ce);

            return cardEmbed;
        }        
    }
}
