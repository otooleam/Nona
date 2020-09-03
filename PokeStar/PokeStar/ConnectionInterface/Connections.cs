﻿using System;
using System.Linq;
using System.Collections.Generic;
using DuoVia.FuzzyStrings;
using PokeStar.DataModels;
using PokeStar.Calculators;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Manages backend connections
   /// </summary>
   public class Connections
   {
      private static Connections connections;

      private static readonly string PokemonImageFolder = "PokemonImages";

      private readonly POGODatabaseConnector POGODBConnector;
      private readonly NONADatabaseConnector NONADBConnector;
      private List<string> PokemonNames;

      private readonly int NumSuggestions = 10;

      /// <summary>
      /// Creates a new Connections object.
      /// Private to implement the singleton design patturn.
      /// </summary>
      private Connections()
      {
         POGODBConnector = new POGODatabaseConnector(Global.POGODB_CONNECTION_STRING);
         NONADBConnector = new NONADatabaseConnector(Global.NONADB_CONNECTION_STRING);
         UpdateNameList();
      }

      /// <summary>
      /// Gets the current Connections instance.
      /// </summary>
      /// <returns>The Connections instance.</returns>
      public static Connections Instance()
      {
         if (connections == null)
         {
            connections = new Connections();
         }
         return connections;
      }

      /// <summary>
      /// Copy a file from PokemonImages to the location of the application.
      /// </summary>
      /// <param name="fileName">Name of file to copy.</param>
      public static void CopyFile(string fileName)
      {
         string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
         System.IO.File.Copy($"{location}\\{PokemonImageFolder}\\{fileName}", $"{location}\\{fileName}", true);
      }

      /// <summary>
      /// Delete a file from the location of the application.
      /// </summary>
      /// <param name="fileName">Name of file to delete.</param>
      public static void DeleteFile(string fileName)
      {
         string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
         System.IO.File.Delete($"{location}\\{fileName}");
      }

      /// <summary>
      /// Converts a pokemon's name to its file name.
      /// </summary>
      /// <param name="pokemonName">Name of pokemon.</param>
      /// <returns>Pokemon picture file name.</returns>
      public static string GetPokemonPicture(string pokemonName)
      {
         pokemonName = pokemonName.Replace(" ", "_");
         pokemonName = pokemonName.Replace(".", "");
         pokemonName = pokemonName.Replace("\'", "");
         pokemonName = pokemonName.Replace("?", "QU");
         return pokemonName + ".png";
      }

      /// <summary>
      /// Gets a list of current raid bosses from The Silph Road.
      /// </summary>
      /// <param name="tier">Tier of bosses to get.</param>
      /// <returns>List of current raid bosses in the given tier.</returns>
      public static List<string> GetBossList(short tier)
      {
         return SilphData.GetRaidBossesTier(tier);
      }

      /// <summary>
      /// Updates the list of pokemon to use for the fuzzy search.
      /// </summary>
      public void UpdateNameList()
      {
         PokemonNames = POGODBConnector.GetNameList();
      }

      /// <summary>
      /// Searches for the closest pokemon names to a given name
      /// </summary>
      /// <param name="name">Name to check pokemon names against.</param>
      /// <returns>List of the closest pokemon names.</returns>
      public List<string> FuzzyNameSearch(string name)
      {
         Dictionary<string, double> fuzzy = new Dictionary<string, double>();
         foreach (string pokemonName in PokemonNames)
         {
            fuzzy.Add(pokemonName, pokemonName.FuzzyMatch(name));
         }
         List<KeyValuePair<string, double>> myList = fuzzy.ToList();
         myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
         fuzzy = myList.ToDictionary(x => x.Key, x => x.Value);
         return fuzzy.Keys.Take(NumSuggestions).ToList();
      }

      /// <summary>
      /// Gets a given raid boss.
      /// Calculates CP values relevant to a raid boss. This includes
      /// min and max cps and weather boosted min and max cps.
      /// </summary>
      /// <param name="raidBossName">Name of the raid boss.</param>
      /// <returns>The raid boss coresponding to the name, otherwise null.</returns>
      public RaidBoss GetRaidBoss(string raidBossName)
      {
         if (raidBossName == null)
         {
            return null;
         }

         string name = ReformatName(raidBossName);
         RaidBoss raidBoss = POGODBConnector.GetRaidBoss(name);
         if (raidBoss == null)
         {
            return null;
         }

         Tuple<Dictionary<string, int>, Dictionary<string, int>> typeRelations = GetTypeDefenseRelations(raidBoss.Type);
         raidBoss.Weakness = typeRelations.Item2.Keys.ToList();
         raidBoss.Resistance = typeRelations.Item1.Keys.ToList();
         raidBoss.Weather = GetWeather(raidBoss.Type);

         raidBoss.CPLow = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, 
            Global.MIN_SPECIAL_IV, Global.RAID_LEVEL);

         raidBoss.CPHigh = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.RAID_LEVEL);

         raidBoss.CPLowBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, 
            Global.RAID_LEVEL + Global.WEATHER_BOOST);

         raidBoss.CPHighBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, 
            Global.RAID_LEVEL + Global.WEATHER_BOOST);

         return raidBoss;
      }

      /// <summary>
      /// Gets a given pokemon.
      /// Calculates max CP value of the pokemon.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon.</param>
      /// <returns>The pokemon coresponding to the name, otherwise null.</returns>
      public Pokemon GetPokemon(string pokemonName)
      {
         if (pokemonName == null)
         {
            return null;
         }

         string name = ReformatName(pokemonName);
         Pokemon pokemon = POGODBConnector.GetPokemon(name);
         if (pokemon == null)
         {
            return null;
         }

         pokemon.Forms = POGODBConnector.GetPokemonByNumber(pokemon.Number);

         Tuple<Dictionary<string, int>, Dictionary<string, int>> typeRelations = GetTypeDefenseRelations(pokemon.Type);
         pokemon.Weakness = typeRelations.Item2.Keys.ToList();
         pokemon.Resistance = typeRelations.Item1.Keys.ToList();
         pokemon.Weather = GetWeather(pokemon.Type);
         pokemon.FastMove = POGODBConnector.GetMoves(name, true);
         pokemon.ChargeMove = POGODBConnector.GetMoves(name, false, pokemon.Shadow);
         pokemon.Counter = POGODBConnector.GetCounters(name);

         foreach (Counter counter in pokemon.Counter)
         {
            counter.FastAttack = POGODBConnector.GetPokemonMove(counter.Name, counter.FastAttack.Name);
            counter.ChargeAttack = POGODBConnector.GetPokemonMove(counter.Name, counter.ChargeAttack.Name);
         }

         pokemon.CPMax = CPCalculator.CalcCPPerLevel(
            pokemon.Attack, pokemon.Defense, pokemon.Stamina,
            Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.MAX_LEVEL);

         return pokemon;
      }

      /// <summary>
      /// Calculates all of the relevant CP valus of a pokemon. This
      /// includes the raid, quest, hatch, and wild perfect IV values.
      /// </summary>
      /// <param name="pokemon">Reference to pokemon to calc cp for.</param>
      public static void CalcAllCP(ref Pokemon pokemon)
      {
         if (pokemon != null)
         {
            pokemon.CPBestBuddy = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, 
               Global.MAX_LEVEL + Global.BUDDY_BOOST);

            pokemon.CPRaidMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, 
               Global.MIN_SPECIAL_IV, Global.RAID_LEVEL);

            pokemon.CPRaidMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.RAID_LEVEL);

            pokemon.CPRaidBoostedMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV,
               Global.RAID_LEVEL + Global.WEATHER_BOOST);

            pokemon.CPRaidBoostedMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV,
               Global.RAID_LEVEL + Global.WEATHER_BOOST);

            pokemon.CPQuestMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV,
               Global.MIN_SPECIAL_IV, Global.QUEST_LEVEL);

            pokemon.CPQuestMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.QUEST_LEVEL);

            pokemon.CPHatchMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV,
               Global.MIN_SPECIAL_IV, Global.HATCH_LEVEL);

            pokemon.CPHatchMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.HATCH_LEVEL);

            for (int level = Global.MIN_WILD_LEVEL; level <= Global.MAX_WILD_LEVEL; level++)
               pokemon.CPWild.Add(CPCalculator.CalcCPPerLevel(
                  pokemon.Attack, pokemon.Defense, pokemon.Stamina,
                  Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, level));
         }
      }

      /// <summary>
      /// Gets all pokemon that have a given number.
      /// </summary>
      /// <param name="pokemonNumber">Pokemon number to find</param>
      /// <returns>List of pokemon with the given number.</returns>
      public List<string> GetPokemonByNumber(int pokemonNumber)
      {
         return POGODBConnector.GetPokemonByNumber(pokemonNumber);
      }

      /// <summary>
      /// Reformats the name from user input to the POGO database format.
      /// </summary>
      /// <param name="originalName">User input name.</param>
      /// <returns>Name formated for the POGO database</returns>
      private string ReformatName(string originalName)
      {
         int index = originalName.IndexOf('\'');
         return index == -1 ? originalName : originalName.Insert(index, "\'");
      }

      /// <summary>
      /// Gets defensive type relations for a pokemon's type.
      /// Separates weaknesses and resistances.
      /// </summary>
      /// <param name="types">List of pokemon types.</param>
      /// <returns>Dictionaries of types and modifiers.</returns>
      public Tuple<Dictionary<string, int>, Dictionary<string, int>> GetTypeDefenseRelations(List<string> types)
      {
         Dictionary<string, int> allRelations = POGODBConnector.GetTypeDefenseRelations(types);
         return new Tuple<Dictionary<string, int>, Dictionary<string, int>>(
            allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => v.Value),
            allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => v.Value)
         );
      }

      /// <summary>
      /// Gets offensive type relations for a move's type.
      /// Separates super and not very effective moves.
      /// </summary>
      /// <param name="type">Move type.</param>
      /// <returns>Dictionaries of types and modifiers.</returns>
      public Tuple<Dictionary<string, int>, Dictionary<string, int>> GetTypeAttackRelations(string type)
      {
         Dictionary<string, int> allRelations = POGODBConnector.GetTypeAttackRelations(type);
         return new Tuple<Dictionary<string, int>, Dictionary<string, int>> (
            allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => v.Value),
            allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => v.Value)
         );
      }

      /// <summary>
      /// Gets all weather that boosts the given types.
      /// </summary>
      /// <param name="types">List of types to get weather for.</param>
      /// <returns>List of weather that boosts the givent types.</returns>
      public List<string> GetWeather(List<string> types)
      {
         return POGODBConnector.GetWeather(types);
      }

      /// <summary>
      /// Adds settings to the database for a guild.
      /// </summary>
      /// <param name="guild">Id of guild to add settings for.</param>
      public void InitSettings(ulong guild)
      {
         NONADBConnector.AddSettings(guild);
      }

      /// <summary>
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of guild to get prefix of.</param>
      /// <returns>Prefix registerd for the guild.</returns>
      public string GetPrefix(ulong guild)
      {
         return NONADBConnector.GetPrefix(guild);
      }

      /// <summary>
      /// Check if setup is completed for a guild.
      /// </summary>
      /// <param name="guild">Id of guild to get setup status of.</param>
      /// <returns>True if setup is complete for the guild, otherwise false.</returns>
      public bool GetSetupComplete(ulong guild)
      {
         return NONADBConnector.GetSetupComplete(guild);
      }

      /// <summary>
      /// Updates the prefix of the guild.
      /// </summary>
      /// <param name="guild">Id of the guild to set the prefix for.</param>
      /// <param name="prefix">New prefix value.</param>
      public void UpdatePrefix(ulong guild, string prefix)
      {
         NONADBConnector.UpdatePrefix(guild, prefix);
      }

      /// <summary>
      /// Marks a guild setup as complete
      /// </summary>
      /// <param name="guild">Id of the guild to complete setup for.</param>
      public void CompleteSetup(ulong guild)
      {
         NONADBConnector.CompleteSetup(guild);
      }

      /// <summary>
      /// Deletes the settings of a guild.
      /// </summary>
      /// <param name="guild">Id of guild to delete settings of.</param>
      public void DeleteSettings(ulong guild)
      {
         NONADBConnector.DeleteSettings(guild);
      }

      /// <summary>
      /// Gets the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild that has the channel.</param>
      /// <param name="channel">Id of the channel that the registration is for.</param>
      /// <returns>Registration string for the channel, otherwise null.</returns>
      public string GetRegistration(ulong guild, ulong channel)
      {
         string registration = NONADBConnector.GetRegistration(guild, channel);
         return registration ?? null;
      }

      /// <summary>
      /// Updates the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild that has the channel.</param>
      /// <param name="channel">Id of the channel to update the registration of.</param>
      /// <param name="register">New registration value.</param>
      public void UpdateRegistration(ulong guild, ulong channel, string register)
      {
         if (GetRegistration(guild, channel) == null)
         {
            NONADBConnector.AddRegistration(guild, channel, register);
         }
         else
         {
            NONADBConnector.UpdateRegistration(guild, channel, register);
         }
      }

      /// <summary>
      /// Deletes the registration of a guild or channel.
      /// If no channel is given then all registrations for the guild are
      /// deleted.
      /// </summary>
      /// <param name="guild">Id of the guild that has the channel, or to remove registrations from.</param>
      /// <param name="channel">Id of the channel to remove the registration from.</param>
      public void DeleteRegistration(ulong guild, ulong? channel = null)
      {
         if (channel == null)
            NONADBConnector.DeleteAllRegistration(guild);
         else
            NONADBConnector.DeleteRegistration(guild, (ulong)channel);
      }
   }
}