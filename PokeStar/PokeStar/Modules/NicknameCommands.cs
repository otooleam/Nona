using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
using PokeStar.ConnectionInterface;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handle nickname commands.
   /// </summary>
   public class NicknameCommands : DexCommandParent
   {
      /// <summary>
      /// Handle editNickname command.
      /// </summary>
      /// <param name="nicknameString">Update the nickname of a Pokémon using this string.</param>
      /// <returns>Completed Task.</returns>
      [Command("editNickname")]
      [Alias("editNicknames")]
      [Summary("Edit Pokémon nicknames.")]
      [Remarks("This command is used for adding, updating, and removing nicknames.\n" +
               "To add or update a nickname a special character (>) is used.\n" +
               "\nFor each option format the nicknameString as following:\n" +
               "Add Nickname..............nickname > Pokémon name\n" +
               "Update Nickname........new nickname > old nickname\n" +
               "Delete Nickname.........> nickname\n" +
               "\nNote: Spaces are allowed for nicknames")]
      [RegisterChannel('D')]
      public async Task EditNickname([Summary("Update the nickname of a Pokémon using this string.")][Remainder] string nicknameString)
      {
         ulong guild = Context.Guild.Id;
         int delimeterIndex = nicknameString.IndexOf(Global.PARSE_DELIMITER);

         if (delimeterIndex == Global.DELIMITER_MISSING)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "editNickname", $"No nicknam delimiter (>) found.");
         }
         else
         {
            string[] arr = nicknameString.Split(Global.PARSE_DELIMITER);
            if (arr.Length == Global.NUM_PARSE_ARGS)
            {
               string newValue = arr[Global.NEW_PARSE_VALUE].Trim();
               string oldValue = arr[Global.OLD_PARSE_VALUE].Trim();

               if (string.IsNullOrEmpty(newValue))
               {
                  string name = Connections.Instance().GetPokemonWithNickname(guild, newValue);
                  if (name == null)
                  {
                     await ResponseMessage.SendErrorMessage(Context.Channel, "editNickname", $"The nickname {newValue} is not registered with a Pokémon.");
                  }
                  else
                  {
                     Connections.Instance().DeleteNickname(guild, newValue);
                     await ResponseMessage.SendInfoMessage(Context.Channel, $"Removed {newValue} from {name}.");
                  }
               }
               else
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(GetPokemonName(oldValue));

                  if (pokemon == null || pokemon.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
                  {
                     if (Connections.Instance().GetPokemonWithNickname(guild, oldValue) == null)
                     {
                        await ResponseMessage.SendErrorMessage(Context.Channel, "editNickname", $"{oldValue} is not a registered nickname.");
                     }
                     else
                     {
                        Connections.Instance().UpdateNickname(guild, oldValue, newValue);
                        string pkmn = Connections.Instance().GetPokemonWithNickname(guild, oldValue);
                        await ResponseMessage.SendInfoMessage(Context.Channel, $"{newValue} has replaced {oldValue} as a valid nickname for {pkmn}.");
                     }
                  }
                  else
                  {
                     Connections.Instance().AddNickname(guild, newValue, pokemon.Name);
                     await ResponseMessage.SendInfoMessage(Context.Channel, $"{newValue} is now a valid nickname for {pokemon.Name}.");
                  }
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "editNickname", $"Too many delimiters (>) found.");
            }
         }
      }

      /// <summary>
      /// Handle nickname command.
      /// </summary>
      /// <param name="pokemon">Get the nicknames for this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("nickname")]
      [Alias("nicknames")]
      [Summary("Gets nicknames for a given Pokémon.")]
      [Remarks("Can search by Pokémon name, nickname, or number.")]
      [RegisterChannel('D')]
      public async Task Nickname([Summary("Get the nicknames for this Pokémon.")][Remainder] string pokemon)
      {
         ulong guild = Context.Guild.Id;
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0 || pokemonNum == Global.DUMMY_POKE_NUM)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "nickname", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > Global.MAX_OPTIONS)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "nickname", $"Pokémon with number {pokemonNum} has to many forms to be displayed. One Pokémon is {pokemonWithNumber.First()}.");
            }
            else if (pokemonWithNumber.Count > 1)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               pkmn.Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
               await SendDexMessage(pkmn, BuildNicknameEmbed, Context.Channel);
            }
         }
         else
         {
            string name = GetPokemonName(pokemon);
            Pokemon pkmn = Connections.Instance().GetPokemon(name);
            if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
            {
               pkmn = Connections.Instance().GetPokemon(Connections.Instance().GetPokemonWithNickname(Context.Guild.Id, name));

               if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
               {
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  pkmn.Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
                  await SendDexMessage(pkmn, BuildNicknameEmbed, Context.Channel);
               }
            }
            else
            {
               pkmn.Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
               await SendDexMessage(pkmn, BuildNicknameEmbed, Context.Channel);
            }
         }
      }
   }
}