using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles raid commands.
   /// </summary>
   public class RaidCommands : RaidCommandParent
   {
      /// <summary>
      /// Handle raid command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("raid")]
      [Summary("Creates a new raid coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task Raid([Summary("Tier of the raid.")] string tier,
                             [Summary("Time the raid will start.")] string time,
                             [Summary("Where the raid will be.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         Raid raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new Raid(calcTier, time, location)
            {
               AllBosses = allBosses
            };

            int selectType = allBosses[calcTier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, selectType, raid.BossPage, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(potentials.Count)).ToArray()
               .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new Raid(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new Raid(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "raid", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle mule command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("mule")]
      [Alias("raidmule")]
      [Summary("Creates a new remote raid coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidMule([Summary("Tier of the raid.")] string tier,
                                 [Summary("Time the raid will start.")] string time,
                                 [Summary("Where the raid will be.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         RaidMule raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new RaidMule(calcTier, time, location)
            {
               AllBosses = allBosses
            };

            int selectType = allBosses[calcTier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, selectType, raid.BossPage, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(potentials.Count)).ToArray()
               .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new RaidMule(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidMule(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "mule", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle train command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("train")]
      [Alias("raidTrain")]
      [Summary("Creates a new raid train coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidTrain([Summary("Tier of the raids.")] string tier,
                                  [Summary("Time the train will start.")] string time,
                                  [Summary("Where the train will start.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         Raid raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User)
            {
               AllBosses = allBosses
            };

            int selectType = allBosses[calcTier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, selectType, raid.BossPage, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(potentials.Count)).ToArray()
               .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "train", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle muletrain command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("muletrain")]
      [Alias("raidMuleTrain")]
      [Summary("Creates a new raid train coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidMuleTrain([Summary("Tier of the raids.")] string tier,
                                      [Summary("Time the train will start.")] string time,
                                      [Summary("Where the train will start.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         RaidMule raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User)
            {
               AllBosses = allBosses
            };

            int selectType = allBosses[calcTier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, selectType, raid.BossPage, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(potentials.Count)).ToArray()
               .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis.Concat(trainEmojis).ToArray());
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis.Concat(trainEmojis).ToArray());
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "muleTrain", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle raidt command.
      /// </summary>
      /// <param name="boss">Boss role of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("raidt")]
      [Summary("Creates a new raid coordination message using a boss role.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n" +
         "Requires a channel registered for raid notifications.")]
      [RegisterChannel('R')]
      public async Task RaidT([Summary("Boss role of the raid.")] IRole boss,
                              [Summary("Time the raid will start.")] string time,
                              [Summary("Where the raid will be.")][Remainder] string location)
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

         Raid raid;
         string fileName;
         if (calcTier == 0)
         {
            raid = new Raid(calcTier, time, location, bossName)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            bossName = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new Raid(calcTier, time, location, bossName)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "raidt", $"No raid bosses found for role {boss.Name}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle mulet command.
      /// </summary>
      /// <param name="boss">Boss role of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("mulet")]
      [Alias("raidmulet")]
      [Summary("Creates a new remote raid coordination message using a boss role.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n" +
         "Requires a channel registered for raid notifications.")]
      [RegisterChannel('R')]
      public async Task RaidMuleT([Summary("Boss role of the raid.")] IRole boss,
                                  [Summary("Time the raid will start.")] string time,
                                  [Summary("Where the raid will be.")][Remainder] string location)
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

         RaidMule raid;
         string fileName;
         if (calcTier == 0)
         {
            raid = new RaidMule(calcTier, time, location, bossName)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            bossName = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidMule(calcTier, time, location, bossName)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "mulet", $"No raid bosses found for role {boss.Name}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle traint command.
      /// </summary>
      /// <param name="boss">Boss role of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("traint")]
      [Alias("raidTraint")]
      [Summary("Creates a new raid train coordination message using a boss role.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n" +
         "Requires a channel registered for raid notifications.")]
      [RegisterChannel('R')]
      public async Task RaidTrainT([Summary("Boss role of the raid.")] IRole boss,
                                   [Summary("Time the raid will start.")] string time,
                                   [Summary("Where the raid will be.")][Remainder] string location)
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

         Raid raid;
         string fileName;
         if (calcTier == 0)
         {
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User, bossName)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
         }
         else if (Global.USE_EMPTY_RAID)
         {
            bossName = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User, bossName)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "traint", $"No raid bosses found for role {boss.Name}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle muletraint command.
      /// </summary>
      /// <param name="boss">Boss role of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("muletrain")]
      [Alias("raidMuleTrain")]
      [Summary("Creates a new raid train coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n" +
         "Requires a channel registered for raid notifications.")]
      [RegisterChannel('R')]
      public async Task RaidMuleTrainT([Summary("Boss role of the raid.")] IRole boss,
                                  [Summary("Time the raid will start.")] string time,
                                  [Summary("Where the raid will be.")][Remainder] string location)
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

         RaidMule raid;
         string fileName;
         if (calcTier == 0)
         {
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User, bossName)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis.Concat(trainEmojis).ToArray());
         }
         else if (Global.USE_EMPTY_RAID)
         {
            bossName = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User, bossName)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis.Concat(trainEmojis).ToArray());
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "muleTraint", $"No raid bosses found for role {bossName}");
         }
         RemoveOldRaids();
      }


      /// <summary>
      /// Handle guide command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <returns>Completed Task.</returns>
      [Command("guide")]
      [Alias("raidguide")]
      [Summary("Gets raid information for a raid boss.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task Guide([Summary("Tier of the raid boss.")] string tier)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            int selectType = allBosses[calcTier].Count > Global.SELECTION_EMOJIS.Length ? (int)SELECTION_TYPES.PAGE : (int)SELECTION_TYPES.STANDARD;

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, selectType, 0, fileName));
            guideMessages.Add(selectMsg.Id, new RaidGuideSelect(calcTier, potentials));
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(new List<IEmote>(Global.SELECTION_EMOJIS.Take(potentials.Count)).ToArray()
               .Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]).Prepend(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]).ToArray());
         }
         else if (potentials.Count == 1)
         {
            Pokemon pkmn = Connections.Instance().GetPokemon(potentials.First());
            Connections.Instance().GetRaidBoss(ref pkmn);

            fileName = Connections.GetPokemonPicture(pkmn.Name);
            Connections.CopyFile(fileName);
            await Context.Channel.SendFileAsync(fileName, embed: BuildRaidGuideEmbed(pkmn, fileName));
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "guide", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle boss command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("boss")]
      [Alias("bosses", "bosslist", "raidboss", "raidbosses", "raidbosslist")]
      [Summary("Get the current list of raid bosses.")]
      [RegisterChannel('I')]
      public async Task Boss()
      {
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);
         embed.WithTitle("Current Raid Bosses:");
         if (allBosses.ContainsKey(Global.EX_RAID_TIER))
         {
            embed.AddField($"EX Raids {BuildRaidTitle(Global.EX_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.EX_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.MEGA_RAID_TIER))
         {
            embed.AddField($"Mega Raids {BuildRaidTitle(Global.MEGA_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.MEGA_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.LEGENDARY_RAID_TIER))
         {
            embed.AddField($"Tier 5 Raids {BuildRaidTitle(Global.LEGENDARY_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.LEGENDARY_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.PREMIUM_RAID_TIER))
         {
            embed.AddField($"Tier 4 Raids {BuildRaidTitle(Global.PREMIUM_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.PREMIUM_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.RARE_RAID_TIER))
         {
            embed.AddField($"Tier 3 Raids {BuildRaidTitle(Global.RARE_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.RARE_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.UNCOMMON_RAID_TIER))
         {
            embed.AddField($"Tier 2 Raids {BuildRaidTitle(Global.UNCOMMON_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.UNCOMMON_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.COMMON_RAID_TIER))
         {
            embed.AddField($"Tier 1 Raids {BuildRaidTitle(Global.COMMON_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.COMMON_RAID_TIER]), true);
         }

         await Context.Channel.SendMessageAsync(embed: embed.Build());
      }

      /// <summary>
      /// Handle difficulty command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("difficulty")]
      [Alias("raiddifficulty", "bossdifficulty", "raidbossdifficulty")]
      [Summary("Get the raid difficulty definitions.")]
      [RegisterChannel('I')]
      public async Task Difficulty()
      {
         Dictionary<string, string> table = Connections.GetRaidDifficultyTable();

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);
         embed.WithTitle("Raid Boss Difficulty Scale:");

         for (int i = 0; i < table.Count - 1; i++)
         {
            KeyValuePair<string, string> difficulty = table.ElementAt(i);
            embed.AddField(difficulty.Key, difficulty.Value);
         }

         embed.WithFooter(table.ElementAt(table.Count - 1).Value);

         await Context.Channel.SendMessageAsync(embed: embed.Build());
      }
   }
}