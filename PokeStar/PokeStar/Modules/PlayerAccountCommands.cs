using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class PlayerAccountCommands : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Handle profile command.
      /// </summary>
      /// <param name="user">(Optional) See profile for this user Must be mentioned using @.</param>
      /// <returns>Completed Task.</returns>
      [Command("profile")]
      [Alias("account")]
      [Summary("View current profile of a user.")]
      [Remarks("Leave blank to view your own profile.\n" +
               "Viewing other user's profiles will show \n" +
               "only friend code, referal code, and location.")]
      [RegisterChannel('A')]
      public async Task Profile([Summary("(Optional) See profile for this user Must be mentioned using @.")] string user = null)
      {
         ulong accountId = user == null ? Context.User.Id : Context.Message.MentionedUsers.Count == 0 ? 0 : Context.Message.MentionedUsers.First().Id;

         if (accountId != 0 && Context.Message.MentionedUsers.Count != 0 && Context.Message.MentionedUsers.First().IsBot)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "profile", "Bots cannot have profiles.");
         }
         else if (accountId == 0)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "profile", "No users found mentioned in message. Make sure you are using @.");
         }
         else
         {
            EnsureProfileExists(accountId);

            Embed embed;
            if (user == null)
            {
               embed = GenerateProfileEmbed((SocketGuildUser)Context.User);
            }
            else
            {
               SocketUser account = Context.Message.MentionedUsers.First();
               embed = BuildProfileCodeEmbed(Connections.Instance().GetProfile(account.Id), account);
            }

            await ReplyAsync(embed: embed);
         }
      }

      /// <summary>
      /// Handle exp command.
      /// </summary>
      /// <param name="totalExp">Current total experiance points.</param>
      /// <returns>Completed Task.</returns>
      [Command("exp")]
      [Summary("Set your current total in game experiance.")]
      [Remarks("Does not take into account completion of level up quests.")]
      [RegisterChannel('A')]
      public async Task Exp([Summary("Current total experiance points.")] uint totalExp)
      {
         SocketGuildUser user = (SocketGuildUser)Context.User;
         EnsureProfileExists(user.Id);
         Connections.Instance().UpdateProfileExp(user.Id, totalExp);
         await ReplyAsync(embed: GenerateProfileEmbed(user));
      }

      /// <summary>
      /// Handle level command.
      /// </summary>
      /// <param name="level">Current trainer level.</param>
      /// <returns>Completed Task.</returns>
      [Command("level")]
      [Summary("Set your current in game trainer level.")]
      [Remarks("Experiance will be set to minimum needed for level.")]
      [RegisterChannel('A')]
      public async Task Level([Summary("Current trainer level.")] int level)
      {
         if (level < 1 || level > Global.LEVEL_UP_EXP.Length)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "level", $"Your level must be between 1 and {Global.LEVEL_UP_EXP.Length} (including bounds).");
         }
         else
         {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            EnsureProfileExists(user.Id);
            Connections.Instance().UpdateProfileExp(user.Id, Global.LEVEL_UP_EXP[level - 1]);
            await ReplyAsync(embed: GenerateProfileEmbed(user));
         }
      }

      /// <summary>
      /// Handle trainer command
      /// </summary>
      /// <param name="code">Twelve digit trainer code with or without spaces.</param>
      /// <returns>Completed Task.</returns>
      [Command("trainer")]
      [Alias("friend")]
      [Summary("Set your trainer friend code.")]
      [RegisterChannel('A')]
      public async Task Trainer([Summary("Twelve digit trainer code with or without spaces.")][Remainder] string code)
      {
         string trimCode = string.Concat(code.Where(c => !char.IsWhiteSpace(c)));
         if (trimCode.Length != Global.FRIEND_CODE_LENGTH)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "trainer", $"Your code must be {Global.FRIEND_CODE_LENGTH} characters long (not including spaces).");
         }
         else
         {
            bool numCode = long.TryParse(trimCode, out long outCode);
            if (!numCode)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "trainer", $"Trainer friend codes must only contain numbers (0-9).");
            }
            else
            {
               SocketGuildUser user = (SocketGuildUser)Context.User;
               EnsureProfileExists(user.Id);
               Connections.Instance().UpdateProfileCode(user.Id, outCode);
               await ReplyAsync(embed: GenerateProfileEmbed(user));
            }
         }
      }

      /// <summary>
      /// Handle refer command
      /// </summary>
      /// <param name="code">Nine character referal code.</param>
      /// <returns>Completed Task.</returns>
      [Command("refer")]
      [Alias("referal")]
      [Summary("Set your trainer referal code.")]
      [RegisterChannel('A')]
      public async Task Refer([Summary("Nine character referal code.")] string code)
      {
         if (code.Length != Global.REFERAL_CODE_LENGTH)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "refer", $"Your code must be {Global.REFERAL_CODE_LENGTH} characters long.");
         }
         else
         {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            EnsureProfileExists(user.Id);
            Connections.Instance().UpdateProfileCode(user.Id, code);
            await ReplyAsync(embed: GenerateProfileEmbed(user));
         }
      }

      /// <summary>
      /// Handle location command.
      /// </summary>
      /// <param name="location">(Optional) Country typically played in.</param>
      /// <returns>Completed Task.</returns>
      [Command("location")]
      [Alias("country")]
      [Summary("Set the country you typically play in.")]
      [Remarks("Leave blank to clear saved location.\n" +
               "Please note some countries are not supported.")]
      [RegisterChannel('A')]
      public async Task Country([Summary("(Optional) Country typically played in.")][Remainder] string location = null)
      {
         List<RegionInfo> regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(culture => new RegionInfo(culture.LCID)).ToList();
         RegionInfo rInfo = regions.FirstOrDefault(region => region.EnglishName.Equals(location ?? string.Empty, StringComparison.OrdinalIgnoreCase));
         if (rInfo == null && location != null)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "location", $"Could not find country {location}.");
         }
         else
         {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            EnsureProfileExists(user.Id);
            Connections.Instance().UpdateProfileCountry(user.Id, rInfo == null ? location : rInfo.EnglishName);
            await ReplyAsync(embed: GenerateProfileEmbed(user));
         }
      }

      /// <summary>
      /// Generate a profile embed.
      /// </summary>
      /// <param name="account">User of the account.</param>
      /// <returns>Embed for viewing a user's profile.</returns>
      private static Embed GenerateProfileEmbed(SocketGuildUser account)
      {
         string team = Global.EMPTY_FIELD;
         foreach (SocketRole role in account.Roles)
         {
            if (team.Equals(Global.EMPTY_FIELD, StringComparison.OrdinalIgnoreCase))
            {
               if (role.Name.Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase) ||
                   role.Name.Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase) ||
                   role.Name.Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase))
               {
                  team = role.Name;
               }
            }
         }
         return BuildProfileEmbed(Connections.Instance().GetProfile(account.Id), account, team);
      }

      /// <summary>
      /// Build profile embed.
      /// </summary>
      /// <param name="profile">Profile of the account.</param>
      /// <param name="user">User of the account.</param>
      /// <param name="team"></param>
      /// <returns>Embed for viewing a user's profile.</returns>
      private static Embed BuildProfileEmbed(Profile profile, SocketGuildUser user, string team)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($"Profile for {user.Nickname ?? user.Username}");
         embed.AddField("Current Discord Tag", user.Mention, true);
         embed.AddField("In Game Name", $"{user.Nickname ?? Global.EMPTY_FIELD}", true);
         embed.AddField("Team", $"{team}", true);
         embed.AddField("Level", $"{profile.GetLevel()}", true);
         embed.AddField("Trainer Code", profile.TrainerCodeToString(), true);
         embed.AddField("Referal Code", string.IsNullOrEmpty(profile.ReferalCode) ? Global.EMPTY_FIELD : profile.ReferalCode, true);
         embed.AddField("Country", profile.LocationToString(), true);
         embed.AddField("Silph Card", user.Nickname != null ? $"https://sil.ph/{user.Nickname}" : Global.EMPTY_FIELD, true);

         if (team.Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase))
         {
            embed.WithColor(Global.ROLE_COLOR_VALOR);
         }
         else if (team.Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase))
         {
            embed.WithColor(Global.ROLE_COLOR_MYSTIC);
         }
         else if (team.Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase))
         {
            embed.WithColor(Global.ROLE_COLOR_INSTINCT);
         }
         else
         {
            embed.WithColor(Global.ROLE_COLOR_TRAINER);
         }

         return embed.Build();
      }

      /// <summary>
      /// Build profile code embed.
      /// </summary>
      /// <param name="profile">Profile of the account.</param>
      /// <param name="user">User of the account.</param>
      /// <returns>Embed for viewing a user's partial profile.</returns>
      private static Embed BuildProfileCodeEmbed(Profile profile, SocketUser user)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithDescription($"**Profile for {user.Mention}**");
         embed.AddField("Trainer Code", profile.TrainerCodeToString(), true);
         embed.AddField("Referal Code", string.IsNullOrEmpty(profile.ReferalCode) ? Global.EMPTY_FIELD : profile.ReferalCode, true);
         embed.AddField("Country", profile.LocationToString(), true);
         embed.WithColor(Global.ROLE_COLOR_TRAINER);

         return embed.Build();
      }

      /// <summary>
      /// Checks if a profile exists for a user.
      /// Creates a profile if it does not exist.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      private static void EnsureProfileExists(ulong account)
      {
         Profile profile = Connections.Instance().GetProfile(account);
         if (profile == null)
         {
            Connections.Instance().CreateProfile(account);
         }
      }
   }
}