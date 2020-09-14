﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using PokeStar.ConnectionInterface;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;

namespace PokeStar.Modules
{
   public class NicknameCommands : DexCommandParent
   {
      private static readonly int ArrayLength = 2;

      private static readonly int IndexNickname = 0;
      private static readonly int IndexName = 1;

      [Command("nickname")]
      [Summary("Edit Pokémon nicknames.")]
      [Remarks("This command is used for adding, updating, and removing nicknames.\n" +
               "To add or update a nickname a special character (>) is used.\n" +
               "\nFor each option format the nicknameString as following:\n" +
               "Add Nickname..............nickname > Pokémon name\n" +
               "Update Nickname........new nickname > old nickname\n" +
               "Delete Nickname.........nickname\n" +
               "\nNote: Spaces are allowed for nicknames")]
      [RegisterChannel('D')]
      public async Task Nickname([Summary("Get CPs for this pokémon.")][Remainder] string nicknameString)
      {
         ulong guild = Context.Guild.Id;
         int delimeterIndex = nicknameString.IndexOf(Global.NICKNAME_DELIMITER);

         if (delimeterIndex == -1)
         {
            string trim = nicknameString.Trim();
            string name = Connections.Instance().GetPokemonWithNickname(guild, trim);
            if (name == null)
            {
               await ResponseMessage.SendErrorMessage(Context, "nickname", $"The nickname {trim} is not registered with a Pokémon.");
            }
            else
            {
               Connections.Instance().DeleteNickname(guild, trim);
               await ResponseMessage.SendInfoMessage(Context, $"Removed {trim} from {name}.");
            }
         }
         else
         {
            string[] arr = nicknameString.Split(Global.NICKNAME_DELIMITER);
            if (arr.Length != ArrayLength)
            {
               await ResponseMessage.SendErrorMessage(Context, "nickname", $"Too many delimiters found.");
            }
            else
            {
               string newNickname = arr[IndexNickname].Trim();
               string other = arr[IndexName].Trim();
               Pokemon pokemon = Connections.Instance().GetPokemon(GetPokemonName(other));

               if (pokemon == null)
               {
                  if (Connections.Instance().GetPokemonWithNickname(guild, other) == null)
                  {
                     await ResponseMessage.SendErrorMessage(Context, "nickname", $"{other} is not a registered nickname,");
                  }
                  else
                  {
                     Connections.Instance().UpdateNickname(guild, other, newNickname);
                     string pkmn = Connections.Instance().GetPokemonWithNickname(guild, other);
                     await ResponseMessage.SendInfoMessage(Context, $"{newNickname} has replaced {other} as a valid nickname for {pkmn}.");
                  }
               }
               else
               {
                  Connections.Instance().AddNickname(guild, newNickname, pokemon.Name);
                  await ResponseMessage.SendInfoMessage(Context, $"{newNickname} is now a valid nickname for {pokemon.Name}.");
               }
            }
         }
      }


      [Command("getNicknames")]
      [Alias("getNickname")]
      [Summary("Gets nicknames for a given Pokémon.")]
      [Remarks("Can search by Pokémon name, nickname, or number.")]
      [RegisterChannel('D')]
      public async Task GetNickname([Summary("Get information for this pokémon.")][Remainder] string pokemon)
      {
         ulong guild = Context.Guild.Id;
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0)
            {
               await ResponseMessage.SendErrorMessage(Context, "getNickname", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonNum == Global.ARCEUS_NUMBER)
            {
               await ResponseMessage.SendErrorMessage(Context, "dex", $"Arceus #{pokemonNum} has too many forms to display, please search by name.");
            }
            else if (pokemonWithNumber.Count > 1 && pokemonNum != Global.UNOWN_NUMBER)
            {
               string fileName = POKEDEX_SELECTION_IMAGE;
               Connections.CopyFile(fileName);
               RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonWithNumber, fileName));
               for (int i = 0; i < pokemonWithNumber.Count; i++)
               {
                  await dexMessage.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
               }
               dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE, pokemonWithNumber));
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               List<string> Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
               string fileName = Connections.GetPokemonPicture(pkmn.Name);
               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: BuildNicknameEmbed(Nicknames, pkmn.Name, fileName));
               Connections.DeleteFile(fileName);
            }
         }
         else // Is string
         {
            string name = GetPokemonName(pokemon);
            Pokemon pkmn = Connections.Instance().GetPokemon(name);
            if (pkmn == null)
            {
               pkmn = Connections.Instance().GetPokemon(Connections.Instance().GetPokemonWithNickname(Context.Guild.Id, name));

               if (pkmn == null)
               {
                  List<string> pokemonNames = Connections.Instance().FuzzyNameSearch(name);

                  string fileName = POKEDEX_SELECTION_IMAGE;
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonNames, fileName));
                  await dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);

                  dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE, pokemonNames));
               }
               else
               {
                  List<string> Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildNicknameEmbed(Nicknames, pkmn.Name, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
            else
            {
               List<string> Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
               string fileName = Connections.GetPokemonPicture(pkmn.Name);
               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: BuildNicknameEmbed(Nicknames, pkmn.Name, fileName));
               Connections.DeleteFile(fileName);
            }
         }
      }
   }
}