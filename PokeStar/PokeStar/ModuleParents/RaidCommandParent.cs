using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.ModuleParents
{
   /// <summary>
   /// Parent for raid command modules.
   /// </summary>
   public class RaidCommandParent : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Raid Train image file name.
      /// </summary>
      public static readonly string RAID_TRAIN_IMAGE_NAME = "Raid_Train.png";

      /// <summary>
      /// Raid Mule Train image file name.
      /// </summary>
      public static readonly string RAID_MULE_TRAIN_IMAGE_NAME = "Raid_Mule_Train.png";

      // Message holders ******************************************************

      /// <summary>
      /// Saved raid messages.
      /// </summary>
      protected static readonly Dictionary<ulong, RaidParent> raidMessages = new Dictionary<ulong, RaidParent>();

      /// <summary>
      /// Saved raid sub messages.
      /// </summary>
      protected static readonly Dictionary<ulong, RaidSubMessage> subMessages = new Dictionary<ulong, RaidSubMessage>();

      /// <summary>
      /// Saved raid guide messages.
      /// Elements are removed upon usage.
      /// </summary>
      protected static readonly Dictionary<ulong, RaidGuideSelect> guideMessages = new Dictionary<ulong, RaidGuideSelect>();

      /// <summary>
      /// Saved raid poll messages. 
      /// Elements are removed upon poll closure.
      /// </summary>
      protected static readonly Dictionary<ulong, Dictionary<string, int>> pollMessages = new Dictionary<ulong, Dictionary<string, int>>();

      // Emotes ***************************************************************

      /// <summary>
      /// Emotes for a raid message.
      /// </summary>
      protected static readonly IEmote[] raidEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("✅"),
         new Emoji("✈️"),
         new Emoji("🤝"),
         new Emoji("🚫")
      };

      /// <summary>
      /// Emotes for a raid mule message.
      /// </summary>
      protected static readonly IEmote[] muleEmojis = {
         new Emoji("🐎"),
         new Emoji("✅"),
         new Emoji("✈️"),
         new Emoji("🤝"),
         new Emoji("🚫")
      };

      /// <summary>
      /// Emotes for a raid train message.
      /// Added onto the emotes for a raid message
      /// </summary>
      protected static readonly Emoji[] trainEmojis = {
         new Emoji("⬅️"),
         new Emoji("➡️"),
         new Emoji("🗺️"),
      };

      /// <summary>
      /// Emotes for a remote sub message.
      /// </summary>
      private static readonly IEmote[] remoteEmojis = {
         new Emoji("✈️"),
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
         new Emoji("🚫"),
      };

      /// <summary>
      /// Emotes for a tier selection sub message.
      /// </summary>
      protected static readonly IEmote[] tierEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
      };

      /// <summary>
      /// Extra emotes.
      /// These will sometimes be added to messages,
      /// but not everytime.
      /// </summary>
      protected static readonly Emoji[] extraEmojis = {
         new Emoji("⬅️"),
         new Emoji("➡️"),
         new Emoji("❌"),
         new Emoji("❓"),
         new Emoji("⬆️"),
      };
#if COMPONENTS
      // Components ***********************************************************

      /// <summary>
      /// Components for a raid message.
      /// </summary>
      protected static readonly string[] raidComponents = {
         "raid_add_1",
         "raid_add_2",
         "raid_add_3",
         "raid_add_4",
         "raid_add_5",
         "raid_ready",
         "raid_remote",
         "raid_invite",
         "raid_remove",
      };

      /// <summary>
      /// Components for a raid mule message.
      /// </summary>
      protected static readonly string[] muleComponents = {
         "mule_add",
         "mule_ready",
         "mule_request",
         "mule_invite",
         "mule_remove",
      };

      /// <summary>
      /// Components for a raid train message.
      /// Added onto the components for a raid message
      /// </summary>
      protected static readonly string[] trainComponents = {
         "train_back",
         "train_forward",
         "train_station",
      };

      /// <summary>
      /// Components for a remote sub message.
      /// </summary>
      private static readonly string[] remoteComponents = {
         "remote_request",
         "remote_add_1",
         "remote_add_2",
         "remote_add_3",
         "remote_add_4",
         "remote_add_5",
         "remote_add_6",
         "remote_remove",
      };

      /// <summary>
      /// Components for a tier selection sub message.
      /// </summary>
      protected static readonly string[] tierComponents = {
         "tier_common",
         "tier_rare",
         "tier_legendary",
         "tier_mega",
      };

      /// <summary>
      /// Extra components.
      /// These will sometimes be added to messages, but not everytime.
      /// </summary>
      protected static readonly string[] extraComponents = {
         "extra_back",
         "extra_forward",
         "extra_cancel",
         "extra_help",
         "extra_change_tier",
      };
