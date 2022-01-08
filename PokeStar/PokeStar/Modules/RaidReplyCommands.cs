using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles raid reply commands.
   /// </summary>
   public class RaidReplyCommands : RaidCommandParent
   {
      /// <summary>
      /// Handle edit command.
      /// </summary>
      /// <param name="attribute">Portion of the raid message to edit.</param>
      /// <param name="value">New value of the edited attribute.</param>
      /// <returns></returns>
      [Command("edit")]
      [Summary("Edit the time, location (loc), or tier/boss of a raid.")]
      [Remarks("Must be a reply to any type of raid message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Edit([Summary("Portion of the raid message to edit.")] string attribute,
                             [Summary("New value of the edited attribute.")][Remainder] string value)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];

         if (!parent.IsSingleStop() && !Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(raidMessage.Channel, "edit", $"Command can only be run by the current conductor.");
         }
         else
         {
            ISocketMessageChannel channel = raidMessage.Channel;
            bool needsEdit = false;
            if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
               parent.UpdateRaidInformation(value, null);
               needsEdit = true;
            }
            else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) || attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
            {
               parent.UpdateRaidInformation(null, value);
               needsEdit = true;
            }
            else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) || attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
            {
               short calcTier = Global.RAID_TIER_STRING.ContainsKey(value) ? Global.RAID_TIER_STRING[value] : Global.INVALID_RAID_TIER;

               if (calcTier == Global.INVALID_RAID_TIER)
               {
                  await ResponseMessage.SendErrorMessage(channel, "edit", $"No raid bosses found for tier {value}.");
               }
               else
               {
                  SocketGuildUser author = (SocketGuildUser)Context.Message.Author;
                  if (parent.IsSingleStop())
                  {
                     parent.BossEditingPlayer = author;
                  }
                  parent.SelectionTier = calcTier;
                  int selectType = parent.AllBosses[calcTier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE_EDIT : (int)SELECTION_TYPES.STANDARD_EDIT;
                  RestUserMessage bossMsg = await Context.Channel.SendMessageAsync(text: $"{author.Mention}",
                     embed: BuildBossSelectEmbed(parent.AllBosses[calcTier], selectType, parent.BossPage, null));
                  subMessages.Add(bossMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.EDIT_BOSS_SUB_MESSAGE, raidMessage.Id));
                  bossMsg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(parent.AllBosses[calcTier].Count)).ToArray()
                     .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW])
                     .Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(channel, "edit", "Please enter a valid field to edit.");
            }

            if (needsEdit)
            {
               await ModifyMessage(raidMessage, parent);
               await Context.Channel.SendMessageAsync(BuildEditPingList(parent.GetAllUsers().ToImmutableList(), (SocketGuildUser)Context.Message.Author, attribute, value));
            }
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle invite command.
      /// </summary>
      /// <param name="invites">List of users to invite, separated by spaces.</param>
      /// <returns>Completed Task.</returns>
      [Command("invite")]
      [Summary("Invite user(s) to the raid.")]
      [Remarks("Users may be mentioned using \'@\' or username/nickname may be used.\n" +
               "The user must be in the raid and not already being invited." +
               "Must be a reply to any type of raid message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Invite([Summary("List of users to invite, separated by spaces.")][Remainder] string invites)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsInRaid((SocketGuildUser)Context.Message.Author, false) != Global.NOT_IN_RAID)
         {
            if (!parent.HasActiveInvite())
            {
               parent.InvitingPlayer = (SocketGuildUser)Context.Message.Author;

               List<string> inviteList = invites.Split(' ').ToList();
               inviteList.RemoveAll(x => x.StartsWith("<@", StringComparison.OrdinalIgnoreCase));

               List<SocketUser> mentioned = Context.Message.MentionedUsers.ToList();
               bool failedInvites = false;

               foreach (string openInvite in inviteList)
               {
                  SocketGuildUser invite = Context.Guild.Users.FirstOrDefault(x => x.Username.Equals(openInvite, StringComparison.OrdinalIgnoreCase) || (
                                                                                   x.Nickname != null && x.Nickname.Equals(openInvite, StringComparison.OrdinalIgnoreCase)));
                  if (invite != null && !mentioned.Contains(invite))
                  {
                     mentioned.Add(invite);
                  }
                  else
                  {
                     failedInvites = true;
                  }
               }

               foreach (SocketUser invite in mentioned)
               {
                  if (parent.InvitePlayer((SocketGuildUser)invite, parent.InvitingPlayer))
                  {
                     await invite.SendMessageAsync($"You have been invited to a raid by {parent.InvitingPlayer.Nickname ?? parent.InvitingPlayer.Username}.");
                  }
                  else
                  {
                     failedInvites = true;
                  }
               }

               if (failedInvites)
               {
                  await ResponseMessage.SendWarningMessage(Context.Channel, "invite", "Some users where not found to be invited");
               }
               await ModifyMessage(raidMessage, parent);
               parent.InvitingPlayer = null;
            }
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle request command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("request")]
      [Summary("Request an invite to a raid.")]
      [Remarks("The user must not already be in the raid." +
               "Must be a reply to a raid or raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Request()
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];

         parent.RequestInvite((SocketGuildUser)Context.Message.Author);
         await ModifyMessage(raidMessage, parent);

         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle remote command.
      /// </summary>
      /// <param name="groupSize">Amount of users raiding remotly 0 - 6.</param>
      /// <returns>Completed Task.</returns>
      [Command("remote")]
      [Summary("Participate in the raid remotly without an invite.")]
      [Remarks("Must be a reply to a raid or raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Remote([Summary("Amount of users raiding remotly 0 - 6.")] int groupSize)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (!(parent is Raid raid))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remote", $"Command must be a reply to a raid or raid train message.");
         }
         else if (groupSize < 0 || groupSize > 6)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remote", "Value must be a number between 0 and 6");
         }
         else
         {
            SocketGuildUser author = (SocketGuildUser)Context.Message.Author;
            raid.AddPlayer(author, groupSize, author);
            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle ready command.
      /// </summary>
      /// <param name="groupNum">Number of the raid group that is ready to start.</param>
      /// <returns>Completed Task.</returns>
      [Command("ready")]
      [Summary("Mark a raid group as ready.")]
      [Remarks("Can only be run by a raid mule for the raid." +
               "Must be a reply to a raid mule message")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Ready([Summary("Number of the raid group that is ready to start.")] int groupNum)
      {
         RaidParent parent = raidMessages[Context.Message.Reference.MessageId.Value];
         if (!(parent is RaidMule mule))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "ready", $"Command must be a reply to a raid mule or raid mule train message.");
         }
         else if (mule.GetTotalGroups() > groupNum || groupNum <= 0)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "ready", $"{groupNum} is not a valid raid group number.");
         }
         else
         {
            await Context.Channel.SendMessageAsync($"{BuildRaidReadyPingList(mule.GetGroup(groupNum - 1).GetPingList(), mule.GetCurrentLocation(), groupNum, false)}");
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle add command.
      /// </summary>
      /// <param name="time">Time of the raid.</param>
      /// <param name="location">Location of the raid.</param>
      /// <returns>Completed Task.</returns>
      [Command("add")]
      [Summary("Add a raid to the end of the raid train.")]
      [Remarks("Can only be run by the train\'s conductor." +
               "Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Add([Summary("Time of the raid.")] string time,
                            [Summary("Location of the raid.")][Remainder] string location)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else if (!Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command can only be run by the current conductor.");
         }
         else
         {
            parent.AddRaid(time, location);
            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle addt command.
      /// </summary>
      /// <param name="boss">Boss role of the raid.</param>
      /// <param name="time">Time of the raid.</param>
      /// <param name="location">Location of the raid.</param>
      /// <returns>Completed Task.</returns>
      [Command("addt")]
      [Summary("Add a raid to the end of the raid train using a boss role.")]
      [Remarks("Can only be run by the train\'s conductor." +
               "Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Add([Summary("Boss role of the raid.")] IRole boss,
                            [Summary("Time of the raid.")] string time,
                            [Summary("Location of the raid.")][Remainder] string location)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else if (!Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command can only be run by the current conductor.");
         }
         else
         {
            Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
            string bossName = Connections.GetPokemonFromPicture(boss.Name);

            short calcTier = 0;

            foreach (KeyValuePair<int, List<string>> tier in allBosses)
            {
               foreach (string potentialBoss in tier.Value)
               {
                  if (potentialBoss.Equals(bossName, StringComparison.OrdinalIgnoreCase))
                  {
                     calcTier = Global.RAID_TIER_STRING[tier.Key.ToString()];
                  }
               }
            }
            if (calcTier != 0)
            {
               parent.AddRaid(time, location, bossName);
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "addt", $"No raid bosses found for role {boss.Name}");
            }
            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle conductor command.
      /// </summary>
      /// <param name="conductor">User to make new conductor.</param>
      /// <returns>Completed Task.</returns>
      [Command("conductor")]
      [Summary("Change the current conductor of the raid train.")]
      [Remarks("Can only be run by the train\'s conductor." +
               "Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Conductor([Summary("User to make new conductor.")] IGuildUser conductor)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else if (!Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"Command can only be run by the current conductor.");
         }
         else if (parent.IsInRaid((SocketGuildUser)conductor, false) == Global.NOT_IN_RAID)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"New conductor must be in the train.");
         }
         else
         {
            parent.Conductor = (SocketGuildUser)conductor;
            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle remove command.
      /// </summary>
      /// <param name="user">User to remove from raid train.</param>
      /// <returns>Completed Task.</returns>
      [Command("remove")]
      [Summary("Remove a user from a raid train.")]
      [Remarks("This should only be used to remove a user that " +
               "is preventing the train from moving forward." +
               "Can only be run by the train\'s conductor." +
               "Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Remove([Summary("User to remove from raid train.")] IGuildUser user)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remove", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else if (!Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remove", $"Command can only be run by the current conductor.");
         }
         else if (parent.IsInRaid((SocketGuildUser)user, false) == Global.NOT_IN_RAID)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remove", $"The user is not in the train.");
         }
         else
         {
            RaidRemoveResult returnValue = parent.RemovePlayer((SocketGuildUser)user);

            foreach (SocketGuildUser invite in returnValue.Users)
            {
               await invite.SendMessageAsync(BuildUnInvitedMessage((SocketGuildUser)user));
            }

            await user.SendMessageAsync(BuildRaidTrainRemoveMessage((SocketGuildUser)Context.Message.Author));

            if (returnValue.Group != Global.NOT_IN_RAID)
            {
               await Context.Channel.SendMessageAsync(BuildRaidReadyPingList(parent.GetGroup(returnValue.Group).GetPingList(), parent.GetCurrentLocation(), returnValue.Group + 1, true));
            }

            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle station command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("station")]
      [Alias("stations")]
      [Summary("View a list of upcoming stations.")]
      [Remarks("Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Station()
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "station", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else
         {
            List<RaidTrainLoc> futureRaids = parent.GetIncompleteRaids();
            if (parent.StationMessageId.HasValue && Context.Channel.GetCachedMessage(parent.StationMessageId.Value) != null)
            {
               await Context.Channel.DeleteMessageAsync(parent.StationMessageId.Value);
            }
            RestUserMessage stationMsg = await Context.Channel.SendMessageAsync(embed: BuildStationEmbed(futureRaids, parent.Conductor));
            parent.StationMessageId = stationMsg.Id;
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle upgrade command.
      /// </summary>
      /// <param name="conductor">(Optional) User to make the conductor.</param>
      /// <returns>Completed Task.</returns>
      [Command("upgrade")]
      [Summary("Upgrade a raid or raid mule to a raid train or raid mule train respectively.")]
      [Remarks("Must be a reply to a raid message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Upgrade([Summary("(Optional) User to make new conductor.")] IGuildUser conductor = null)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         RaidParent parent = raidMessages[raidMessageId];
         SocketGuildUser newConductor = conductor == null ? (SocketGuildUser)Context.Message.Author : (SocketGuildUser)conductor;

         if (!parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "upgrade", $"Command must be a reply to a raid or raid mule message.");
         }
         else
         {
            parent.Conductor = newConductor;
            Context.Channel.DeleteMessageAsync(raidMessageId);
            raidMessages.Remove(raidMessageId);

            string fileName = RAID_TRAIN_IMAGE_NAME;
            Connections.CopyFile(fileName);
            if (parent is Raid raid)
            {
               RestUserMessage raidMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
               raidMessages.Add(raidMessage.Id, raid);
               SetEmojis(raidMessage, raidEmojis.Concat(trainEmojis).ToArray());
            }
            else if (parent is RaidMule mule)
            {
               RestUserMessage raidMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(mule, fileName));
               raidMessages.Add(raidMessage.Id, mule);
               SetEmojis(raidMessage, muleEmojis.Concat(trainEmojis).ToArray());
            }
            Connections.DeleteFile(fileName);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle cancel command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("cancel")]
      [Summary("Cancel a raid sub message.")]
      [Remarks("Must be a reply to a raid sub message.")]
      [RegisterChannel('R')]
      [RaidSubMessageReply()]
      public async Task Cancel()
      {
         ulong subMessageId = Context.Message.Reference.MessageId.Value;
         raidMessages[subMessages[subMessageId].MainMessageId].BossPage = 0;
         await (await Context.Channel.GetMessageAsync(Context.Message.Reference.MessageId.Value)).DeleteAsync();
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle next command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("next")]
      [Alias("nextPage", "nPage")]
      [Summary("Move to next page of raid select message.")]
      [Remarks("Must be a reply to a raid select message or raid edit message.")]
      [RegisterChannel('R')]
      [BossSelectReply()]
      public async Task Next()
      {
         ulong selectMessageId = Context.Message.Reference.MessageId.Value;
         RaidParent parent = raidMessages.ContainsKey(selectMessageId) ? raidMessages[selectMessageId] : raidMessages[subMessages[selectMessageId].MainMessageId];

         if (parent.AllBosses[parent.SelectionTier].Count > (parent.BossPage + 1) * Global.SELECTION_EMOJIS.Length)
         {
            parent.BossPage++;
            string fileName = $"Egg{parent.SelectionTier}.png";
            bool messageType = IsRaidSelectMessage(Context.Message.Reference.MessageId.Value);
            int selectType = parent.AllBosses[parent.SelectionTier].Count > Global.SELECTION_EMOJIS.Length ? messageType ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.PAGE_EDIT : messageType ? (int)SELECTION_TYPES.STANDARD : (int)SELECTION_TYPES.STANDARD_EDIT;
            Connections.CopyFile(fileName);
            await ((SocketUserMessage)await Context.Channel.GetMessageAsync(selectMessageId)).ModifyAsync(x =>
            {
               x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.SelectionTier], selectType, parent.BossPage, fileName);
            });
            Connections.DeleteFile(fileName);
         }

         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle previous command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("previous")]
      [Alias("prev", "previousPage", "prevPage", "pPage")]
      [Summary("Move to previous page of raid select message.")]
      [Remarks("Must be a reply to a raid select message or raid edit message.")]
      [RegisterChannel('R')]
      [BossSelectReply()]
      public async Task Previous()
      {
         ulong selectMessageId = Context.Message.Reference.MessageId.Value;
         RaidParent parent = raidMessages.ContainsKey(selectMessageId) ? raidMessages[selectMessageId] : raidMessages[subMessages[selectMessageId].MainMessageId];

         if (parent.BossPage > 0)
         {
            parent.BossPage--;
            string fileName = $"Egg{parent.SelectionTier}.png";
            bool messageType = IsRaidSelectMessage(Context.Message.Reference.MessageId.Value);
            int selectType = parent.AllBosses[parent.SelectionTier].Count > Global.SELECTION_EMOJIS.Length ? messageType ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.PAGE_EDIT : messageType ? (int)SELECTION_TYPES.STANDARD : (int)SELECTION_TYPES.STANDARD_EDIT;
            Connections.CopyFile(fileName);
            await ((SocketUserMessage)await Context.Channel.GetMessageAsync(selectMessageId)).ModifyAsync(x =>
            {
               x.Embed = BuildBossSelectEmbed(parent.AllBosses[parent.SelectionTier], selectType, parent.BossPage, fileName);
            });
            Connections.DeleteFile(fileName);
         }

         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle select command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("select")]
      [Summary("Select an option on a raid select message.")]
      [Remarks("Must be a reply to a raid select message or raid edit message.")]
      [RegisterChannel('R')]
      [BossSelectReply()]
      public async Task Select([Summary("Option to select.")] int selection)
      {
         ulong selectMessageId = Context.Message.Reference.MessageId.Value;
         RaidParent parent = raidMessages.ContainsKey(selectMessageId) ? raidMessages[selectMessageId] : raidMessages[subMessages[selectMessageId].MainMessageId];
         int pageMax = (parent.BossPage + 1) * Global.SELECTION_EMOJIS.Length;
         if (selection > 0 && selection < (pageMax > parent.AllBosses[parent.SelectionTier].Count ? parent.AllBosses[parent.SelectionTier].Count - (pageMax - Global.SELECTION_EMOJIS.Length) : Global.SELECTION_EMOJIS.Length))
         {
            if (IsRaidSelectMessage(Context.Message.Reference.MessageId.Value))
            {
               await SelectBoss((SocketUserMessage)await Context.Channel.GetMessageAsync(selectMessageId), Context.Channel, parent, 
                  (parent.BossPage * Global.SELECTION_EMOJIS.Length) + (selection - 1));
            }
            else
            {
               await EditBoss((SocketUserMessage)await Context.Channel.GetMessageAsync(selectMessageId), Context.Channel, parent,
                  subMessages[selectMessageId].MainMessageId, (parent.BossPage * Global.SELECTION_EMOJIS.Length) + (selection - 1));
               parent.BossEditingPlayer = null;
            }
         }

         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle tier command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("tier")]
      [Summary("Move to the select tier menu of a raid edit message.")]
      [Remarks("Must be a reply to a raid edit message.")]
      [RegisterChannel('R')]
      [BossEditReply()]
      public async Task Tier()
      {
         ulong editMessageId = Context.Message.Reference.MessageId.Value;
         RaidParent parent = raidMessages[subMessages[editMessageId].MainMessageId];
         var message = await Context.Channel.GetMessageAsync(Context.Message.Reference.MessageId.Value);

         parent.BossPage = 0;
         await message.RemoveAllReactionsAsync();
         await ((SocketUserMessage)message).ModifyAsync(x =>
         {
            x.Embed = BuildTierSelectEmbed();
         });
         ((SocketUserMessage)message).AddReactionsAsync(tierEmojis.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());

         await Context.Message.DeleteAsync();
      }
   }
}