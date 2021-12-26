using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using PokeStar.DataModels;

namespace PokeStar
{
   /// <summary>
   /// Holds global variables.
   /// </summary>
   public static class Global
   {
      // é ********************************************************************
      
      /// <summary>
      /// Save formated Pokémon.
      /// </summary>
      private const string POKEMON  = "Pokémon";

      /// <summary>
      /// Save formated PokéDex.
      /// </summary>
      private const string POKEDEX  = "PokéDex";

      /// <summary>
      /// Save formated PokéBall.
      /// </summary>
      private const string POKEBALL = "PokéBall";

      /// <summary>
      /// Save formated Poké Stop.
      /// </summary>
      private const string POKESTOP = "Poké Stop";

      // File IO **************************************************************

      /// <summary>
      /// Path to .exe file.
      /// </summary>
      public static string PROGRAM_PATH { get; set; }

      /// <summary>
      /// Decoded ENV file.
      /// </summary>
      public static JObject ENV_FILE { get; set; }

      /// <summary>
      /// Default prefix.
      /// </summary>
      public static string DEFAULT_PREFIX { get; set; }

      /// <summary>
      /// Version of Nona.
      /// </summary>
      public static string VERSION { get; set; }

      /// <summary>
      /// Name of home server.
      /// </summary>
      public static string HOME_SERVER { get; set; }

      /// <summary>
      /// Name of emote server.
      /// Used for generating raid emotes.
      /// </summary>
      public static string EMOTE_SERVER { get; set; }

      /// <summary>
      /// Logging level.
      /// </summary>
      public static LogSeverity LOG_LEVEL { get; set; }

      /// <summary>
      /// Pokémon database connection string.
      /// </summary>
      public static string POGO_DB_CONNECTION_STRING { get; set; }

      /// <summary>
      /// Nona database connection string.
      /// </summary>
      public static string NONA_DB_CONNECTION_STRING { get; set; }

      /// <summary>
      /// Can test bot be used.
      /// </summary>
      public static bool USE_NONA_TEST { get; set; }

      /// <summary>
      /// Can empty raid be used.
      /// </summary>
      public static bool USE_EMPTY_RAID { get; set; }

      public static bool INIT_COMPLETE { get; set; } = false;

      // Help command hidding *************************************************

      /// <summary>
      /// List of all commands.
      /// </summary>
      public static List<CommandInfo> COMMAND_INFO { get; set; }

      /// <summary>
      /// Commands that are always hidden.
      /// </summary>
      public static readonly List<string> HIDDEN_COMMANDS = new List<string>()
      {
         "ping",
         "marco",
         "rave",
         "screm",
         "sink",
      };

      /// <summary>
      /// Commands for server admins.
      /// </summary>
      public static readonly List<string> ADMIN_COMMANDS = new List<string>()
      {
         "status",
      };

      /// <summary>
      /// Commands for Nona admins.
      /// </summary>
      public static readonly List<string> NONA_ADMIN_COMMANDS = new List<string>()
      {
         "updatePokemonNames",
         "updateMovenNames",
         "toggleUseEmptyRaid",
         "toggleUseNonaTest",
         "updatePokemon",
         "updatePokemonMove",
         "updateEggList",
         "updateRocketList",
         "updateReactions",
      };

      // Embed colors *********************************************************

      /// <summary>
      /// Nona info response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_INFO_RESPONSE      = Color.Purple;

      /// <summary>
      /// Warning response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_WARNING_RESPONSE   = Color.Orange;

      /// <summary>
      /// Error response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_ERROR_RESPONSE     = Color.Red;

      /// <summary>
      /// Help response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_HELP_RESPONSE      = Color.Gold;

      /// <summary>
      /// Raid response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_RAID_RESPONSE      = Color.Blue;

      /// <summary>
      /// Dex response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_DEX_RESPONSE       = Color.Green;

      /// <summary>
      /// Game info response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_GAME_INFO_RESPONSE = Color.Teal;

      /// <summary>
      /// POI response embed color.
      /// </summary>
      public static readonly Color EMBED_COLOR_POI_RESPONSE       = Color.Magenta;

      // Raid tiers ***********************************************************

      /// <summary>
      /// EX raid tier value.
      /// </summary>
      public static readonly short EX_RAID_TIER        = 9;

      /// <summary>
      /// Mega raid tier value.
      /// </summary>
      public static readonly short MEGA_RAID_TIER      = 7;

      /// <summary>
      /// Legendary raid tier value.
      /// </summary>
      public static readonly short LEGENDARY_RAID_TIER = 5;

