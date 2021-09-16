using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.Calculators;
using PokeStar.ConnectionInterface;

namespace PokeStar.ModuleParents
{
   /// <summary>
   /// Parent for dex command modules.
   /// </summary>
   public class DexCommandParent : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Empty Pokémon image file name.
      /// </summary>
      protected static readonly string POKEDEX_SELECTION_IMAGE = "quest_pokemon.png";

      /// <summary>
      /// Generic dex image file name.
      /// </summary>
      protected static readonly string GENERIC_IMAGE = "battle.png";

      // Message holders ******************************************************

      /// <summary>
      /// Saved dex selection messages.
      /// </summary>
      protected static readonly Dictionary<ulong, DexSelectionMessage> dexSelectMessages = new Dictionary<ulong, DexSelectionMessage>();

      /// <summary>
      /// Saved dex messages.
      /// </summary>
      protected static readonly Dictionary<ulong, Pokemon> dexMessages = new Dictionary<ulong, Pokemon>();

      /// <summary>
      /// Saved catch messages.
      /// </summary>
      protected static readonly Dictionary<ulong, CatchSimulation> catchMessages = new Dictionary<ulong, CatchSimulation>();

      // Emotes ***************************************************************

      /// <summary>
      /// Emotes for a dex message.
      /// </summary>
      private static readonly IEmote[] dexEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
         new Emoji("7️⃣"),
         new Emoji("❓"),
      };

      /// <summary>
      /// Emotes for a catch message.
      /// </summary>
      protected static readonly Emoji[] catchEmojis = {
         new Emoji("⬅️"),
         new Emoji("⏺️"),
         new Emoji("➡️"),
         new Emoji("❓"),
      };

      /// <summary>
      /// Descriptions for dex emotes.
      /// </summary>
      private static readonly string[] dexEmojisDesc = {
         "means switch to the Main dex page.",
         "means switch to the CP page.",
         "means switch to the Evolution page.",
         "means switch to the Counter page.",
         "means switch to the PvP IV page.",
         "means switch to the Form page.",
         "means switch to the Nickname page.",
      };

      /// <summary>
      /// Descriptions for catch emotes.
      /// </summary>
      private static readonly string[] catchEmojisDesc = {
         "means decrement current modifier value.",
         "means cycle through modifiers to edit.",
         "means increment current modifier value."
      };

      /// <summary>
      /// Replies for a catch message.
      /// </summary>
      private static readonly string[] catchReplies = {
         "level <level>",
         "radius <radius>"
      };

      // Enumerations *********************************************************

      /// <summary>
      /// Types of dex sub messages.
      /// </summary>
      protected enum DEX_MESSAGE_TYPES
      {
         /// <summary>
         /// Dex message type.
         /// </summary>
         DEX_MESSAGE,

         /// <summary>
         /// CP message type.
         /// </summary>
         CP_MESSAGE,

         /// <summary>
         /// Evo message type.
         /// </summary>
         EVO_MESSAGE,

         /// <summary>
         /// Counter message type.
         /// </summary>
         COUNTER_MESSAGE,

         /// <summary>
         /// PvP message type.
         /// </summary>
         PVP_MESSAGE,

         /// <summary>
         /// Form message type.
         /// </summary>
         FORM_MESSAGE,

         /// <summary>
         /// Nickname message type.
         /// </summary>
         NICKNAME_MESSAGE,

         /// <summary>
         /// Catch message type.
         /// </summary>
         CATCH_MESSAGE,

         /// <summary>
         /// Move message type.
         /// </summary>
         MOVE_MESSAGE,
      }

      /// <summary>
      /// Index of emotes on a dex message.
      /// </summary>
      private enum DEX_EMOJI_INDEX
      {
         DEX_MESSAGE,
         CP_MESSAGE,
         EVO_MESSAGE,
         COUNTER_MESSAGE,
         PVP_MESSAGE,
         FORM_MESSAGE,
         NICKNAME_MESSAGE,
         HELP,
      }

      /// <summary>
      /// Index of emotes on a catch message.
      /// </summary>
      private enum CATCH_EMOJI_INDEX
      {
         DECREMENT,
         MODIFIER,
         INCREMENT,
         HELP,
      }

      // Message checkers *****************************************************

      /// <summary>
      /// Checks if a message is a dex select message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a dex select message, otherwise false.</returns>
      public static bool IsDexSelectMessage(ulong id)
      {
         return dexSelectMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a dex message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a dex select message, otherwise false.</returns>
      public static bool IsDexMessage(ulong id)
      {
         return dexMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a catch message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a catch message, otherwise false.</returns>
      public static bool IsCatchMessage(ulong id)
      {
         return catchMessages.ContainsKey(id);
      }

      // Message reaction handlers ********************************************

      /// <summary>
      /// Handles a reaction on a dex select message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="guildId">Id of the guild that the message was sent in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task DexSelectMessageReactionHandle(IMessage message, SocketReaction reaction, ulong guildId)
      {
         DexSelectionMessage dexMessage = dexSelectMessages[message.Id];
         for (int i = 0; i < dexMessage.Selections.Count; i++)
         {
            if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
            {
               await message.DeleteAsync();
               if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.DEX_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  Connections.Instance().GetPokemonStats(ref pokemon);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildDexEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.CP_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  Connections.GetPokemonCP(ref pokemon);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildCPEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  Connections.Instance().GetPokemonCounter(ref pokemon);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildCounterEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.EVO_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  pokemon.Evolutions = GenerateEvoDict(pokemon.Name);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildEvoEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.PVP_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  Connections.Instance().GetPokemonPvP(ref pokemon);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildPvPEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.FORM_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemon.Number);

                  if (pokemonWithNumber.Count == 1)
                  {
                     pokemon.Forms = new Form();
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                     pokemon.Forms = Connections.Instance().GetFormTags(baseName);
                  }
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildFormEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  pokemon.Nicknames = Connections.Instance().GetNicknames(guildId, pokemon.Name);
                  await SendDexMessage(pokemon, BuildNicknameEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.MOVE_MESSAGE)
               {
                  Move pkmnMove = Connections.Instance().GetMove(dexMessage.Selections[i]);
                  string fileName = GENERIC_IMAGE;
                  Connections.CopyFile(fileName);
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildMoveEmbed(pkmnMove, fileName));
                  Connections.DeleteFile(fileName);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.CATCH_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  CatchSimulation catchSim = new CatchSimulation(pokemon);
                  string fileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(fileName);
                  RestUserMessage catchMessage = await reaction.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
                  catchMessages.Add(catchMessage.Id, catchSim);
                  Connections.DeleteFile(fileName);
                  catchMessage.AddReactionsAsync(catchEmojis);
               }
               dexSelectMessages.Remove(message.Id);
               return;
            }
         }
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// <summary>
      /// Handles a reaction on a dex message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="guildId">Id of the guild that the message was sent in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task DexMessageReactionHandle(IMessage message, SocketReaction reaction, ulong guildId)
      {
         Pokemon pokemon = dexMessages[message.Id];
         SocketUserMessage msg = (SocketUserMessage)message;
         string fileName = Connections.GetPokemonPicture(pokemon.Name);
         Connections.CopyFile(fileName);
         if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.HELP]))
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**Dex Message Emoji Help:**");

            for (int i = 0; i < dexEmojis.Length - 1; i++)
            {
               sb.AppendLine($"{dexEmojis[i]} {dexEmojisDesc[i]}");
            }
            await reaction.User.Value.SendMessageAsync(sb.ToString());
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.DEX_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE])
            {
               Connections.Instance().GetPokemonStats(ref pokemon);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildDexEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.CP_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE])
            {
               Connections.GetPokemonCP(ref pokemon);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildCPEmbed(pokemon, fileName);
            });
         }

         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.EVO_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE])
            {
               pokemon.Evolutions = GenerateEvoDict(pokemon.Name);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildEvoEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.COUNTER_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE])
            {
               Connections.Instance().GetPokemonCounter(ref pokemon);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildCounterEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.PVP_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE])
            {
               Connections.Instance().GetPokemonPvP(ref pokemon);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildPvPEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.FORM_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE])
            {
               List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemon.Number);

               if (pokemonWithNumber.Count == 1)
               {
                  pokemon.Forms = new Form();
               }
               else if (pokemonWithNumber.Count > 1)
               {
                  string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                  pokemon.Forms = Connections.Instance().GetFormTags(baseName);
               }
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildFormEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_EMOJI_INDEX.NICKNAME_MESSAGE]))
         {
            pokemon.Nicknames = Connections.Instance().GetNicknames(guildId, pokemon.Name);
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildNicknameEmbed(pokemon, fileName);
            });
         }
         Connections.DeleteFile(fileName);
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// <summary>
      /// Handles a reaction on a catch message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task CatchMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         CatchSimulation catchSim = catchMessages[message.Id];
         bool needsUpdate = true;
         if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.DECREMENT]))
         {
            catchSim.DecrementModifierValue();
         }
         else if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.MODIFIER]))
         {
            catchSim.UpdateModifier();
         }
         else if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.INCREMENT]))
         {
            catchSim.IncrementModifierValue();
         }
         else if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.HELP]))
         {
            string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("**Catch Emoji Help:**");
            for (int i = 0; i < catchEmojisDesc.Length; i++)
            {
               sb.AppendLine($"{catchEmojis[i]} {catchEmojisDesc[i]}");
            }

            sb.AppendLine("\n**Catch Reply Help:**");
            foreach (string reply in catchReplies)
            {
               sb.AppendLine($"{prefix}{reply}");
            }

            await ((SocketGuildUser)reaction.User).SendMessageAsync(sb.ToString());
            needsUpdate = false;
         }
         else
         {
            needsUpdate = false;
         }

         if (needsUpdate)
         {
            SocketUserMessage msg = (SocketUserMessage)message;
            string fileName = Connections.GetPokemonPicture(catchSim.Pokemon.Name);
            Connections.CopyFile(fileName);
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildCatchEmbed(catchSim, fileName);
            });
            Connections.DeleteFile(fileName);
         }

         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      // Embed builders *******************************************************

      /// <summary>
      /// Builds a dex embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon.</returns>
      protected static Embed BuildDexEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{pokemon.Number} {pokemon.Name}");
         embed.WithDescription(pokemon.Description);
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Type", pokemon.TypeToString(), true);
         embed.AddField("Weather Boosts", pokemon.WeatherToString(), true);
         embed.AddField("Status", pokemon.StatusToString(), true);
         embed.AddField("Resistances", pokemon.ResistanceToString(), true);
         embed.AddField("Weaknesses", pokemon.WeaknessToString(), true);
         embed.AddField("Stats", pokemon.StatsToString(), true);
         embed.AddField("Fast Moves", pokemon.FastMoveToString(), true);
         embed.AddField("Charge Moves", pokemon.ChargeMoveToString(), true);
         embed.AddField("Details", pokemon.DetailsToString(), true);
         if (pokemon.IsRegional())
         {
            embed.AddField("Regions", pokemon.RegionalToString(), true);
         }
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithFooter($"{Global.STAB_SYMBOL} denotes STAB move.\n {Global.LEGACY_MOVE_SYMBOL} denotes Legacy move.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a cp embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's CP.</returns>
      protected static Embed BuildCPEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"Max CP values for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField($"Max Half Level CP (Level {Global.MAX_REG_LEVEL})", pokemon.CPMaxHalf, true);
         embed.AddField($"Max CP (Level {Global.MAX_XL_LEVEL})", pokemon.CPMax, true);
         embed.AddField($"Max Buddy CP (Level {Global.MAX_XL_LEVEL + Global.BUDDY_BOOST})", pokemon.CPBestBuddy, true);
         embed.AddField($"Raid CP (Level {Global.RAID_LEVEL})", pokemon.RaidCPToString(), true);
         embed.AddField($"Hatch CP (Level {Global.HATCH_LEVEL})", pokemon.HatchCPToString(), true);
         embed.AddField($"Quest CP (Level {Global.QUEST_LEVEL})", pokemon.QuestCPToString(), true);
         embed.AddField($"Shadow CP (Level {Global.SHADOW_LEVEL})", pokemon.ShadowCPToString(), false);
         embed.AddField($"Wild CP (Level {Global.MIN_WILD_LEVEL}-{Global.MAX_WILD_LEVEL})", pokemon.WildCPToString(), false);

         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithFooter($"{Global.WEATHER_BOOST_SYMBOL} denotes Weather Boosted CP.\n" +
                          $"Weather boosted level is {Global.WEATHER_BOOST} levels over base level.");
         return embed.Build();
      }

      /// <summary>
      /// Builds an evolution embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's evolution family.</returns>
      protected static Embed BuildEvoEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($"Evolution Family for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         if (pokemon.Evolutions.Count == 1)
         {
            embed.WithDescription("This Pokémon does not evolve in to or from any other Pokémon.");
         }
         else
         {
            foreach (KeyValuePair<string, string> pkmn in pokemon.Evolutions)
            {
               embed.AddField($"{pkmn.Key}", pkmn.Value);
            }
         }
         return embed.Build();
      }

      /// <summary>
      /// Builds a counter embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's counters.</returns>
      protected static Embed BuildCounterEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle(pokemon.Number == Global.DUMMY_POKE_NUM ? $@"Top general DPS Pokémon" : $@"Top counters for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Normal Counters", pokemon.CounterToString());
         embed.AddField("Special Counters", pokemon.SpecialCounterToString());
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithFooter($"Special Counters inclued top Mega and Shadow counters.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a pvp embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's best PvP IVs.</returns>
      protected static Embed BuildPvPEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"Max PvP IV values for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         if (pokemon.CanBeLittleLeague)
         {
            embed.AddField($"Little League (Max Level 41)", pokemon.LittleIVs, false);
            embed.AddField($"Little League XL (Max Level 51)", pokemon.LittleXLIVs, false);
         }
         embed.AddField($"Great League (Max Level 41)", pokemon.GreatIVs, false);
         embed.AddField($"Great League XL (Max Level 51)", pokemon.GreatXLIVs, false);
         embed.AddField($"Ultra League (Max Level 41)", pokemon.UltraIVs, false);
         embed.AddField($"Ultra League XL (Max Level 51)", pokemon.UltraXLIVs, false);
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         return embed.Build();
      }

      /// <summary>
      /// Builds a form embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's form differences.</returns>
      protected static Embed BuildFormEmbed(Pokemon pokemon, string fileName)
      {

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithFooter($"{Global.DEFAULT_FORM_SYMBOL} denotes default form.");

         StringBuilder sb = new StringBuilder();
         if (pokemon.Forms.FormList == null)
         {
            sb.AppendLine($"There are no alternate forms for {pokemon.Name}.");
         }
         else
         {
            foreach (string form in pokemon.Forms.FormList)
            {
               sb.AppendLine($"{form}{(form.Equals(pokemon.Forms.DefaultForm, StringComparison.OrdinalIgnoreCase) ? $"{Global.DEFAULT_FORM_SYMBOL}" : "")}");
            }
         }
         embed.AddField($"Forms for {pokemon.Name}", sb.ToString(), false);
         return embed.Build();
      }

      /// <summary>
      /// Builds a nickname embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's nicknames.</returns>
      protected static Embed BuildNicknameEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);

         if (pokemon.Nicknames.Count == 0)
         {
            embed.WithTitle($"There are no nicknames registered for {pokemon.Name}.");
         }
         else
         {
            StringBuilder sb = new StringBuilder();
            foreach (string nickname in pokemon.Nicknames)
            {
               sb.AppendLine(nickname);
            }
            embed.AddField($"**Nicknames for {pokemon.Name}**", sb.ToString());
         }
         return embed.Build();
      }

      /// <summary>
      /// Builds a move embed.
      /// </summary>
      /// <param name="move">Move to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a move.</returns>
      protected static Embed BuildMoveEmbed(Move move, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle(move.Name);
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Type", move.TypeToString(), true);
         embed.AddField("Weather Boosts", move.WeatherToString(), true);
         embed.AddField("Category", move.Category, true);
         embed.AddField("PvP Power", move.PvPPower, true);
         embed.AddField("PvP Energy", move.EnergyToString(move.PvPEnergy), true);
         embed.AddField("PvP Turns", move.PvPTurns, true);
         embed.AddField("PvE Power", move.PvEPower, true);
         embed.AddField("PvE Energy", move.EnergyToString(move.PvEEnergy), true);
         embed.AddField("PvE Cooldown", $"{move.Cooldown} ms", true);
         embed.AddField("PvE Damage Window", move.DamageWindowString(), true);

         if (move.BuffChance != 0)
         {
            embed.AddField("PvP Buff:", move.BuffString(), false);
         }

         embed.AddField("Number of Pokémon that can learn this move", move.PokemonWithMove, false);
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         return embed.Build();
      }

      /// <summary>
      /// Builds a catch embed.
      /// </summary>
      /// <param name="catchSim">Catch simulator to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a catch simulation.</returns>
      protected static Embed BuildCatchEmbed(CatchSimulation catchSim, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{catchSim.Pokemon.Number} {catchSim.Pokemon.Name}");
         embed.WithDescription($"**Catch Chance:** {catchSim.CatchChance}%");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField($"Base Catch Rate:", $"{catchSim.Pokemon.CatchRate * 100.0}%", true);
         embed.AddField("Pokémon Level:", $"{catchSim.GetLevel()}", true);
         embed.AddField("Pokéball Type:", $"{catchSim.GetBall()}", true);
         embed.AddField("Berry Type:", $"{catchSim.GetBerry()}", true);
         embed.AddField("Throw Type:", $"{catchSim.GetThrow()}", true);
         embed.AddField("Is Curveball:", $"{catchSim.GetCurveball()}", true);
         embed.AddField("Medal 1 Bonus:", $"{catchSim.GetMedal1()}", true);
         if (catchSim.Pokemon.Type.Count != 1)
         {
            embed.AddField("Medal 2 Bonus:", $"{catchSim.GetMedal2()}", true);
         }
         embed.AddField("Encounter Type:", $"{catchSim.GetEncounter()}", true);
         embed.WithColor(catchSim.CalcRingColor());
         embed.WithFooter($"Currently editing: {catchSim.GetCurrentModifier()}");
         return embed.Build();
      }

      /// <summary>
      /// Builds the PokéDex select embed.
      /// </summary>
      /// <param name="potentials">List of potential Pokémon.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for selecting a Pokémon.</returns>
      protected static Embed BuildDexSelectEmbed(List<string> potentials, string fileName)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithTitle("Do you mean...?");
         embed.WithDescription(sb.ToString());
         embed.WithThumbnailUrl($"attachment://{fileName}");
         return embed.Build();
      }

      // Name processors ******************************************************

      /// <summary>
      /// Processes the Pokémon name given from a command.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Full name of the Pokémon.</returns>
      protected static string GetPokemonName(string pokemonName)
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
      /// Gets the full name of a Pokémon.
      /// The following Pokémon have multiple forms:
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
      /// Note: Nidoran defaults to the female form.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="form">Form of the Pokémon.</param>
      /// <returns>Full name of the Pokémon.</returns>
      private static string GetFullName(string pokemonName, string form = "")
      {
         if (form.Length == 2)
         {
            string mega = "";
            if ((form.Equals("-x", StringComparison.OrdinalIgnoreCase) || form.Equals("-y", StringComparison.OrdinalIgnoreCase)) &&
                (pokemonName.Equals("charizard", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("mewtwo", StringComparison.OrdinalIgnoreCase)))
               mega = "mega ";
            return $"{mega}{pokemonName} {form.ToCharArray()[1]}";
         }
         // Alolan
         else if (form.Equals("-alola", StringComparison.OrdinalIgnoreCase) || form.Equals("-alolan", StringComparison.OrdinalIgnoreCase))
            return $"Alolan {pokemonName}";
         // Galarian
         else if (form.Equals("-galar", StringComparison.OrdinalIgnoreCase) || form.Equals("-galarian", StringComparison.OrdinalIgnoreCase))
            return $"Galarian {pokemonName}";
         // Mega and Primal
         else if ((form.Equals("-mega", StringComparison.OrdinalIgnoreCase) || form.Equals("-primal", StringComparison.OrdinalIgnoreCase)) && (pokemonName.Equals("kyogre", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("groudon", StringComparison.OrdinalIgnoreCase)))
            return $"Primal {pokemonName}";
         else if (form.Equals("-megay", StringComparison.OrdinalIgnoreCase) || form.Equals("-mega-y", StringComparison.OrdinalIgnoreCase) || (form.Equals("-mega", StringComparison.OrdinalIgnoreCase) && (pokemonName.Equals("charizard", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("mewtwo", StringComparison.OrdinalIgnoreCase))))
            return $"Mega {pokemonName} Y";
         else if (form.Equals("-megax", StringComparison.OrdinalIgnoreCase) || form.Equals("-mega-x", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName} X";
         else if (form.Equals("-mega", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName}";
         // Gigantamax and Eternamax
         else if (form.Equals("-max", StringComparison.OrdinalIgnoreCase))
            if (pokemonName.Equals("eternatus", StringComparison.OrdinalIgnoreCase))
               return $"Eternamax {pokemonName}";
            else
               return $"Gigantamax {pokemonName}";
         // Gender
         else if (form.Equals("-male", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("nidoran", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("pyroar", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("meowstic", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("indeedee", StringComparison.OrdinalIgnoreCase))))
            return $"{pokemonName} M";
         // Unown and Gender
         else if (form.Equals("-female", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} F";
         // Mewtwo
         else if (form.Equals("-armor", StringComparison.OrdinalIgnoreCase) || form.Equals("-armored", StringComparison.OrdinalIgnoreCase))
            return $"Armored {pokemonName}";
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
         // Arceus, Silvally, and Calyrex
         else if (form.Equals("-normal", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("arceus", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("silvally", StringComparison.OrdinalIgnoreCase))))
            return $"{pokemonName} Normal";
         else if (form.Equals("-bug", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Bug";
         else if (form.Equals("-dark", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dark";
         else if (form.Equals("-dragon", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dragon";
         else if (form.Equals("-electric", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Electric";
         else if (form.Equals("-fairy", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fairy";
         else if (form.Equals("-fighting", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fighting";
         else if (form.Equals("-fire", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fire";
         else if (form.Equals("-flying", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Flying";
         else if (form.Equals("-ghost", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ghost";
         else if (form.Equals("-grass", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Grass";
         else if (form.Equals("-ground", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ground";
         else if (form.Equals("-poison", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Poison";
         else if (form.Equals("-psychic", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Psychic";
         else if (form.Equals("-rock", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Rock";
         else if (form.Equals("-steel", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Steel";
         else if (form.Equals("-water", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Water";
         else if (form.Equals("-ice", StringComparison.OrdinalIgnoreCase))
            if (pokemonName.Equals("calyrex", StringComparison.OrdinalIgnoreCase))
               return $"Ice Rider {pokemonName}";
            else
               return $"{pokemonName} Ice";
         else if (form.Equals("-shadow", StringComparison.OrdinalIgnoreCase))
            return $"Shadow Rider {pokemonName}";
         // Darmanitan
         else if (form.Equals("-zen", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Zen Mode";
         else if (form.Equals("-galar-zen", StringComparison.OrdinalIgnoreCase) || form.Equals("-galarian-zen", StringComparison.OrdinalIgnoreCase))
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
         // Genesect
         else if (form.Equals("-burn", StringComparison.OrdinalIgnoreCase))
            return $"Burn Drive {pokemonName}";
         else if (form.Equals("-chill", StringComparison.OrdinalIgnoreCase))
            return $"Chill Drive {pokemonName}";
         else if (form.Equals("-douse", StringComparison.OrdinalIgnoreCase))
            return $"Douse Drive {pokemonName}";
         else if (form.Equals("-shock", StringComparison.OrdinalIgnoreCase))
            return $"Shock Drive {pokemonName}";
         // Vivillon
         else if (form.Equals("-poke", StringComparison.OrdinalIgnoreCase) || form.Equals("-poke-ball", StringComparison.OrdinalIgnoreCase) || form.Equals("-pokeball", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("vivillon", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Poke Ball Pattern";
         else if (form.Equals("-fancy", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fancy Pattern";
         else if (form.Equals("-archipelago", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Archipelago Pattern";
         else if (form.Equals("-continental", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Continental Pattern";
         else if (form.Equals("-elegant", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Elegant Pattern";
         else if (form.Equals("-garden", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Garden Pattern";
         else if (form.Equals("-plains", StringComparison.OrdinalIgnoreCase) || form.Equals("-high-plains", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} High Plains Pattern";
         else if (form.Equals("-icy", StringComparison.OrdinalIgnoreCase) || form.Equals("-icy-snow", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Icy Snow Pattern";
         else if (form.Equals("-jungle", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Jungle Pattern";
         else if (form.Equals("-marine", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Marine Pattern";
         else if (form.Equals("-meadow", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Meadow Pattern";
         else if (form.Equals("-modern", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Modern Pattern";
         else if (form.Equals("-monsoon", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Monsoon Pattern";
         else if (form.Equals("-ocean", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ocean Pattern";
         else if (form.Equals("-polar", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Polar Pattern";
         else if (form.Equals("-river", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} River Pattern";
         else if (form.Equals("-sandstorm", StringComparison.OrdinalIgnoreCase) || form.Equals("-sand", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Sandstorm Pattern";
         else if (form.Equals("-savanna", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Savanna Pattern";
         else if (form.Equals("-sun", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Sun Pattern";
         else if (form.Equals("-tundra", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Tundra Pattern";
         // Basculin, Flabébé, Floette, Florges and Minior
         else if (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("basculin", StringComparison.OrdinalIgnoreCase))
            return $"Blue Striped {pokemonName}";
         else if (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("flabebe", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("floette", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("florges", StringComparison.OrdinalIgnoreCase)))
            return $"Red Flower {pokemonName}";
         else if (form.Equals("-meteor", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("minior", StringComparison.OrdinalIgnoreCase)))
            return "Minior Meteor Core";
         else if (form.Equals("-red", StringComparison.OrdinalIgnoreCase))
            if (pokemonName.Equals("basculin", StringComparison.OrdinalIgnoreCase))
               return $"Blue Striped {pokemonName}";
            else if (pokemonName.Equals("minior", StringComparison.OrdinalIgnoreCase))
               return $"{pokemonName} Red Core";
            else
               return $"Red Flower {pokemonName}";
         else if (form.Equals("-blue", StringComparison.OrdinalIgnoreCase))
            if (pokemonName.Equals("basculin", StringComparison.OrdinalIgnoreCase))
               return $"Blue Striped {pokemonName}";
            else if (pokemonName.Equals("minior", StringComparison.OrdinalIgnoreCase))
               return $"{pokemonName} Blue Core";
            else
               return $"Blue Flower {pokemonName}";
         else if (form.Equals("-orange", StringComparison.OrdinalIgnoreCase))
            if (pokemonName.Equals("minior", StringComparison.OrdinalIgnoreCase))
               return $"{pokemonName} Orange Core";
            else
               return $"Orange Flower {pokemonName}";
         else if (form.Equals("-yellow", StringComparison.OrdinalIgnoreCase))
            if (pokemonName.Equals("minior", StringComparison.OrdinalIgnoreCase))
               return $"{pokemonName} Yellow Core";
            else
               return $"Yellow Flower {pokemonName}";
         else if (form.Equals("-green", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Green Core";
         else if (form.Equals("-indigo", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Indigo Core";
         else if (form.Equals("-violet", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Violet Core";
         else if (form.Equals("-white", StringComparison.OrdinalIgnoreCase))
            return $"White Flower {pokemonName}";
         // Furfrou
         else if (form.Equals("-heart", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Heart Trim";
         else if (form.Equals("-star", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Star Trim";
         else if (form.Equals("-diamond", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Diamond Trim";
         else if (form.Equals("-debutante", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Debutante Trim";
         else if (form.Equals("-matron", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Matron Trim";
         else if (form.Equals("-dandy", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dandy Trim";
         else if (form.Equals("-reine", StringComparison.OrdinalIgnoreCase) || form.Equals("-la-reine", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} La Reine Trim";
         else if (form.Equals("-kabuki", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Kabuki Trim";
         else if (form.Equals("-pharaoh", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Pharaoh Trim";
         // Pumpkaboo and Gourgeist
         else if (form.Equals("-average", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("pumpkaboo", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("gourgeist", StringComparison.OrdinalIgnoreCase))))
            return $"{pokemonName} Average Size";
         else if (form.Equals("-small", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Small Size";
         else if (form.Equals("-large", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Large Size";
         else if (form.Equals("-super", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Super Size";
         // Aegislash
         else if (form.Equals("-blade", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("aegislash", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Blade Form";
         else if (form.Equals("-shield", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Shield Form";
         // Hoopa
         else if (form.Equals("-confined", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("hoopa", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Confined";
         else if (form.Equals("-unbound", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Unbound";
         // Zygarde
         else if (form.Equals("-50", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("zygarde", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} 50% Form";
         else if (form.Equals("-10", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} 10% Form";
         else if (form.Equals("-complete", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Complete Form";
         // Oricorio
         else if (form.Equals("-baile", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("oricorio", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Baile Style";
         else if (form.Equals("-pom", StringComparison.OrdinalIgnoreCase) || form.Equals("-pom-pom", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Pom-Pom Style";
         else if (form.Equals("-pau", StringComparison.OrdinalIgnoreCase) || form.Equals("-pa\'u", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Pa\'u Style Form";
         else if (form.Equals("-sensu", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Sensu Style Form";
         // Lycanroc
         else if (form.Equals("-midday", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("lycanroc", StringComparison.OrdinalIgnoreCase)))
            return $"Midday {pokemonName}";
         else if (form.Equals("-dusk", StringComparison.OrdinalIgnoreCase))
            return $"Dusk {pokemonName}";
         else if (form.Equals("-midnight", StringComparison.OrdinalIgnoreCase))
            return $"Midnight {pokemonName}";
         // Wishiwashi
         else if (form.Equals("-solo", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("wishiwashi", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Solo";
         else if (form.Equals("-school", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} School";
         // Mimikyu
         else if (form.Equals("-busted", StringComparison.OrdinalIgnoreCase))
            return $"Busted {pokemonName}";
         // Solgaleo
         else if (form.Equals("-radiant", StringComparison.OrdinalIgnoreCase) || form.Equals("-radiant-sun", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Radiant Sun Phase";
         // Lunala
         else if (form.Equals("-full", StringComparison.OrdinalIgnoreCase) || form.Equals("-full-moon", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Full Moon Phase";
         // Necrozma
         else if (form.Equals("-dawn", StringComparison.OrdinalIgnoreCase) || form.Equals("-dawn-wings", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dawn Wings";
         else if (form.Equals("-dusk", StringComparison.OrdinalIgnoreCase) || form.Equals("-dusk-mane", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dusk Mane";
         else if (form.Equals("-ultra", StringComparison.OrdinalIgnoreCase))
            return $"Ultra {pokemonName}";
         // Magearna
         else if (form.Equals("-original", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Original";
         // Marshadow
         else if (form.Equals("-zenith", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Zenith";
         // Cramorant
         else if (form.Equals("-gorge", StringComparison.OrdinalIgnoreCase) || form.Equals("-gorging", StringComparison.OrdinalIgnoreCase))
            return $"Gulping {pokemonName}";
         else if (form.Equals("-gulp", StringComparison.OrdinalIgnoreCase) || form.Equals("-gulping", StringComparison.OrdinalIgnoreCase))
            return $"Gorging {pokemonName}";
         // Toxtricity
         else if (form.Equals("-amped", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("toxtricity", StringComparison.OrdinalIgnoreCase)))
            return $"Amped {pokemonName}";
         else if (form.Equals("-low", StringComparison.OrdinalIgnoreCase) || form.Equals("-low-key", StringComparison.OrdinalIgnoreCase) || form.Equals("-lowkey", StringComparison.OrdinalIgnoreCase))
            return $"Low Key {pokemonName}";
         // Sinistea and Polteageist
         else if (form.Equals("-phony", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("sinistea", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("polteageist", StringComparison.OrdinalIgnoreCase))))
            return $"Phony {pokemonName}";
         else if (form.Equals("-antique", StringComparison.OrdinalIgnoreCase))
            return $"Antique {pokemonName}";
         //Alcremie
         else if (form.Equals("-vanilla-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-vanilla", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("Alcremie", StringComparison.OrdinalIgnoreCase)))
            return $"Strawberry Sweet Vanilla Cream {pokemonName}";
         else if (form.Equals("-vanilla-berry", StringComparison.OrdinalIgnoreCase) || form.Equals("-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Vanilla Cream {pokemonName}";
         else if (form.Equals("-vanilla-clover", StringComparison.OrdinalIgnoreCase) || form.Equals("-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Vanilla Cream {pokemonName}";
         else if (form.Equals("-vanilla-flower", StringComparison.OrdinalIgnoreCase) || form.Equals("-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Vanilla Cream {pokemonName}";
         else if (form.Equals("-vanilla-love", StringComparison.OrdinalIgnoreCase) || form.Equals("-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Vanilla Cream {pokemonName}";
         else if (form.Equals("-vanilla-ribbon", StringComparison.OrdinalIgnoreCase) || form.Equals("-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Vanilla Cream {pokemonName}";
         else if (form.Equals("-vanilla-star", StringComparison.OrdinalIgnoreCase) || form.Equals("-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Vanilla Cream {pokemonName}";
         else if (form.Equals("-caramel-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-caramel", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Caramel Swirl {pokemonName}";
         else if (form.Equals("-caramel-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Caramel Swirl {pokemonName}";
         else if (form.Equals("-caramel-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Caramel Swirl {pokemonName}";
         else if (form.Equals("-caramel-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Caramel Swirl {pokemonName}";
         else if (form.Equals("-caramel-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Caramel Swirl {pokemonName}";
         else if (form.Equals("-caramel-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Caramel Swirl {pokemonName}";
         else if (form.Equals("-caramel-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Caramel Swirl {pokemonName}";
         else if (form.Equals("-rubyc-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-rubyc", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Ruby Cream {pokemonName}";
         else if (form.Equals("-rubyc-berry", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Ruby Cream {pokemonName}";
         else if (form.Equals("-rubyc-clover", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Ruby Cream {pokemonName}";
         else if (form.Equals("-rubyc-flower", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Ruby Cream {pokemonName}";
         else if (form.Equals("-rubyc-love", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Ruby Cream {pokemonName}";
         else if (form.Equals("-rubyc-ribbon", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Ruby Cream {pokemonName}";
         else if (form.Equals("-rubyc-star", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-c-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Ruby Cream {pokemonName}";
         else if (form.Equals("-rubys-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-rubys", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Ruby Swirl {pokemonName}";
         else if (form.Equals("-rubys-berry", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Ruby Swirl {pokemonName}";
         else if (form.Equals("-rubys-clover", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Ruby Swirl {pokemonName}";
         else if (form.Equals("-rubys-flower", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Ruby Swirl {pokemonName}";
         else if (form.Equals("-rubys-love", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Ruby Swirl {pokemonName}";
         else if (form.Equals("-rubys-ribbon", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Ruby Swirl {pokemonName}";
         else if (form.Equals("-rubys-star", StringComparison.OrdinalIgnoreCase) || form.Equals("-ruby-s-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Ruby Swirl {pokemonName}";
         else if (form.Equals("-matcha-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-matcha", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Matcha Cream {pokemonName}";
         else if (form.Equals("-matcha-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Matcha Cream {pokemonName}";
         else if (form.Equals("-matcha-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Matcha Cream {pokemonName}";
         else if (form.Equals("-matcha-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Matcha Cream {pokemonName}";
         else if (form.Equals("-matcha-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Matcha Cream {pokemonName}";
         else if (form.Equals("-matcha-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Matcha Cream {pokemonName}";
         else if (form.Equals("-matcha-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Matcha Cream {pokemonName}";
         else if (form.Equals("-lemon-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-lemon", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Lemon Cream {pokemonName}";
         else if (form.Equals("-lemon-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Lemon Cream {pokemonName}";
         else if (form.Equals("-lemon-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Lemon Cream {pokemonName}";
         else if (form.Equals("-lemon-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Lemon Cream {pokemonName}";
         else if (form.Equals("-lemon-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Lemon Cream {pokemonName}";
         else if (form.Equals("-lemon-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Lemon Cream {pokemonName}";
         else if (form.Equals("-lemon-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Lemon Cream {pokemonName}";
         else if (form.Equals("-salted-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-salted", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Salted Cream {pokemonName}";
         else if (form.Equals("-salted-berry", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Salted Cream {pokemonName}";
         else if (form.Equals("-salted-clover", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Salted Cream {pokemonName}";
         else if (form.Equals("-salted-flower", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Salted Cream {pokemonName}";
         else if (form.Equals("-salted-love", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Salted Cream {pokemonName}";
         else if (form.Equals("-salted-ribbon", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Salted Cream {pokemonName}";
         else if (form.Equals("-salted-star", StringComparison.OrdinalIgnoreCase) || form.Equals("-salt-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Salted Cream {pokemonName}";
         else if (form.Equals("-mint-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-mint", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Mint Cream {pokemonName}";
         else if (form.Equals("-mint-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Mint Cream {pokemonName}";
         else if (form.Equals("-mint-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Mint Cream {pokemonName}";
         else if (form.Equals("-mint-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Mint Cream {pokemonName}";
         else if (form.Equals("-mint-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Mint Cream {pokemonName}";
         else if (form.Equals("-mint-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Mint Cream {pokemonName}";
         else if (form.Equals("-mint-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Mint Cream {pokemonName}";
         else if (form.Equals("-rainbow-strawberry", StringComparison.OrdinalIgnoreCase) || form.Equals("-rainbow", StringComparison.OrdinalIgnoreCase))
            return $"Strawberry Sweet Rainbow Swirl {pokemonName}";
         else if (form.Equals("-rainbow-berry", StringComparison.OrdinalIgnoreCase))
            return $"Berry Sweet Rainbow Swirl {pokemonName}";
         else if (form.Equals("-rainbow-clover", StringComparison.OrdinalIgnoreCase))
            return $"Clover Sweet Rainbow Swirl {pokemonName}";
         else if (form.Equals("-rainbow-flower", StringComparison.OrdinalIgnoreCase))
            return $"Flower Sweet Rainbow Swirl {pokemonName}";
         else if (form.Equals("-rainbow-love", StringComparison.OrdinalIgnoreCase))
            return $"Love Sweet Rainbow Swirl {pokemonName}";
         else if (form.Equals("-rainbow-ribbon", StringComparison.OrdinalIgnoreCase))
            return $"Ribbon Sweet Rainbow Swirl {pokemonName}";
         else if (form.Equals("-rainbow-star", StringComparison.OrdinalIgnoreCase))
            return $"Star Sweet Rainbow Swirl {pokemonName}";
         // Eiscue
         else if (form.Equals("-ice", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("eiscue", StringComparison.OrdinalIgnoreCase)))
            return $"Ice Face {pokemonName}";
         else if (form.Equals("-noice", StringComparison.OrdinalIgnoreCase))
            return $"Noice Face {pokemonName}";
         // Morpeko
         else if (form.Equals("-full", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("morpeko", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Full Belly Mode";
         else if (form.Equals("-hangry", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Hangry Mode";
         // Zacian and Zamazenta
         else if (form.Equals("-hero", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Hero of Many Battles";
         else if (form.Equals("-crown", StringComparison.OrdinalIgnoreCase) || form.Equals("-crowned", StringComparison.OrdinalIgnoreCase))
            if (pokemonName.Equals("zacian", StringComparison.OrdinalIgnoreCase))
               return $"{pokemonName} Crowned Sword";
            else if (pokemonName.Equals("zamazenta", StringComparison.OrdinalIgnoreCase))
               return $"{pokemonName} Crowned Shield";
            else
               return $"Crown {pokemonName}";
         else if (form.Equals("-sword", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("zacian", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Crowned Sword";
         else if (form.Equals("-shield", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("zamazenta", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Crowned Shield";
         // Urshifu
         else if (form.Equals("-single", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("urshifu", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Single Strike Style";
         else if (form.Equals("-rapid", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Rapid Strike Style";
         // Zarude
         else if (form.Equals("-dada", StringComparison.OrdinalIgnoreCase))
            return $"Dada {pokemonName}";
         return pokemonName;
      }

      // Evolution processors *************************************************

      /// <summary>
      /// Generage an ordered dictionary of evolutions.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Ordered dictionary of evolutions.</returns>
      public static Dictionary<string, string> GenerateEvoDict(string pokemonName)
      {
         List<Evolution> initEvoFamily = Connections.Instance().GetEvolutionFamily(pokemonName);

         if (initEvoFamily.Count == 0)
         {
            return new Dictionary<string, string>()
            {
               [pokemonName] = ""
            };
         }

         foreach (Evolution evo in initEvoFamily)
         {
            foreach (Evolution evoComp in initEvoFamily)
            {
               evo.Combine(evoComp);
            }
         }
         List<Evolution> normalEvoFamily = initEvoFamily.Where(x => x.Candy != Global.BAD_EVOLUTION).ToList();

         string basePokemon = normalEvoFamily.First().Start;
         bool baseChanged = true;
         while (baseChanged)
         {
            baseChanged = false;
            foreach (Evolution evo in normalEvoFamily)
            {
               if (evo.End.Equals(basePokemon, StringComparison.OrdinalIgnoreCase))
               {
                  basePokemon = evo.Start;
                  baseChanged = true;
               }
            }
         }

         EvolutionNode tree = BuildEvolutionTree(basePokemon, normalEvoFamily);
         return EvolutionTreeToString(tree);
      }

      /// <summary>
      /// Recursivly builds an evolution tree.
      /// A tree is made up of evolution nodes.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="evolutions">List of evolutions.</param>
      /// <returns>Evolution node that starts the tree.</returns>
      private static EvolutionNode BuildEvolutionTree(string pokemonName, List<Evolution> evolutions)
      {
         string method = "";
         foreach (Evolution evo in evolutions)
         {
            if (pokemonName.Equals(evo.End, StringComparison.OrdinalIgnoreCase))
            {
               method = evo.MethodToString();
            }
         }

         EvolutionNode node = new EvolutionNode
         {
            Name = pokemonName,
            Method = method
         };

         foreach (Evolution evo in evolutions)
         {
            if (pokemonName.Equals(evo.Start, StringComparison.OrdinalIgnoreCase))
            {
               node.Evolutions.Add(BuildEvolutionTree(evo.End, evolutions));
            }
         }
         return node;
      }

      /// <summary>
      /// Converts an evolution tree to a dictionary.
      /// </summary>
      /// <param name="node">Node to convert to dictionary.</param>
      /// <param name="previousEvolution">Name of previous evolution.</param>
      /// <returns>Ordered dictionary of evolutions.</returns>
      private static Dictionary<string, string> EvolutionTreeToString(EvolutionNode node, string previousEvolution = null)
      {
         Dictionary<string, string> evolutions = new Dictionary<string, string>();
         string evoString = previousEvolution == null ? "Base Form" : $"Evolves from {previousEvolution} with {node.Method}";
         evolutions.Add(node.Name, evoString);

         foreach (EvolutionNode evo in node.Evolutions)
         {
            evolutions = evolutions.Union(EvolutionTreeToString(evo, node.Name)).ToDictionary(x => x.Key, x => x.Value);
         }
         return evolutions;
      }

      // Type processors *************************************************

      /// <summary>
      /// Formats weather boosts as a string.
      /// </summary>
      /// <param name="weatherList">List of weather that boosts the type(s).</param>
      /// <returns>Weather for type(s) as a string.</returns>
      protected static string FormatWeatherList(List<string> weatherList)
      {
         StringBuilder sb = new StringBuilder();
         foreach (string weather in weatherList)
         {
            sb.Append($"{Global.NONA_EMOJIS[$"{weather.Replace(' ', '_')}_emote"]} ");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Formats type relations as a string.
      /// </summary>
      /// <param name="relations">Dictionary of type relations for the type(s).</param>
      /// <returns>Type relations for type(s) as a string.</returns>
      protected static string FormatTypeList(Dictionary<string, double> relations)
      {
         if (relations.Count == 0)
         {
            return Global.EMPTY_FIELD;
         }

         string relationString = "";
         foreach (KeyValuePair<string, double> relation in relations)
         {
            double multiplier = relation.Value * 100.0;
            string typeEmote = Global.NONA_EMOJIS[$"{relation.Key.ToUpper()}_EMOTE"];
            relationString += $"{typeEmote} {relation.Key}: {multiplier}%\n";
         }
         return relationString;
      }

      /// <summary>
      /// Checks if a type is valid.
      /// </summary>
      /// <param name="type">Type to check.</param>
      /// <returns>True if the type is valid, otherwise false.</returns>
      protected static bool CheckValidType(string type)
      {
         return Global.NONA_EMOJIS.ContainsKey($"{type}_emote");
      }

      // Message senders ******************************************************

      /// <summary>
      /// Sends a dex selection message.
      /// </summary>
      /// <param name="messageType">Type of message to select.</param>
      /// <param name="options">List of options.</param>
      /// <param name="channel">Channel to send message to.</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SendDexSelectionMessage(int messageType, List<string> options, ISocketMessageChannel channel)
      {
         string fileName = POKEDEX_SELECTION_IMAGE;
         Connections.CopyFile(fileName);
         RestUserMessage dexMessage = await channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(options, fileName));
         dexSelectMessages.Add(dexMessage.Id, new DexSelectionMessage(messageType, options));
         Connections.DeleteFile(fileName);
         dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(options.Count).ToArray());
      }

      /// <summary>
      /// Sends a dex message using a given embed method.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="EmbedMethod">Embed method to use.</param>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="addEmojis">Should emotes be added. Defaults to false.</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SendDexMessage(Pokemon pokemon, Func<Pokemon, string, Embed> EmbedMethod, ISocketMessageChannel channel, bool addEmojis = false)
      {
         string fileName = Connections.GetPokemonPicture(pokemon.Name);
         Connections.CopyFile(fileName);
         RestUserMessage message = await channel.SendFileAsync(fileName, embed: EmbedMethod(pokemon, fileName));
         dexMessages.Add(message.Id, pokemon);
         Connections.DeleteFile(fileName);
         if (addEmojis)
         {
            message.AddReactionsAsync(dexEmojis);
         }
      }

      // Miscellaneous ********************************************************

      /// <summary>
      /// Sets custom emotes used for dex messages.
      /// </summary>
      public static void SetInitialEmotes()
      {
         dexEmojis[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.CP_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.COUNTER_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE];
      }

      /// <summary>
      /// Removes old dex messages from the list of dex messages.
      /// Old dex messages are messages older than one day.
      /// </summary>
      protected static void RemoveOldDexMessages()
      {
         List<ulong> ids = new List<ulong>();
         foreach (KeyValuePair<ulong, Pokemon> dexMessage in dexMessages)
         {
            if (Math.Abs((DateTime.Now - dexMessage.Value.CreatedAt).TotalDays) >= 1)
            {
               ids.Add(dexMessage.Key);
            }
         }
         foreach (ulong id in ids)
         {
            dexMessages.Remove(id);
         }
      }
   }
}