using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles type commands.
   /// </summary>
   public class TypeCommands : DexCommandParent
   {
      /// <summary>
      /// Handle type command.
      /// </summary>
      /// <param name="type1">(Optional) Get information about this type.</param>
      /// <param name="type2">(Optional) Get information about this secondary type.</param>
      /// <returns>Completed Task.</returns>
      [Command("type")]
      [Summary("Gets information for a given Pokémon type.")]
      [RegisterChannel('D')]
      public async Task Type([Summary("(Optional) Get information about this type.")] string type1 = null,
                           [Summary("(Optional) Get information about this secondary type.")] string type2 = null)
      {
         if (type1 == null)
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Global.NONA_EMOJIS["bug_emote"]} Bug");
            sb.AppendLine($"{Global.NONA_EMOJIS["dark_emote"]} Dark");
            sb.AppendLine($"{Global.NONA_EMOJIS["dragon_emote"]} Dragon");
            sb.AppendLine($"{Global.NONA_EMOJIS["electric_emote"]} Electric");
            sb.AppendLine($"{Global.NONA_EMOJIS["fairy_emote"]} Fairy");
            sb.AppendLine($"{Global.NONA_EMOJIS["fighting_emote"]} Fighting");
            sb.AppendLine($"{Global.NONA_EMOJIS["fire_emote"]} Fire");
            sb.AppendLine($"{Global.NONA_EMOJIS["flying_emote"]} Flying");
            sb.AppendLine($"{Global.NONA_EMOJIS["ghost_emote"]} Ghost");
            sb.AppendLine($"{Global.NONA_EMOJIS["grass_emote"]} Grass");
            sb.AppendLine($"{Global.NONA_EMOJIS["ground_emote"]} Ground");
            sb.AppendLine($"{Global.NONA_EMOJIS["ice_emote"]} Ice");
            sb.AppendLine($"{Global.NONA_EMOJIS["normal_emote"]} Normal");
            sb.AppendLine($"{Global.NONA_EMOJIS["poison_emote"]} Poison");
            sb.AppendLine($"{Global.NONA_EMOJIS["psychic_emote"]} Psychic");
            sb.AppendLine($"{Global.NONA_EMOJIS["rock_emote"]} Rock");
            sb.AppendLine($"{Global.NONA_EMOJIS["steel_emote"]} Steel");
            sb.AppendLine($"{Global.NONA_EMOJIS["water_emote"]} Water");

            EmbedBuilder embed = new EmbedBuilder();
            embed.AddField($"Pokémon Types:", sb.ToString());
            embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
            embed.WithFooter("Pokémon have 1 or 2 types. Moves always have 1 type.");
            await ReplyAsync(embed: embed.Build());
         }
         else
         {
            List<string> types = new List<string> { type1 };
            if (type2 != null && !type1.Equals(type2, StringComparison.OrdinalIgnoreCase))
            {
               types.Add(type2);
            }

            if (!CheckValidType(type1) || (types.Count == 2 && !CheckValidType(type2)))
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "type", $"{(!CheckValidType(type1) ? type1 : type2)} is not a valid type.");
            }
            else
            {
               string title = $"{type1}";
               if (types.Count == 2)
               {
                  title += $", {type2}";
               }

               string description = Global.NONA_EMOJIS[$"{type1}_emote"];
               if (types.Count == 2)
               {
                  description += Global.NONA_EMOJIS[$"{type2}_emote"];
               }

               TypeRelation? type1AttackRelations = (types.Count == 2) ? null : (TypeRelation?)Connections.Instance().GetTypeAttackRelations(type1);
               TypeRelation defenseRelations = Connections.Instance().GetTypeDefenseRelations(types);
               List<string> weather = Connections.Instance().GetWeather(types);

               EmbedBuilder embed = new EmbedBuilder();
               string fileName = BATTLE_IMAGE;
               embed.WithTitle($"Type {title.ToUpper()}");
               embed.WithDescription(description);
               embed.WithThumbnailUrl($"attachment://{fileName}");
               embed.AddField("Weather Boosts:", FormatWeatherList(weather), false);
               if (type1AttackRelations.HasValue)
               {
                  embed.AddField($"Super Effective against:", FormatTypeList(type1AttackRelations.Value.Strong), false);
                  embed.AddField($"Not Very Effective against:", FormatTypeList(type1AttackRelations.Value.Weak), false);
               }
               embed.AddField($"Weaknesses:", FormatTypeList(defenseRelations.Weak), false);
               embed.AddField($"Resistances:", FormatTypeList(defenseRelations.Strong), false);
               embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);

               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: embed.Build());
               Connections.DeleteFile(fileName);
            }
         }
      }
   }
}