      /// <summary>
      /// Premium raid tier value.
      /// </summary>
      public static readonly short PREMIUM_RAID_TIER   = 4;

      /// <summary>
      /// Rare raid tier value.
      /// </summary>
      public static readonly short RARE_RAID_TIER      = 3;

      /// <summary>
      /// Uncommon raid tier value.
      /// </summary>
      public static readonly short UNCOMMON_RAID_TIER  = 2;

      /// <summary>
      /// Common raid tier value.
      /// </summary>
      public static readonly short COMMON_RAID_TIER    = 1;

      /// <summary>
      /// Invalid raid tier value.
      /// </summary>
      public static readonly short INVALID_RAID_TIER   = 0;

      /// <summary>
      /// Mega raid string.
      /// </summary>
      public static readonly string RAID_STRING_MEGA = "Mega";

      /// <summary>
      /// EX raid string.
      /// </summary>
      public static readonly string RAID_STRING_EX   = "EX";

      /// <summary>
      /// Normal raid string.
      /// </summary>
      public static readonly string RAID_STRING_TIER = "Tier";

      /// <summary>
      /// Silph raid strings to raid values.
      /// </summary>
      public static readonly Dictionary<string, int> RAID_TIER_TITLE = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
      {
         ["Mega Raids"] = MEGA_RAID_TIER,
         ["Tier 5"]     = LEGENDARY_RAID_TIER,
         ["Tier 4"]     = PREMIUM_RAID_TIER,
         ["Tier 3"]     = RARE_RAID_TIER,
         ["Tier 2"]     = UNCOMMON_RAID_TIER,
         ["Tier 1"]     = COMMON_RAID_TIER,
      };

      /// <summary>
      /// Raid strings to raid values.
      /// </summary>
      public static readonly Dictionary<string, short> RAID_TIER_STRING = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase)
      {
         ["EX"]        = EX_RAID_TIER,
         ["E"]         = EX_RAID_TIER,
         ["9"]         = EX_RAID_TIER,
         ["MEGA"]      = MEGA_RAID_TIER,
         ["M"]         = MEGA_RAID_TIER,
         ["7"]         = MEGA_RAID_TIER,
         ["LEGENDARY"] = LEGENDARY_RAID_TIER,
         ["L"]         = LEGENDARY_RAID_TIER,
         ["5"]         = LEGENDARY_RAID_TIER,
         ["PREMIUM"]   = PREMIUM_RAID_TIER,
         ["P"]         = PREMIUM_RAID_TIER,
         ["4"]         = PREMIUM_RAID_TIER,
         ["SNORLAX"]   = PREMIUM_RAID_TIER,
         ["RARE"]      = RARE_RAID_TIER,
         ["R"]         = RARE_RAID_TIER,
         ["3"]         = RARE_RAID_TIER,
         ["UNCOMMON"]  = UNCOMMON_RAID_TIER,
         ["U"]         = UNCOMMON_RAID_TIER,
         ["2"]         = UNCOMMON_RAID_TIER,
         ["COMMON"]    = COMMON_RAID_TIER,
         ["C"]         = COMMON_RAID_TIER,
         ["1"]         = COMMON_RAID_TIER,
      };

      // Raid limits **********************************************************

      /// <summary>
      /// Raid player limit.
      /// </summary>
      public static readonly int LIMIT_RAID_PLAYER      = 20;

      /// <summary>
      /// Raid invite limit.
      /// </summary>
      public static readonly int LIMIT_RAID_INVITE      = 10;

      /// <summary>
      /// Raid group limit.
      /// </summary>
      public static readonly int LIMIT_RAID_GROUP       = 3;

      /// <summary>
      /// Mule mule limit.
      /// </summary>
      public static readonly int LIMIT_RAID_MULE_MULE   = 2;

      /// <summary>
      /// Mule invite limit.
      /// </summary>
      public static readonly int LIMIT_RAID_MULE_INVITE = 5;

      /// <summary>
      /// Mule group limit.
      /// </summary>
      public static readonly int LIMIT_RAID_MULE_GROUP  = 6;

      /// <summary>
      /// Boss emotes on a notify message limit.
      /// </summary>
      public static readonly int LIMIT_RAID_BOSS_NOTIFY = 20;

      // Raid Group bits ******************************************************

      /// <summary>
      /// Mask for attending value.
      /// 0000 0XXX mask will clear 4th bit.
      /// </summary>
      public static readonly int ATTEND_MASK  = 0b00000111;

