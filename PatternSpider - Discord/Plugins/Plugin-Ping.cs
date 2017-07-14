﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;


namespace PatternSpider_Discord.Plugins
{
    class PluginPing : IPatternSpiderPlugin
    {
        public string Name => "ping";
        public List<string> Commands=> new List<string>{"ping"};

        public async Task Command(string command, string messsage, SocketMessage m)
        {
            await m.Channel.SendMessageAsync("Pong.");
        }

        public async Task Message(string messsage, SocketMessage m)
        {            
        }
    }
}