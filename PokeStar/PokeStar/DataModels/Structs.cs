using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Remove from raid result.
   /// </summary>
   public struct RaidRemoveResult
   {
      /// <summary>
      /// Group the user was removed from.
      /// </summary>
      public int Group { get; }

      /// <summary>
      /// Users that were invited by the removed user.
      /// </summary>
      public List<SocketGuildUser> Users { get; }

      /// <summary>
      /// Creates a new RaidRemoveResult.
      /// </summary>
      /// <param name="group">Group the user was removed from.</param>
      /// <param name="users">Users that were invited by the removed user.</param>
      public RaidRemoveResult(int group, List<SocketGuildUser> users)
      {
         Group = group;
         Users = users;
      }
   }

   /// <summary>
   /// Location in a raid train.
   /// </summary>
   public struct RaidTrainLoc
   {
      /// <summary>
      /// Time the raid starts.
      /// </summary>
      public string Time { get; }
      /// <summary>
      /// Location of the raid.
      /// </summary>
      public string Location { get; }

      /// <summary>
      /// Boss that is at the location.
      /// </summary>
      public string BossName { get; }

      /// <summary>
      /// Creates a new RaidTrainLoc.
      /// </summary>
      /// <param name="time">Time the raid starts.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="bossName">Boss that is at the location.</param>
      public RaidTrainLoc(string time, string location, string bossName)
      {
         Time = time;
         Location = location;
         BossName = bossName;
      }
   }

   /// <summary>
   /// Relation between types.
   /// </summary>
   public struct TypeRelation
   {
      /// <summary>
      /// Types the type is strong against.
      /// </summary>
      public Dictionary<string, double> Strong { get; }

      /// <summary>
      /// Types the type is weak against.
      /// </summary>
      public Dictionary<string, double> Weak { get; }

      /// <summary>
      /// Creates a new TypeRelation.
      /// </summary>
      /// <param name="strong">Types the type is strong against.</param>
      /// <param name="weak">Types the type is weak against.</param>
      public TypeRelation(Dictionary<string, double> strong, Dictionary<string, double> weak)
      {
         Strong = strong;
         Weak = weak;
      }
   }

   /// <summary>
   /// Chammel registration result.
   /// </summary>
   public struct RegisterResult
   {
      /// <summary>
      /// Current registration string.
      /// </summary>
      public string RegistrationString { get; }

      /// <summary>
      /// Should setup be checked for completion.
      /// </summary>
      public bool CheckSetupComplete { get; }

      /// <summary>
      /// Creates a new RegisterResult.
      /// </summary>
      /// <param name="registration">Current registration string.</param>
      /// <param name="checkSetupComplete">Should setup be checked for completion.</param>
      public RegisterResult(string registration, bool checkSetupComplete)
      {
         RegistrationString = registration;
         CheckSetupComplete = checkSetupComplete;
      }
   }

   /// <summary>
   /// Pokémon forms.
   /// </summary>
   public struct Form
   {
      /// <summary>
      /// List of all form tags.
      /// </summary>
      public List<string> FormList { get; }

      /// <summary>
      /// Default form tag.
      /// Empty string if no default form.
      /// </summary>
      public string DefaultForm { get; }

      /// <summary>
      /// Creates a new Form.
      /// </summary>
      /// <param name="fromList"></param>
      /// <param name="defaultForm"></param>
      public Form(List<string> fromList, string defaultForm)
      {
         FormList = fromList;
         DefaultForm = defaultForm;
      }
   }

   /// <summary>
   /// Pokémon used to run counter sims.
   /// </summary>
   public struct SimPokemon
   {
      /// <summary>
      /// Number of the Pokémon.
      /// </summary>
      public int Number { get; set; }

      /// <summary>
      /// Name of the Pokémon.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Fast move to use.
      /// </summary>
      public Move Fast { get; set; }

      /// <summary>
      /// Charge move to use.
      /// </summary>
      public Move Charge { get; set; }

      /// <summary>
      /// Effectiveness of the fast move.
      /// </summary>
      public double FastEffect { get; set; }

      /// <summary>
      /// Effectiveness of the charge move.
      /// </summary>
      public double ChargeEffect { get; set; }

      /// <summary>
      /// STAB modifier of the fast move.
      /// </summary>
      public double FastStab { get; set; }

      /// <summary>
      /// STAB modifier of the charge move.
      /// </summary>
      public double ChargeStab { get; set; }

      /// <summary>
      /// Shadow attack modifier.
      /// </summary>
      public double StadowAtkMul { get; set; }

      /// <summary>
      /// Shadow defense modifier.
      /// </summary>
      public double StadowDefMul { get; set; }

      /// <summary>
      /// Calculated attack stat.
      /// </summary>
      public int AtkStat { get; set; }

      /// <summary>
      /// Calculated defense stat.
      /// </summary>
      public int DefStat { get; set; }

      /// <summary>
      /// Calculated stamina stat.
      /// </summary>
      public int StamStat { get; set; }
   }

   /// <summary>
   /// Results for counter calculations.
   /// </summary>
   public struct CounterCalcResults
   {
      /// <summary>
      /// List of regular counters.
      /// </summary>
      public List<Counter> Regular { get; set; }

      /// <summary>
      /// List of special counters.
      /// Special counters are shadow and mega Pokémon.
      /// </summary>
      public List<Counter> Special { get; set; }
   }

   /// <summary>
   /// Inputs for DPS calculations.
   /// </summary>
   public struct DPSInput
   {
      /// <summary>
      /// Energy left over after the battle.
      /// </summary>
      public double X { get; }

      /// <summary>
      /// DPS of the boss Pokémon.
      /// </summary>
      public double Y { get; }

      /// <summary>
      /// Creates a new DPSInput.
      /// </summary>
      /// <param name="x">Energy left over after the battle.</param>
      /// <param name="y">DPS of the boss Pokémon.</param>
      public DPSInput(double x, double y)
      {
         X = x;
         Y = y;
      }
   }

   /// <summary>
   /// Raid sub message.
   /// </summary>
   public struct RaidSubMessage
   {
      /// <summary>
      /// Type of sub message.
      /// </summary>
      public int Type { get; }

      /// <summary>
      /// Base message Id.
      /// </summary>
      public ulong MainMessageId { get; }

      /// <summary>
      /// Creates a new RaidSubMessage.
      /// </summary>
      /// <param name="type">Type of sub message.</param>
      /// <param name="mainMessageId">Base message Id.</param>
      public RaidSubMessage(int type, ulong mainMessageId)
      {
         Type = type;
         MainMessageId = mainMessageId;
      }
   }

   /// <summary>
   /// Location in a raid train.
   /// </summary>
   public struct RaidGuideSelect
   {
      /// <summary>
      /// Current selection page.
      /// </summary>
      public int Page { get; }
      /// <summary>
      /// List of bosses.
      /// </summary>
      public List<string> Bosses { get; }

      /// <summary>
      /// Tier of bosses.
      /// </summary>
      public short Tier { get; }

      /// <summary>
      /// Creates a new initial RaidTrainLoc.
      /// </summary>
      /// <param name="tier">Tier of bosses.</param>
      /// <param name="bosses">List of bosses.</param>
      public RaidGuideSelect(short tier, List<string> bosses)
      {
         Page = 0;
         Tier = tier;
         Bosses = bosses;
      }

      /// <summary>
      /// Creates a new RaidTrainLoc.
      /// </summary>
      /// <param name="page">Current selection page.</param>
      /// <param name="tier">Tier of bosses.</param>
      /// <param name="bosses">List of bosses.</param>
      public RaidGuideSelect(int page, short tier, List<string> bosses)
      {
         Page = page;
         Tier = tier;
         Bosses = bosses;
      }
   }

   /// <summary>
   /// Dex selection message.
   /// </summary>
   public struct DexSelectionMessage
   {
      /// <summary>
      /// Type of selection message.
      /// </summary>
      public int Type { get; }

      /// <summary>
      /// Possible selections.
      /// </summary>
      public List<string> Selections { get; }

      /// <summary>
      /// Creates a new DexSelectionMessage.
      /// </summary>
      /// <param name="type">Type of selection message.</param>
      /// <param name="selections">Possible selections.</param>
      public DexSelectionMessage(int type, List<string> selections)
      {
         Type = type;
         Selections = selections;
      }
   }

   /// <summary>
   /// Help message.
   /// </summary>
   public struct HelpMessage
   {
      /// <summary>
      /// Page the message is on. 0 based.
      /// </summary>
      public int Page { get; set; }

      /// <summary>
      /// All commands that can be shown.
      /// </summary>
      public List<CommandInfo> Commands { get; }

      /// <summary>
      /// Creates a new HelpMessage.
      /// </summary>
      /// <param name="commands">All commands that can be shown.</param>
      /// <param name="page">Current Page. Defaults to 0.</param>
      public HelpMessage(List<CommandInfo> commands, int page = 0)
      {
         Page = page;
         Commands = commands;
      }
   }
}