      /// <summary>
      /// Mask for remote value.
      /// 0YYY 0000 mask will clear 8th bit.
      /// </summary>
      public static readonly int REMOTE_MASK  = 0b01110000;

      /// <summary>
      /// Shift for remote value.
      /// 0YYY 0XXX where Y is the remote value and X is the attending value.
      /// </summary>
      public static readonly int REMOTE_SHIFT = 4;

      // Bad raid values ******************************************************

      /// <summary>
      /// Do not change values.
      /// </summary>
      public static readonly int NO_ADD_VALUE              = -1;

      /// <summary>
      /// User not in raid.
      /// </summary>
      public static readonly int NOT_IN_RAID               = -1;

      /// <summary>
      /// Default raid boss name.
      /// </summary>
      public static readonly string DEFAULT_RAID_BOSS_NAME = "Empty";

      /// <summary>
      /// Empty embed field.
      /// </summary>
      public static readonly string EMPTY_FIELD            = "-----";

      // Level constents ******************************************************

      /// <summary>
      /// Discrete CP multiplier.
      /// </summary>
      public static readonly double[] DISCRETE_CPM = {
         0.094     , 0.16639787, 0.21573247, 0.25572005, 0.29024988,
         0.3210876 , 0.34921268, 0.3752356 , 0.39956728, 0.4225    ,
         0.44310755, 0.4627984 , 0.48168495, 0.49985844, 0.51739395,
         0.5343543 , 0.5507927 , 0.5667545 , 0.5822789 , 0.5974    ,
         0.6121573 , 0.6265671 , 0.64065295, 0.65443563, 0.667934  ,
         0.6811649 , 0.69414365, 0.7068842 , 0.7193991 , 0.7317    ,
         0.7377695 , 0.74378943, 0.74976104, 0.7556855 , 0.76156384,
         0.76739717, 0.7731865 , 0.77893275, 0.784637  , 0.7903    ,
         0.7953    , 0.8003    , 0.8053    , 0.8103    , 0.8153    ,
         0.8203    , 0.8253    , 0.8303    , 0.8353    , 0.8403    ,
         0.8453    , 0.8503    , 0.8553    , 0.8603    , 0.8653
      };

      /// <summary>
      /// Step between levels.
      /// </summary>
      public static readonly double LEVEL_STEP  = 0.5;

      /// <summary>
      /// Max level with XL candy.
      /// </summary>
      public static readonly int MAX_XL_LEVEL   = 50;

      /// <summary>
      /// Max level with regular candy.
      /// </summary>
      public static readonly int MAX_REG_LEVEL  = 40;

      /// <summary>
      /// Min wild level.
      /// </summary>
      public static readonly int MIN_WILD_LEVEL = 1;

      /// <summary>
      /// Max wild level.
      /// </summary>
      public static readonly int MAX_WILD_LEVEL = 35;

      /// <summary>
      /// Raid level.
      /// </summary>
      public static readonly int RAID_LEVEL     = 20;

      /// <summary>
      /// Hatch level.
      /// </summary>
      public static readonly int HATCH_LEVEL    = 20;

      /// <summary>
      /// Quest level.
      /// </summary>
      public static readonly int QUEST_LEVEL    = 15;

      /// <summary>
      /// Shadow level.
      /// </summary>
      public static readonly int SHADOW_LEVEL   = 8;

      /// <summary>
      /// Weather boost bonus levels.
      /// </summary>
      public static readonly int WEATHER_BOOST  = 5;

      /// <summary>
      /// Buddy boost bonus levels.
      /// </summary>
      public static readonly int BUDDY_BOOST    = 1;

      // IV constants *********************************************************

      /// <summary>
      /// Min special IV.
      /// </summary>
      public static readonly int MIN_SPECIAL_IV = 10;

      /// <summary>
      /// Max IV.
      /// </summary>
      public static readonly int MAX_IV         = 15;

      // GBL League constants *************************************************

      /// <summary>
      /// Little league max CP.
      /// </summary>
      public static readonly int MAX_LITTLE_CP = 500;

      /// <summary>
      /// Great league max CP.
      /// </summary>
      public static readonly int MAX_GREAT_CP  = 1500;

      /// <summary>
      /// Ultra league max CP.
      /// </summary>
      public static readonly int MAX_ULTRA_CP  = 2500;

      // Type Calculator ******************************************************

      /// <summary>
      /// Coefficient for type effectivness.
      /// </summary>
      public const double TYPE_COEFFICIENT = 1.6;

      // Dex switch options ***************************************************

