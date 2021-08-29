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

      /// <summary>
      /// Descriptions for raid emotes.
      /// </summary>
      private static readonly string[] raidEmojisDesc = {
         "are the number of Trainers in a group that are raiding in person.",
         "means you are ready for the raid to begin. Nona will notify everyone when all trainers are ready.",
         "means you and/or a group will be either doing the raid remotely; or you need another trainer to send you an invite to raid. *",
         "means you want to invite a trainer who is asking for an invite. The trainer will be counted in the raid as raiding remotely. Nona will notify the person you plan to invite. *",
         "means you want to remove yourself from the raid. Nona will notify anyone you were planning to invite."
      };

      /// <summary>
      /// Descriptions for raid mule emotes.
      /// </summary>
      private static readonly string[] muleEmojisDesc = {
         "means you are able to invite others to the raid.",
         "means that a raid group is ready to go. Can only be done by done by a raid mule. *",
         "means you need a raid mule to send you an invite to the raid.",
         "means you want to invite a trainer who is asking for an invite. Nona will notify the person you plan to invite. Can only be done by a raid mule. *",
         "means you want to remove yourself from the raid. Nona will notify anyone you were planning to invite."
      };

      /// <summary>
      /// Descriptions for raid train emotes.
      /// Only emotes added onto raid.
      /// </summary>
      private static readonly string[] trainEmojisDesc = {
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
         var x = text.Contains(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER].Name);

         return subMessages.ContainsKey(id) && subMessages[id].Type == (int)SUB_MESSAGE_TYPES.EDIT_BOSS_SUB_MESSAGE &&
            text.Contains(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER].Name);
      }

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
         bool messageExists = true;

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

                     messageExists = false;
                     await SelectBoss(message, reaction.Channel, parent, (parent.BossPage * Global.SELECTION_EMOJIS.Length) + i);
                     return;
                  }
               }
            }

            if(messageExists)
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
         bool messageExists = true;

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
                  messageExists = false;
                  message.DeleteAsync();

                  Pokemon pkmn = Connections.Instance().GetPokemon(guide.Bosses[(guide.Page * Global.SELECTION_EMOJIS.Length) + i]);
                  Connections.Instance().GetRaidBoss(ref pkmn);

                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidGuideEmbed(pkmn, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
         }

         if (messageExists)
         {
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
         }
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
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;
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
               RestUserMessage remoteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                  embed: BuildPlayerRemoteEmbed(reactingPlayer.Nickname ?? reactingPlayer.Username));
               subMessages.Add(remoteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE, message.Id));
               remoteMsg.AddReactionsAsync(remoteEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
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
                     RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                        embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize));
                     subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));

                     IEmote[] emotes = Global.SELECTION_EMOJIS.Take(listSize).ToArray();
                     if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                     {
                        emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).ToArray();
                     }
                     inviteMsg.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
                  }
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               RaidRemoveResult returnValue = raid.RemovePlayer(reactingPlayer);

               foreach (SocketGuildUser invite in returnValue.Users)
               {
                  await invite.SendMessageAsync(BuildUnInvitedMessage(reactingPlayer));
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
                  await reactingPlayer.SendMessageAsync(BuildHelpMessage(raidEmojis, raidEmojisDesc, raidReplies, prefix));
               }
               else
               {
                  IEmote[] emojis = raidEmojis.Concat(trainEmojis).ToArray();
                  string[] desc = raidEmojisDesc.Concat(trainEmojisDesc).ToArray();
                  string[] replies = raidReplies.Concat(trainReplies).ToArray();
                  await reactingPlayer.SendMessageAsync(BuildHelpMessage(emojis, desc, replies, prefix));
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
                        message.DeleteAsync();
                        string fileName = RAID_TRAIN_IMAGE_NAME;
                        Connections.CopyFile(fileName);
                        RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
                        raidMessages.Add(raidMsg.Id, raid);
                        Connections.DeleteFile(fileName);
                        SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());

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
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer);
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
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;
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
                  RestUserMessage readyMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                     embed: BuildMuleReadyEmbed(raid.GetTotalGroups(), reactingPlayer.Nickname ?? reactingPlayer.Username));
                  subMessages.Add(readyMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE, message.Id));
                  IEmote[] emotes = Global.SELECTION_EMOJIS.Take(raid.GetTotalGroups()).ToArray();
                  readyMsg.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
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
                  RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                     embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize));
                  subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
                  IEmote[] emotes = Global.SELECTION_EMOJIS.Take(listSize).ToArray();
                  if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                  {
                     emotes = emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).ToArray();
                  }
                  inviteMsg.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
               }

            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               List<SocketGuildUser> returnValue = raid.RemovePlayer(reactingPlayer).Users;

               foreach (SocketGuildUser invite in returnValue)
               {
                  await invite.SendMessageAsync($"{reactingPlayer.Nickname ?? reactingPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".");
               }
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);

               if (raid.IsSingleStop())
               {
                  await reactingPlayer.SendMessageAsync(BuildHelpMessage(muleEmojis, muleEmojisDesc, muleReplies, prefix));
               }
               else
               {
                  IEmote[] emojis = muleEmojis.Concat(trainEmojis).ToArray();
                  string[] desc = muleEmojisDesc.Concat(trainEmojisDesc).ToArray();
                  string[] replies = muleReplies.Concat(trainReplies).ToArray();
                  await reactingPlayer.SendMessageAsync(BuildHelpMessage(emojis, desc, replies, prefix));
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
                        message.DeleteAsync();
                        string fileName = RAID_TRAIN_IMAGE_NAME;
                        Connections.CopyFile(fileName);
                        RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
                        raidMessages.Add(raidMsg.Id, raid);
                        Connections.DeleteFile(fileName);
                        SetEmojis(raidMsg, muleEmojis.Concat(trainEmojis).ToArray());

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
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer);
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
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (reactingPlayer.Equals(parent.InvitingPlayer) || message.MentionedUserIds.Contains(reactingPlayer.Id))
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
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
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
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                  {
                     int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
                     SocketGuildUser player = parent.GetReadonlyInviteList().ElementAt(i + offset);
                     if (parent.InvitePlayer(player, reactingPlayer))
                     {
                        await ModifyMessage((SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId), parent);
                        await player.SendMessageAsync($"You have been invited to a raid by {reactingPlayer.Nickname ?? reactingPlayer.Username}.");
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
         bool needEdit = false;
         Raid raid = (Raid)raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (message.MentionedUserIds.Contains(reactingPlayer.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]))
            {
               raid.AddPlayer(reactingPlayer, 1, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]))
            {
               raid.AddPlayer(reactingPlayer, 2, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]))
            {
               raid.AddPlayer(reactingPlayer, 3, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]))
            {
               raid.AddPlayer(reactingPlayer, 4, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]))
            {
               raid.AddPlayer(reactingPlayer, 5, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6]))
            {
               raid.AddPlayer(reactingPlayer, 6, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOVE_REMOTE]))
            {
               raid.AddPlayer(reactingPlayer, 0, reactingPlayer);

               Dictionary<SocketGuildUser, List<SocketGuildUser>> empty = raid.ClearEmptyPlayer(reactingPlayer);
               foreach (KeyValuePair<SocketGuildUser, List<SocketGuildUser>> user in empty)
               {
                  foreach (SocketGuildUser invite in user.Value)
                  {
                     await invite.SendMessageAsync(BuildUnInvitedMessage(user.Key));
                  }
               }
               needEdit = true;
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
                  ((SocketUserMessage)message).AddReactionsAsync(tierEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
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
               msg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(raidBosses.Count)).ToArray()
                  .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW])
                  .Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
            }
         }
      }

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
      /// Builds a raid mule train embed.
      /// </summary>
      /// <param name="raid">Raid mule train to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid mule train.</returns>
      protected static Embed BuildRaidMuleTrainEmbed(RaidMule raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**Raid Mule Train Lead By: {raid.Conductor.Nickname ?? raid.Conductor.Username}**");
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
      /// Builds a raid train embed.
      /// </summary>
      /// <param name="raid">Raid train to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid train.</returns>
      protected static Embed BuildRaidTrainEmbed(Raid raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**Raid Train Lead By: {raid.Conductor.Nickname ?? raid.Conductor.Username}**");
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
         embed.AddField("Please Select Boss", sb.ToString());

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
      protected static Embed BuildStationEmbed(List<RaidTrainLoc> futureRaids, SocketGuildUser conductor)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**Stations for Raid Train Lead By: {conductor.Nickname ?? conductor.Username}**");

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
      private static Embed BuildPlayerInviteEmbed(ImmutableList<SocketGuildUser> invite, string player, int offset, int listSize)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = offset; i < listSize; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {invite.ElementAt(i).Nickname ?? invite.ElementAt(i).Username}");
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
      protected static string BuildRaidReadyPingList(ImmutableList<SocketGuildUser> players, string location, int groupNumber, bool isNormalRaid)
      {
         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
         {
            sb.Append($"{player.Mention} ");
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
      protected static string BuildEditPingList(ImmutableList<SocketGuildUser> players, SocketGuildUser editor, string field, string value)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Edit Alert: ");
         foreach (SocketGuildUser player in players)
         {
            sb.Append($"{player.Mention} ");
         }
         sb.Append($"{editor.Nickname ?? editor.Username} has changed {field} to {value} for a raid you are in.");
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players to ping when a raid train is advanced.
      /// </summary>
      /// <param name="players">List of players.</param>
      /// <param name="newLocation">Where the train is heading next.</param>
      /// <returns>List of players to ping as a string.</returns>
      protected static string BuildTrainAdvancePingList(ImmutableList<SocketGuildUser> players, string newLocation)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("The train has left the station: ");
         foreach (SocketGuildUser player in players)
         {
            sb.Append($"{player.Mention} ");
         }
         sb.Append($"The raid train is moving to {newLocation}. Please mark yourself as ready when you arrive, or remove yourself if you wish to get off the train.");
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players in a raid.
      /// </summary>
      /// <param name="players">Dictionary of players and the number of accounts they are bringing.</param>
      /// <returns>List of players as a string.</returns>
      private static string BuildPlayerList(ImmutableDictionary<SocketGuildUser, int> players)
      {
         if (players.IsEmpty)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (KeyValuePair<SocketGuildUser, int> player in players)
         {
            sb.AppendLine($"{Global.NUM_EMOJIS[RaidGroup.GetFullPartySize(player.Value) - 1]} {player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} ");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds the invite list for a raid group.
      /// </summary>
      /// <param name="players">Dictionary of invited players and who invited them.</param>
      /// <returns>List of invited players as a string.</returns>
      private static string BuildInvitedList(ImmutableDictionary<SocketGuildUser, SocketGuildUser> players)
      {
         if (players.IsEmpty)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (KeyValuePair<SocketGuildUser, SocketGuildUser> player in players)
         {
            sb.AppendLine($"{raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]} {player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} invited by {player.Value.Nickname ?? player.Value.Username}");
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
      private static string BuildRequestInviteList(ImmutableList<SocketGuildUser> players)
      {
         if (players.Count == 0)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
         {
            sb.AppendLine($"{player.Nickname ?? player.Username} {GetPlayerTeam(player)}");
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
      protected static string BuildUnInvitedMessage(SocketGuildUser player)
      {
         return $"{player.Nickname ?? player.Username} has left the raid. You have been moved back to \"Need Invite\".";
      }

      /// <summary>
      /// Builds the raid train remove message.
      /// For when the conductor removes someone from the raid train.
      /// </summary>
      /// <param name="conductor">Conductor of the raid train.</param>
      /// <returns>Remove message as a string.</returns>
      protected static string BuildRaidTrainRemoveMessage(SocketGuildUser conductor)
      {
         return $"You have been removed from a raid train by {conductor.Nickname ?? conductor.Username}\n" +
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
      private static string GetPlayerTeam(SocketGuildUser player)
      {
         if (player.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["valor_emote"];
         }
         else if (player.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["mystic_emote"];
         }
         else if (player.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["instinct_emote"];
         }
         return "";
      }

      // Miscellaneous ********************************************************

      /// <summary>
      /// Set emotes on a raid message.
      /// Will add the help emote at the end.
      /// </summary>
      /// <param name="message">Message to add emotes to.</param>
      /// <param name="emotes">Emotes to add.</param>
      /// <returns>Completed Task.</returns>
      protected static void SetEmojis(RestUserMessage message, IEmote[] emotes)
      {
         message.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]).ToArray());
      }

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
               string fileName = Connections.GetPokemonPicture(parent.GetCurrentBoss());
               Connections.CopyFile(fileName);
               RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
               raidMessages.Add(raidMsg.Id, parent);
               Connections.DeleteFile(fileName);
               SetEmojis(raidMsg, raidEmojis);
            }
            else
            {
               string fileName = RAID_TRAIN_IMAGE_NAME;
               Connections.CopyFile(fileName);
               RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
               raidMessages.Add(raidMsg.Id, parent);
               Connections.DeleteFile(fileName);
               SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
            }
         }
         else if (parent is RaidMule mule)
         {
            if (mule.IsSingleStop())
            {
               string fileName = Connections.GetPokemonPicture(parent.GetCurrentBoss());
               Connections.CopyFile(fileName);
               RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
               raidMessages.Add(raidMsg.Id, parent);
               Connections.DeleteFile(fileName);
               SetEmojis(raidMsg, muleEmojis);
            }
            else
            {
               string fileName = RAID_TRAIN_IMAGE_NAME;
               Connections.CopyFile(fileName);
               RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(mule, fileName));
               raidMessages.Add(raidMsg.Id, parent);
               Connections.DeleteFile(fileName);
               SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
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

            string fileName = Connections.GetPokemonPicture(parent.GetCurrentBoss());
            Connections.CopyFile(fileName);
            if (parent is Raid raid)
            {
               RestUserMessage raidMessage = await channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
               raidMessages.Add(raidMessage.Id, raid);
               SetEmojis(raidMessage, raidEmojis);
            }
            else if (parent is RaidMule mule)
            {
               RestUserMessage raidMessage = await channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
               raidMessages.Add(raidMessage.Id, mule);
               SetEmojis(raidMessage, muleEmojis);
            }
            Connections.DeleteFile(fileName);
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