using System;
using System.Linq;
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
   /// Handles catch commands.
   /// </summary>
   public class CatchCommand : DexCommandParent
   {
      /// <summary>
      /// Handle catch command.
      /// </summary>
      /// <param name="pokemon">Simulate catching this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("catch")]
      [Summary("Simulates catching a Pokémon.")]
      [RegisterChannel('I')]
      public async Task Catch([Summary("Simulate catching this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0 || pokemonNum == 0)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "catch", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > 1 && pokemonNum != Global.UNOWN_NUMBER && pokemonNum != Global.ARCEUS_NUMBER)
            {
               IEmote[] emotes = Global.SELECTION_EMOJIS.Take(pokemonWithNumber.Count).ToArray();
               string fileName = POKEDEX_SELECTION_IMAGE;
               Connections.CopyFile(fileName);
#if BUTTONS
               RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, 
                  embed: BuildDexSelectEmbed(pokemonWithNumber, fileName), components: Global.BuildButtons(emotes));
#else
               RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonWithNumber, fileName));
#endif
               dexSelectMessages.Add(dexMessage.Id, new DexSelectionMessage((int)DEX_MESSAGE_TYPES.CATCH_MESSAGE, pokemonWithNumber));
               Connections.DeleteFile(fileName);
#if !BUTTONS
               await dexMessage.AddReactionsAsync(emotes);
#endif
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               CatchSimulation catchSim = new CatchSimulation(pkmn);
               string fileName = Connections.GetPokemonPicture(pkmn.Name);
               Connections.CopyFile(fileName);
#if BUTTONS
               RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, 
                  embed: BuildCatchEmbed(catchSim, fileName), components: Global.BuildButtons(catchEmojis, catchComponents));
#else
               RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
#endif
               catchMessages.Add(catchMessage.Id, catchSim);
               Connections.DeleteFile(fileName);
#if !BUTTONS
               await catchMessage.AddReactionsAsync(catchEmojis);
#endif
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
                  List<string> pokemonNames = Connections.Instance().SearchPokemon(name);

                  string fileName = POKEDEX_SELECTION_IMAGE;
                  Connections.CopyFile(fileName);
#if BUTTONS
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, 
                     embed: BuildDexSelectEmbed(pokemonNames, fileName), components: Global.BuildButtons(Global.SELECTION_EMOJIS));
#else
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonNames, fileName));
#endif
                  dexSelectMessages.Add(dexMessage.Id, new DexSelectionMessage((int)DEX_MESSAGE_TYPES.CATCH_MESSAGE, pokemonNames));
                  Connections.DeleteFile(fileName);
#if !BUTTONS
                  await dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);
#endif
               }
               else
               {
                  CatchSimulation catchSim = new CatchSimulation(pkmn);
                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
#if BUTTONS
                  RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName,
                     embed: BuildCatchEmbed(catchSim, fileName), components: Global.BuildButtons(catchEmojis, catchComponents));
#else
                  RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
#endif
                  catchMessages.Add(catchMessage.Id, catchSim);
                  Connections.DeleteFile(fileName);
#if !BUTTONS
               await catchMessage.AddReactionsAsync(catchEmojis);
#endif
               }
            }
            else
            {
               CatchSimulation catchSim = new CatchSimulation(pkmn);
               string fileName = Connections.GetPokemonPicture(pkmn.Name);
               Connections.CopyFile(fileName);
#if BUTTONS
               RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, 
                  embed: BuildCatchEmbed(catchSim, fileName), components: Global.BuildButtons(catchEmojis, catchComponents));
#else
               RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
#endif
               catchMessages.Add(catchMessage.Id, catchSim);
               Connections.DeleteFile(fileName);
#if !BUTTONS
               await catchMessage.AddReactionsAsync(catchEmojis);
#endif
            }
         }
      }
   }
}