      /// <summary>
      /// Dex switch option number.
      /// </summary>
      public static readonly int DEX_SWITCH_OPTIONS = 6;

      // Shadow moves *********************************************************

      public static readonly int SHADOW_INDEX   = 0;
      public static readonly int PURIFIED_INDEX = 1;

      public static readonly List<Move> SHADOW_MOVES = new List<Move>()
      {
         new Move()
         {
            Name = "Frustration",
            Type = "Normal"
         },
         new Move()
         {
            Name = "Return",
            Type = "Normal"
         },
      };

      // Bad dex values *******************************************************

      /// <summary>
      /// Evolution method has been combined.
      /// </summary>
      public static readonly int BAD_EVOLUTION = -1;

      /// <summary>
      /// Unown Pokemon number.
      /// </summary>
      public static readonly int UNOWN_NUMBER  = 201;

      /// <summary>
      /// Arceus Pokemon number.
      /// </summary>
      public static readonly int ARCEUS_NUMBER = 493;

      // Training dummy Pokémon ***********************************************

      /// <summary>
      /// Dummy Pokémon number.
      /// </summary>
      public static int DUMMY_POKE_NUM     = 0;

      /// <summary>
      /// Dummy Pokémon name.
      /// </summary>
      public static string DUMMY_POKE_NAME = "MissingNo";

      // Catch multipliers ****************************************************

      /// <summary>
      /// PokéBall multipliers.
      /// </summary> 
      public static readonly Dictionary<string, double> POKE_BALL_RATE = new Dictionary<string, double>()
      {
         ["Pokéball"]   = 1.0,
         ["Great Ball"] = 1.5,
         ["Ultra Ball"] = 2.0,
      };

      /// <summary>
      /// Berry multipliers.
      /// </summary>
      public static readonly Dictionary<string, double> BERRY_RATE = new Dictionary<string, double>()
      {
         ["None"]         = 1.0,
         ["Razz"]         = 1.5,
         ["Silver Pinap"] = 1.8,
         ["Golden Razz"]  = 2.5,
      };

      /// <summary>
      /// Throw multipliers.
      /// </summary>
      public static readonly Dictionary<string, double> THROW_RATE = new Dictionary<string, double>()
      {
         ["Regular"]   = 1.0,
         ["Nice"]      = 1.0,
         ["Great"]     = 1.3,
         ["Excellent"] = 1.7,
      };

      /// <summary>
      /// Curveball multipliers.
      /// </summary>
      public static readonly Dictionary<string, double> CURVEBALL_RATE = new Dictionary<string, double>()
      {
         ["No"]  = 1.0,
         ["Yes"] = 1.7,
      };
      
      /// <summary>
      /// Medal multipliers.
      /// </summary>
      public static readonly Dictionary<string, double> MEDAL_RATE = new Dictionary<string, double>()
      {
         ["None"]     = 1.0,
         ["Bronze"]   = 1.1,
         ["Silver"]   = 1.2,
         ["Gold"]     = 1.3,
         ["Platinum"] = 1.4,
      };

      /// <summary>
      /// Encounter multipliers.
      /// </summary>
      public static readonly Dictionary<string, double> ENCOUNTER_RATE = new Dictionary<string, double>()
      {
         ["Standard"] = 1.0,
         ["Special"]  = 2.0,
      };

      /// <summary>
      /// Dictionary of modifer values.
      /// Key is modifier name, value is max modifier value.
      /// </summary>
      public static readonly Dictionary<string, int> MODIFIER_STATS = new Dictionary<string, int>()
      {
         ["Pokémon Level"]  = MAX_WILD_LEVEL - 1,
         ["Pokéball Type"]  = POKE_BALL_RATE.Count - 1,
         ["Berry Type"]     = BERRY_RATE.Count - 1,
         ["Throw Type"]     = THROW_RATE.Count - 1,
         ["Curveball"]      = CURVEBALL_RATE.Count - 1,
         ["Medal 1 Bonus"]  = MEDAL_RATE.Count - 1,
         ["Medal 2 Bonus"]  = MEDAL_RATE.Count - 1,
         ["Encounter Type"] = ENCOUNTER_RATE.Count - 1,
      };

      // Catch ring colors ****************************************************

      /// <summary>
      /// Minimum color value.
      /// Red - 0%.
      /// </summary>
      public static readonly uint MIN_CATCH_COLOR = 0xFF0000;

      /// <summary>
      /// Middle color value.
      /// Yellow - 50%.
      /// </summary>
      public static readonly uint MID_CATCH_COLOR = 0xFFFF00;

