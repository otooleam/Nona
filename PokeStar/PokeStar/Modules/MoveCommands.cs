using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles move commands.
   /// </summary>
   public class MoveCommands : DexCommandParent
   {
      /// <summary>
      /// Handle move command.
      /// </summary>
      /// <param name="move">Get information about this move.</param>
      /// <returns>Completed Task.</returns>
      [Command("move")]
      [Summary("Gets information for a given move.")]
      [RegisterChannel('D')]
      public async Task Move([Summary("Get information about this move.")][Remainder] string move)
      {
         Move pkmnMove = Connections.Instance().GetMove(move);
         if (pkmnMove == null)
         {
            List<string> moveNames = Connections.Instance().SearchMove(move);

            string fileName = GENERIC_IMAGE;
            Connections.CopyFile(fileName);
#if COMPONENTS
#if DROP_DOWNS
            RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName,
               embed: BuildDexSelectEmbed(fileName), components: Global.BuildButtonsWithMenu(moveNames.ToArray()));
#else
            RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, 
               embed: BuildDexSelectEmbed(moveNames, fileName), components: Global.BuildButtons(Global.SELECTION_EMOJIS));
#endif
#else
            RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(moveNames, fileName));
#endif
            dexSelectMessages.Add(dexMessage.Id, new DexSelectionMessage((int)DEX_MESSAGE_TYPES.MOVE_MESSAGE, moveNames));
            Connections.DeleteFile(fileName);
#if !COMPONENTS
            await dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);
#endif
         }
         else
         {
            string fileName = GENERIC_IMAGE;
            Connections.CopyFile(fileName);
            await Context.Channel.SendFileAsync(fileName, embed: BuildMoveEmbed(pkmnMove, fileName));
            Connections.DeleteFile(fileName);
         }
      }

      /// <summary>
      /// Handle movetype command.
      /// </summary>
      /// <param name="type">Get moves for this type.</param>
      /// <param name="category">(Optional) Get moves of this category (fast / charge).</param>
      /// <returns>Completed Task.</returns>
      [Command("movetype")]
      [Summary("Gets information for a given type of move.")]
      [RegisterChannel('D')]
      public async Task MoveType([Summary("Get moves for this type.")] string type,
                                 [Summary("(Optional) Get moves of this category (fast / charge).")] string category = null)
      {
         if (CheckValidType(type))
         {
            if (category == null)
            {
               List<string> fastMoves = Connections.Instance().GetMoveByType(type, Global.FAST_MOVE_CATEGORY);
               List<string> chargeMoves = Connections.Instance().GetMoveByType(type, Global.CHARGE_MOVE_CATEGORY);

               StringBuilder sbFast = new StringBuilder();
               foreach (string move in fastMoves)
               {
                  sbFast.AppendLine(move);
               }

               StringBuilder sbCharge = new StringBuilder();
               foreach (string move in chargeMoves)
               {
                  sbCharge.AppendLine(move);
               }

               string fileName = GENERIC_IMAGE;
               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle($"{type.ToUpper()} moves");
               embed.WithDescription(Global.NONA_EMOJIS[$"{type}_emote"]);
               embed.AddField("Fast Moves", sbFast.ToString());
               embed.AddField("Charge Moves", sbCharge.ToString());
               embed.WithThumbnailUrl($"attachment://{fileName}");

               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: embed.Build());
               Connections.DeleteFile(fileName);

            }
            else if (category.Equals(Global.FAST_MOVE_CATEGORY, StringComparison.OrdinalIgnoreCase) || 
                     category.Equals(Global.CHARGE_MOVE_CATEGORY, StringComparison.OrdinalIgnoreCase))
            {
               List<string> moves = Connections.Instance().GetMoveByType(type, category);

               StringBuilder sb = new StringBuilder();
               foreach (string move in moves)
               {
                  sb.AppendLine(move);
               }

               string fileName = GENERIC_IMAGE;
               EmbedBuilder embed = new EmbedBuilder();
               embed.AddField($"{type.ToUpper()} {category.ToUpper()} Moves", sb.ToString());
               embed.WithDescription(Global.NONA_EMOJIS[$"{type}_emote"]);
               embed.WithThumbnailUrl($"attachment://{fileName}");

               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: embed.Build());
               Connections.DeleteFile(fileName);
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "movetype", $"{category} is not a valid move category.");
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "movetype", $"{type} is not a valid move type.");
         }
      }
   }
}