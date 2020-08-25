﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.Calculators;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles pokedex commands.
   /// </summary>
   public class DexCommands : ModuleBase<SocketCommandContext>
   {
      [Command("dex")]
      [Alias("pokedex")]
      [Summary("Gets the PokéDex entry for a given pokémon.")]
      public async Task Dex([Summary("The pokémon you want information about.")][Remainder] string pokemonName)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "D"))
         {
            string name = GetPokemon(pokemonName);
            Pokemon pokemon = Connections.Instance().GetPokemon(name);
            if (pokemon == null)
            {
               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle("PokéDex Command Error");
               embed.WithDescription($"The Pokémon {name} cannot be found.");
               embed.WithColor(Color.DarkRed);

               await Context.Channel.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
            }
            else
            {
               string fileName = Connections.GetPokemonPicture(pokemon.Name);
               Connections.CopyFile(fileName);

               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle($@"#{pokemon.Number} {pokemon.Name}");
               embed.WithDescription(pokemon.Description);
               embed.WithThumbnailUrl($"attachment://{fileName}");
               embed.AddField("Type", pokemon.TypeToString(), true);
               embed.AddField("Weather Boosts", pokemon.WeatherToString(), true);
               embed.AddField("Details", pokemon.DetailsToString(), true);
               embed.AddField("Stats", pokemon.StatsToString(), true);
               embed.AddField("Resistances", pokemon.ResistanceToString(), true);
               embed.AddField("Weaknesses", pokemon.WeaknessToString(), true);
               embed.AddField("Fast Moves", pokemon.FastMoveToString(), true);
               embed.AddField("Charge Moves", pokemon.ChargeMoveToString(), true);
               embed.AddField("Counters", pokemon.CounterToString(), false);
               embed.WithColor(Color.Red);
               embed.WithFooter("* denotes STAB move ! denotes Legacy move");

               await Context.Channel.SendFileAsync(fileName, embed: embed.Build()).ConfigureAwait(false);

               Connections.DeleteFile(fileName);
            }
         }
         else
         {
            await Context.Channel.SendMessageAsync("This channel is not registered to process PokéDex commands.").ConfigureAwait(false);
         }
      }

      [Command("cp")]
      [Summary("Gets max CP values for a given pokémon.")]
      public async Task CP([Summary("The pokémon you want CP information about.")][Remainder] string pokemonName)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "D"))
         {
            string name = GetPokemon(pokemonName);
            Pokemon pokemon = Connections.Instance().GetPokemon(name);
            if (pokemon == null)
            {
               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle("CP Command Error");
               embed.WithDescription($"The Pokémon {name} cannot be found.");
               embed.WithColor(Color.DarkRed);

               await Context.Channel.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
            }
            else
            {
               Connections.CalcAllCP(ref pokemon);
               string fileName = Connections.GetPokemonPicture(pokemon.Name);
               Connections.CopyFile(fileName);

               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle($@"#{pokemon.Number} {pokemon.Name} CP");
               embed.WithDescription($"Max CP values for {pokemon.Name}");
               embed.WithThumbnailUrl($"attachment://{fileName}");
               embed.AddField($"Max CP (Level 40)", pokemon.CPMax, true);
               embed.AddField($"Max Buddy CP (Level 41)", pokemon.CPBestBuddy, true);
               embed.AddField($"Raid CP (Level 20)", pokemon.RaidCPToString(), false);
               embed.AddField($"Hatch CP (Level 20)", pokemon.HatchCPToString(), false);
               embed.AddField($"Quest CP (Level 15)", pokemon.QuestCPToString(), false);
               embed.AddField("Wild CP (Level 1-35)", pokemon.WildCPToString(), false);
               embed.WithColor(Color.Blue);
               embed.WithFooter("* denotes Weather Boosted CP");

               await Context.Channel.SendFileAsync(fileName, embed: embed.Build()).ConfigureAwait(false);

               Connections.DeleteFile(fileName);
            }
         }
         else
         {
            await Context.Channel.SendMessageAsync("This channel is not registered to process PokéDex commands.").ConfigureAwait(false);
         }
      }

      [Command("type")]
      [Summary("Gets information for a given pokémon type.")]
      public async Task PokeType([Summary("The typing you want info about.")] string type1,
                                 [Summary("(Optional) Secondary typing you want info about.")] string type2 = null)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "D"))
         {
            List<string> types = new List<string>
            {
               type1,
            };
            if (type2 != null && !type1.Equals(type2, StringComparison.OrdinalIgnoreCase))
            {
               types.Add(type2);
            }

            if (!CheckValidType(type1) || (types.Count == 2 && !CheckValidType(type2)))
            {
               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle("Type Command Error");
               embed.WithDescription($"{(!CheckValidType(type1) ? type1 : type2)} is not a valid type.");
               embed.WithColor(Color.DarkRed);

               await Context.Channel.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
            }
            else
            {
               string title = $"{type1}";
               if (types.Count == 2)
               {
                  title += $", {type2}";
               }

               string description = Emote.Parse(Environment.GetEnvironmentVariable($"{type1.ToUpper()}_EMOTE")).ToString();
               if (types.Count == 2)
               {
                  description += Emote.Parse(Environment.GetEnvironmentVariable($"{type2.ToUpper()}_EMOTE")).ToString();
               }

               TypeRelation? type1AttackRelations = (types.Count == 2) ? null : Connections.Instance().GetTypeAttackRelations(type1);
               TypeRelation? defenseRelations = Connections.Instance().GetTypeDefenseRelations(types);
               List<string> weather = Connections.Instance().GetWeather(types);

               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle($@"Type {title.ToUpper()}");
               embed.WithDescription(description);
               embed.AddField("Weather Boosts:", FormatWeatherList(weather), false);
               if (type1AttackRelations.HasValue)
               {
                  embed.AddField($"Super Effective against:", FormatTypeList(type1AttackRelations.Value.strong), false);
                  embed.AddField($"Not Very Effective against:", FormatTypeList(type1AttackRelations.Value.weak), false);
               }
               embed.AddField($"Weaknesses:", FormatTypeList(defenseRelations.Value.weak), false);
               embed.AddField($"Resistances:", FormatTypeList(defenseRelations.Value.strong), false);
               embed.WithColor(Color.Blue);

               await Context.Channel.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
            }
         }
         else
         {
            await Context.Channel.SendMessageAsync("This channel is not registered to process PokéDex commands.").ConfigureAwait(false);
         }
      }

      /// <summary>
      /// Processes the pokemon name given from a command.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon.</param>
      /// <returns>Full name of the pokemon</returns>
      private static string GetPokemon(string pokemonName)
      {
         List<string> words = new List<string>(pokemonName.Split(' '));

         string form = words[words.Count - 1];
         if (form.Substring(0, 1).Equals("-", StringComparison.OrdinalIgnoreCase))
         {
            words.RemoveAt(words.Count - 1);
         }
         else
         {
            form = "";
         }

         string name = "";
         foreach (string str in words)
         {
            name += str + " ";
         }
         name = name.TrimEnd(' ');

         return GetFullName(name, form);
      }

      /// <summary>
      /// Gets the full name of a pokemon.
      /// The following pokemon have multiple forms:
      /// Name       Default Form
      /// -----------------------
      /// Unown      F
      /// Burmy      Plant Cloak
      /// Wormadam   Plant Cloak
      /// Cherrim    Sunshine
      /// Shellos    East Sea
      /// Gastrodon  East Sea
      /// Giratina   Altered Form
      /// Shaymin    Land Form
      /// Arceus     Normal
      /// Basculin   Blue Striped
      /// Deerling   Summer Form
      /// Sawsbuck   Summer Form
      /// Tornadus   Incarnate
      /// Thundurus  Incarnate
      /// Landorus   Incarnate
      /// Meloetta   Aria
      /// Note: nidoran defaults to the female form.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon</param>
      /// <param name="form">Form of the pokemon.</param>
      /// <returns>Full name of the pokemon.</returns>
      private static string GetFullName(string pokemonName, string form = "")
      {
         if (form.Length == 2)
            return $"{pokemonName} {form.ToCharArray()[1]}";
         // Alolan
         else if (form.Equals("-alola", StringComparison.OrdinalIgnoreCase))
            return $"Alolan {pokemonName}";
         // Galarian
         else if (form.Equals("-galar", StringComparison.OrdinalIgnoreCase))
            return $"Galarian {pokemonName}";
         // Mega
         else if (form.Equals("-mega", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName}";
         else if (form.Equals("-megax", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName} X";
         else if (form.Equals("-megay", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName} Y";
         // Nidoran
         else if (form.Equals("-female", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} F";
         else if (form.Equals("-male", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} M";
         // Mewtwo
         else if (form.Equals("-armor", StringComparison.OrdinalIgnoreCase))
            return $"Armored {pokemonName}";
         /// Unown and Nidoran
         else if (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("unown", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("nidoran", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} F";
         // Castform
         else if (form.Equals("-rain", StringComparison.OrdinalIgnoreCase))
            return $"Rainy {pokemonName}";
         else if (form.Equals("-snow", StringComparison.OrdinalIgnoreCase))
            return $"Snowy {pokemonName}";
         else if (form.Equals("-sun", StringComparison.OrdinalIgnoreCase))
            return $"Sunny {pokemonName}";
         // Deoxys
         else if (form.Equals("-attack", StringComparison.OrdinalIgnoreCase))
            return $"Attack Form {pokemonName}";
         else if (form.Equals("-defense", StringComparison.OrdinalIgnoreCase))
            return $"Defense Form {pokemonName}";
         else if (form.Equals("-speed", StringComparison.OrdinalIgnoreCase))
            return $"Speed Form {pokemonName}";
         // Burmy and Wormadam
         else if (form.Equals("-plant", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("burmy", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("wormadam", StringComparison.OrdinalIgnoreCase))))
            return $"Plant Cloak {pokemonName}";
         else if (form.Equals("-sand", StringComparison.OrdinalIgnoreCase))
            return $"Sand Cloak {pokemonName}";
         else if (form.Equals("-trash", StringComparison.OrdinalIgnoreCase))
            return $"Trash Cloak {pokemonName}";
         // Cherrim
         else if (form.Equals("-sunshine", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("cherrim", StringComparison.OrdinalIgnoreCase)))
            return $"Sunshine {pokemonName}";
         else if (form.Equals("-overcast", StringComparison.OrdinalIgnoreCase))
            return $"Overcast {pokemonName}";
         // Shellos and Gastrodon
         else if (form.Equals("-east", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("shellos", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("gastrodon", StringComparison.OrdinalIgnoreCase))))
            return $"East Sea {pokemonName}";
         else if (form.Equals("-west", StringComparison.OrdinalIgnoreCase))
            return $"West Sea {pokemonName}";
         // Rotom
         else if (form.Equals("-fan", StringComparison.OrdinalIgnoreCase))
            return $"Fan {pokemonName}";
         else if (form.Equals("-frost", StringComparison.OrdinalIgnoreCase))
            return $"Frost {pokemonName}";
         else if (form.Equals("-heat", StringComparison.OrdinalIgnoreCase))
            return $"Heat {pokemonName}";
         else if (form.Equals("-mow", StringComparison.OrdinalIgnoreCase))
            return $"Mow {pokemonName}";
         else if (form.Equals("-wash", StringComparison.OrdinalIgnoreCase))
            return $"Wash {pokemonName}";
         // Giratina
         else if (form.Equals("-altered", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("giratina", StringComparison.OrdinalIgnoreCase)))
            return $"Altered Form {pokemonName}";
         else if (form.Equals("-origin", StringComparison.OrdinalIgnoreCase))
            return $"Origin Form {pokemonName}";
         // Shayman
         else if (form.Equals("-land", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("shayman", StringComparison.OrdinalIgnoreCase)))
            return $"Land Form {pokemonName}";
         else if (form.Equals("-sky", StringComparison.OrdinalIgnoreCase))
            return $"Sky Form {pokemonName}";
         // Arceus
         else if (form.Equals("-normal", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("arceus", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Normal";
         else if (form.Equals("-bug", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Bug";
         else if (form.Equals("-dark", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dark";
         else if (form.Equals("-bug", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Bug";
         else if (form.Equals("-dragon", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dragon";
         else if (form.Equals("-electric", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Electric";
         else if (form.Equals("-fairy", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fairy";
         else if (form.Equals("-fighting", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("-fight", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fighting";
         else if (form.Equals("-fire", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fire";
         else if (form.Equals("-flying", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("-fly", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Flying";
         else if (form.Equals("-ghost", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ghost";
         else if (form.Equals("-grass", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Grass";
         else if (form.Equals("-ground", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ground";
         else if (form.Equals("-ice", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ice";
         else if (form.Equals("-poison", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Poison";
         else if (form.Equals("-psychic", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("-psy", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Psychic";
         else if (form.Equals("-rock", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Rock";
         else if (form.Equals("-steel", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Steel";
         else if (form.Equals("-water", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Water";
         // Basculin
         else if (form.Equals("-blue", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("basculin", StringComparison.OrdinalIgnoreCase)))
            return $"Blue Striped {pokemonName}";
         else if (form.Equals("-red", StringComparison.OrdinalIgnoreCase))
            return $"Red Striped {pokemonName}";
         // Darmanitan
         else if (form.Equals("-zen", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Zen Mode";
         else if (form.Equals("-galarzen", StringComparison.OrdinalIgnoreCase))
            return $"Galarian {pokemonName} Zen Mode";
         // Deerling and Sawsbuck
         else if (form.Equals("-summer", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("deerling", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("sawsbuck", StringComparison.OrdinalIgnoreCase))))
            return $"{pokemonName} Summer Form";
         else if (form.Equals("-spring", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Spring Form";
         else if (form.Equals("-winter", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Winter Form";
         else if (form.Equals("-autumn", StringComparison.OrdinalIgnoreCase) || form.Equals("-fall", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Autumn Form";
         // Tornadus, Thundurus, and Landorus
         else if (form.Equals("-incarnate", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("tornadus", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("thundurus", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("landorus", StringComparison.OrdinalIgnoreCase))))
            return $"Incarnate {pokemonName}";
         else if (form.Equals("-therian", StringComparison.OrdinalIgnoreCase))
            return $"Therian {pokemonName}";
         // Kyurem
         else if (form.Equals("-black", StringComparison.OrdinalIgnoreCase))
            return $"Black {pokemonName}";
         else if (form.Equals("-white", StringComparison.OrdinalIgnoreCase))
            return $"White {pokemonName}";
         // Keldeo
         else if (form.Equals("-resolute", StringComparison.OrdinalIgnoreCase))
            return $"Resolute {pokemonName}";
         // Meloetta
         else if (form.Equals("-aria", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("meloetta", StringComparison.OrdinalIgnoreCase)))
            return $"Aria {pokemonName}";
         else if (form.Equals("-pirouette", StringComparison.OrdinalIgnoreCase))
            return $"Pirouette {pokemonName}";
         return pokemonName;
      }

      /// <summary>
      /// Formats weather boosts as a string.
      /// </summary>
      /// <param name="weatherList">List of weather that boosts the type(s).</param>
      /// <returns>Weather for type(s) as a string.</returns>
      private static string FormatWeatherList(List<string> weatherList)
      {
         string weatherString = "";
         foreach (var weather in weatherList)
         {
            weatherString += $"{Emote.Parse(Environment.GetEnvironmentVariable($"{weather.Replace(' ', '_').ToUpper()}_EMOTE"))} ";
         }
         return weatherString;
      }

      /// <summary>
      /// Formats type relations as a string.
      /// </summary>
      /// <param name="relations">Dictionary of type relations for the type(s).</param>
      /// <returns>Type relations for type(s) as a string.</returns>
      private static string FormatTypeList(Dictionary<string, int> relations)
      {
         if (relations.Count == 0)
            return "-----";

         string relationString = "";
         foreach(var relation in relations)
         {
            double multiplier = TypeCalculator.CalcTypeEffectivness(relation.Value) * 100.0;
            string typeEmote = Emote.Parse(Environment.GetEnvironmentVariable($"{relation.Key.ToUpper()}_EMOTE")).ToString();
            relationString += $"{typeEmote} {relation.Key}: {multiplier}%\n";
         }
         return relationString;
      }

      /// <summary>
      /// Checks if a type is vaid.
      /// </summary>
      /// <param name="type">The type to check.</param>
      /// <returns>True if the type is valid, otherwise false.</returns>
      private static bool CheckValidType(string type)
      {
         return Environment.GetEnvironmentVariable($"{type.ToUpper()}_EMOTE") != null;
      }
   }
}