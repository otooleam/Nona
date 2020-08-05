﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class RaidCommands : ModuleBase<SocketCommandContext>
   {
      private static readonly Dictionary<ulong, Raid> currentRaids = new Dictionary<ulong, Raid>();
      private static readonly Dictionary<ulong, List<string>> selections = new Dictionary<ulong, List<string>>();
      private static readonly Dictionary<ulong, ulong> raidMessages = new Dictionary<ulong, ulong>();
      private static readonly Emoji[] raidEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("✅"),
         new Emoji("✈️"),
         new Emoji("🤝"),
         new Emoji("🚫"),
         new Emoji("❓")
      };
      private static readonly Emoji[] selectionEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
         new Emoji("7️⃣"),
         new Emoji("8️⃣"),
         new Emoji("9️⃣"),
         new Emoji("🔟")
      };

      private enum RAID_EMOJI_INDEX
      {
         ADD_PLAYER_1,
         ADD_PLAYER_2,
         ADD_PLAYER_3,
         ADD_PLAYER_4,
         ADD_PLAYER_5,
         PLAYER_HERE,
         REQUEST_INVITE,
         INVITE_PLAYER,
         REMOVE_PLAYER,
         HELP
      }

      [Command("raid")]
      [Summary("Creates a new Raid message.")]
      public async Task Raid([Summary("Tier of the raid.")] short tier, //TODO feedback if you enter tier ! 1-5
                             [Summary("Time the raid will start.")] string time,
                             [Summary("Where the raid will be.")][Remainder] string location)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "R"))
         {
            List<string> potentials = Connections.GetBossList(tier);
            if (potentials.Count > 1)
            {
               string fileName = $"Egg{tier}.png";
               Connections.CopyFile(fileName);

               var selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
               for (int i = 0; i < potentials.Count; i++)
                  await selectMsg.AddReactionAsync(selectionEmojis[i]);

               currentRaids.Add(selectMsg.Id, new Raid(tier, time, location));
               selections.Add(selectMsg.Id, potentials);

               Connections.DeleteFile(fileName);
            }
            else
            {
               string fileName;
               Raid raid;
               if (potentials.Count == 1)
               {
                  string boss = potentials.First();
                  raid = new Raid(tier, time, location, boss);
                  fileName = Connections.GetPokemonPicture(raid.Boss.Name);
               }
               else //silph is mid-update or something else went wrong
               {
                  raid = new Raid(tier, time, location, "noboss");
                  fileName = $"Egg{tier}.png";
               }

               Connections.CopyFile(fileName);
               var raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
               await raidMsg.AddReactionsAsync(raidEmojis);
               currentRaids.Add(raidMsg.Id, raid);

               Connections.DeleteFile(fileName);
            }
         }
         else
            await Context.Channel.SendMessageAsync("This channel is not registered to process Raid commands.");
         RemoveOldRaids();
      }

      public static async Task RaidReaction(IMessage message, SocketReaction reaction)
      {
         Raid raid = currentRaids[message.Id];
         SocketGuildUser player = (SocketGuildUser)reaction.User;
         bool needsUpdate = true;

         if (raid.Boss == null)
         {
            bool validReactionAdded = false;
            for (int i = 0; i < selections[message.Id].Count; i++)
            {
               if (reaction.Emote.Equals(selectionEmojis[i]))
               {
                  raid.SetBoss(selections[message.Id][i]);
                  validReactionAdded = true;
               }
            }

            if (validReactionAdded)
            {
               await reaction.Channel.DeleteMessageAsync(message);

               string filename = Connections.GetPokemonPicture(raid.Boss.Name);
               var raidMsg = await reaction.Channel.SendFileAsync(filename, embed: BuildRaidEmbed(raid, filename));
               await raidMsg.AddReactionsAsync(raidEmojis);
               currentRaids.Add(raidMsg.Id, raid);
               needsUpdate = false;
            }
         }
         else
         {
            if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1]))
            {
               raid.PlayerAdd(player, 1);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2]))
            {
               raid.PlayerAdd(player, 2);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3]))
            {
               raid.PlayerAdd(player, 3);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4]))
            {
               raid.PlayerAdd(player, 4);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5]))
            {
               raid.PlayerAdd(player, 5);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_HERE]))
            {
               if (raid.PlayerReady(player)) //true if all players are marked here
               {
                  await reaction.Channel.SendMessageAsync(raid.BuildPingList(player));
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.PlayerRequestInvite(player);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.InviteReqs.Count == 0)
                  await reaction.Channel.SendMessageAsync($"{player.Mention}, There are no players to invite.");
               else if (raid.PlayerIsAttending(player))
               {
                  var inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{player.Mention}", embed: BuildPlayerInviteEmbed(raid, player.Nickname));
                  for (int i = 0; i < raid.InviteReqs.Count; i++)
                     await inviteMsg.AddReactionAsync(selectionEmojis[i]);
                  raidMessages.Add(inviteMsg.Id, message.Id);
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               raid.RemovePlayer(player);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.HELP]))
            {
               //help message - needs no update
               await player.SendMessageAsync(BuildRaidHelpMessage(message.Id));
               await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, player);
               needsUpdate = false;
            }
            else
               needsUpdate = false;
         }

         if (needsUpdate)
         {
            var msg = (SocketUserMessage)message;
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildRaidEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
            });
            await msg.RemoveReactionAsync(reaction.Emote, player);
         }
      }

      public static async Task RaidInviteReaction(IMessage message, SocketReaction reaction, ISocketMessageChannel channel)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         var raidMessageId = raidMessages[message.Id];
         Raid raid = currentRaids[raidMessageId];
         for (int i = 0; i < raid.InviteReqs.Count; i++)
         {
            if (reaction.Emote.Equals(selectionEmojis.ElementAt(i)))
            {
               var player = raid.InviteReqs.ElementAt(i);
               if (raid.InvitePlayer(player, (SocketGuildUser)reaction.User))
               {
                  var raidMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
                  });

                  await player.SendMessageAsync($"You have been invited to a raid by {reaction.User.Value.Username}. Please mark yourself as \"HERE\" when ready.");
                  raidMessages.Remove(message.Id);
                  await message.DeleteAsync();
               }
               return;
            }
         }
      }

      private static Embed BuildRaidEmbed(Raid raid, string fileName = null)
      {
         if (fileName != null)
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);
         Connections.CopyFile(fileName);

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{(raid.Boss.Name.Equals("Bossless") ? "" : raid.Boss.Name)} {BuildRaidTitle(raid.Tier)}");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Time", raid.Time, true);
         embed.AddField("Location", raid.Location, true);

         if (raid.PlayerGroups.Count == 1) //single group
         {
            embed.AddField($"Here ({raid.PlayerGroups[0].ReadyCount}/{raid.PlayerGroups[0].AttendingCount})", $"{BuildPlayerList(raid.PlayerGroups[0].Ready)}");
            embed.AddField("Attending", $"{BuildPlayerList(raid.PlayerGroups[0].Attending)}");
         }
         else //multiple groups
         {
            int groupNum = 0;
            foreach (PlayerGroup group in raid.PlayerGroups) //TODO group header - add limit
            {
               embed.AddField($"Group {groupNum + 1} Here ({raid.PlayerGroups[groupNum].ReadyCount}/{raid.PlayerGroups[groupNum].AttendingCount})", $"{BuildPlayerList(raid.PlayerGroups[groupNum].Ready)}");
               embed.AddField($"Group {groupNum + 1} Attending", $"{BuildPlayerList(raid.PlayerGroups[groupNum].Attending)}"); //TODO embed must have less than 25 fields
               groupNum++;
            }
         }
         embed.AddField("Need Invite", $"{BuildPlayerList(raid.InviteReqs)}");
         embed.WithDescription("Press ? for help");
         embed.WithFooter("Note: the max number of members in a raid is 20, and the max number of invites is 10.");

         return embed.Build();
      }

      private static Embed BuildBossSelectEmbed(List<string> potentials, string selectPic)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
            sb.AppendLine($"{raidEmojis[i]} {potentials[i]}");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle("Raid");
         embed.WithThumbnailUrl($"attachment://{selectPic}");
         embed.AddField("Please Select Boss", sb.ToString());

         return embed.Build();
      }

      private static Embed BuildPlayerInviteEmbed(Raid raid, string user)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < raid.InviteReqs.Count; i++)
            sb.AppendLine($"{raidEmojis[i]} {raid.InviteReqs.ElementAt(i).Nickname}");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{user} - Invite");
         embed.AddField("Please Select Player to invite", sb.ToString());

         return embed.Build();
      }

      private static string BuildRaidTitle(int tier)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Raid ");

         string raidSymbol = Emote.Parse(Environment.GetEnvironmentVariable("RAID_EMOTE")).ToString();

         for (int i = 0; i < tier; i++)
            sb.Append(raidSymbol);

         return sb.ToString();
      }

      private static string BuildPingList(List<SocketGuildUser> players, string loc)
      {
         StringBuilder sb = new StringBuilder();

         foreach (SocketGuildUser player in players)
            sb.Append($"{player.Mention} ");
         sb.Append($"Everyone is ready at {loc}");
         return sb.ToString();
      }

      private static string BuildPlayerList(Dictionary<SocketGuildUser, int> list)
      {
         if (list.Count == 0)
            return "-----";

         StringBuilder sb = new StringBuilder();

         foreach (KeyValuePair<SocketGuildUser, int> player in list)
         {
            string teamString = GetPlayerTeam(player.Key);
            sb.AppendLine($"{raidEmojis[player.Value - 1]} {player.Key.Nickname ?? player.Key.Username} {teamString}");
         }

         return sb.ToString();
      }

      private static string BuildPlayerList(List<SocketGuildUser> list)
      {
         if (list.Count == 0)
            return "-----";

         StringBuilder sb = new StringBuilder();

         foreach (SocketGuildUser player in list)
         {
            string teamString = GetPlayerTeam(player);
            sb.AppendLine($"{player.Nickname ?? player.Username} {teamString}");
         }

         return sb.ToString();
      }

      private static string BuildRaidHelpMessage(ulong code)
      {
         StringBuilder sb = new StringBuilder();

         sb.AppendLine("Raid Help:");
         sb.AppendLine("The numbers represent the number of accounts that you have with you." +
            " React with one of the numbers to show that you intend to participate in the raid.");
         sb.AppendLine($"Once you are ready for the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_HERE]} to show others that you are ready." +
            $" When all players have marked that they are ready, Nona will send a message telling the group to jump.");
         sb.AppendLine($"If you need an invite to participate in the raid remotely, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REQUEST_INVITE]}.");
         sb.AppendLine($"To invite someone to a raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]} and react with the coresponding emote for the player.");
         sb.AppendLine($"If you wish to remove yourself from the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]}.");

         sb.AppendLine("\nRaid Edit (Note this is not implemented yet):");
         sb.AppendLine("To edit the desired raid send the following command in raid channel:");
         sb.AppendLine($"{Environment.GetEnvironmentVariable("PREFIX_STRING")}edit {code} time location");
         sb.AppendLine("Note: Change time and location to desired time and location. Editing Location is optional.");

         return sb.ToString();
      }

      private static string GetPlayerTeam(SocketGuildUser user)
      {
         if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase)) != null)
            return Emote.Parse(Environment.GetEnvironmentVariable("VALOR_EMOTE")).ToString();
         else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase)) != null)
            return Emote.Parse(Environment.GetEnvironmentVariable("MYSTIC_EMOTE")).ToString();
         else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase)) != null)
            return Emote.Parse(Environment.GetEnvironmentVariable("INSTINCT_EMOTE")).ToString();
         return "";
      }

      private static void RemoveOldRaids()
      {
         List<ulong> ids = new List<ulong>();
         foreach (var temp in currentRaids)
            if ((temp.Value.CreatedAt - DateTime.Now).TotalDays >= 1)
               ids.Add(temp.Key);
         foreach (var id in ids)
            currentRaids.Remove(id);
      }

      public static bool IsCurrentRaid(ulong id)
      {
         return currentRaids.ContainsKey(id);
      }

      public static bool isRaidInvite(ulong id)
      {
         return raidMessages.ContainsKey(id);
      }
   }
}