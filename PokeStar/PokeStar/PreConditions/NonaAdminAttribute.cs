﻿using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace PokeStar.PreConditions
{
   /// <summary>
   /// Checks if the current server is the home server.
   /// </summary>
   class NonaAdminAttribute : PreconditionAttribute
   {
      /// <summary>
      /// Checks permissions for the command.
      /// </summary>
      /// <param name="context">Context that the command was sent with.</param>
      /// <param name="command">Command that was sent.</param>
      /// <param name="services">Service collection used for dependency injection</param>
      /// <returns>Precondition result.</returns>
      public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
      {
         if (context.Guild.Name.Equals(Global.HOME_SERVER, StringComparison.OrdinalIgnoreCase))
         {
            return await Task.FromResult(PreconditionResult.FromSuccess());
         }
         else
         {
            return await Task.FromResult(PreconditionResult.FromError(""));
         }
      }
   }
}
