using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles PokéDex commands.
   /// </summary>
   public class DexCommands : DexCommandParent
   {
      /// <summary>
      /// Handle dex command.
      /// </summary>
      /// <param name="pokemon">Get information for this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("dex")]
      [Alias("pokedex")]
      [Summary("Gets the PokéDex entry for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
      [RegisterChannel('D')]
      public async Task Dex([Summary("Get information for this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0 || pokemonNum == Global.DUMMY_POKE_NUM)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "dex", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > Global.MAX_OPTIONS)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "dex", $"Pokémon with number {pokemonNum} has to many forms to be displayed. One Pokémon is {pokemonWithNumber.First()}.");
            }
            else if (pokemonWithNumber.Count > 1)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.DEX_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               Connections.Instance().GetPokemonStats(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildDexEmbed, Context.Channel);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.DEX_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  Connections.Instance().GetPokemonStats(ref pkmn);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildDexEmbed, Context.Channel);
               }
            }
            else
            {
               Connections.Instance().GetPokemonStats(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildDexEmbed, Context.Channel);
            }
         }
      }

      /// <summary>
      /// Handle cp command.
      /// </summary>
      /// <param name="pokemon">Get CPs for this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("cp")]
      [Summary("Gets max CP values for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
      [RegisterChannel('D')]
      public async Task CP([Summary("Get CPs for this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0 || pokemonNum == Global.DUMMY_POKE_NUM)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "cp", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > Global.MAX_OPTIONS)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "cp", $"Pokémon with number {pokemonNum} has to many forms to be displayed. One Pokémon is {pokemonWithNumber.First()}.");
            }
            else if (pokemonWithNumber.Count > 1)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.CP_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               Connections.GetPokemonCP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildCPEmbed, Context.Channel);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.CP_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  Connections.GetPokemonCP(ref pkmn);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildCPEmbed, Context.Channel);
               }
            }
            else
            {
               Connections.GetPokemonCP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildCPEmbed, Context.Channel);
            }
         }
      }

      /// <summary>
      /// Handle evo command.
      /// </summary>
      /// <param name="pokemon">Get evolution family for this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("evo")]
      [Alias("evolution", "evolve")]
      [Summary("Gets the evolution family for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
      [RegisterChannel('D')]
      public async Task Evolution([Summary("Get evolution family for this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0 || pokemonNum == Global.DUMMY_POKE_NUM)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "evo", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > Global.MAX_OPTIONS)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "evo", $"Pokémon with number {pokemonNum} has to many forms to be displayed. One Pokémon is {pokemonWithNumber.First()}.");
            }
            else if (pokemonWithNumber.Count > 1)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.EVO_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               pkmn.Evolutions = GenerateEvoDict(pkmn.Name);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildEvoEmbed, Context.Channel);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.EVO_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  pkmn.Evolutions = GenerateEvoDict(pkmn.Name);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildEvoEmbed, Context.Channel);
               }
            }
            else
            {
               pkmn.Evolutions = GenerateEvoDict(pkmn.Name);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildEvoEmbed, Context.Channel);
            }
         }
      }

      /// <summary>
      /// Handle counter command.
      /// </summary>
      /// <param name="pokemon">Get counters for this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("counter")]
      [Alias("counters")]
      [Summary("Gets top counters for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
      [RegisterChannel('D')]
      public async Task Counter([Summary("Get counters for this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0 || pokemonNum == Global.DUMMY_POKE_NUM)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "counter", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > Global.MAX_OPTIONS)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "counter", $"Pokémon with number {pokemonNum} has to many forms to be displayed. One Pokémon is {pokemonWithNumber.First()}.");
            }
            else if (pokemonWithNumber.Count > 1)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               Connections.Instance().GetPokemonCounter(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildCounterEmbed, Context.Channel);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  Connections.Instance().GetPokemonCounter(ref pkmn);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildCounterEmbed, Context.Channel);
               }
            }
            else
            {
               Connections.Instance().GetPokemonCounter(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildCounterEmbed, Context.Channel);
            }
         }
      }

      /// <summary>
      /// Handle pvp command.
      /// </summary>
      /// <param name="pokemon">Get PvP IVs for this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("pvp")]
      [Alias("pvpiv", "iv")]
      [Summary("Gets rank 1 PvP IV values for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
      [RegisterChannel('D')]
      public async Task PvP([Summary("Get PvP IVs for this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0 || pokemonNum == Global.DUMMY_POKE_NUM)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "pvp", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > Global.MAX_OPTIONS)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "pvp", $"Pokémon with number {pokemonNum} has to many forms to be displayed. One Pokémon is {pokemonWithNumber.First()}.");
            }
            else if (pokemonWithNumber.Count > 1)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.PVP_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               Connections.Instance().GetPokemonPvP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildPvPEmbed, Context.Channel);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.PVP_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  Connections.Instance().GetPokemonPvP(ref pkmn);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildPvPEmbed, Context.Channel);
               }
            }
            else
            {
               Connections.Instance().GetPokemonPvP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildPvPEmbed, Context.Channel);
            }
         }
      }

      /// <summary>
      /// Handle form command.
      /// </summary>
      /// <param name="pokemon">(Optional) Get form information for this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("form")]
      [Summary("Gets all forms for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.\n" +
               "Leave blank to get a list of all Pokémon with forms.\n")]
      [RegisterChannel('D')]
      public async Task Form([Summary("(Optional) Get form information for this Pokémon.")][Remainder] string pokemon = null)
      {
         if (pokemon == null)
         {
            List<string> baseForms = Connections.Instance().GetBaseForms();
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= baseForms.Count; i++)
            {
               string format = (i % 2 == 0) ? "" : "**";
               sb.Append($"{format}{baseForms.ElementAt(i - 1)}{format} ");
               if (i % 4 == 0)
               {
                  sb.Append('\n');
               }
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.AddField($"Pokémon with form differences:", sb.ToString());
            embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
            await ReplyAsync(embed: embed.Build());
         }
         else
         {
            bool isNumber = int.TryParse(pokemon, out int pokemonNum);
            if (isNumber)
            {
               List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

               if (pokemonWithNumber.Count == 0 || pokemonNum == Global.DUMMY_POKE_NUM)
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "form", $"Pokémon with number {pokemonNum} cannot be found.");
               }
               else
               {
                  Pokemon pkmn = new Pokemon();
                  if (pokemonWithNumber.Count == 1)
                  {
                     pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
                     pkmn.Forms = new Form();
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                     pkmn = Connections.Instance().GetPokemon(baseName);
                     pkmn.Forms = Connections.Instance().GetFormTags(baseName);
                  }
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildFormEmbed, Context.Channel);
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
                     await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.FORM_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
                  }
                  else
                  {
                     List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pkmn.Number);

                     if (pokemonWithNumber.Count == 1)
                     {
                        pkmn.Forms = new Form();
                     }
                     else if (pokemonWithNumber.Count > 1)
                     {
                        string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                        pkmn.Forms = Connections.Instance().GetFormTags(baseName);
                     }
                     pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                     await SendDexMessage(pkmn, BuildFormEmbed, Context.Channel);
                  }
               }
               else
               {
                  List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pkmn.Number);

                  if (pokemonWithNumber.Count == 1)
                  {
                     pkmn.Forms = new Form();
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                     pkmn.Forms = Connections.Instance().GetFormTags(baseName);
                  }
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildFormEmbed, Context.Channel);
               }
            }
         }
      }

      /// <summary>
      /// Handle dps command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("dps")]
      [Summary("Gets Pokémon with top general DPS.")]
      [Remarks("DPS: Damage Per Second")]
      [RegisterChannel('D')]
      public async Task DPS()
      {
         string fileName = GENERIC_IMAGE;
         Pokemon pokemon = Connections.Instance().GetPokemon(Connections.Instance().GetPokemonByNumber(Global.DUMMY_POKE_NUM).First());
         Connections.Instance().GetPokemonCounter(ref pokemon);
         Connections.CopyFile(fileName);
         await Context.Channel.SendFileAsync(fileName, embed: BuildCounterEmbed(pokemon, fileName));
         Connections.DeleteFile(fileName);
      }
   }
}