      /// <summary>
      /// Maximum color value.
      /// Green - 100%.
      /// </summary>
      public static readonly uint MAX_CATCH_COLOR = 0x00FF00;

      // Text formating *******************************************************

      /// <summary>
      /// Character denotes STAB (Same type attack bonus).
      /// </summary>
      public static readonly char STAB_SYMBOL            = '*';

      /// <summary>
      /// Character denotes default form.
      /// </summary>
      public static readonly char DEFAULT_FORM_SYMBOL    = '*';

      /// <summary>
      /// Character denotes weather boost.
      /// </summary>
      public static readonly char WEATHER_BOOST_SYMBOL   = '*';

      /// <summary>
      /// Character denotes Pokémon can be caught from a rocket encounter.
      /// </summary>
      public static readonly char ROCKET_CATCH_SYMBOL    = '*';

      /// <summary>
      /// Character denotes Pokémon can only be found in eggs from a specific region.
      /// </summary>
      public static readonly char EGG_REGIONAL_SYMBOL    = '*';

      /// <summary>
      /// Character denotes legacy move.
      /// </summary>
      public static readonly char LEGACY_MOVE_SYMBOL     = '!';

      /// <summary>
      /// Character denotes value read from the Silph Road is unverified.
      /// </summary>
      public static readonly char UNVERIFIED_SYMBOL      = '?';

      /// <summary>
      /// Number of wild CP per column in cp list.
      /// </summary>
      public static readonly int WILD_CP_COLUMN_LENGTH   = 12;

      /// <summary>
      /// Number of Pokémon per row in egg list.
      /// </summary>
      public static readonly int EGG_ROW_LENGTH          = 3;

      /// <summary>
      /// Fast move name.
      /// </summary>
      public static readonly string FAST_MOVE_CATEGORY   = "Fast";

      /// <summary>
      /// Charge move name.
      /// </summary>
      public static readonly string CHARGE_MOVE_CATEGORY = "Charge";

      /// <summary>
      /// Mega Pokémon tag.
      /// </summary>
      public static readonly string MEGA_TAG             = "Mega";

      /// <summary>
      /// Shadow Pokémon tag.
      /// </summary>
      public static readonly string SHADOW_TAG           = "Shadow";

      /// <summary>
      /// Max values in mega Pokémon name.
      /// </summary>
      public static readonly int MAX_LEN_MEGA            = 3;

      // Parsers **************************************************************

      /// <summary>
      /// Number of expected parsing arguments.
      /// </summary>
      public static readonly int NUM_PARSE_ARGS          = 2;

      /// <summary>
      /// New parse value location.
      /// </summary>
      public static readonly int NEW_PARSE_VALUE         = 0;

      /// <summary>
      /// Old parse value location.
      /// </summary>
      public static readonly int OLD_PARSE_VALUE         = 1;

      /// <summary>
      /// Delimiter for parsing multi input string.
      /// </summary>
      public static readonly char PARSE_DELIMITER        = '>';

      /// <summary>
      /// Missing delimiter value.
      /// </summary>
      public static readonly int DELIMITER_MISSING       = -1;

      // Egg tiers ************************************************************

      /// <summary>
      /// 2KM egg value.
      /// </summary>
      public static readonly short EGG_TIER_2KM     = 2;

      /// <summary>
      /// 5KM egg value.
      /// </summary>
      public static readonly short EGG_TIER_5KM     = 5;

      /// <summary>
      /// 10KM egg value.
      /// </summary>
      public static readonly short EGG_TIER_10KM    = 10;

      /// <summary>
      /// 5KM adventure sync egg value.
      /// </summary>
      public static readonly short EGG_TIER_5AS     = 25;

      /// <summary>
      /// 10KM adventure sync egg value.
      /// </summary>
      public static readonly short EGG_TIER_10AS    = 50;

      /// <summary>
      /// 7KM egg value.
      /// </summary>
      public static readonly short EGG_TIER_7KM     = 7;

      /// <summary>
      /// 12KM egg value.
      /// </summary>
      public static readonly short EGG_TIER_12KM    = 12;

      /// <summary>
      /// Invalid egg value.
      /// </summary>
      public static readonly short EGG_TIER_INVALID = 0;

      /// <summary>
      /// Silph egg strings to egg values.
      /// </summary>
      public static readonly Dictionary<string, int> EGG_TIER_TITLE = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
      {
         ["2KM Eggs"]          = EGG_TIER_2KM,
         ["5KM Eggs"]          = EGG_TIER_5KM,
         ["10KM Eggs"]         = EGG_TIER_10KM,
         ["5KM Eggs (25KM)"]   = EGG_TIER_5AS,
         ["10KM Eggs (50KM)"]  = EGG_TIER_10AS,
         ["7KM Gift Eggs"]     = EGG_TIER_7KM,
         ["12KM Strange Eggs"] = EGG_TIER_12KM,
      };

