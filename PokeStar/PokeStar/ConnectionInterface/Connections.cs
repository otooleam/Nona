using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using DuoVia.FuzzyStrings;
using PokeStar.DataModels;
using PokeStar.Calculators;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Manages backend connections to the databases.
   /// </summary>
   public class Connections
   {
      private static Connections connections;

      private const string PokemonImageFolder = "PokemonImages";

      private readonly POGODatabaseConnector POGODBConnector;
      private readonly NONADatabaseConnector NONADBConnector;

      private List<string> PokemonNames;
      private List<string> MoveNames;
      private Dictionary<int, List<string>> RaidBosses;
      private Dictionary<int, List<string>> Eggs;
      private Dictionary<string, Rocket> Rockets;

      /// <summary>
      /// Creates a new Connections object.
      /// Private to implement the singleton design patturn.
      /// </summary>
      private Connections()
      {
         POGODBConnector = new POGODatabaseConnector(Global.POGO_DB_CONNECTION_STRING);
         NONADBConnector = new NONADatabaseConnector(Global.NONA_DB_CONNECTION_STRING);
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
      /// <param name="fileName">Name of the file.</param>
      public static void CopyFile(string fileName)
      {
         System.IO.File.Copy($"{Global.PROGRAM_PATH}\\{PokemonImageFolder}\\{fileName}", $"{Global.PROGRAM_PATH}\\{fileName}", true);
      }

      /// <summary>
      /// Delete a file from the location of the application.
      /// </summary>
      /// <param name="fileName">Name of the file.</param>
      public static void DeleteFile(string fileName)
      {
         System.IO.File.Delete($"{Global.PROGRAM_PATH}\\{fileName}");
      }

      /// <summary>
      /// Converts a Pokémon's name to its file name.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Pokémon picture file name.</returns>
      public static string GetPokemonPicture(string pokemonName)
      {
         pokemonName = pokemonName.Replace("’", "");
         pokemonName = pokemonName.Replace("`", "");
         pokemonName = pokemonName.Replace(" ", "_");
         pokemonName = pokemonName.Replace("-", "_");
         pokemonName = pokemonName.Replace(".", "");
         pokemonName = pokemonName.Replace("%", "");
         pokemonName = pokemonName.Replace("\'", "");
         pokemonName = pokemonName.Replace("?", "QU");
         pokemonName = pokemonName.Replace("!", "EX");
         return pokemonName + ".png";
      }

      /// <summary>
      /// Gets a Pokémon by its picture file.
      /// </summary>
      /// <param name="fileName">Name of the file.</param>
      /// <returns>Pokémon name.</returns>
      public static string GetPokemonFromPicture(string fileName)
      {
         Dictionary<string, string> ChangedPokemon = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
         {
            ["Farfetchd"]          = "Farfetch'd",
            ["Galarian_Farfetchd"] = "Galarian Farfetch'd",
            ["Galarian_Mr_Mime"]   = "Galarian Mr. Mime",
            ["Ho_Oh"]              = "Ho-Oh",
            ["Mime_Jr"]            = "Mime Jr.",
            ["Mr_Mime"]            = "Mr. Mime",
            ["Mr_Rime"]            = "Mr. Rime",
            ["Porygon_Z"]          = "Porygon-Z",
            ["Sirfetchd"]          = "Sirfetch'd",
            ["Unown_EX"]           = "Unown !",
            ["Unown_QU"]           = "Unown ?",
            ["Zygarde_10_Form"]    = "Zygarde 10% Form",
            ["Zygarde_50_Form"]    = "Zygarde 50% Form",
         };

         return ChangedPokemon.ContainsKey(fileName) ? ChangedPokemon[fileName] : fileName.Replace('_', ' ');
      }

      /// <summary>
      /// Gets raid difficulty table.
      /// </summary>
      /// <returns>List of difficulties and definitions.</returns>
      public static Dictionary<string, string> GetRaidDifficultyTable()
      {
         return SilphData.GetRaidBossDifficultyTable();
      }

      /// <summary>
      /// Get the full list of raid bosses.
      /// </summary>
      /// <returns>List of raid bosses sorted by tier.</returns>
      public Dictionary<int, List<string>> GetFullBossList()
      {
         return RaidBosses;
      }

      /// <summary>
      /// Gets a list of current eggs for a given tier.
      /// </summary>
      /// <param name="tier">Tier of eggs.</param>
      /// <returns>List of current eggs for the tier.</returns>
      public List<string> GetEggList(int tier)
      {
         return Eggs[tier];
      }

      /// <summary>
      /// Gets the rocket of a specific type.
      /// </summary>
      /// <param name="type">Type of rocket.</param>
      /// <returns>Rocket of the given type.</returns>
      public Rocket GetRocket(string type)
      {
         return Rockets.ContainsKey(type) ? Rockets[type] : null;
      }

      /// <summary>
      /// Gets all valid types of Rockets.
      /// </summary>
      /// <returns>List of Rocket types.</returns>
      public List<string> GetRocketTypes()
      {
         return Rockets.Keys.ToList();
      }

      /// <summary>
      /// Updates the list of Pokémon to use for the fuzzy search.
      /// Only needs to be ran when a Pokémon name has changed.
      /// </summary>
      public void UpdatePokemonNameList()
      {
         PokemonNames = POGODBConnector.GetPokemonNameList();
      }

      /// <summary>
      /// Updates the list of Moves to use for the fuzzy search.
      /// Only needs to be ran when a move name has changed.
      /// </summary>
      public void UpdateMoveNameList()
      {
         MoveNames = POGODBConnector.GetMoveNameList();
      }

      /// <summary>
      /// Updates the list of raidbosses.
      /// </summary>
      public void UpdateRaidBossList()
      {
         Dictionary<int, List<string>> bosses = SilphData.GetRaidBosses();

         RaidBosses = bosses ?? new Dictionary<int, List<string>>();
      }

      /// <summary>
      /// Updates the list of eggs.
      /// </summary>
      public void UpdateEggList()
      {
         Dictionary<int, List<string>> eggs = SilphData.GetEggs();

         Eggs = eggs ?? new Dictionary<int, List<string>>();
      }

      /// <summary>
      /// Updates the list of current Team Rockets.
      /// </summary>
      public void UpdateRocketList()
      {
         Dictionary<string, Rocket> leaders = SilphData.GetRocketLeaders();
         Dictionary<string, Rocket> grunts = SilphData.GetRockets();

         if (leaders != null && grunts != null)
         {
            Rockets = grunts.Union(leaders).ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
         }
         else if (leaders != null)
         {
            Rockets = leaders;
         }
         else if (grunts != null)
         {
            Rockets = grunts;
         }
         else
         {
            Rockets = new Dictionary<string, Rocket>();
         }
      }

      /// <summary>
      /// Runs updates to the system read from the Silph road.
      /// </summary>
      /// <param name="guilds">List of guilds the bot is currently in.</param>
      /// <returns>Completed Task.</returns>
      public async Task RunSilphUpdate(List<SocketGuild> guilds)
      {
         UpdatePokemonNameList();
         UpdateMoveNameList();
         UpdateEggList();
         UpdateRocketList();

         Dictionary<int, List<string>> newBosses = SilphData.GetRaidBosses();

         if (newBosses != null)
         {

            bool bossesChanged = RaidBosses == null || RaidBosses.Keys.Count != newBosses.Keys.Count || !RaidBosses.Keys.All(newBosses.Keys.Contains);

            foreach (int tier in newBosses.Keys)
            {
               bossesChanged = bossesChanged || RaidBosses[tier].Count != newBosses[tier].Count || !RaidBosses[tier].All(newBosses[tier].Contains);
            }

            if (bossesChanged)
            {
               if (SilphData.GetRaidBossesConfirmed())
               {
                  UpdateRaidBosses(guilds);
               }
               else
               {
                  SocketGuild emoteServer = guilds.FirstOrDefault(x => x.Name.Equals(Global.EMOTE_SERVER, StringComparison.OrdinalIgnoreCase));
                  GuildEmote[] previousEmotes = emoteServer.Emotes.ToArray();

                  List<string> topBosses = newBosses[Global.MEGA_RAID_TIER].Union(newBosses[Global.LEGENDARY_RAID_TIER]).ToList();
                  bool updateTopBosses = false;

                  foreach (string boss in topBosses)
                  {
                     string fileName = GetPokemonPicture(boss);
                     string emoteName = fileName.Remove(fileName.Length - 4);
                     updateTopBosses = updateTopBosses || (previousEmotes.FirstOrDefault(x => x.Name.Equals(emoteName)) == null);
                  }
                  if (updateTopBosses)
                  {
                     UpdateRaidBosses(guilds);
                  }
                  else
                  {
                     UpdateRaidBossList();
                  }
               }
            }
         }
         Global.INIT_COMPLETE = true;
      }

      public async Task UpdateRaidBosses(List<SocketGuild> guilds)
      {
         UpdateRaidBossList();
         SocketGuild emoteServer = guilds.FirstOrDefault(x => x.Name.Equals(Global.EMOTE_SERVER, StringComparison.OrdinalIgnoreCase));
         GuildEmote[] previousEmotes = emoteServer.Emotes.ToArray();
         bool msgChange = false;

         foreach (KeyValuePair<int, List<string>> tier in RaidBosses)
         {
            foreach (string boss in tier.Value)
            {
               string fileName = GetPokemonPicture(boss);
               string emoteName = fileName.Remove(fileName.Length - 4);
               if (previousEmotes.FirstOrDefault(x => x.Name.Equals(emoteName)) == null)
               {
                  CopyFile(fileName);
                  Image img = new Image(fileName);
                  await emoteServer.CreateEmoteAsync(emoteName, img);
                  img.Dispose();
                  DeleteFile(fileName);
                  msgChange = true;
               }
            }
         }

         foreach (GuildEmote emote in previousEmotes)
         {
            bool inCurrentList = false;
            foreach (KeyValuePair<int, List<string>> tier in RaidBosses)
            {
               foreach (string boss in tier.Value)
               {
                  string fileName = GetPokemonPicture(boss);
                  string emoteName = fileName.Remove(fileName.Length - 4);
                  inCurrentList = inCurrentList || emote.Name.Equals(emoteName);
               }
            }
            if (!inCurrentList)
            {
               await emoteServer.DeleteEmoteAsync(emote);
               msgChange = true;
            }
         }
         if (msgChange)
         {
            Dictionary<ulong, ulong> channels = Instance().GetNotificationChannels();

            foreach (KeyValuePair<ulong, ulong> chan in channels)
            {
               SocketGuild guild = guilds.FirstOrDefault(x => x.Id == chan.Key);
               ISocketMessageChannel channel = (ISocketMessageChannel)guild.Channels.FirstOrDefault(x => x.Id == chan.Value);
               await ClearNotifyMessage(guild, channel, Instance().GetNotificationMessages(guild.Id, channel.Id));
               await SetNotifyMessage(guild, channel, emoteServer.Emotes.ToArray());
            }
         }
      }

      /// <summary>
      /// Checks if a message is a raid notification message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid notification message, otherwise false.</returns>
      public static bool IsNotifyMessage(ulong id)
      {
         return Instance().CheckNotificationMessage(id);
      }

      /// <summary>
      /// Handles a reaction added to a raid notification message.
      /// </summary>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="guild">Guild the reaction was made in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task NotifyMessageReactionAddedHandle(SocketReaction reaction, SocketGuild guild)
      {
         SocketRole role = guild.Roles.FirstOrDefault(x => x.Name.Equals(reaction.Emote.Name, StringComparison.OrdinalIgnoreCase));

         if (role != null)
         {
            SocketGuildUser user = guild.Users.FirstOrDefault(x => x.Id == reaction.User.Value.Id);
            await user.AddRoleAsync(role);
         }
      }

      /// <summary>
      /// Handles a reaction removed to a raid notification message.
      /// </summary>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="guild">Guild the reaction was made in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task NotifyMessageReactionRemovedHandle(SocketReaction reaction, SocketGuild guild)
      {
         SocketRole role = guild.Roles.FirstOrDefault(x => x.Name.Equals(reaction.Emote.Name, StringComparison.OrdinalIgnoreCase));

         if (role != null)
         {
            SocketGuildUser user = guild.Users.FirstOrDefault(x => x.Id == reaction.User.Value.Id);
            await user.RemoveRoleAsync(role);
         }
      }

      /// <summary>
      /// Sets values for a raid notification message.
      /// </summary>
      /// <param name="guild">Guild to set notify message.</param>
      /// <param name="channel">Registered channel.</param>
      /// <param name="emotes">Emotes to represent roles.</param>
      /// <returns></returns>
      public static async Task SetNotifyMessage(SocketGuild guild, ISocketMessageChannel channel, Emote[] emotes)
      {
         foreach (GuildEmote emote in emotes)
         {
            await guild.CreateRoleAsync(emote.Name, null, Global.ROLE_COLOR_RAID, false, true, null);
         }

         StringBuilder messages = new StringBuilder();

         var initialMessage = await ResponseMessage.SendInfoMessage(channel,
            "React to the following messages to be notified when a specific raid is called.\n" +
            "When the raid bosses change, your role will be removed and you will have to re-select desired bosses.\n" +
            "If you no longer wish to be notified for a boss, re-react for the desired boss.");
         messages.AppendLine($"{initialMessage.Id},");

         StringBuilder emoteMsg = new StringBuilder();

         int count;
         for (count = 0; count < +emotes.Length; count++)
         {
            emoteMsg.AppendLine($"{emotes[count]} {emotes[count].Name}");

            if (count % Global.LIMIT_RAID_BOSS_NOTIFY == Global.LIMIT_RAID_BOSS_NOTIFY - 1)
            {
               IUserMessage emoteMessage = await ResponseMessage.SendInfoMessage(channel, emoteMsg.ToString());
               await emoteMessage.AddReactionsAsync(emotes.Skip(count - (Global.LIMIT_RAID_BOSS_NOTIFY - 1)).Take(Global.LIMIT_RAID_BOSS_NOTIFY).ToArray());
               messages.AppendLine($"{emoteMessage.Id},");
               emoteMsg.Clear();
            }
         }
         IUserMessage finalEmoteMessage = await ResponseMessage.SendInfoMessage(channel, emoteMsg.ToString());
         await finalEmoteMessage.AddReactionsAsync(emotes.Skip(count - (count % Global.LIMIT_RAID_BOSS_NOTIFY)).Take(Global.LIMIT_RAID_BOSS_NOTIFY).ToArray());
         messages.AppendLine($"{finalEmoteMessage.Id},");

         Instance().UpdateNotificationMessage(guild.Id, channel.Id, messages.ToString().Remove(messages.Length - 1, 1));
      }

      /// <summary>
      /// Clears values for a raid notification message.
      /// </summary>
      /// <param name="guild">Guild to clear notify message.</param>
      /// <param name="channel">Id of registered channel.</param>
      /// <param name="emotes">Emotes that represent roles.</param>
      /// <returns></returns>
      public static async Task ClearNotifyMessage(SocketGuild guild, ISocketMessageChannel channel, List<ulong> messages)
      {
         List<SocketRole> roles = guild.Roles.Where(x => x.Color.Equals(Global.ROLE_COLOR_RAID)).ToList();
         foreach (SocketRole role in roles)
         {
            await role.DeleteAsync();
         }

         foreach (ulong message in messages)
         {
            await channel.DeleteMessageAsync(message);
         }

         Instance().ClearNotificationMessage(guild.Id, channel.Id);
      }

      /// <summary>
      /// Searches for the closest Pokémon by name.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of closest Pokémon names.</returns>
      public List<string> SearchPokemon(string pokemonName)
      {
         return FuzzyNameSearch(pokemonName, PokemonNames);
      }

      /// <summary>
      /// Searches for the closests moves by name.
      /// </summary>
      /// <param name="moveName">Name of the move.</param>
      /// <returns>List of closest move names.</returns>
      public List<string> SearchMove(string moveName)
      {
         return FuzzyNameSearch(moveName, MoveNames);
      }

      /// <summary>
      /// Searches for the closest guild POI by name.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poi">Name of the POI.</param>
      /// <returns>List of closest guild POI names.</returns>
      public List<string> SearchPOI(ulong guild, string poi)
      {
         return FuzzyNameSearch(poi, NONADBConnector.GetGuildPOIs(guild));
      }

      /// <summary>
      /// Searches for the closest strings in a list of strings.
      /// </summary>
      /// <param name="search">Value to search for.</param>
      /// <param name="dir">List of strings to search in.</param>
      /// <returns>List of the closest strings from the list.</returns>
      private static List<string> FuzzyNameSearch(string search, List<string> dir)
      {
         Dictionary<string, double> fuzzy = new Dictionary<string, double>();
         foreach (string value in dir)
         {
            fuzzy.Add(value, value.FuzzyMatch(search));
         }
         List<KeyValuePair<string, double>> myList = fuzzy.ToList();
         myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
         fuzzy = myList.ToDictionary(x => x.Key, x => x.Value);
         return fuzzy.Keys.Take(Global.SELECTION_EMOJIS.Length).ToList();
      }

      /// <summary>
      /// Gets a given Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>The Pokémon coresponding to the name, otherwise null.</returns>
      public Pokemon GetPokemon(string pokemonName)
      {
         return pokemonName == null ? null : POGODBConnector.GetPokemon(ReformatName(pokemonName));
      }

      /// <summary>
      /// Gets needed information for a raid boss.
      /// </summary>
      /// <param name="pokemon">Reference to a Pokémon.</param>
      public void GetRaidBoss(ref Pokemon pokemon)
      {
         GetPokemonStats(ref pokemon);
         GetPokemonCP(ref pokemon);
         GetPokemonCounter(ref pokemon);
         Dictionary<string, int> difficulty = SilphData.GetRaidBossDifficulty(pokemon.Name);
         Dictionary<string, string> definition = SilphData.GetRaidBossDifficultyTable();
         pokemon.Difficulty = new Dictionary<string, string>();

         if (difficulty != null && definition != null)
         {
            foreach (KeyValuePair<string, int> party in difficulty)
            {
               pokemon.Difficulty.Add(party.Key, definition.ElementAt(party.Value).Key);
            }
         }
      }

      /// <summary>
      /// Calculates all of the relevant Stats of a Pokémon. This
      /// includes the Forms, Moves, League IVs, Type interactions, and weather boosts.
      /// </summary>
      /// <param name="pokemon">Reference to a Pokémon.</param>
      public void GetPokemonStats(ref Pokemon pokemon)
      {
         if (pokemon != null)
         {
            TypeRelation typeRelations = GetTypeDefenseRelations(pokemon.Type);
            pokemon.Weakness = typeRelations.Weak.Keys.ToList();
            pokemon.Resistance = typeRelations.Strong.Keys.ToList();
            pokemon.Weather = GetWeather(pokemon.Type);
            pokemon.FastMove = POGODBConnector.GetPokemonMoves(ReformatName(pokemon.Name), Global.FAST_MOVE_CATEGORY);
            pokemon.ChargeMove = POGODBConnector.GetPokemonMoves(ReformatName(pokemon.Name), Global.CHARGE_MOVE_CATEGORY, pokemon.Shadow);

            pokemon.CPMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.MAX_XL_LEVEL);
         }
      }

      /// <summary>
      /// Calculates all of the relevant CP valus of a Pokémon. This
      /// includes the raid, quest, hatch, shadow, and wild perfect IV values.
      /// </summary>
      /// <param name="pokemon">Reference to a Pokémon.</param>
      public static void GetPokemonCP(ref Pokemon pokemon)
      {
         if (pokemon != null)
         {
            pokemon.CPMaxHalf = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.MAX_REG_LEVEL);

            pokemon.CPMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.MAX_XL_LEVEL);

            pokemon.CPBestBuddy = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV,
               Global.MAX_XL_LEVEL + Global.BUDDY_BOOST);

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

            pokemon.CPShadow = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.SHADOW_LEVEL);

            pokemon.CPShadowBoosted = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV,
               Global.SHADOW_LEVEL + Global.WEATHER_BOOST);

            for (int level = Global.MIN_WILD_LEVEL; level <= Global.MAX_WILD_LEVEL; level++)
            {
               pokemon.CPWild.Add(CPCalculator.CalcCPPerLevel(
                  pokemon.Attack, pokemon.Defense, pokemon.Stamina,
                  Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, level));
            }
         }
      }

      /// <summary>
      /// Calculates the best PvP IVs of a Pokémon. Calculated for
      /// Little, Great, and Ultra Leagues for both levels 41 and 51 max.
      /// </summary>
      /// <param name="pokemon">Reference to a Pokémon.</param>
      public void GetPokemonPvP(ref Pokemon pokemon)
      {
         if (pokemon != null)
         {
            pokemon.GreatIVs = CPCalculator.CalcPvPIVsPerLeague(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_GREAT_CP, Global.MAX_REG_LEVEL + Global.BUDDY_BOOST);

            pokemon.UltraIVs = CPCalculator.CalcPvPIVsPerLeague(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_ULTRA_CP, Global.MAX_REG_LEVEL + Global.BUDDY_BOOST);

            pokemon.GreatXLIVs = CPCalculator.CalcPvPIVsPerLeague(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_GREAT_CP, Global.MAX_XL_LEVEL + Global.BUDDY_BOOST);

            pokemon.UltraXLIVs = CPCalculator.CalcPvPIVsPerLeague(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_ULTRA_CP, Global.MAX_XL_LEVEL + Global.BUDDY_BOOST);

            pokemon.CanBeLittleLeague = CanBeLittleLeague(ReformatName(pokemon.Name));
            if (pokemon.CanBeLittleLeague)
            {
               pokemon.LittleIVs = CPCalculator.CalcPvPIVsPerLeague(
                  pokemon.Attack, pokemon.Defense, pokemon.Stamina,
                  Global.MAX_LITTLE_CP, Global.MAX_REG_LEVEL + Global.BUDDY_BOOST);

               pokemon.LittleXLIVs = CPCalculator.CalcPvPIVsPerLeague(
                  pokemon.Attack, pokemon.Defense, pokemon.Stamina,
                  Global.MAX_LITTLE_CP, Global.MAX_XL_LEVEL + Global.BUDDY_BOOST);
            }
         }
      }

      /// <summary>
      /// Calculates the counters of a Pokémon.
      /// </summary>
      /// <param name="pokemon"></param>
      public void GetPokemonCounter(ref Pokemon pokemon)
      {
         pokemon.Counter = new List<Counter>();
         pokemon.Counter.AddRange(POGODBConnector.GetCounters(ReformatName(pokemon.Name), false));
         pokemon.Counter.AddRange(POGODBConnector.GetCounters(ReformatName(pokemon.Name), true));

         foreach (Counter counter in pokemon.Counter)
         {
            counter.FastAttack = POGODBConnector.GetPokemonMove(ReformatName(counter.Name.Replace($"{Global.SHADOW_TAG} ", string.Empty)), counter.FastAttack.Name);
            counter.ChargeAttack = POGODBConnector.GetPokemonMove(ReformatName(counter.Name.Replace($"{Global.SHADOW_TAG} ", string.Empty)), counter.ChargeAttack.Name);
         }
      }

      /// <summary>
      /// Checks if the Pokémon is little league eligable.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>True if the Pokémon can be in little league, otherwise false.</returns>
      public bool CanBeLittleLeague(string pokemonName)
      {
         return POGODBConnector.IsBaseForm(pokemonName);
      }

      /// <summary>
      /// Gets all evolutions in a Pokémon's evolution family.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="namesChecked">Pokémon in the familly already checked.</param>
      /// <returns>List of evolutions in the Pokémon's family.</returns>
      public List<Evolution> GetEvolutionFamily(string pokemonName, List<string> namesChecked = null)
      {
         if (namesChecked == null)
         {
            namesChecked = new List<string>();
         }

         List<Evolution> evolutions = POGODBConnector.GetEvolutions(ReformatName(pokemonName));

         if (evolutions.Count == 0)
         {
            return new List<Evolution>();
         }
         namesChecked.Add(pokemonName);

         List<Evolution> evoList = new List<Evolution>();
         foreach (Evolution evo in evolutions)
         {
            if (!evo.Start.Equals(pokemonName, StringComparison.OrdinalIgnoreCase) && !namesChecked.Contains(evo.Start))
            {
               evoList.AddRange(GetEvolutionFamily(evo.Start, namesChecked));
            }
            else if (!evo.End.Equals(pokemonName, StringComparison.OrdinalIgnoreCase) && !namesChecked.Contains(evo.End))
            {
               evoList.AddRange(GetEvolutionFamily(evo.End, namesChecked));
            }
         }
         evoList.AddRange(evolutions);
         evolutions.Clear();
         evolutions.AddRange(evoList.Where(x => !evolutions.Contains(x)));
         return evolutions;
      }

      /// <summary>
      /// Get form tags for a Pokémon.
      /// </summary>
      /// <param name="pokemonName">name of the Pokémon.</param>
      /// <returns>Struct containing tag information.</returns>
      public Form GetFormTags(string pokemonName)
      {
         return POGODBConnector.GetFormTags(ReformatName(pokemonName));
      }

      /// <summary>
      /// Gets the base forms of Pokémon with multiple forms.
      /// </summary>
      /// <returns>List of Pokémon names.</returns>
      public List<string> GetBaseForms()
      {
         return POGODBConnector.GetPokemonWithTags();
      }

      /// <summary>
      /// Gets a given move.
      /// </summary>
      /// <param name="moveName">Name of the move.</param>
      /// <returns>The Move coresponding to the name, otherwise null.</returns>
      public Move GetMove(string moveName)
      {
         if (moveName == null)
         {
            return null;
         }

         string name = ReformatName(moveName);
         Move move = POGODBConnector.GetMove(name);

         if (move == null)
         {
            return null;
         }

         move.Weather = GetWeather(new List<string>() { move.Type });
         move.PokemonWithMove = POGODBConnector.GetPokemonWithMove(name);

         return move;
      }

      /// <summary>
      /// Gets all moves of a given type.
      /// </summary>
      /// <param name="type">Type of the move.</param>
      /// <param name="category">Category of the move.</param>
      /// <returns>List of moves.</returns>
      public List<string> GetMoveByType(string type, string category)
      {
         return POGODBConnector.GetMoveByType(type, category);
      }

      /// <summary>
      /// Checks if the Pokémon already has a move.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="moveName">Name of the Move.</param>
      /// <returns>True if the Pokémon has the Move, otherwise false.</returns>
      public bool PokemonHasMove(string pokemonName, string moveName)
      {
         return POGODBConnector.GetPokemonMove(pokemonName, moveName) != null;
      }

      /// <summary>
      /// Gets all Pokémon that have a given number.
      /// </summary>
      /// <param name="pokemonNumber">Number of the Pokémon</param>
      /// <returns>List of Pokémon with the given number.</returns>
      public List<string> GetPokemonByNumber(int pokemonNumber)
      {
         return POGODBConnector.GetPokemonByNumber(pokemonNumber);
      }

      /// <summary>
      /// Gets list of Pokémon to use in counter sims.
      /// </summary>
      /// <returns>List of Pokémon to use.</returns>
      public List<Pokemon> GetPokemonForSim()
      {
         List<Pokemon> simPokemon = POGODBConnector.GetSimPokemon();

         foreach (Pokemon poke in simPokemon)
         {
            string name = ReformatName(poke.Name);

            List<Move> initFastMove = POGODBConnector.GetPokemonMoves(name, "Fast", poke.Shadow);
            List<Move> initChargeMove = POGODBConnector.GetPokemonMoves(name, "Charge", poke.Shadow);

            poke.FastMove = new List<Move>();
            poke.ChargeMove = new List<Move>();

            foreach (Move fast in initFastMove)
            {
               Move fullFast = POGODBConnector.GetMove(fast.Name);
               fullFast.IsLegacy = fast.IsLegacy;
               poke.FastMove.Add(fullFast);
            }

            foreach (Move charge in initChargeMove)
            {
               Move fullCharge = POGODBConnector.GetMove(charge.Name);
               fullCharge.IsLegacy = charge.IsLegacy;
               poke.ChargeMove.Add(fullCharge);
            }
         }

         return simPokemon;
      }

      /// <summary>
      /// Reformats the name from user input to the POGO database format.
      /// </summary>
      /// <param name="originalName">User input name.</param>
      /// <returns>Name formated for the POGO database</returns>
      private static string ReformatName(string originalName)
      {
         return originalName.Replace("\'", "\'\'");
      }

      /// <summary>
      /// Gets defensive type relations for a Pokémon's type.
      /// Separates weaknesses and resistances.
      /// </summary>
      /// <param name="types">List of Pokémon types.</param>
      /// <returns>Relations when the types are defending.</returns>
      public TypeRelation GetTypeDefenseRelations(List<string> types)
      {
         Dictionary<string, int> allRelations = POGODBConnector.GetTypeDefenseRelations(types);
         return new TypeRelation(
            allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => TypeCalculator.CalcTypeEffectivness(v.Value)),
            allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => TypeCalculator.CalcTypeEffectivness(v.Value))
         );
      }

      /// <summary>
      /// Gets offensive type relations for a move's type.
      /// Separates super and not very effective moves.
      /// </summary>
      /// <param name="type">Move type.</param>
      /// <returns>Relations when the type is attacking.</returns>
      public TypeRelation GetTypeAttackRelations(string type)
      {
         Dictionary<string, int> allRelations = POGODBConnector.GetTypeAttackRelations(type);
         return new TypeRelation(
            allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => TypeCalculator.CalcTypeEffectivness(v.Value)),
            allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => TypeCalculator.CalcTypeEffectivness(v.Value))
         );
      }

      /// <summary>
      /// Gets all weather that boosts the given types.
      /// </summary>
      /// <param name="types">List of types.</param>
      /// <returns>List of weather that boosts the givent types.</returns>
      public List<string> GetWeather(List<string> types)
      {
         return POGODBConnector.GetWeather(types);
      }

      /// <summary>
      /// Updates an attribute of a Pokémon.
      /// Only updates true false values.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="attribute">Attribute to change.</param>
      /// <param name="value">New value of the attribute.</param>
      public void UpdatePokemon(string pokemonName, string attribute, int value)
      {
         POGODBConnector.SetPokemonAttribute(ReformatName(pokemonName), attribute, value);
      }

      /// <summary>
      /// Sets a move for a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="moveName">Name of the move.</param>
      /// <param name="isLegacy">Is the move a legacy move.</param>
      public void UpdatePokemonMove(string pokemonName, string moveName, int isLegacy, bool moveAssigned)
      {
         if (moveAssigned)
         {
            POGODBConnector.UpdatePokemonMove(ReformatName(pokemonName), moveName, isLegacy);
         }
         else
         {
            POGODBConnector.SetPokemonMove(ReformatName(pokemonName), moveName, isLegacy);
         }
      }

      /// <summary>
      /// Updates counters for a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="counters">List of normal counters.</param>
      /// <param name="specialCounters">List of special counters.</param>
      public void UpdateCounters(string pokemonName, List<Counter> counters, List<Counter> specialCounters)
      {
         string name = ReformatName(pokemonName);

         POGODBConnector.ClearPokemonCounters(name);

         foreach (Counter counter in counters)
         {
            POGODBConnector.AddCounter(name, ReformatName(counter.Name), ReformatName(counter.FastAttack.Name), ReformatName(counter.ChargeAttack.Name), counter.Rating, false);
         }

         foreach (Counter counter in specialCounters)
         {
            POGODBConnector.AddCounter(name, ReformatName(counter.Name), ReformatName(counter.FastAttack.Name), ReformatName(counter.ChargeAttack.Name), counter.Rating, true);
         }
      }

      /// <summary>
      /// Adds settings to the database for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void InitSettings(ulong guild)
      {
         NONADBConnector.AddSettings(guild);
      }

      /// <summary>
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>Prefix registerd for the guild.</returns>
      public string GetPrefix(ulong guild)
      {
         return NONADBConnector.GetPrefix(guild);
      }

      /// <summary>
      /// Check if setup is completed for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>True if setup is complete for the guild, otherwise false.</returns>
      public bool GetSetupComplete(ulong guild)
      {
         return NONADBConnector.GetSetupComplete(guild);
      }

      /// <summary>
      /// Updates the prefix of the guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="prefix">New prefix value.</param>
      public void UpdatePrefix(ulong guild, string prefix)
      {
         NONADBConnector.UpdatePrefix(guild, prefix);
      }

      /// <summary>
      /// Marks a guild setup as complete
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void CompleteSetup(ulong guild)
      {
         NONADBConnector.CompleteSetup(guild);
      }

      /// <summary>
      /// Deletes the settings of a guild.
      /// </summary>
      /// <param name="guild">Id of guild.</param>
      public void DeleteSettings(ulong guild)
      {
         NONADBConnector.DeleteSettings(guild);
      }

      /// <summary>
      /// Gets the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel in the guild.</param>
      /// <returns>Registration string for the channel, otherwise null.</returns>
      public string GetRegistration(ulong guild, ulong channel)
      {
         return NONADBConnector.GetRegistration(guild, channel);
      }

      /// <summary>
      /// Checks if guild has a channel registered for raid notifications.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>True if registered channel exists, otherwise false.</returns>
      public bool CheckNotificationRegister(ulong guild)
      {
         return NONADBConnector.CheckNotificationRegister(guild);
      }

      /// <summary>
      /// Get all channels registerd for raid notifications.
      /// </summary>
      /// <returns>Dictionary of channels where key is the guild id and value is channel id.</returns>
      public Dictionary<ulong, ulong> GetNotificationChannels()
      {
         return NONADBConnector.GetNotificationChannels();
      }

      /// <summary>
      /// Updates the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel in the guild.</param>
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
      /// Checks if guild has a channel registered for raid notifications.
      /// </summary>
      /// <param name="message">Id of the message.</param>
      /// <returns>True if message is a raid notification message, otherwise false.</returns>
      public bool CheckNotificationMessage(ulong message)
      {
         return NONADBConnector.CheckNotificationMessage(message);
      }

      /// <summary>
      /// Get notification messages for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <returns>List of message Ids.</returns>
      public List<ulong> GetNotificationMessages(ulong guild, ulong channel)
      {
         string messageStr = NONADBConnector.GetNotificationMessages(guild, channel);

         if (messageStr == null)
         {
            return null;
         }

         string[] messages = messageStr.Split(',');
         List<ulong> msgList = new List<ulong>();
         foreach (string msg in messages)
         {
            if (!string.IsNullOrEmpty(msg) && !string.IsNullOrWhiteSpace(msg))
            {
               msgList.Add(ulong.Parse(msg));
            }
         }
         return msgList;
      }

      /// <summary>
      /// Updates the notification message for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <param name="message">Id of the message</param>
      public void UpdateNotificationMessage(ulong guild, ulong channel, string message)
      {
         NONADBConnector.UpdateNotificationMessage(guild, channel, message);
      }

      /// <summary>
      /// Clears the notification message for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      public void ClearNotificationMessage(ulong guild, ulong channel)
      {
         NONADBConnector.UpdateNotificationMessage(guild, channel);
      }

      /// <summary>
      /// Deletes the registration of a guild or channel.
      /// If no channel is given then all registrations for the guild are
      /// deleted.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel in the guild.</param>
      public void DeleteRegistration(ulong guild, ulong? channel = null)
      {
         if (channel == null)
            NONADBConnector.DeleteAllRegistration(guild);
         else
            NONADBConnector.DeleteRegistration(guild, (ulong)channel);
      }

      /// <summary>
      /// Gets a Pokémon by its guild nickname.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of the Pokémon.</param>
      /// <returns>Name of the Pokémon if it is assigned, otherwise null.</returns>
      public string GetPokemonWithNickname(ulong guild, string nickname)
      {
         return NONADBConnector.GetPokemonByNickname(guild, ReformatName(nickname));
      }

      /// <summary>
      /// Gets a guild's list of nicknames for a Pokémon.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of nicknames assigned to the Pokémon for the guild.</returns>
      public List<string> GetNicknames(ulong guild, string pokemonName)
      {
         return NONADBConnector.GetNicknames(guild, ReformatName(pokemonName));
      }

      /// <summary>
      /// Adds a nickname to a Pokémon for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname for the Pokémon.</param>
      /// <param name="pokemonName">Pokémon the nickname applies to.</param>
      public void AddNickname(ulong guild, string nickname, string pokemonName)
      {
         NONADBConnector.AddNickname(guild, pokemonName, nickname);
      }

      /// <summary>
      /// Updates the nickname of a Pokémon for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="oldNickname">Nickname to replace.</param>
      /// <param name="newNickname">Nickname to replace with.</param>
      public void UpdateNickname(ulong guild, string oldNickname, string newNickname)
      {
         NONADBConnector.UpdateNickname(guild, newNickname, oldNickname);
      }

      /// <summary>
      /// Deletes a nickname from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname to delete.</param>
      public void DeleteNickname(ulong guild, string nickname)
      {
         NONADBConnector.DeleteNickname(guild, nickname);
      }

      /// <summary>
      /// Gets a POI.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poi">Name of the POI.</param>
      /// <returns>POI registered the guild with the given name.</returns>
      public POI GetPOI(ulong guild, string poi)
      {
         return poi == null ? null : NONADBConnector.GetPOI(guild, ReformatName(poi));
      }

      /// <summary>
      /// Gets a POI by its nickname.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of the POI.</param>
      /// <returns>Name of the POI if it is assigned, otherwise null.</returns>
      public string GetPOIWithNickname(ulong guild, string nickname)
      {
         return NONADBConnector.GetPOIByNickname(guild, ReformatName(nickname));
      }

      /// <summary>
      /// Gets a guild's list of nicknames for a POI.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poi">Name of the POI.</param>
      /// <returns>List of nicknames assigned to the POI for the guild.</returns>
      public List<string> GetPOINicknames(ulong guild, string poi)
      {
         return NONADBConnector.GetPOINicknames(guild, ReformatName(poi));
      }

      /// <summary>
      /// Adds a Point of Interest to a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="name">Name of the Point of Interest.</param>
      /// <param name="latitude">Latitudinal coordinate.</param>
      /// <param name="longitude">Longitudinal coordinate.</param>
      /// <param name="gym">Is the Point of Interest a gym.</param>
      public void AddPOI(ulong guild, string name, float latitude, float longitude, int gym)
      {
         NONADBConnector.AddPOI(guild, name, latitude, longitude, gym, 0, 0);
      }

      /// <summary>
      /// Adds a Sponsored Point of Interest to a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="name">Name of the Point of Interest.</param>
      /// <param name="latitude">Latitudinal coordinate.</param>
      /// <param name="longitude">Longitudinal coordinate.</param>
      /// <param name="gym">Is the Point of Interest a gym.</param>
      public void AddSponsoredPOI(ulong guild, string name, float latitude, float longitude, int gym)
      {
         NONADBConnector.AddPOI(guild, name, latitude, longitude, gym, 1, 0);
      }

      /// <summary>
      /// Adds an EX Gym Point of Interest to a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="name">Name of the Point of Interest.</param>
      /// <param name="latitude">Latitudinal coordinate.</param>
      /// <param name="longitude">Longitudinal coordinate.</param>
      public void AddExGym(ulong guild, string name, float latitude, float longitude)
      {
         NONADBConnector.AddPOI(guild, name, latitude, longitude, 1, 0, 1);
      }

      /// <summary>
      /// Updates an attribute of a Point of Interest.
      /// Only updates true/false values.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="name">Name of the Point of Interest.</param>
      /// <param name="attribute">Attribute to change.</param>
      /// <param name="value">New value of the attribute</param>
      public void UpdatePOI(ulong guild, string name, string attribute, int value)
      {
         NONADBConnector.UpdatePOI(guild, name, attribute, value);
      }

      /// <summary>
      /// Delete a Point of Interest from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="name">Name of the Point of Interest.</param>
      public void RemovePOI(ulong guild, string name)
      {
         NONADBConnector.RemovePOI(guild, name);
      }

      /// <summary>
      /// Adds a nickname to a Point of Interest for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname for the Point of Interest.</param>
      /// <param name="poiName">Point of Interest the nickname applies to.</param>
      public void AddPOINickname(ulong guild, string nickname, string poiName)
      {
         NONADBConnector.AddPOINickname(guild, poiName, nickname);
      }

      /// <summary>
      /// Updates the nickname of a Point of Interest for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="oldNickname">Nickname to replace.</param>
      /// <param name="newNickname">Nickname to replace with.</param>
      public void UpdatePOINickname(ulong guild, string oldNickname, string newNickname)
      {
         NONADBConnector.UpdatePOINickname(guild, newNickname, oldNickname);
      }

      /// <summary>
      /// Deletes a Point of Interest nickname from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname to delete.</param>
      public void DeletePOINickname(ulong guild, string nickname)
      {
         NONADBConnector.DeletePOINickname(guild, nickname);
      }
   }
}