#endif
      // Descriptions *********************************************************

      /// <summary>
      /// Descriptions for raid interactions.
      /// </summary>
      private static readonly string[] raidInteractionDesc = {
         "are the number of Trainers in a group that are raiding in person.",
         "means you are ready for the raid to begin. Nona will notify everyone when all trainers are ready.",
         "means you and/or a group will be either doing the raid remotely; or you need another trainer to send you an invite to raid. *",
         "means you want to invite a trainer who is asking for an invite. The trainer will be counted in the raid as raiding remotely. Nona will notify the person you plan to invite. *",
         "means you want to remove yourself from the raid. Nona will notify anyone you were planning to invite."
      };

      /// <summary>
      /// Descriptions for raid mule interactions.
      /// </summary>
      private static readonly string[] muleInteractionDesc = {
         "means you are able to invite others to the raid.",
         "means that a raid group is ready to go. Can only be done by done by a raid mule. *",
         "means you need a raid mule to send you an invite to the raid.",
         "means you want to invite a trainer who is asking for an invite. Nona will notify the person you plan to invite. Can only be done by a raid mule. *",
         "means you want to remove yourself from the raid. Nona will notify anyone you were planning to invite."
      };

      /// <summary>
      /// Descriptions for raid train interactions.
      /// Only emotes added onto raid.
      /// </summary>
      private static readonly string[] trainInteractionDesc = {
         "means return to the previous gym. Can only be done by the train conductor.",
         "means continue to the next gym. Can only be done by the train conductor.",
         "means check the list of incomplete raids.",
      };

      // Replies **************************************************************

      /// <summary>
      /// Replies for a raid message.
      /// </summary>
      private static readonly string[] raidReplies = {
         "edit <attribute> <value>",
         "invite <invites>",
         "remote <group size>",
         "request",
      };

      /// <summary>
      /// Replies for a raid mule message.
      /// </summary>
      private static readonly string[] muleReplies = {
         "edit <attribute> <value>",
         "invite <invites>",
         "ready <group number>",
      };

      /// <summary>
      /// Replies for a raid train message.
      /// </summary>
      private static readonly string[] trainReplies = {
         "add <time> <location>",
         "conductor <conductor>",
         "station",
         "remove <user>",
      };

      // Enumerations *********************************************************

      /// <summary>
      /// Index of emotes on a raid message.
      /// </summary>
      private enum RAID_EMOJI_INDEX
      {
         ADD_PLAYER_1,
         ADD_PLAYER_2,
         ADD_PLAYER_3,
         ADD_PLAYER_4,
         ADD_PLAYER_5,
         PLAYER_READY,
         REMOTE_RAID,
         INVITE_PLAYER,
         REMOVE_PLAYER,
      }

      /// <summary>
      /// Index of emotes on a raid mule message.
      /// </summary>
      private enum MULE_EMOJI_INDEX
      {
         ADD_MULE,
         RAID_READY,
         REQUEST_INVITE,
         INVITE_PLAYER,
         REMOVE_PLAYER,
      }

      /// <summary>
      /// Index of emotes added to a raid train message.
      /// </summary>
      private enum TRAIN_EMOJI_INDEX
      {
         BACK_ARROW,
         FORWARD_ARROR,
         STATION,
      }

      /// <summary>
      /// Index of emotes on a tier selection message.
      /// </summary>
      private enum TIER_EMOJI_INDEX
      {
         COMMON,
         RARE,
         LEGENDARY,
         MEGA,
      }

      /// <summary>
      /// Index of emotes on a remote sub message.
      /// </summary>
      private enum REMOTE_EMOJI_INDEX
      {
         REQUEST_INVITE,
         REMOTE_PLAYER_1,
         REMOTE_PLAYER_2,
         REMOTE_PLAYER_3,
         REMOTE_PLAYER_4,
         REMOTE_PLAYER_5,
         REMOTE_PLAYER_6,
         REMOVE_REMOTE,
      }

      /// <summary>
      /// Index of extra emotes.
      /// </summary>
      protected enum EXTRA_EMOJI_INDEX
      {
         BACK_ARROW,
         FORWARD_ARROR,
         CANCEL,
         HELP,
         CHANGE_TIER,
      }

      /// <summary>
      /// Types of raid sub messages.
      /// </summary>
      protected enum SUB_MESSAGE_TYPES
      {
         INVITE_SUB_MESSAGE,
         RAID_REMOTE_SUB_MESSAGE,
         MULE_READY_SUB_MESSAGE,
         EDIT_BOSS_SUB_MESSAGE,
      }

      /// <summary>
      /// Types of raid boss selection messages.
      /// </summary>
      protected enum SELECTION_TYPES
      {
         STANDARD,
         PAGE,
         STANDARD_EDIT,
         PAGE_EDIT
      }

      // Message checkers *****************************************************

      /// <summary>
      /// Checks if a message is a raid message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid message, otherwise false.</returns>
      public static bool IsRaidMessage(ulong id)
      {
         return raidMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid sub message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid sub message, otherwise false.</returns>
      public static bool IsRaidSubMessage(ulong id)
      {
         return subMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid guide message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid guide message, otherwise false.</returns>
      public static bool IsRaidGuideMessage(ulong id)
      {
         return guideMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid poll message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid poll message, otherwise false.</returns>
      public static bool IsRaidPollMessage(ulong id)
      {
         return pollMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid select message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid select message, otherwise false.</returns>
      public static bool IsRaidSelectMessage(ulong id)
      {
         return raidMessages.ContainsKey(id) && raidMessages[id].GetCurrentBoss() == null;
      }

      /// <summary>
      /// Checks if a message is a raid edit boss message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid edit boss message, otherwise false.</returns>
      public static bool IsRaidEditBossMessage(ulong id, string text)
      {
         return subMessages.ContainsKey(id) && subMessages[id].Type == (int)SUB_MESSAGE_TYPES.EDIT_BOSS_SUB_MESSAGE &&
            text.Contains(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER].Name);
      }

#if COMPONENTS
      // Message component handlers *******************************************

      /// <summary>
      /// Handles a button press on a general raid message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidMessageButtonHandle(IMessage message, SocketMessageComponent component)
      {
         RaidParent parent = raidMessages[message.Id];

         if (parent.GetCurrentBoss() == null)
         {
            if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]) && parent.BossPage > 0)
            {
               parent.BossPage--;
               string fileName = $"Egg{parent.Tier}.png";
               int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
               Connections.CopyFile(fileName);
               await ((SocketUserMessage)message).ModifyAsync(x =>
               {
                  x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
               });
               Connections.DeleteFile(fileName);
            }
            else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]) &&
                     parent.AllBosses[parent.Tier].Count > (parent.BossPage + 1) * Global.SELECTION_EMOJIS.Length)
            {
               parent.BossPage++;
               string fileName = $"Egg{parent.Tier}.png";
               int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
               Connections.CopyFile(fileName);
               await ((SocketUserMessage)message).ModifyAsync(x =>
               {
                  x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
               });
               Connections.DeleteFile(fileName);
            }
            else
            {
               int options = parent.AllBosses[parent.Tier].Skip(parent.BossPage * Global.SELECTION_EMOJIS.Length).Take(Global.SELECTION_EMOJIS.Length).ToList().Count;
               for (int i = 0; i < options; i++)
               {
                  if (component.Data.CustomId.Equals($"{Global.SELECTION_BUTTON_PREFIX}{i + 1}"))
                  {
                     await SelectBoss(message, component.Channel, parent, (parent.BossPage * Global.SELECTION_EMOJIS.Length) + i);
                  }
               }
            }
         }
         else
         {
            if (parent is Raid raid)
            {
               await RaidButtonHandle(message, component, raid);
            }
            else if (parent is RaidMule mule)
            {
               await RaidMuleButtonHandle(message, component, mule);
            }
         }
      }

      /// <summary>
      /// Handles a button press on a raid sub message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidSubMessageButtonHandle(IMessage message, SocketMessageComponent component)
      {
         int subMessageType = subMessages[message.Id].Type;
         if (subMessageType == (int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE)
         {
            await RaidInviteButtonHandle(message, component);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE)
         {
            await RaidRemoteButtonHandle(message, component);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE)
         {
            await RaidMuleReadyButtonHandle(message, component);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.EDIT_BOSS_SUB_MESSAGE)
         {
            await BossEditSelectionButtonHandle(message, component);
         }
      }

      /// <summary>
      /// Handles a button press on a raid guide message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidGuideMessageButtonHandle(IMessage message, SocketMessageComponent component)
      {
         RaidGuideSelect guide = guideMessages[message.Id];

         if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]) && guide.Page > 0)
         {
            guideMessages[message.Id] = new RaidGuideSelect(guide.Page - 1, guide.Tier, guide.Bosses);
            guide = guideMessages[message.Id];
            string fileName = $"Egg{guide.Tier}.png";
            int selectType = guide.Bosses.Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
            Connections.CopyFile(fileName);
            await ((SocketUserMessage)message).ModifyAsync(x =>
            {
               x.Embed = BuildBossSelectEmbed(guide.Bosses, selectType, guide.Page, fileName);
            });
            Connections.DeleteFile(fileName);
         }
         else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]) &&
                  guide.Bosses.Count > (guide.Page + 1) * Global.SELECTION_EMOJIS.Length)
         {
            guideMessages[message.Id] = new RaidGuideSelect(guide.Page + 1, guide.Tier, guide.Bosses);
            guide = guideMessages[message.Id];
            string fileName = $"Egg{guide.Tier}.png";
            int selectType = guide.Bosses.Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
            Connections.CopyFile(fileName);
            await ((SocketUserMessage)message).ModifyAsync(x =>
            {
               x.Embed = BuildBossSelectEmbed(guide.Bosses, selectType, guide.Page, fileName);
            });
            Connections.DeleteFile(fileName);
         }
         else
         {
            int options = guide.Bosses.Skip(guide.Page * Global.SELECTION_EMOJIS.Length).Take(Global.SELECTION_EMOJIS.Length).ToList().Count;
            for (int i = 0; i < options; i++)
            {
               if (component.Data.CustomId.Equals($"{Global.SELECTION_BUTTON_PREFIX}{i + 1}"))
               {
                  guideMessages.Remove(message.Id);
                  await message.DeleteAsync();

                  Pokemon pkmn = Connections.Instance().GetPokemon(guide.Bosses[(guide.Page * Global.SELECTION_EMOJIS.Length) + i]);
                  Connections.Instance().GetRaidBoss(ref pkmn);

                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await component.Channel.SendFileAsync(fileName, embed: BuildRaidGuideEmbed(pkmn, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
         }
      }

      // Component handlers ***************************************************

      /// <summary>
      /// Handles a button press on a raid message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <param name="raid">Raid to apply the button press to.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidButtonHandle(IMessage message, SocketMessageComponent component, Raid raid)
      {
         Player reactingPlayer = new Player((SocketGuildUser)component.User);

         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool messageExists = true;
            bool needsUpdate = true;
            if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1]))
            {
               raid.AddPlayer(reactingPlayer, 1);
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2]))
            {
               raid.AddPlayer(reactingPlayer, 2);
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3]))
            {
               raid.AddPlayer(reactingPlayer, 3);
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4]))
            {
               raid.AddPlayer(reactingPlayer, 4);
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5]))
            {
               raid.AddPlayer(reactingPlayer, 5);
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.PLAYER_READY]))
            {
               int group = raid.MarkPlayerReady(reactingPlayer);
               if (group != Global.NOT_IN_RAID)
               {
                  await component.Channel.SendMessageAsync(BuildRaidReadyPingList(raid.GetGroup(group).GetPingList(), raid.GetCurrentLocation(), group + 1, true));
               }
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.REMOTE_RAID]))
            {
               RestUserMessage remoteMsg = await component.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                  embed: BuildPlayerRemoteEmbed(reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username), 
                  components: Global.BuildButtons(remoteEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray(),
                     remoteComponents.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray()));
               subMessages.Add(remoteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE, message.Id));
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID)
               {
                  if (!raid.GetReadonlyInviteList().IsEmpty && !raid.HasActiveInvite())
                  {
                     raid.InvitingPlayer = reactingPlayer;
                     int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
                     int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);

                     IEmote[] emotes = Global.SELECTION_EMOJIS.Take(listSize).ToArray();
                     string[] components = Global.BuildSelectionCustomIDs(emotes.Length);
                     if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                     {
                        emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW])
                           .Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).ToArray();
                        components = components.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR])
                           .Append(extraComponents[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).ToArray();
                     }
                     emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray();
                     components = components.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray();

                     RestUserMessage inviteMsg = await component.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                        embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), 
                           reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize), 
                        components: Global.BuildButtons(emotes, components));
                     subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
                  }
               }
            }
            else if (component.Data.CustomId.Equals(raidComponents[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               RaidRemoveResult returnValue = raid.RemovePlayer(reactingPlayer);

               foreach (Player invite in returnValue.Users)
               {
                  await invite.SocketPlayer.SendMessageAsync(BuildUnInvitedMessage(reactingPlayer));
               }

               if (returnValue.Group != Global.NOT_IN_RAID)
               {
                  await component.Channel.SendMessageAsync(BuildRaidReadyPingList(raid.GetGroup(returnValue.Group).GetPingList(), raid.GetCurrentLocation(), returnValue.Group + 1, true));
               }
            }
            else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               if (raid.IsSingleStop())
               {
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(raidEmojis, raidInteractionDesc, raidReplies, prefix));
               }
               else
               {
                  IEmote[] emojis = raidEmojis.Concat(trainEmojis).ToArray();
                  string[] desc = raidInteractionDesc.Concat(trainInteractionDesc).ToArray();
                  string[] replies = raidReplies.Concat(trainReplies).ToArray();
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(emojis, desc, replies, prefix));
               }
               needsUpdate = false;
            }
            else if (!raid.IsSingleStop())
            {
               if (reactingPlayer.Equals(raid.Conductor))
               {
                  if (component.Data.CustomId.Equals(trainComponents[(int)TRAIN_EMOJI_INDEX.BACK_ARROW]))
                  {
                     needsUpdate = raid.PreviousLocation();
                  }
                  else if (component.Data.CustomId.Equals(trainComponents[(int)TRAIN_EMOJI_INDEX.FORWARD_ARROR]))
                  {
                     if (raid.AllReady() && raid.NextLocation())
                     {
                        await component.Channel.SendMessageAsync(BuildTrainAdvancePingList(raid.GetAllUsers().ToImmutableList(), raid.GetCurrentLocation()));

                        raidMessages.Remove(message.Id);
                        await message.DeleteAsync();

                        string fileName = RAID_TRAIN_IMAGE_NAME;
                        Connections.CopyFile(fileName);
                        RestUserMessage raidMsg = await component.Channel.SendFileAsync(fileName,
                           embed: BuildRaidTrainEmbed(raid, fileName), components: Global.BuildButtons(
                              raidEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                              raidComponents.Concat(trainComponents).Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
                        raidMessages.Add(raidMsg.Id, raid);
                        Connections.DeleteFile(fileName);

                        messageExists = false;
                     }
                  }
                  else if (component.Data.CustomId.Equals(trainComponents[(int)TRAIN_EMOJI_INDEX.STATION]))
                  {
                     List<RaidTrainLoc> futureRaids = raid.GetIncompleteRaids();
                     if (raid.StationMessageId.HasValue && component.Channel.GetCachedMessage(raid.StationMessageId.Value) != null)
                     {
                        await component.Channel.DeleteMessageAsync(raid.StationMessageId.Value);
                     }
                     RestUserMessage stationMsg = await component.Channel.SendMessageAsync(embed: BuildStationEmbed(futureRaids, raid.Conductor));
                     raid.StationMessageId = stationMsg.Id;

                     needsUpdate = false;
                  }
               }
            }
            else
            {
               needsUpdate = false;
            }

            if (messageExists && needsUpdate)
            {
               await ModifyMessage((SocketUserMessage)message, raid);
            }
         }
      }

      /// <summary>
      /// Handles a button press on a raid mule message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <param name="raid">Raid mule to apply the button press to.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidMuleButtonHandle(IMessage message, SocketMessageComponent component, RaidMule raid)
      {
         Player reactingPlayer = new Player((SocketGuildUser)component.User);

         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool messageExists = true;
            bool needsUpdate = true;
            if (component.Data.CustomId.Equals(muleComponents[(int)MULE_EMOJI_INDEX.ADD_MULE]))
            {
               raid.AddPlayer(reactingPlayer, 1);
            }
            else if (component.Data.CustomId.Equals(muleComponents[(int)MULE_EMOJI_INDEX.RAID_READY]))
            {
               if (raid.HasInvites() && raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID)
               {
                  RestUserMessage readyMsg = await component.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                     embed: BuildMuleReadyEmbed(raid.GetTotalGroups(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username), 
                     components: Global.BuildButtons(new List<IEmote>(Global.SELECTION_EMOJIS.Take(raid.GetTotalGroups()))
                        .Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray(), 
                        Global.BuildSelectionCustomIDs(new List<IEmote>(Global.SELECTION_EMOJIS.Take(raid.GetTotalGroups())).Count())
                        .Append(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray()));
                  subMessages.Add(readyMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE, message.Id));
               }
            }
            else if (component.Data.CustomId.Equals(muleComponents[(int)MULE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(muleComponents[(int)MULE_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID &&
                  raid.GetReadonlyInviteList().Count != 0 &&
                  !raid.HasActiveInvite())
               {
                  raid.InvitingPlayer = reactingPlayer;
                  int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
                  int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);

                  IEmote[] emotes = Global.SELECTION_EMOJIS.Take(listSize).ToArray();
                  string[] components = Global.BuildSelectionCustomIDs(emotes.Length);
                  if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                  {
                     emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW])
                        .Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).ToArray();
                     components = components.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR])
                        .Append(extraComponents[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).ToArray();
                  }
                  emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray();
                  components = components.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray();

                  RestUserMessage inviteMsg = await component.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                     embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), 
                        reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize),
                     components: Global.BuildButtons(emotes, components));
                  subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
               }

            }
            else if (component.Data.CustomId.Equals(muleComponents[(int)MULE_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               List<Player> returnValue = raid.RemovePlayer(reactingPlayer).Users;

               foreach (Player invite in returnValue)
               {
                  await invite.SocketPlayer.SendMessageAsync($"{reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".");
               }
            }
            else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);

               if (raid.IsSingleStop())
               {
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(muleEmojis, muleInteractionDesc, muleReplies, prefix));
               }
               else
               {
                  IEmote[] emojis = muleEmojis.Concat(trainEmojis).ToArray();
                  string[] desc = muleInteractionDesc.Concat(trainInteractionDesc).ToArray();
                  string[] replies = muleReplies.Concat(trainReplies).ToArray();
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(emojis, desc, replies, prefix));
               }
               needsUpdate = false;
            }
            else if (!raid.IsSingleStop())
            {
               if (reactingPlayer.Equals(raid.Conductor))
               {
                  if (component.Data.CustomId.Equals(trainComponents[(int)TRAIN_EMOJI_INDEX.BACK_ARROW]))
                  {
                     needsUpdate = raid.PreviousLocation();
                  }
                  else if (component.Data.CustomId.Equals(trainComponents[(int)TRAIN_EMOJI_INDEX.FORWARD_ARROR]))
                  {
                     if (raid.NextLocation())
                     {
                        await component.Channel.SendMessageAsync(BuildTrainAdvancePingList(raid.GetAllUsers().ToImmutableList(), raid.GetCurrentLocation()));

                        raidMessages.Remove(message.Id);
                        await message.DeleteAsync();

                        string fileName = RAID_TRAIN_IMAGE_NAME;
                        Connections.CopyFile(fileName);
                        RestUserMessage raidMsg = await component.Channel.SendFileAsync(fileName, 
                           embed: BuildRaidMuleTrainEmbed(raid, fileName), components: Global.BuildButtons(
                              muleEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                              muleComponents.Concat(trainComponents).Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
                        raidMessages.Add(raidMsg.Id, raid);
                        Connections.DeleteFile(fileName);

                        messageExists = false;
                     }
                  }
                  else if (component.Data.CustomId.Equals(trainComponents[(int)TRAIN_EMOJI_INDEX.STATION]))
                  {
                     List<RaidTrainLoc> futureRaids = raid.GetIncompleteRaids();
                     if (raid.StationMessageId.HasValue && component.Channel.GetCachedMessage(raid.StationMessageId.Value) != null)
                     {
                        await component.Channel.DeleteMessageAsync(raid.StationMessageId.Value);
                     }
                     RestUserMessage stationMsg = await component.Channel.SendMessageAsync(embed: BuildStationEmbed(futureRaids, raid.Conductor));
                     raid.StationMessageId = stationMsg.Id;

                     needsUpdate = false;
                  }
               }
            }
            else
            {
               needsUpdate = false;
            }

            if (messageExists && needsUpdate)
            {
               await ModifyMessage((SocketUserMessage)message, raid);
            }
         }
      }

      /// <summary>
      /// Handles a button press on a raid invite message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidInviteButtonHandle(IMessage message, SocketMessageComponent component)
      {
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         RaidParent parent = raidMessages[raidMessageId];
         Player reactingPlayer = new Player((SocketGuildUser)component.User);

         if (reactingPlayer.Equals(parent.InvitingPlayer) || message.MentionedUserIds.Contains(reactingPlayer.SocketPlayer.Id))
         {
            if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
               parent.InvitingPlayer = null;
            }
            else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]))
            {
               parent.ChangeInvitePage(false, Global.SELECTION_EMOJIS.Length);
               int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(parent.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await component.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize);
               });
            }
            else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]))
            {
               parent.ChangeInvitePage(true, Global.SELECTION_EMOJIS.Length);
               int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(parent.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await component.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize);
               });
            }
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (component.Data.CustomId.Equals($"{Global.SELECTION_BUTTON_PREFIX}{i + 1}"))
                  {
                     int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
                     Player player = parent.GetReadonlyInviteList().ElementAt(i + offset);
                     if (parent.InvitePlayer(player, reactingPlayer))
                     {
                        await ModifyMessage((SocketUserMessage)await component.Channel.GetMessageAsync(raidMessageId), parent);
                        await player.SocketPlayer.SendMessageAsync($"You have been invited to a raid by {reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username}.");
                        subMessages.Remove(message.Id);
                        parent.InvitingPlayer = null;
                        await message.DeleteAsync();
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// Handles a button press on a raid remote message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidRemoteButtonHandle(IMessage message, SocketMessageComponent component)
      {
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         bool needEdit = true;
         Raid raid = (Raid)raidMessages[raidMessageId];
         Player reactingPlayer = new Player((SocketGuildUser)component.User);

         if (message.MentionedUserIds.Contains(reactingPlayer.SocketPlayer.Id))
         {
            if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
               needEdit = false;
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]))
            {
               raid.AddPlayer(reactingPlayer, 1, reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]))
            {
               raid.AddPlayer(reactingPlayer, 2, reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]))
            {
               raid.AddPlayer(reactingPlayer, 3, reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]))
            {
               raid.AddPlayer(reactingPlayer, 4, reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]))
            {
               raid.AddPlayer(reactingPlayer, 5, reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6]))
            {
               raid.AddPlayer(reactingPlayer, 6, reactingPlayer);
            }
            else if (component.Data.CustomId.Equals(remoteComponents[(int)REMOTE_EMOJI_INDEX.REMOVE_REMOTE]))
            {
               raid.AddPlayer(reactingPlayer, 0, reactingPlayer);

               Dictionary<Player, List<Player>> empty = raid.ClearEmptyPlayer(reactingPlayer);
               foreach (KeyValuePair<Player, List<Player>> user in empty)
               {
                  foreach (Player invite in user.Value)
                  {
                     await invite.SocketPlayer.SendMessageAsync(BuildUnInvitedMessage(user.Key));
                  }
               }
            }
            else
            {
               needEdit = false;
            }

            if (needEdit)
            {
               await ModifyMessage((SocketUserMessage)await component.Channel.GetMessageAsync(raidMessageId), raid);
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
         }
      }

      /// <summary>
      /// Handles a button press on a raid mule ready message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidMuleReadyButtonHandle(IMessage message, SocketMessageComponent component)
      {
         ulong raidMuleMessageId = subMessages[message.Id].MainMessageId;
         RaidMule raid = (RaidMule)raidMessages[raidMuleMessageId];

         if (message.MentionedUserIds.Contains(component.User.Id))
         {
            if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (component.Data.CustomId.Equals($"{Global.SELECTION_BUTTON_PREFIX}{i + 1}"))
                  {
                     await component.Channel.SendMessageAsync($"{BuildRaidReadyPingList(raid.GetGroup(i).GetPingList(), raid.GetCurrentLocation(), i + 1, false)}");
                     subMessages.Remove(message.Id);
                     await message.DeleteAsync();
                  }
               }
            }
         }
      }

      /// <summary>
      /// Handles a button press on a raid train boss update message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <returns>Completed Task.</returns>
      private static async Task BossEditSelectionButtonHandle(IMessage message, SocketMessageComponent component)
      {
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         RaidParent parent = raidMessages[raidMessageId];
         List<string> raidBosses = null;

         if ((parent.IsSingleStop() && parent.BossEditingPlayer.Equals(component.User))
            || parent.Conductor.Equals(component.User))
         {
            if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               parent.BossPage = 0;
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
            else if (message.Components.Where(button => button.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER])).Count() != 0)
            {
               if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]))
               {
                  parent.BossPage = 0;
                  await message.RemoveAllReactionsAsync();
                  await ((SocketUserMessage)message).ModifyAsync(x =>
                  {
                     x.Embed = BuildTierSelectEmbed();
                     x.Components = Global.BuildButtons(tierEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray(),
                        tierComponents.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());

                  });
               }
               else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]) && parent.BossPage > 0)
               {
                  parent.BossPage--;
                  string fileName = $"Egg{parent.Tier}.png";
                  int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
                  Connections.CopyFile(fileName);
                  await ((SocketUserMessage)message).ModifyAsync(x =>
                  {
                     x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (component.Data.CustomId.Equals(extraComponents[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]) &&
                        parent.AllBosses[parent.Tier].Count > (parent.BossPage + 1) * Global.SELECTION_EMOJIS.Length)
               {
                  parent.BossPage++;
                  string fileName = $"Egg{parent.Tier}.png";
                  int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
                  Connections.CopyFile(fileName);
                  await ((SocketUserMessage)message).ModifyAsync(x =>
                  {
                     x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else
               {
                  int options = parent.AllBosses[parent.Tier].Skip(parent.BossPage * Global.SELECTION_EMOJIS.Length).Take(Global.SELECTION_EMOJIS.Length).ToList().Count;
                  for (int i = 0; i < options; i++)
                  {
                     if (component.Data.CustomId.Equals($"{Global.SELECTION_BUTTON_PREFIX}{i + 1}"))
                     {
                        await EditBoss(message, component.Channel, parent, raidMessageId, (parent.BossPage * Global.SELECTION_EMOJIS.Length) + i);
                        parent.BossEditingPlayer = null;
                     }
                  }
               }
            }
            else if (component.Data.CustomId.Equals(tierComponents[(int)TIER_EMOJI_INDEX.COMMON]))
            {
               parent.SelectionTier = Global.COMMON_RAID_TIER;
               raidBosses = parent.AllBosses[Global.COMMON_RAID_TIER];
            }
            else if (component.Data.CustomId.Equals(tierComponents[(int)TIER_EMOJI_INDEX.RARE]))
            {
               parent.SelectionTier = Global.RARE_RAID_TIER;
               raidBosses = parent.AllBosses[Global.RARE_RAID_TIER];
            }
            else if (component.Data.CustomId.Equals(tierComponents[(int)TIER_EMOJI_INDEX.LEGENDARY]))
            {
               parent.SelectionTier = Global.LEGENDARY_RAID_TIER;
               raidBosses = parent.AllBosses[Global.LEGENDARY_RAID_TIER];
            }
            else if (component.Data.CustomId.Equals(tierComponents[(int)TIER_EMOJI_INDEX.MEGA]))
            {
               parent.SelectionTier = Global.MEGA_RAID_TIER;
               raidBosses = parent.AllBosses[Global.MEGA_RAID_TIER];
            }

            if (raidBosses != null)
            {
               SocketUserMessage msg = (SocketUserMessage)message;
               await msg.RemoveAllReactionsAsync();

               int selectType = raidBosses.Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE_EDIT : (int)SELECTION_TYPES.STANDARD_EDIT;
               await msg.ModifyAsync(x =>
               {
                  x.Embed = BuildBossSelectEmbed(raidBosses, selectType, parent.BossPage, null);
                  x.Components = Global.BuildButtons(new List<IEmote>(Global.SELECTION_EMOJIS.Take(raidBosses.Count))
                        .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW])
                        .Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray(),
                        new List<string>(Global.BuildSelectionCustomIDs(Global.SELECTION_EMOJIS.Take(raidBosses.Count).Count()))
                        .Prepend(extraComponents[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraComponents[(int)EXTRA_EMOJI_INDEX.BACK_ARROW])
                        .Append(extraComponents[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]).Append(extraComponents[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());

               });
            }
         }
      }
#else
      // Message reaction handlers ********************************************

      /// <summary>
      /// Handles a reaction on a general raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         RaidParent parent = raidMessages[message.Id];
         bool selectionMade = false;

         if (parent.GetCurrentBoss() == null)
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]) && parent.BossPage > 0)
            {
               parent.BossPage--;
               string fileName = $"Egg{parent.Tier}.png";
               int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
               Connections.CopyFile(fileName);
               await ((SocketUserMessage)message).ModifyAsync(x =>
               {
                  x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
               });
               Connections.DeleteFile(fileName);
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]) &&
                     parent.AllBosses[parent.Tier].Count > (parent.BossPage + 1) * Global.SELECTION_EMOJIS.Length)
            {
               parent.BossPage++;
               string fileName = $"Egg{parent.Tier}.png";
               int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
               Connections.CopyFile(fileName);
               await ((SocketUserMessage)message).ModifyAsync(x =>
               {
                  x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
               });
               Connections.DeleteFile(fileName);
            }
            else
            {
               int options = parent.AllBosses[parent.Tier].Skip(parent.BossPage * Global.SELECTION_EMOJIS.Length).Take(Global.SELECTION_EMOJIS.Length).ToList().Count;
               for (int i = 0; i < options; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                  {
                     selectionMade = true;
                     await SelectBoss(message, reaction.Channel, parent, (parent.BossPage * Global.SELECTION_EMOJIS.Length) + i);
                  }
               }
            }

            if (!selectionMade)
            {
               await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
            }
         }
         else
         {
            if (parent is Raid raid)
            {
               await RaidReactionHandle(message, reaction, raid);
            }
            else if (parent is RaidMule mule)
            {
               await RaidMuleReactionHandle(message, reaction, mule);
            }
         }
      }

      /// <summary>
      /// Handles a reaction on a raid sub message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidSubMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         int subMessageType = subMessages[message.Id].Type;
         if (subMessageType == (int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE)
         {
            await RaidInviteReactionHandle(message, reaction);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE)
         {
            await RaidRemoteReactionHandle(message, reaction);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE)
         {
            await RaidMuleReadyReactionHandle(message, reaction);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.EDIT_BOSS_SUB_MESSAGE)
         {
            await BossEditSelectionReactionHandle(message, reaction);
         }
      }

      /// <summary>
      /// Handles a reaction on a raid guide message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidGuideMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         RaidGuideSelect guide = guideMessages[message.Id];
         bool selectionMade = false;

         if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]) && guide.Page > 0)
         {
            guideMessages[message.Id] = new RaidGuideSelect(guide.Page - 1, guide.Tier, guide.Bosses);
            guide = guideMessages[message.Id];
            string fileName = $"Egg{guide.Tier}.png";
            int selectType = guide.Bosses.Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
            Connections.CopyFile(fileName);
            await ((SocketUserMessage)message).ModifyAsync(x =>
            {
               x.Embed = BuildBossSelectEmbed(guide.Bosses, selectType, guide.Page, fileName);
            });
            Connections.DeleteFile(fileName);
         }
         else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]) &&
                  guide.Bosses.Count > (guide.Page + 1) * Global.SELECTION_EMOJIS.Length)
         {
            guideMessages[message.Id] = new RaidGuideSelect(guide.Page + 1, guide.Tier, guide.Bosses);
            guide = guideMessages[message.Id];
            string fileName = $"Egg{guide.Tier}.png";
            int selectType = guide.Bosses.Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
            Connections.CopyFile(fileName);
            await ((SocketUserMessage)message).ModifyAsync(x =>
            {
               x.Embed = BuildBossSelectEmbed(guide.Bosses, selectType, guide.Page, fileName);
            });
            Connections.DeleteFile(fileName);
         }
         else
         {
            int options = guide.Bosses.Skip(guide.Page * Global.SELECTION_EMOJIS.Length).Take(Global.SELECTION_EMOJIS.Length).ToList().Count;
            for (int i = 0; i < options; i++)
            {
               if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
               {
                  guideMessages.Remove(message.Id);
                  selectionMade = true;
                  await message.DeleteAsync();

                  Pokemon pkmn = Connections.Instance().GetPokemon(guide.Bosses[(guide.Page * Global.SELECTION_EMOJIS.Length) + i]);
                  Connections.Instance().GetRaidBoss(ref pkmn);

                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidGuideEmbed(pkmn, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
         }

         if (!selectionMade)
         {
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
         }
      }

      /// <summary>
      /// Handles a reaction on a raid poll message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidPollMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         Dictionary<string, int> bosses = pollMessages[message.Id];
         bool pollClosed = false;

         if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
         {
            pollMessages.Remove(message.Id);
            pollClosed = true;
         }

         for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
         {
            if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
            {
               bosses[bosses.ElementAt(i).Key]++;
            }
         }

         string img = message.Embeds.First().Thumbnail.Value.Url;
         await ((SocketUserMessage)message).ModifyAsync(x =>
         {
            x.Embed = BuildRaidPollEmbed(bosses, img.Substring(img.LastIndexOf('/') + 1), pollClosed);
         });
      }

      // Reaction handlers ****************************************************

      /// <summary>
      /// Handles a reaction on a raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="raid">Raid to apply the reaction to.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidReactionHandle(IMessage message, SocketReaction reaction, Raid raid)
      {
         Player reactingPlayer = new Player((SocketGuildUser)reaction.User);
         bool messageExists = true;

         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool needsUpdate = true;
            if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1]))
            {
               raid.AddPlayer(reactingPlayer, 1);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2]))
            {
               raid.AddPlayer(reactingPlayer, 2);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3]))
            {
               raid.AddPlayer(reactingPlayer, 3);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4]))
            {
               raid.AddPlayer(reactingPlayer, 4);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5]))
            {
               raid.AddPlayer(reactingPlayer, 5);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_READY]))
            {
               int group = raid.MarkPlayerReady(reactingPlayer);
               if (group != Global.NOT_IN_RAID)
               {
                  await reaction.Channel.SendMessageAsync(BuildRaidReadyPingList(raid.GetGroup(group).GetPingList(), raid.GetCurrentLocation(), group + 1, true));
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]))
            {
               RestUserMessage remoteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                  embed: BuildPlayerRemoteEmbed(reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username));
               subMessages.Add(remoteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE, message.Id));
               await remoteMsg.AddReactionsAsync(remoteEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID)
               {
                  if (!raid.GetReadonlyInviteList().IsEmpty && !raid.HasActiveInvite())
                  {
                     raid.InvitingPlayer = reactingPlayer;
                     int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
                     int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
                     RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                        embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize));
                     subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));

                     IEmote[] emotes = Global.SELECTION_EMOJIS.Take(listSize).ToArray();
                     if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                     {
                        emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).ToArray();
                     }
                     await inviteMsg.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
                  }
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               RaidRemoveResult returnValue = raid.RemovePlayer(reactingPlayer);

               foreach (Player invite in returnValue.Users)
               {
                  await invite.SocketPlayer.SendMessageAsync(BuildUnInvitedMessage(reactingPlayer));
               }

               if (returnValue.Group != Global.NOT_IN_RAID)
               {
                  await reaction.Channel.SendMessageAsync(BuildRaidReadyPingList(raid.GetGroup(returnValue.Group).GetPingList(), raid.GetCurrentLocation(), returnValue.Group + 1, true));
               }
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               if (raid.IsSingleStop())
               {
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(raidEmojis, raidInteractionDesc, raidReplies, prefix));
               }
               else
               {
                  IEmote[] emojis = raidEmojis.Concat(trainEmojis).ToArray();
                  string[] desc = raidInteractionDesc.Concat(trainInteractionDesc).ToArray();
                  string[] replies = raidReplies.Concat(trainReplies).ToArray();
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(emojis, desc, replies, prefix));
               }
               needsUpdate = false;
            }
            else if (!raid.IsSingleStop())
            {
               if (reactingPlayer.Equals(raid.Conductor))
               {
                  if (reaction.Emote.Equals(trainEmojis[(int)TRAIN_EMOJI_INDEX.BACK_ARROW]))
                  {
                     needsUpdate = raid.PreviousLocation();
                  }
                  else if (reaction.Emote.Equals(trainEmojis[(int)TRAIN_EMOJI_INDEX.FORWARD_ARROR]))
                  {
                     if (raid.AllReady() && raid.NextLocation())
                     {
                        await reaction.Channel.SendMessageAsync(BuildTrainAdvancePingList(raid.GetAllUsers().ToImmutableList(), raid.GetCurrentLocation()));

                        raidMessages.Remove(message.Id);
                        await message.DeleteAsync();

                        string fileName = RAID_TRAIN_IMAGE_NAME;
                        Connections.CopyFile(fileName);
                        RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
                        raidMessages.Add(raidMsg.Id, raid);
                        Connections.DeleteFile(fileName);
                        raidMsg.AddReactionsAsync(raidEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());

                        messageExists = false;
                     }
                  }
                  else if (reaction.Emote.Equals(trainEmojis[(int)TRAIN_EMOJI_INDEX.STATION]))
                  {
                     List<RaidTrainLoc> futureRaids = raid.GetIncompleteRaids();
                     if (raid.StationMessageId.HasValue && reaction.Channel.GetCachedMessage(raid.StationMessageId.Value) != null)
                     {
                        await reaction.Channel.DeleteMessageAsync(raid.StationMessageId.Value);
                     }
                     RestUserMessage stationMsg = await reaction.Channel.SendMessageAsync(embed: BuildStationEmbed(futureRaids, raid.Conductor));
                     raid.StationMessageId = stationMsg.Id;

                     needsUpdate = false;
                  }
               }
            }
            else
            {
               needsUpdate = false;
            }

            if (messageExists && needsUpdate)
            {
               await ModifyMessage((SocketUserMessage)message, raid);
            }
         }
         if (messageExists)
         {
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer.SocketPlayer);
         }
      }

      /// <summary>
      /// Handles a reaction on a raid mule message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="raid">Raid mule to apply the reaction to.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidMuleReactionHandle(IMessage message, SocketReaction reaction, RaidMule raid)
      {
         Player reactingPlayer = new Player((SocketGuildUser)reaction.User);
         bool messageExists = true;

         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool needsUpdate = true;
            if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.ADD_MULE]))
            {
               raid.AddPlayer(reactingPlayer, 1);
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.RAID_READY]))
            {
               if (raid.HasInvites() && raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID)
               {
                  RestUserMessage readyMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                     embed: BuildMuleReadyEmbed(raid.GetTotalGroups(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username));
                  subMessages.Add(readyMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE, message.Id));
                  IEmote[] emotes = Global.SELECTION_EMOJIS.Take(raid.GetTotalGroups()).ToArray();
                  await readyMsg.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
               }
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID &&
                  raid.GetReadonlyInviteList().Count != 0 &&
                  !raid.HasActiveInvite())
               {
                  raid.InvitingPlayer = reactingPlayer;
                  int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
                  int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
                  RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.SocketPlayer.Mention}",
                     embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize));
                  subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
                  IEmote[] emotes = Global.SELECTION_EMOJIS.Take(listSize).ToArray();
                  if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                  {
                     emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).ToArray();
                  }
                  await inviteMsg.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
               }

            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               List<Player> returnValue = raid.RemovePlayer(reactingPlayer).Users;

               foreach (Player invite in returnValue)
               {
                  await invite.SocketPlayer.SendMessageAsync($"{reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".");
               }
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);

               if (raid.IsSingleStop())
               {
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(muleEmojis, muleInteractionDesc, muleReplies, prefix));
               }
               else
               {
                  IEmote[] emojis = muleEmojis.Concat(trainEmojis).ToArray();
                  string[] desc = muleInteractionDesc.Concat(trainInteractionDesc).ToArray();
                  string[] replies = muleReplies.Concat(trainReplies).ToArray();
                  await reactingPlayer.SocketPlayer.SendMessageAsync(BuildHelpMessage(emojis, desc, replies, prefix));
               }
               needsUpdate = false;
            }
            else if (!raid.IsSingleStop())
            {
               if (reactingPlayer.Equals(raid.Conductor))
               {
                  if (reaction.Emote.Equals(trainEmojis[(int)TRAIN_EMOJI_INDEX.BACK_ARROW]))
                  {
                     needsUpdate = raid.PreviousLocation();
                  }
                  else if (reaction.Emote.Equals(trainEmojis[(int)TRAIN_EMOJI_INDEX.FORWARD_ARROR]))
                  {
                     if (raid.NextLocation())
                     {
                        await reaction.Channel.SendMessageAsync(BuildTrainAdvancePingList(raid.GetAllUsers().ToImmutableList(), raid.GetCurrentLocation()));

                        raidMessages.Remove(message.Id);
                        await message.DeleteAsync();

                        string fileName = RAID_TRAIN_IMAGE_NAME;
                        Connections.CopyFile(fileName);
                        RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
                        raidMessages.Add(raidMsg.Id, raid);
                        Connections.DeleteFile(fileName);
                        raidMsg.AddReactionsAsync(muleEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());

                        messageExists = false;
                     }
                  }
                  else if (reaction.Emote.Equals(trainEmojis[(int)TRAIN_EMOJI_INDEX.STATION]))
                  {
                     List<RaidTrainLoc> futureRaids = raid.GetIncompleteRaids();
                     if (raid.StationMessageId.HasValue && reaction.Channel.GetCachedMessage(raid.StationMessageId.Value) != null)
                     {
                        await reaction.Channel.DeleteMessageAsync(raid.StationMessageId.Value);
                     }
                     RestUserMessage stationMsg = await reaction.Channel.SendMessageAsync(embed: BuildStationEmbed(futureRaids, raid.Conductor));
                     raid.StationMessageId = stationMsg.Id;

                     needsUpdate = false;
                  }
               }
            }
            else
            {
               needsUpdate = false;
            }

            if (messageExists && needsUpdate)
            {
               await ModifyMessage((SocketUserMessage)message, raid);
            }
         }
         if (messageExists)
         {
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer.SocketPlayer);
         }
      }

      /// <summary>
      /// Handles a reaction on a raid invite message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidInviteReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         RaidParent parent = raidMessages[raidMessageId];
         Player reactingPlayer = new Player((SocketGuildUser)reaction.User);

         if (reactingPlayer.Equals(parent.InvitingPlayer) || message.MentionedUserIds.Contains(reactingPlayer.SocketPlayer.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
               parent.InvitingPlayer = null;
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]))
            {
               parent.ChangeInvitePage(false, Global.SELECTION_EMOJIS.Length);
               int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(parent.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize);
               });
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]))
            {
               parent.ChangeInvitePage(true, Global.SELECTION_EMOJIS.Length);
               int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(parent.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username, offset, listSize);
               });
            }
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                  {
                     int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
                     Player player = parent.GetReadonlyInviteList().ElementAt(i + offset);
                     if (parent.InvitePlayer(player, reactingPlayer))
                     {
                        await ModifyMessage((SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId), parent);
                        await player.SocketPlayer.SendMessageAsync($"You have been invited to a raid by {reactingPlayer.SocketPlayer.Nickname ?? reactingPlayer.SocketPlayer.Username}.");
                        subMessages.Remove(message.Id);
                        parent.InvitingPlayer = null;
                        await message.DeleteAsync();
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// Handles a reaction on a raid remote message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidRemoteReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         bool needEdit = true;
         Raid raid = (Raid)raidMessages[raidMessageId];
         Player reactingPlayer = new Player((SocketGuildUser)reaction.User);

         if (message.MentionedUserIds.Contains(reactingPlayer.SocketPlayer.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
               needEdit = false;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]))
            {
               raid.AddPlayer(reactingPlayer, 1, reactingPlayer);
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]))
            {
               raid.AddPlayer(reactingPlayer, 2, reactingPlayer);
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]))
            {
               raid.AddPlayer(reactingPlayer, 3, reactingPlayer);
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]))
            {
               raid.AddPlayer(reactingPlayer, 4, reactingPlayer);
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]))
            {
               raid.AddPlayer(reactingPlayer, 5, reactingPlayer);
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6]))
            {
               raid.AddPlayer(reactingPlayer, 6, reactingPlayer);
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOVE_REMOTE]))
            {
               raid.AddPlayer(reactingPlayer, 0, reactingPlayer);

               Dictionary<Player, List<Player>> empty = raid.ClearEmptyPlayer(reactingPlayer);
               foreach (KeyValuePair<Player, List<Player>> user in empty)
               {
                  foreach (Player invite in user.Value)
                  {
                     await invite.SocketPlayer.SendMessageAsync(BuildUnInvitedMessage(user.Key));
                  }
               }
            }
            else
            {
               needEdit = false;
            }

            if (needEdit)
            {
               await ModifyMessage((SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId), raid);
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
         }
      }

      /// <summary>
      /// Handles a reaction on a raid mule ready message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidMuleReadyReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMuleMessageId = subMessages[message.Id].MainMessageId;
         RaidMule raid = (RaidMule)raidMessages[raidMuleMessageId];

         if (message.MentionedUserIds.Contains(reaction.User.Value.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                  {
                     await reaction.Channel.SendMessageAsync($"{BuildRaidReadyPingList(raid.GetGroup(i).GetPingList(), raid.GetCurrentLocation(), i + 1, false)}");
                     subMessages.Remove(message.Id);
                     await message.DeleteAsync();
                  }
               }
            }
         }
      }

      /// <summary>
      /// Handles a reaction on a raid train boss update message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      private static async Task BossEditSelectionReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         RaidParent parent = raidMessages[raidMessageId];
         List<string> raidBosses = null;

         if ((parent.IsSingleStop() && parent.BossEditingPlayer.Equals(reaction.User.Value))
            || parent.Conductor.Equals(reaction.User.Value))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               parent.BossPage = 0;
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
            else if (message.Reactions.ContainsKey(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]))
            {
               if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]))
               {
                  parent.BossPage = 0;
                  await message.RemoveAllReactionsAsync();
                  await ((SocketUserMessage)message).ModifyAsync(x =>
                  {
                     x.Embed = BuildTierSelectEmbed();
                  });
                  await ((SocketUserMessage)message).AddReactionsAsync(tierEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
               }
               else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]) && parent.BossPage > 0)
               {
                  parent.BossPage--;
                  string fileName = $"Egg{parent.Tier}.png";
                  int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
                  Connections.CopyFile(fileName);
                  await ((SocketUserMessage)message).ModifyAsync(x =>
                  {
                     x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]) &&
                        parent.AllBosses[parent.Tier].Count > (parent.BossPage + 1) * Global.SELECTION_EMOJIS.Length)
               {
                  parent.BossPage++;
                  string fileName = $"Egg{parent.Tier}.png";
                  int selectType = parent.AllBosses[parent.Tier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;
                  Connections.CopyFile(fileName);
                  await ((SocketUserMessage)message).ModifyAsync(x =>
                  {
                     x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.Tier], selectType, parent.BossPage, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else
               {
                  int options = parent.AllBosses[parent.Tier].Skip(parent.BossPage * Global.SELECTION_EMOJIS.Length).Take(Global.SELECTION_EMOJIS.Length).ToList().Count;
                  for (int i = 0; i < options; i++)
                  {
                     if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                     {
                        await EditBoss(message, reaction.Channel, parent, raidMessageId, (parent.BossPage * Global.SELECTION_EMOJIS.Length) + i);
                        parent.BossEditingPlayer = null;
                     }
                  }
               }
            }
            else if (reaction.Emote.Equals(tierEmojis[(int)TIER_EMOJI_INDEX.COMMON]))
            {
               parent.SelectionTier = Global.COMMON_RAID_TIER;
               raidBosses = parent.AllBosses[Global.COMMON_RAID_TIER];
            }
            else if (reaction.Emote.Equals(tierEmojis[(int)TIER_EMOJI_INDEX.RARE]))
            {
               parent.SelectionTier = Global.RARE_RAID_TIER;
               raidBosses = parent.AllBosses[Global.RARE_RAID_TIER];
            }
            else if (reaction.Emote.Equals(tierEmojis[(int)TIER_EMOJI_INDEX.LEGENDARY]))
            {
               parent.SelectionTier = Global.LEGENDARY_RAID_TIER;
               raidBosses = parent.AllBosses[Global.LEGENDARY_RAID_TIER];
            }
            else if (reaction.Emote.Equals(tierEmojis[(int)TIER_EMOJI_INDEX.MEGA]))
            {
               parent.SelectionTier = Global.MEGA_RAID_TIER;
               raidBosses = parent.AllBosses[Global.MEGA_RAID_TIER];
            }

            if (raidBosses != null)
            {
               SocketUserMessage msg = (SocketUserMessage)message;
               await msg.RemoveAllReactionsAsync();

               int selectType = raidBosses.Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE_EDIT : (int)SELECTION_TYPES.STANDARD_EDIT;
               await msg.ModifyAsync(x =>
               {
                  x.Embed = BuildBossSelectEmbed(raidBosses, selectType, parent.BossPage, null);
               });
               await msg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(raidBosses.Count)).ToArray()
                        .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW])
                        .Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
            }
         }
      }