      /// <summary>
      /// Egg strings to egg values.
      /// </summary>
      public static readonly Dictionary<string, short> EGG_TIER_STRING = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase)
      {
         ["2"]     = EGG_TIER_2KM,
         ["2 KM"]  = EGG_TIER_2KM,
         ["5"]     = EGG_TIER_5KM,
         ["5 KM"]  = EGG_TIER_5KM,
         ["10"]    = EGG_TIER_10KM,
         ["10 KM"] = EGG_TIER_10KM,
         ["25"]    = EGG_TIER_5AS,
         ["5 AS"]  = EGG_TIER_5AS,
         ["50"]    = EGG_TIER_10AS,
         ["10 AS"] = EGG_TIER_10AS,
         ["7"]     = EGG_TIER_7KM,
         ["7 KM"]  = EGG_TIER_7KM,
         ["12"]    = EGG_TIER_12KM,
         ["12 KM"] = EGG_TIER_12KM,
      };

      // Channel register values **********************************************

      /// <summary>
      /// User account register character.
      /// </summary>
      public static readonly char REGISTER_STRING_ACCOUNT      = 'A';

      /// <summary>
      /// Dex register character.
      /// </summary>
      public static readonly char REGISTER_STRING_DEX          = 'D';

      /// <summary>
      /// EX raid register character.
      /// </summary>
      public static readonly char REGISTER_STRING_EX           = 'E';

      /// <summary>
      /// Information register character.
      /// </summary>
      public static readonly char REGISTER_STRING_INFO         = 'I';

      /// <summary>
      /// Raid Notification register character.
      /// </summary>
      public static readonly char REGISTER_STRING_NOTIFICATION = 'N';

      /// <summary>
      /// Role register character.
      /// </summary>
      public static readonly char REGISTER_STRING_ROLE         = 'P';

      /// <summary>
      /// Raid register character.
      /// </summary>
      public static readonly char REGISTER_STRING_RAID         = 'R';

      /// <summary>
      /// Point of interest register character.
      /// </summary>
      public static readonly char REGISTER_STRING_POI          = 'S';

      /// <summary>
      /// Full register string.
      /// </summary>
      public static readonly string FULL_REGISTER_STRING       = "ADEIPRS";

      /// <summary>
      /// Register character to register string.
      /// </summary>
      public static readonly Dictionary<string, string> REGISTER_STRING_TYPE = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
         [REGISTER_STRING_ACCOUNT.ToString()]      = "Player Account",
         [REGISTER_STRING_DEX.ToString()]          = "PokéDex",
         [REGISTER_STRING_EX.ToString()]           = "EX Raid",
         [REGISTER_STRING_INFO.ToString()]         = "Information",
         [REGISTER_STRING_NOTIFICATION.ToString()] = "Raid Notifications",
         [REGISTER_STRING_ROLE.ToString()]         = "Player Role",
         [REGISTER_STRING_RAID.ToString()]         = "Raid",
         [REGISTER_STRING_POI.ToString()]          = "Point of Interest",
      };

      /// <summary>
      /// Register value to register character.
      /// </summary>
      public static readonly Dictionary<string, string> REGISTER_VALIE_STRING = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
         [FULL_REGISTER_STRING] = FULL_REGISTER_STRING,
         ["ACCOUNT"]            = REGISTER_STRING_ACCOUNT.ToString(),
         ["A"]                  = REGISTER_STRING_ACCOUNT.ToString(),
         ["POKEDEX"]            = REGISTER_STRING_DEX.ToString(),
         ["DEX"]                = REGISTER_STRING_DEX.ToString(),
         ["D"]                  = REGISTER_STRING_DEX.ToString(),
         ["EX"]                 = REGISTER_STRING_EX.ToString(),
         ["E"]                  = REGISTER_STRING_EX.ToString(),
         ["INFORMATION"]        = REGISTER_STRING_INFO.ToString(),
         ["INFO"]               = REGISTER_STRING_INFO.ToString(),
         ["I"]                  = REGISTER_STRING_INFO.ToString(),
         ["REACT"]              = REGISTER_STRING_NOTIFICATION.ToString(),
         ["NOTIFICATION"]       = REGISTER_STRING_NOTIFICATION.ToString(),
         ["N"]                  = REGISTER_STRING_NOTIFICATION.ToString(),
         ["ROLE"]               = REGISTER_STRING_ROLE.ToString(),
         ["P"]                  = REGISTER_STRING_ROLE.ToString(),
         ["RAID"]               = REGISTER_STRING_RAID.ToString(),
         ["R"]                  = REGISTER_STRING_RAID.ToString(),
         ["POI"]                = REGISTER_STRING_POI.ToString(),
         ["POKESTOP"]           = REGISTER_STRING_POI.ToString(),
         ["STOP"]               = REGISTER_STRING_POI.ToString(),
         ["GYM"]                = REGISTER_STRING_POI.ToString(),
         ["S"]                  = REGISTER_STRING_POI.ToString(),

      };

      // Role values **********************************************************

      /// <summary>
      /// Trainer role name.
      /// </summary>
      public static readonly string ROLE_TRAINER          = "Trainer";

      /// <summary>
      /// Valor role name.
      /// </summary>
      public static readonly string ROLE_VALOR            = "Valor";

      /// <summary>
      /// Mystic role name.
      /// </summary>
      public static readonly string ROLE_MYSTIC           = "Mystic";

      /// <summary>
      /// Instinct role value.
      /// </summary>
      public static readonly string ROLE_INSTINCT         = "Instinct";

      /// <summary>
      /// Invalid team role index.
      /// </summary>
      public static readonly int ROLE_INDEX_NO_TEAM_FOUND = -1;

      /// <summary>
      /// Valor role index.
      /// </summary>
      public static readonly int ROLE_INDEX_VALOR         = 0;

      /// <summary>
      /// Mystic role index.
      /// </summary>
      public static readonly int ROLE_INDEX_MYSTIC        = 1;

      /// <summary>
      /// Instinct role index.
      /// </summary>
      public static readonly int ROLE_INDEX_INSTINCT      = 2;

      // Role colors **********************************************************

      /// <summary>
      /// Trainer role color.
      /// </summary>
      public static readonly Color ROLE_COLOR_TRAINER  = new Color(185, 187, 190);

      /// <summary>
      /// Valor role color.
      /// </summary>
      public static readonly Color ROLE_COLOR_VALOR    = new Color(153, 45, 34);

      /// <summary>
      /// Mystic role color.
      /// </summary>
      public static readonly Color ROLE_COLOR_MYSTIC   = new Color(39, 126, 205);

      /// <summary>
      /// Instinct role color.
      /// </summary>
      public static readonly Color ROLE_COLOR_INSTINCT = new Color(241, 196, 15);

      /// <summary>
      /// Raid role color.
      /// </summary>
      public static readonly Color ROLE_COLOR_RAID     = new Color(235, 52, 204);

      // Image processing *****************************************************

      /// <summary>
      /// Scaled image width.
      /// </summary>
      public static readonly int SCALE_WIDTH = 495;

      /// <summary>
      /// Scaled image height.
      /// </summary>
      public static readonly int SCALE_HEIGHT = 880;

      /// <summary>
      /// Rectange used to get nickname.
      /// </summary>
      public static readonly System.Drawing.Rectangle IMAGE_RECT_NICKNAME   = new System.Drawing.Rectangle(10, 120, 480, 100);

      /// <summary>
      /// Rectangle used to get team color.
      /// </summary>
      public static readonly System.Drawing.Rectangle IMAGE_RECT_TEAM_COLOR = new System.Drawing.Rectangle(410, 60, 10, 10);

      // Emotes ***************************************************************

      /// <summary>
      /// Nona emotes.
      /// Names will be switched out for values.
      /// </summary>
      public static readonly Dictionary<string, string> NONA_EMOJIS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      { 
         ["valor_emote"]         = "Badge_Team_Valor_01",
         ["mystic_emote"]        = "Badge_Team_Mystic_01",
         ["instinct_emote"]      = "Badge_Team_Instinct_01",

         ["raid_emote"]          = "Badge_Raid_01",
         ["ex_emote"]            = "Badge_EX_01",
         ["mega_emote"]          = "Badge_Mega_01",
         ["ex_pass_emote"]       = "Badge_EX_Pass_01",
         ["remote_pass_emote"]   = "Badge_Remote_Pass_01",

         ["bug_emote"]           = "Badge_Type_Bug_01",
         ["dark_emote"]          = "Badge_Type_Dark_01",
         ["dragon_emote"]        = "Badge_Type_Dragon_01",
         ["electric_emote"]      = "Badge_Type_Electric_01",
         ["fairy_emote"]         = "Badge_Type_Fairy_01",
         ["fighting_emote"]      = "Badge_Type_Fighting_01",
         ["fire_emote"]          = "Badge_Type_Fire_01",
         ["flying_emote"]        = "Badge_Type_Flying_01",
         ["ghost_emote"]         = "Badge_Type_Ghost_01",
         ["grass_emote"]         = "Badge_Type_Grass_01",
         ["ground_emote"]        = "Badge_Type_Ground_01",
         ["ice_emote"]           = "Badge_Type_Ice_01",
         ["normal_emote"]        = "Badge_Type_Normal_01",
         ["poison_emote"]        = "Badge_Type_Poison_01",
         ["psychic_emote"]       = "Badge_Type_Psychic_01",
         ["rock_emote"]          = "Badge_Type_Rock_01",
         ["steel_emote"]         = "Badge_Type_Steel_01",
         ["water_emote"]         = "Badge_Type_Water_01",

         ["sunny_emote"]         = "Badge_Weather_Sunny_01",
         ["clear_emote"]         = "Badge_Weather_Clear_01",
         ["rain_emote"]          = "Badge_Weather_Rain_01",
         ["partly_cloudy_emote"] = "Badge_Weather_Partly_Cloudy_01",
         ["cloudy_emote"]        = "Badge_Weather_Cloudy_01",
         ["windy_emote"]         = "Badge_Weather_Windy_01",
         ["snow_emote"]          = "Badge_Weather_Snow_01",
         ["fog_emote"]           = "Badge_Weather_Fog_01",

         ["rave_emote"]          = "ravewizard",
         ["scream_emote"]        = "rowletscrem",
         ["sink_emote"]          = "sink",
      };

      /// <summary>
      /// Number Emote names
      /// </summary>
      public static readonly Dictionary<string, string> NUM_EMOJI_NAMES = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
         ["number_1_emote"] = "number_1",
         ["number_2_emote"] = "number_2",
         ["number_3_emote"] = "number_3",
         ["number_4_emote"] = "number_4",
         ["number_5_emote"] = "number_5",
         ["number_6_emote"] = "number_6",
         ["number_7_emote"] = "number_7",
         ["number_8_emote"] = "number_8",
         ["number_9_emote"] = "number_9",
         ["number_10_emote"] = "number_10",
         ["number_11_emote"] = "number_11",
      };

      /// <summary>
      /// Max number of selections.
      /// </summary>
      public static readonly int NUM_SELECTIONS = 10;

      /// <summary>
      /// Selection emotes.
      /// </summary>
      public static readonly Emote[] SELECTION_EMOJIS = new Emote[NUM_SELECTIONS];

      /// <summary>
      /// All custom number emotes.
      /// </summary>
      public static readonly List<Emote> NUM_EMOJIS = new List<Emote>();

