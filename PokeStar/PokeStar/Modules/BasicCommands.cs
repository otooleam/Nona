﻿using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace PokeStar.Modules
{
   public class BasicCommands : ModuleBase<SocketCommandContext>
   {
      [Command("ping")]
      public async Task Ping()
      {
         await ReplyAsync("Pong");
      }
   }
}