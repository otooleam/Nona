using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ModuleParents;

namespace PokeStar.PreConditions
{
   /// <summary>
   /// Checks if message is a reply to a raid boss select message.
   /// </summary>
   class BossSelectReplyAttribute : PreconditionAttribute
   {
      /// <summary>
      /// Checks message is a reply to a boss select message.
      /// </summary>
      /// <param name="context">Context that the command was sent with.</param>
      /// <param name="command">Command that was sent.</param>
      /// <param name="services">Service collection used for dependency injection</param>
      /// <returns>Precondition result.</returns>
      public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
      {
         if (context.Message.Reference != null && (RaidCommandParent.IsRaidSelectMessage(context.Message.Reference.MessageId.Value) || 
             RaidCommandParent.IsRaidEditBossMessage(context.Message.Reference.MessageId.Value, 
             (await context.Channel.GetMessageAsync(context.Message.Reference.MessageId.Value)).Embeds.First().Fields.First().Value)))
         {
            return await Task.FromResult(PreconditionResult.FromSuccess());
         }
         else
         {
            string message = $"{command.Name} command must be a reply to a raid boss select message.";
            await ResponseMessage.SendErrorMessage(context.Channel, command.Name, message);
            return await Task.FromResult(PreconditionResult.FromError(""));
         }
      }
   }
}