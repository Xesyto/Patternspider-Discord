﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;
using Discord.WebSocket;
using PatternSpider_Discord.Config;

namespace PatternSpider_Discord.Plugins
{
    class PluginMath : IPatternSpiderPlugin
    {
        public string Name => "Math";
        public List<string> Commands => new List<string> { "math", "m", "calculate", "calc", "c" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var messageParts = message.Split(' ');
            message = string.Join(" ", messageParts.Skip(1));

            var result = CalculateString(message);

            if (result == "NaN")
            {
                await m.Channel.SendMessageAsync("Could not process math expression.");
            }
            else
            {
                await m.Channel.SendMessageAsync(result);
            }
           
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        public string CalculateString(string input)
        {
            var expresion = new Expression(input);
            double result;

            try
            {
                result = expresion.calculate();
            }
            catch (Exception e)
            {
                return $"Could not process math expression. \n {e.Message}";
            }
                   
            return result.ToString(CultureInfo.InvariantCulture);
        }
    }
}