#endif
      // Embed builders *******************************************************

      /// <summary>
      /// Builds a raid embed.
      /// </summary>
      /// <param name="raid">Raid to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid.</returns>
      protected static Embed BuildRaidEmbed(Raid raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle(raid.GetCurrentBoss() == null ? "**Empty Raid**" : $"**{raid.GetCurrentBoss()} Raid {BuildRaidTitle(raid.Tier)}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.GetCurrentTime(), true);
         embed.AddField("**Location**", raid.GetCurrentLocation(), true);
         for (int i = 0; i < raid.GetTotalGroups(); i++)
         {
            string groupPrefix = raid.GetTotalGroups() == 1 ? "" : $"Group {i + 1} ";
            RaidGroup group = raid.GetGroup(i);
            int total = group.TotalPlayers();
            int ready = group.GetReadyCount() + group.GetReadyRemoteCount() + group.GetInviteCount();
            int remote = group.GetRemoteCount();

            string attendList = BuildPlayerList(group.GetReadonlyAttending());
            string readyList = BuildPlayerList(group.GetReadonlyHere());
            string invitedAttendList = BuildInvitedList(group.GetReadonlyInvitedAttending());
            string invitedReadyList = BuildInvitedList(group.GetReadonlyInvitedReady());

            embed.AddField($"**{groupPrefix}Ready {ready}/{total}** (Remote {remote}/{Global.LIMIT_RAID_INVITE})", $"{BuildTotalList(readyList, invitedReadyList)}");
            embed.AddField($"**{groupPrefix}Attending**", $"{BuildTotalList(attendList, invitedAttendList)}");
         }
         embed.AddField($"**Need Invite:**", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter($"The max number of members in a raid is {Global.LIMIT_RAID_PLAYER}, and the max number of remote raiders is {Global.LIMIT_RAID_INVITE}.\n" +
                           "Remote raiders include both remotes and invites.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid mule embed.
      /// </summary>
      /// <param name="raid">Raid mule to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid mule.</returns>
      protected static Embed BuildRaidMuleEmbed(RaidMule raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle(raid.GetCurrentBoss() == null ? "**Empty Raid**" : $"**{raid.GetCurrentBoss()} Raid {BuildRaidTitle(raid.Tier)}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.GetCurrentTime(), true);
         embed.AddField("**Location**", raid.GetCurrentLocation(), true);
         embed.AddField($"Mules", $"{BuildPlayerList(raid.Mules.GetReadonlyAttending())}");
         for (int i = 0; i < raid.GetTotalGroups(); i++)
         {
            embed.AddField($"{(raid.GetTotalGroups() == 1 ? "" : $"Group {i + 1} ")}Remote", $"{BuildInvitedList(raid.GetGroup(i).GetReadonlyInvitedAll())}");
         }
         embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter($"Note: The max number of invites is {Global.LIMIT_RAID_INVITE}, and the max number of invites per person is {Global.LIMIT_RAID_MULE_INVITE}.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid train embed.
      /// </summary>
      /// <param name="raid">Raid train to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid train.</returns>
      protected static Embed BuildRaidTrainEmbed(Raid raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**Raid Train Lead By: {raid.Conductor.SocketPlayer.Nickname ?? raid.Conductor.SocketPlayer.Username}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.GetCurrentTime(), true);
         embed.AddField($"**Current Location {raid.GetCurrentRaidCount()}**", $"{raid.GetCurrentLocation()} ({raid.GetCurrentBoss()})", true);
         embed.AddField("**Next Location**", raid.GetNextRaid(), true);
         for (int i = 0; i < raid.GetTotalGroups(); i++)
         {
            string groupPrefix = raid.GetTotalGroups() == 1 ? "" : $"Group {i + 1} ";
            RaidGroup group = raid.GetGroup(i);
            int total = group.TotalPlayers();
            int ready = group.GetReadyCount() + group.GetReadyRemoteCount() + group.GetInviteCount();
            int remote = group.GetRemoteCount();

            string attendList = BuildPlayerList(group.GetReadonlyAttending());
            string readyList = BuildPlayerList(group.GetReadonlyHere());
            string invitedAttendList = BuildInvitedList(group.GetReadonlyInvitedAttending());
            string invitedReadyList = BuildInvitedList(group.GetReadonlyInvitedReady());

            embed.AddField($"**{groupPrefix}Ready {ready}/{total}** (Remote {remote}/{Global.LIMIT_RAID_INVITE})", $"{BuildTotalList(readyList, invitedReadyList)}");
            embed.AddField($"**{groupPrefix}Attending**", $"{BuildTotalList(attendList, invitedAttendList)}");
         }
         embed.AddField($"**Need Invite:**", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter($"Note: the max number of members in a raid is {Global.LIMIT_RAID_PLAYER}, and the max number of invites is {Global.LIMIT_RAID_INVITE}.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid mule train embed.
      /// </summary>
      /// <param name="raid">Raid mule train to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid mule train.</returns>
      protected static Embed BuildRaidMuleTrainEmbed(RaidMule raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**Raid Mule Train Lead By: {raid.Conductor.SocketPlayer.Nickname ?? raid.Conductor.SocketPlayer.Username}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.GetCurrentTime(), true);
         embed.AddField($"**Current Location {raid.GetCurrentRaidCount()}**", $"{raid.GetCurrentLocation()} ({raid.GetCurrentBoss()})", true);
         embed.AddField("**Next Location**", raid.GetNextRaid(), true);
         embed.AddField($"Mules", $"{BuildPlayerList(raid.Mules.GetReadonlyAttending())}");
         for (int i = 0; i < raid.GetTotalGroups(); i++)
         {
            embed.AddField($"{(raid.GetTotalGroups() == 1 ? "" : $"Group {i + 1} ")}Remote", $"{BuildInvitedList(raid.GetGroup(i).GetReadonlyInvitedAll())}");
         }
         embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter($"Note: The max number of invites is {Global.LIMIT_RAID_INVITE}, and the max number of invites per person is {Global.LIMIT_RAID_MULE_INVITE}.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid boss select embed.
      /// </summary>
      /// <param name="potentials">List of potential raid bosses.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <param name="isEdit">Is the selection to edit a raid.</param>
      /// <returns>Embed for selecting a raid boss.</returns>
      protected static Embed BuildBossSelectEmbed(List<string> potentials, int type, int page, string fileName = null)
      {
         List<string> bosses = potentials.Skip(page * Global.SELECTION_EMOJIS.Length).Take(Global.SELECTION_EMOJIS.Length).ToList();
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < bosses.Count; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[(page * Global.SELECTION_EMOJIS.Length) + i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();

         if (type == (int)SELECTION_TYPES.PAGE)
         {
            embed.WithDescription($"Current Page: {page + 1}");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]} Next Page");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]} Previous Page");
         }
         else if (type == (int)SELECTION_TYPES.STANDARD_EDIT)
         {
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]} Change Tier");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");
         }
         else if (type == (int)SELECTION_TYPES.PAGE_EDIT)
         {
            embed.WithDescription($"Current Page: {page + 1}");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]} Next Page");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]} Previous Page");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]} Change Tier");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");
         }

         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"Boss Selection");
         if (!string.IsNullOrEmpty(fileName))
         {
            embed.WithThumbnailUrl($"attachment://{fileName}");
         }
         embed.AddField("Please Select a Boss", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds a raid tier select embed.
      /// </summary>
      /// <returns>Embed for selecting a raid tier.</returns>
      protected static Embed BuildTierSelectEmbed()
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine($"{tierEmojis[(int)TIER_EMOJI_INDEX.COMMON]} Tier 1");
         sb.AppendLine($"{tierEmojis[(int)TIER_EMOJI_INDEX.RARE]} Tier 3");
         sb.AppendLine($"{tierEmojis[(int)TIER_EMOJI_INDEX.LEGENDARY]} Tier 5");
         sb.AppendLine($"{tierEmojis[(int)TIER_EMOJI_INDEX.MEGA]} Mega");
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"Raid Tier Selection");
         embed.AddField("Please Select Tier", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds a train station embed.
      /// </summary>
      /// <param name="futureRaids">List of incomplete raids.</param>
      /// <param name="conductor">Current conductor of the train.</param>
      /// <returns>Embed for viewing future train stations.</returns>
      protected static Embed BuildStationEmbed(List<RaidTrainLoc> futureRaids, Player conductor)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**Stations for Raid Train Lead By: {conductor.SocketPlayer.Nickname ?? conductor.SocketPlayer.Username}**");

         RaidTrainLoc currentLoc = futureRaids.First();

         embed.AddField("**Current Time**", currentLoc.Time, true);
         embed.AddField("**Current Location**", currentLoc.Location, true);
         embed.AddField("**Current Boss**", currentLoc.BossName, true);

         StringBuilder timeSB = new StringBuilder();
         StringBuilder locSB = new StringBuilder();
         StringBuilder bossSB = new StringBuilder();

         foreach (RaidTrainLoc raid in futureRaids.Skip(1))
         {
            timeSB.AppendLine(raid.Time);
            locSB.AppendLine(raid.Location);
            bossSB.AppendLine(raid.BossName);
         }

         embed.AddField("**Time**", futureRaids.Count == 1 ? Global.EMPTY_FIELD : timeSB.ToString(), true);
         embed.AddField("**Location**", futureRaids.Count == 1 ? Global.EMPTY_FIELD : locSB.ToString(), true);
         embed.AddField("**Boss**", futureRaids.Count == 1 ? Global.EMPTY_FIELD : bossSB.ToString(), true);

         return embed.Build();
      }

      /// <summary>
      /// Builds a raid invite embed.
      /// </summary>
      /// <param name="invite">List of players to invite.</param>
      /// <param name="player">Player that wants to invite someone.</param>
      /// <param name="offset">Where to start in the list of invites.</param>
      /// <param name="listSize">How many players to display.</param>
      /// <returns>Embed for inviting a player to a raid.</returns>
      private static Embed BuildPlayerInviteEmbed(ImmutableList<Player> invite, string player, int offset, int listSize)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = offset; i < listSize; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {invite.ElementAt(i).SocketPlayer.Nickname ?? invite.ElementAt(i).SocketPlayer.Username}");
         }

         if (invite.Count > Global.SELECTION_EMOJIS.Length)
         {
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]} Previous Page");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]} Next Page");
         }
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"{player} - Invite");
         embed.AddField("Please Select Player to Invite.", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds a raid remote embed.
      /// </summary>
      /// <param name="player">Player that wants to attend raid via remote.</param>
      /// <returns>Embed for player to attend a raid via remote.</returns>
      private static Embed BuildPlayerRemoteEmbed(string player)
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE]} Need Invite");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]} 1 Remote Raider");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]} 2 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]} 3 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]} 4 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]} 5 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6]} 6 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOVE_REMOTE]} Remove Remote Raiders");
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**{player} - Remote**");
         embed.AddField("Please Select How You Will Remote to the Raid.", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds a raid mule ready embed.
      /// </summary>
      /// <param name="groups">Number of raid groups.</param>
      /// <param name="player">Player acting as the raid mule.</param>
      /// <returns>Embed for mule to mark group as ready.</returns>
      private static Embed BuildMuleReadyEmbed(int groups, string player)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < groups; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} Raid Group {i + 1}");
         }
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"{player} - Raid Mule Ready");
         embed.AddField("Please Select Which Group is Ready.", sb.ToString());
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid guide embed.
      /// </summary>
      /// <param name="pokemon">Raid Boss to display</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Raid Boss.</returns>
      protected static Embed BuildRaidGuideEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{pokemon.Number} {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Type", pokemon.TypeToString(), true);
         embed.AddField("Weather Boosts", pokemon.WeatherToString(), true);
         embed.AddField($"Raid CP (Level {Global.RAID_LEVEL})", pokemon.RaidCPToString(), true);
         embed.AddField("Resistances", pokemon.ResistanceToString(), true);
         embed.AddField("Weaknesses", pokemon.WeaknessToString(), true);
         embed.AddField("Shiniable", pokemon.ShinyToString(), true);

         embed.AddField("Fast Moves", pokemon.FastMoveToString(false), true);
         embed.AddField("Charge Moves", pokemon.ChargeMoveToString(false), true);

         if (pokemon.Difficulty != null)
         {
            embed.AddField("Difficulty", pokemon.DifficultyToString(), true);
         }

         embed.AddField("Normal Counters", pokemon.CounterToString());
         embed.AddField("Special Counters", pokemon.SpecialCounterToString());

         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithFooter($"{Global.STAB_SYMBOL} denotes STAB move.\n {Global.LEGACY_MOVE_SYMBOL} denotes Legacy move.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid boss poll embed.
      /// </summary>
      /// <param name="poll">Dictionary of potential raid bosses and number of votes.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for a raid boss poll.</returns>
      protected static Embed BuildRaidPollEmbed(Dictionary<string, int> poll, string fileName, bool closed)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < poll.Count; i++)
         {
            KeyValuePair<string, int> boss = poll.ElementAt(i);
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} ({boss.Value}) {boss.Key}");
         }

         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Close Poll");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle(closed ? "Poll is Closed" : "Poll is Open");
         embed.WithDescription($"Currently leading: {GetMostVotes(poll)}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Please Select a Boss", sb.ToString());

         return embed.Build();
      }

      // String builders ******************************************************

      /// <summary>
      /// Builds the title of the raid.
      /// </summary>
      /// <param name="tier">Raid tier.</param>
      /// <returns>Raid title as a string.</returns>
      protected static string BuildRaidTitle(short tier)
      {
         if (tier == Global.MEGA_RAID_TIER)
         {
            return Global.NONA_EMOJIS["mega_emote"];
         }
         if (tier == Global.EX_RAID_TIER)
         {
            return Global.NONA_EMOJIS["ex_emote"];
         }
         StringBuilder sb = new StringBuilder();
         string raidSymbol = Global.NONA_EMOJIS["raid_emote"]; ;
         for (int i = 0; i < tier; i++)
         {
            sb.Append(raidSymbol);
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players to ping when a raid group is ready.
      /// </summary>
      /// <param name="players">List of players.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="groupNumber">Group number the players are part of.</param>
      /// <param name="isNormalRaid">Is the raid a normal raid (raid or raid train).</param>
      /// <returns>List of players to ping as a string.</returns>
      protected static string BuildRaidReadyPingList(ImmutableList<Player> players, string location, int groupNumber, bool isNormalRaid)
      {
         StringBuilder sb = new StringBuilder();
         foreach (Player player in players)
         {
            sb.Append($"{player.SocketPlayer.Mention} ");
         }
         if (isNormalRaid)
         {
            sb.Append($"Everyone in Group {groupNumber} is ready at {location}");
         }
         else
         {
            sb.Append($"Invites are going out to Group {groupNumber} at {location}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players to ping when a raid is edited.
      /// </summary>
      /// <param name="players">List of players.</param>
      /// <param name="editor">Player who edited the raid.</param>
      /// <param name="field">Field edited.</param>
      /// <param name="value">New value of the field.</param>
      /// <returns>List of players to ping as a string.</returns>
      protected static string BuildEditPingList(ImmutableList<Player> players, Player editor, string field, string value)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Edit Alert: ");
         foreach (Player player in players)
         {
            sb.Append($"{player.SocketPlayer.Mention} ");
         }
         sb.Append($"{editor.SocketPlayer.Nickname ?? editor.SocketPlayer.Username} has changed {field} to {value} for a raid you are in.");
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players to ping when a raid train is advanced.
      /// </summary>
      /// <param name="players">List of players.</param>
      /// <param name="newLocation">Where the train is heading next.</param>
      /// <returns>List of players to ping as a string.</returns>
      protected static string BuildTrainAdvancePingList(ImmutableList<Player> players, string newLocation)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("The train has left the station: ");
         foreach (Player player in players)
         {
            sb.Append($"{player.SocketPlayer.Mention} ");
         }
         sb.Append($"The raid train is moving to {newLocation}. Please mark yourself as ready when you arrive, or remove yourself if you wish to get off the train.");
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players in a raid.
      /// </summary>
      /// <param name="players">Dictionary of players and the number of accounts they are bringing.</param>
      /// <returns>List of players as a string.</returns>
      private static string BuildPlayerList(ImmutableDictionary<Player, int> players)
      {
         if (players.IsEmpty)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (KeyValuePair<Player, int> player in players)
         {
            sb.AppendLine($"{Global.NUM_EMOJIS[RaidGroup.GetFullPartySize(player.Value) - 1]} {player.Key.SocketPlayer.Nickname ?? player.Key.SocketPlayer.Username} {GetPlayerTeam(player.Key)} ");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds the invite list for a raid group.
      /// </summary>
      /// <param name="players">Dictionary of invited players and who invited them.</param>
      /// <returns>List of invited players as a string.</returns>
      private static string BuildInvitedList(ImmutableDictionary<Player, Player> players)
      {
         if (players.IsEmpty)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (KeyValuePair<Player, Player> player in players)
         {
            sb.AppendLine($"{raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]} {player.Key.SocketPlayer.Nickname ?? player.Key.SocketPlayer.Username} {GetPlayerTeam(player.Key)} invited by {player.Value.SocketPlayer.Nickname ?? player.Value.SocketPlayer.Username}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Combines two lists together.
      /// </summary>
      /// <param name="initList">Initial player list.</param>
      /// <param name="inviteList">Player list to add.</param>
      /// <returns>Combined player list.</returns>
      private static string BuildTotalList(string initList, string inviteList)
      {
         if (initList.Equals(Global.EMPTY_FIELD))
         {
            return inviteList;
         }
         else if (inviteList.Equals(Global.EMPTY_FIELD))
         {
            return initList;
         }
         return initList + inviteList;
      }

      /// <summary>
      /// Builds a list of players requesting an invite to a raid.
      /// </summary>
      /// <param name="players">List of players.</param>
      /// <returns>List of players as a string.</returns>
      private static string BuildRequestInviteList(ImmutableList<Player> players)
      {
         if (players.Count == 0)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (Player player in players)
         {
            sb.AppendLine($"{player.SocketPlayer.Nickname ?? player.SocketPlayer.Username} {GetPlayerTeam(player)}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds the help message.
      /// </summary>
      /// <param name="emotes">List of emotes.</param>
      /// <param name="descriptions">List of emote descriptions.</param>
      /// <param name="replies">List of reply options.</param>
      /// <param name="prefix">Command prefix used for the guild.</param>
      /// <returns>Help messsage as a string.</returns>
      private static string BuildHelpMessage(IEmote[] emotes, string[] descriptions, string[] replies, string prefix)
      {
         int offset = 0;
         IEmote startEmoji = null;
         IEmote endEmoji = null;
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("**Raid Emoji Help:**");

         for (int i = 0; i < emotes.Length; i++)
         {
            if (Global.NUM_EMOJIS.Contains(emotes[i]))
            {
               if (startEmoji == null)
               {
                  startEmoji = emotes[i];
               }
               offset++;
            }
            else
            {
               if (startEmoji != null && endEmoji == null)
               {
                  endEmoji = emotes[i - 1];
                  sb.AppendLine($"{startEmoji} - {endEmoji} {descriptions[i - offset]}");
                  offset--;
               }
               sb.AppendLine($"{emotes[i]} {descriptions[i - offset]}");
            }
         }

         sb.AppendLine("\n*See raid reply help for more options.");

         sb.AppendLine("\n**Raid Reply Help:**");
         sb.AppendLine($"Note: The following must be sent in a reply to the raid embed Use {prefix}help for more information.\n");
         foreach (string reply in replies)
         {
            sb.AppendLine($"{prefix}{reply}");
         }

         return sb.ToString();
      }

      /// <summary>
      /// Builds the uninvited message.
      /// For when the player that has invited others has left the raid.
      /// </summary>
      /// <param name="player">Player that has left the raid.</param>
      /// <returns>Uninvited message as a string</returns>
      protected static string BuildUnInvitedMessage(Player player)
      {
         return $"{player.SocketPlayer.Nickname ?? player.SocketPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".";
      }

      /// <summary>
      /// Builds the raid train remove message.
      /// For when the conductor removes someone from the raid train.
      /// </summary>
      /// <param name="conductor">Conductor of the raid train.</param>
      /// <returns>Remove message as a string.</returns>
      protected static string BuildRaidTrainRemoveMessage(Player conductor)
      {
         return $"You have been removed from a raid train by {conductor.SocketPlayer.Nickname ?? conductor.SocketPlayer.Username}\n" +
                $"This was most likely due to you not marking yourself as ready and holding up the raid train.\n" +
                $"Please keep in mind you are free to leave and rejoin a raid train as you wish.";
      }

      /// <summary>
      /// Builds a list of raid bosses.
      /// </summary>
      /// <param name="bosses">List of raid bosses.</param>
      /// <returns>List of raid bosses as a string.</returns>
      protected static string BuildRaidBossListString(List<string> bosses)
      {
         if (bosses.Count == 0)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (string boss in bosses)
         {
            sb.AppendLine(boss);
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the team role registered to a player.
      /// </summary>
      /// <param name="player">Player to get team role of.</param>
      /// <returns>Team role name of the player.</returns>
      private static string GetPlayerTeam(Player player)
      {
         if (player.SocketPlayer.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["valor_emote"];
         }
         else if (player.SocketPlayer.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["mystic_emote"];
         }
         else if (player.SocketPlayer.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["instinct_emote"];
         }
         return "";
      }

      /// <summary>
      /// Finds which boss(es) has the most votes.
      /// </summary>
      /// <param name="poll">Dictionary of bosses and votes.</param>
      /// <returns>Bosses as a formated string.</returns>
      private static string GetMostVotes(Dictionary<string, int> poll)
      {
         List<string> leading = poll.Where(boss => boss.Value == poll.Values.ToList().Max()).Select(boss => boss.Key).ToList();
         if (leading.Count == poll.Count)
         {
            return "None";
         }
         else if (leading.Count == 2)
         {
            return $"{leading.First()} and {leading.Last()}";
         }
         else if (leading.Count == 1)
         {
            return leading.First();
         }

         StringBuilder sb = new StringBuilder();
         foreach (string boss in leading)
         {
            if (boss.Equals(leading.Last()))
            {
               sb.Append($"and {boss}");
            }
            else
            {
               sb.Append($"{boss}, ");
            }
         }
         return sb.ToString();
      }

      // Miscellaneous ********************************************************

#if COMPONENTS
      /// <summary>
      /// Sends a raid message using a given embed method.
      /// </summary>
      /// <param name="raid">Raid to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <param name="EmbedMethod">Embed method to use.</param>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="component">Components to add to the message</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SendRaidMessage(Raid raid, string fileName, Func<Raid, string, Embed> EmbedMethod, ISocketMessageChannel channel, MessageComponent component)
      {
         Connections.CopyFile(fileName);
         RestUserMessage message = await channel.SendFileAsync(fileName, embed: EmbedMethod(raid, fileName), components: component);
         raidMessages.Add(message.Id, raid);
         Connections.DeleteFile(fileName);
      }

      /// <summary>
      /// Sends a raid mule message using a given embed method.
      /// </summary>
      /// <param name="mule">RaidMule to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <param name="EmbedMethod">Embed method to use.</param>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="component">Components to add to the message</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SendRaidMuleMessage(RaidMule mule, string fileName, Func<RaidMule, string, Embed> EmbedMethod, ISocketMessageChannel channel, MessageComponent component)
      {
         Connections.CopyFile(fileName);
         RestUserMessage message = await channel.SendFileAsync(fileName, embed: EmbedMethod(mule, fileName), components: component);
         raidMessages.Add(message.Id, mule);
         Connections.DeleteFile(fileName);
      }
#else
      /// <summary>
      /// Sends a raid message using a given embed method.
      /// </summary>
      /// <param name="raid">Raid to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <param name="EmbedMethod">Embed method to use.</param>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="emotes">Emotes to add to the message</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SendRaidMessage(Raid raid, string fileName, Func<Raid, string, Embed> EmbedMethod, ISocketMessageChannel channel, IEmote[] emotes)
      {
         Connections.CopyFile(fileName);
         RestUserMessage message = await channel.SendFileAsync(fileName, embed: EmbedMethod(raid, fileName));
         raidMessages.Add(message.Id, raid);
         Connections.DeleteFile(fileName);
         await message.AddReactionsAsync(emotes);
      }

      /// <summary>
      /// Sends a raid mule message using a given embed method.
      /// </summary>
      /// <param name="mule">RaidMule to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <param name="EmbedMethod">Embed method to use.</param>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="emotes">Emotes to add to the message</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SendRaidMuleMessage(RaidMule mule, string fileName, Func<RaidMule, string, Embed> EmbedMethod, ISocketMessageChannel channel, IEmote[] emotes)
      {
         Connections.CopyFile(fileName);
         RestUserMessage message = await channel.SendFileAsync(fileName, embed: EmbedMethod(mule, fileName));
         raidMessages.Add(message.Id, mule);
         Connections.DeleteFile(fileName);
         await message.AddReactionsAsync(emotes);
      }
#endif

      /// <summary>
      /// Sets custom emotes used for raid messages.
      /// </summary>
      public static void SetInitialEmotes()
      {
         raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);
         muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);

         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5];

         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6 - 1];

         tierEmojis[(int)TIER_EMOJI_INDEX.COMMON] = Global.NUM_EMOJIS[(int)TIER_EMOJI_INDEX.COMMON];
         tierEmojis[(int)TIER_EMOJI_INDEX.RARE] = Global.NUM_EMOJIS[(int)TIER_EMOJI_INDEX.RARE + 1];
         tierEmojis[(int)TIER_EMOJI_INDEX.LEGENDARY] = Global.NUM_EMOJIS[(int)TIER_EMOJI_INDEX.LEGENDARY + 2];
         tierEmojis[(int)TIER_EMOJI_INDEX.MEGA] = Emote.Parse(Global.NONA_EMOJIS["mega_emote"]);
      }

      /// <summary>
      /// Removes old raid messages from the list of raid messages.
      /// Old raid messages are messages older than one day.
      /// </summary>
      protected static void RemoveOldRaids()
      {
         List<ulong> ids = new List<ulong>();
         foreach (KeyValuePair<ulong, RaidParent> raid in raidMessages)
         {
            if (Math.Abs((DateTime.Now - raid.Value.CreatedAt).TotalDays) >= 1)
            {
               ids.Add(raid.Key);
            }
         }
         foreach (ulong id in ids)
         {
            raidMessages.Remove(id);
         }
      }

      /// <summary>
      /// Modify the embed of a raid message.
      /// </summary>
      /// <param name="raidMessage">Message to modify.</param>
      /// <param name="parent">Raid parent that defines the embed.</param>
      /// <returns>Completed Task.</returns>
      protected static async Task ModifyMessage(SocketUserMessage raidMessage, RaidParent parent)
      {
         if (parent.IsSingleStop())
         {
            string fileName = Connections.GetPokemonPicture(parent.GetCurrentBoss());
            Connections.CopyFile(fileName);
            if (parent is Raid raid)
            {
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidEmbed(raid, fileName);
               });
            }
            else if (parent is RaidMule mule)
            {
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidMuleEmbed(mule, fileName);
               });
            }
            Connections.DeleteFile(fileName);
         }
         else
         {
            string fileName = RAID_TRAIN_IMAGE_NAME;
            Connections.CopyFile(fileName);
            if (parent is Raid raid)
            {
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidTrainEmbed(raid, fileName);
               });
            }
            else if (parent is RaidMule mule)
            {
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidMuleTrainEmbed(mule, fileName);
               });
            }
            Connections.DeleteFile(fileName);
         }
      }

      /// <summary>
      /// Select a boss for an empty raid.
      /// </summary>
      /// <param name="raidMessage">Selection message for a raid.</param>
      /// <param name="channel">Channel message was sent in.</param>
      /// <param name="parent">Raid that the boss is part of.</param>
      /// <param name="selection">Index of selected boss.</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SelectBoss(IMessage raidMessage, ISocketMessageChannel channel, RaidParent parent, int selection)
      {
         parent.UpdateBoss(selection);

         if (parent is Raid raid)
         {
            if (raid.IsSingleStop())
            {
#if COMPONENTS
               SendRaidMessage(raid, Connections.GetPokemonPicture(raid.GetCurrentBoss()), BuildRaidEmbed, channel,
                  Global.BuildButtons(raidEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                     raidComponents.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
#else
               SendRaidMessage(raid, Connections.GetPokemonPicture(raid.GetCurrentBoss()), BuildRaidEmbed,
                  channel, raidEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());
#endif
            }
            else
            {
#if COMPONENTS
               SendRaidMessage(raid, RAID_TRAIN_IMAGE_NAME, BuildRaidTrainEmbed, channel, Global.BuildButtons(
                  raidEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                  raidComponents.Concat(trainComponents).Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
#else
               SendRaidMessage(raid, RAID_TRAIN_IMAGE_NAME, BuildRaidTrainEmbed, channel,
                  raidEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());
#endif
            }
         }
         else if (parent is RaidMule mule)
         {
            if (mule.IsSingleStop())
            {
#if COMPONENTS
               SendRaidMuleMessage(mule, Connections.GetPokemonPicture(mule.GetCurrentBoss()), BuildRaidMuleEmbed, channel,
                  Global.BuildButtons(muleEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                     muleComponents.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
#else
               SendRaidMuleMessage(mule, Connections.GetPokemonPicture(mule.GetCurrentBoss()), BuildRaidMuleEmbed,
                  channel, muleEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());
#endif
            }
            else
            {
#if COMPONENTS
               SendRaidMuleMessage(mule, RAID_TRAIN_IMAGE_NAME, BuildRaidMuleTrainEmbed, channel, Global.BuildButtons(
                  muleEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                  muleComponents.Concat(trainComponents).Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
#else
               SendRaidMuleMessage(mule, RAID_TRAIN_IMAGE_NAME, BuildRaidMuleTrainEmbed, channel,
                  muleEmojis.Concat(trainEmojis).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());
#endif
            }
         }

         parent.BossPage = 0;
         await raidMessage.DeleteAsync();
         raidMessages.Remove(raidMessage.Id);
      }

      /// <summary>
      /// Select a boss to edit a raid.
      /// </summary>
      /// <param name="subMessage">Boss edit message for a raid.</param>
      /// <param name="channel">Channel message was sent in.</param>
      /// <param name="parent">Raid that the boss is part of.</param>
      /// <param name="raidMessageId">Id of the base raid message.</param>
      /// <param name="selection">Index of selected boss.</param>
      /// <returns>Completed Task.</returns>
      protected static async Task EditBoss(IMessage subMessage, ISocketMessageChannel channel, RaidParent parent, ulong raidMessageId, int selection)
      {
         parent.UpdateBoss(selection);
         SocketUserMessage msg = (SocketUserMessage)await channel.GetMessageAsync(raidMessageId);

         if (parent.IsSingleStop())
         {
            await msg.DeleteAsync();
            raidMessages.Remove(raidMessageId);

            if (parent is Raid raid)
            {
#if COMPONENTS
               SendRaidMessage(raid, Connections.GetPokemonPicture(raid.GetCurrentBoss()), BuildRaidEmbed, channel,
                  Global.BuildButtons(raidEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                     raidComponents.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
#else
               SendRaidMessage(raid, Connections.GetPokemonPicture(raid.GetCurrentBoss()), BuildRaidEmbed,
                  channel, raidEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());
#endif
            }
            else if (parent is RaidMule mule)
            {
#if COMPONENTS
               SendRaidMuleMessage(mule, Connections.GetPokemonPicture(mule.GetCurrentBoss()), BuildRaidMuleEmbed, channel,
                  Global.BuildButtons(muleEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray(),
                     muleComponents.Append(extraComponents[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray()));
#else
               SendRaidMuleMessage(mule, Connections.GetPokemonPicture(mule.GetCurrentBoss()), BuildRaidMuleEmbed,
                  channel, muleEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());
#endif
            }
         }
         else
         {
            await ModifyMessage(msg, parent);
         }

         parent.BossPage = 0;
         subMessages.Remove(subMessage.Id);
         await subMessage.DeleteAsync();
      }
   }
}