﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using HtmlAgilityPack;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Scrapes The Silph Road raid page to get current raid bosses.
   /// </summary>
   public static class SilphData
   {
      /// <summary>
      /// Values used to scrape raid bosses.
      /// </summary>
      private static Uri RaidBossUrl { get; } = new Uri("https://thesilphroad.com/raid-bosses");
      private const string RaidBossHTMLPattern = "//*[@class = 'col-md-4']";

      /// <summary>
      /// Values used to scrape egg pools.
      /// </summary>
      private static Uri EggUrl { get; } = new Uri("https://thesilphroad.com/egg-distances");
      private const string EggHTMLPattern = "//*[@class='tab-content']";

      /// <summary>
      /// Values used to scrape rocket line ups.
      /// </summary>
      private static Uri RocketUrl { get; } = new Uri("https://thesilphroad.com/rocket-invasions");
      private const string RocketHTMLPattern = "//*[@class='lineupGroup normalGroup']";
      private const string RocketLeaderHTMLPattern = "//*[@class='lineupGroup specialGroup']";

      private const int SILPH_GUILD_NUM = 0;

      /// <summary>
      /// Gets a list of all current raid bosses.
      /// The list is dependent on the current raid bosses on 
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of current raid bosses.</returns>
      public static Dictionary<int, List<string>> GetRaidBosses()
      {
         int tier = -1;
         bool tierStart = false;
         bool nextInTier = false;
         bool megaTierStarted = false;
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RaidBossUrl);
         HtmlNodeCollection bosses = doc.DocumentNode.SelectNodes(RaidBossHTMLPattern);

         Dictionary<int, List<string>> raidBossList = new Dictionary<int, List<string>>();
         foreach (HtmlNode col in bosses)
         {
            string[] words = col.InnerText.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            foreach (string word in words)
            {
               string line = word.Trim();
               int firstSpace = line.IndexOf(' ');
               string checkTier = firstSpace != -1 ? line.Substring(0, firstSpace) : "";

               if (checkTier.Equals(Global.RAID_STRING_MEGA, StringComparison.OrdinalIgnoreCase) && !megaTierStarted)
               {
                  tier = Global.MEGA_RAID_TIER;
                  raidBossList.Add(tier, new List<string>());
                  tierStart = true;
                  nextInTier = false;
                  megaTierStarted = true;
               }
               else if (checkTier.Equals(Global.RAID_STRING_EX, StringComparison.OrdinalIgnoreCase))
               {
                  tier = Global.EX_RAID_TIER;
                  raidBossList.Add(tier, new List<string>());
                  tierStart = true;
                  nextInTier = false;
               }
               else if (checkTier.Equals(Global.RAID_STRING_TIER, StringComparison.OrdinalIgnoreCase))
               {
                  tier = Convert.ToInt32(line.Trim().Substring(Global.RAID_STRING_TIER.Length));
                  raidBossList.Add(tier, new List<string>());
                  tierStart = true;
                  nextInTier = false;
               }
               else if (line.Equals("8+", StringComparison.OrdinalIgnoreCase))
               {
                  nextInTier = true;
               }
               else if (tierStart)
               {
                  raidBossList[tier].Add(ReformatName(line));
                  tierStart = false;
               }
               else if (nextInTier)
               {
                  raidBossList[tier].Add(ReformatName(line));
                  nextInTier = false;
               }
            }
         }
         return raidBossList;
      }

      /// <summary>
      /// Gets a list of all current egg pools.
      /// Thie list is dependent on the current egg pools on
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of Pokémon currently in eggs.</returns>
      public static Dictionary<int, List<string>> GetEggs()
      {
         int eggCategory = 0;
         int pokemonNameOffset = 0;
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(EggUrl);
         HtmlNodeCollection eggs = doc.DocumentNode.SelectNodes(EggHTMLPattern);

         Dictionary<int, List<string>> eggList = new Dictionary<int, List<string>>();

         foreach (HtmlNode col in eggs)
         {
            string[] words = col.InnerText.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            foreach (string word in words)
            {
               pokemonNameOffset--;
               string line = word.Trim();
               if (line.Contains("KM"))
               {
                  eggCategory = Global.EGG_TIER_TITLE[line];
                  eggList.Add(eggCategory, new List<string>());
               }
               else if (line.IndexOf('#') != -1)
               {
                  pokemonNameOffset = 2;
               }
               else if (pokemonNameOffset == 0)
               {
                  eggList[eggCategory].Add(ReformatName(line));
               }
            }
         }
         return eggList;
      }

      /// <summary>
      /// Gets a list of all current Rocket Grunts.
      /// Thie list is dependent on the current rocket grunts on
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of line ups used by rocket grunts.</returns>
      public static Dictionary<string, Rocket> GetRockets()
      {
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RocketUrl);
         HtmlNodeCollection rockets = doc.DocumentNode.SelectNodes(RocketHTMLPattern);

         Dictionary<string, Rocket> rocketList = new Dictionary<string, Rocket>();

         foreach (HtmlNode col in rockets)
         {
            int slot = 0;
            string[] words = col.InnerText.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim()) &&
                                                                  x.Trim().IndexOf('%') == -1 &&
                                                                  x.Trim().IndexOf(':') == -1 &&
                                                                  x.Trim().IndexOf('/') == -1
                                                                  ).ToArray();
            Rocket rocket = new Rocket();
            string type = "";

            for (int i = 0; i < words.Length; i++)
            {
               string line = words[i] = words[i].Trim().Replace("&#39;", "\'");
               int numIndex = line.IndexOf('#');
               if (numIndex != -1)
               {
                  if (slot == 0)
                  {
                     type = words[i - 1];

                     StringBuilder phrase = new StringBuilder();
                     for (int j = 0; j < i - 1; j++)
                     {
                        phrase.AppendLine(words[j]);
                     }
                     rocket.SetGrunt(type, phrase.ToString());
                  }

                  slot = Convert.ToInt32(line.Substring(numIndex + 1));
               }
               else if (slot != 0)
               {
                  if (line.Length != 1 && line.IndexOf('%') == -1 && line.IndexOf('/') == -1)
                  {
                     rocket.Slots[slot - 1].Add(line);
                  }
               }
            }
            rocketList.Add(type, rocket);
         }
         return rocketList;
      }

      /// <summary>
      /// Gets a list of all current Rocket Grunts.
      /// Thie list is dependent on the current rocket leaders on
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of line ups used by rocket leaders.</returns>
      public static Dictionary<string, Rocket> GetRocketLeaders()
      {
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RocketUrl);
         HtmlNodeCollection leaders = doc.DocumentNode.SelectNodes(RocketLeaderHTMLPattern);

         Dictionary<string, Rocket> leaderList = new Dictionary<string, Rocket>();

         foreach (HtmlNode col in leaders)
         {
            int slot = 0;
            string[] words = col.InnerText.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim()) &&
                                                                  x.Trim().IndexOf('%') == -1 &&
                                                                  x.Trim().IndexOf(':') == -1 &&
                                                                  x.Trim().IndexOf('/') == -1
                                                                  ).ToArray();
            Rocket rocket = new Rocket();
            rocket.SetLeader(words[0].Trim());

            foreach (string word in words)
            {
               string line = word.Trim();
               int numIndex = line.IndexOf('#');
               if (numIndex != -1)
               {
                  slot = Convert.ToInt32(line.Substring(numIndex + 1));
               }
               else if (slot != 0)
               {
                  if (line.Length != 1 && line.IndexOf('%') == -1 && line.IndexOf('/') == -1)
                  {
                     rocket.Slots[slot - 1].Add(line);
                  }
               }
            }
            leaderList.Add(rocket.Name, rocket);
         }
         return leaderList;
      }

      /// <summary>
      /// Reformats names to comply with the database names.
      /// </summary>
      /// <param name="name">Name read from The Silph Road.</param>
      /// <returns>Name formated for the database.</returns>
      private static string ReformatName(string name)
      {
         string silphName = name.IndexOf('’') != -1 ? name.Replace('’', '\'') : name;
         return Connections.Instance().GetPokemonWithNickname(SILPH_GUILD_NUM, silphName) ?? name;
      }
   }
}