#if BUTTONS
      // Buttons **************************************************************

      public static readonly string SELECTION_BUTTON_PREFIX = "selection_";

      /// <summary>
      /// Builds a component builder with buttons.
      /// Giving null customIDs will auto generate selection custom ids.
      /// </summary>
      /// <param name="emotes">List of button emotes.</param>
      /// <param name="customIDs">List of component ids.</param>
      /// <returns>ComponentBuilder with buttons, otherwise null.</returns>
      public static MessageComponent BuildButtons(IEmote[] emotes, string[] customIDs = null)
      {
         string[] IDs = customIDs ?? BuildSelectionCustomIDs(emotes.Length);

         if (emotes.Length != IDs.Length)
         {
            return null;
         }

         ComponentBuilder component = new ComponentBuilder();
         for (int i = 0; i < emotes.Length; i++)
         {
            component.WithButton(customId: IDs[i], emote: emotes[i]);
         }
         return component.Build();
      }

      /// <summary>
      /// Builds custom ID array for selection components.
      /// Value passed will be constrained between 1 and NUM_SELECTIONS.
      /// </summary>
      /// <param name="numSelections">Number of selections between 1 and NUM_SELECTIONS.</param>
      /// <returns>Custom ID array for selections</returns>
      public static string[] BuildSelectionCustomIDs(int numSelections)
      {
         int selectionIndex = Math.Max(1, Math.Min(numSelections, NUM_SELECTIONS));

         string[] customIDs = new string[selectionIndex];

         for (int i = 0; i < selectionIndex; i++)
         {
            customIDs[i] = $"{SELECTION_BUTTON_PREFIX}{i + 1}";
         }

         return customIDs;
      }
#endif
   }
}