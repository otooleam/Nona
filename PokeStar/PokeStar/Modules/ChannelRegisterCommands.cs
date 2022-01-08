using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles channel registration commands.
   /// </summary>
   public class ChannelRegisterCommands : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Handle register command.
      /// </summary>
      /// <param name="register">(Optional) Register the channel for these commands.</param>
      /// <returns>Completed Task.</returns>
      [Command("register")]
      [Summary("Registers a channel to run a given type of command.")]
      [Remarks("To register a channel\n" +
               "for this.............................use one of these:\n" +
               //"Player Accounts............account / a\n" +
               "PokéDex..........................pokedex / dex / d\n" +
               //"EX Raids..........................ex / e\n" +
               "Information....................info / i\n" +
               "Raid Notifications.........react / notification / n\n" +
               "Player Registration.......role / p\n" +
               "Raids................................raid / r\n" +
               "Point of Interest............poi / pokestop / stop / gym / s\n" +
               "Leave blank to register for all command types." + 
               "Raid notifications must be the only type of registration in the channel and only one channel per server.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Register([Summary("(Optional) Register the channel for these commands.")] string register = null)
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;
         string registration = Connections.Instance().GetRegistration(guild, channel);
         RegisterResult? result = GenerateRegistrationString(register ?? Global.FULL_REGISTER_STRING, registration ?? "");

         if (result.HasValue)
         {
            if (registration != null && result.Value.RegistrationString.Contains(Global.REGISTER_STRING_NOTIFICATION.ToString()) &&
                !result.Value.RegistrationString.Equals(Global.REGISTER_STRING_NOTIFICATION.ToString(), StringComparison.OrdinalIgnoreCase))
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "register", "Raid Notifications must be the only registration in the channel.");
            }
            else if (Connections.Instance().CheckNotificationRegister(guild) &&
                     result.Value.RegistrationString.Contains(Global.REGISTER_STRING_NOTIFICATION.ToString()))
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "register", "Only one channel per server may be registerd for Raid Notifications.");
            }
            else
            {
               Connections.Instance().UpdateRegistration(guild, channel, result.Value.RegistrationString);
               await ResponseMessage.SendInfoMessage(Context.Channel, $"Channel is now registered for the following command types {GenerateSummaryString(result.Value.RegistrationString)}");

               if (result.Value.CheckSetupComplete && !Connections.Instance().GetSetupComplete(guild))
               {
                  await ResponseMessage.SendWarningMessage(Context.Channel, "register", "Please run the .setup command to ensure required roles have been setup.");
               }

               if (result.Value.RegistrationString.Contains(Global.REGISTER_STRING_NOTIFICATION.ToString()) && registration == null)
               {
                  await Connections.SetNotifyMessage(Context.Guild, Context.Channel,
                     Context.Client.Guilds.FirstOrDefault(x => x.Name.Equals(Global.EMOTE_SERVER, StringComparison.OrdinalIgnoreCase)).Emotes.ToArray());
               }
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "register", "Please enter a valid registration value.");
         }
      }

      /// <summary>
      /// Handle unregister command.
      /// </summary>
      /// <param name="unregister">(Optional) Unregister the channel from these commands.</param>
      /// <returns>Completed Task.</returns>
      [Command("unregister")]
      [Summary("Unregisters a channel from a given type of command.")]
      [Remarks("To register a channel\n" +
               "for this.............................use one of these:\n" +
               //"Player Accounts............account / a\n" +
               "PokéDex..........................pokedex / dex / d\n" +
               //"EX Raids..........................ex / e\n" +
               "Information....................info / i\n" +
               "Raid Notifications.........react / notification / n\n" +
               "Player Registration.......role / p\n" +
               "Raids................................raid / r\n" +
               "Point of Interest............poi / pokestop / stop / gym / s\n" +
               "Leave blank to register for all command types.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Unregister([Summary("(Optional) Unregister the channel from these commands.")] string unregister = null)
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;

         string registration = Connections.Instance().GetRegistration(guild, channel);

         if (registration != null)
         {
            bool notify = registration.Contains(Global.REGISTER_STRING_NOTIFICATION.ToString());
            registration = GenerateUnregistrationString(unregister ?? Global.FULL_REGISTER_STRING, registration);
            if (registration == null)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "unregister", "Please enter a valid registration value.");
            }
            else if (string.IsNullOrEmpty(registration))
            {
               List<ulong> messages = Connections.Instance().GetNotificationMessages(Context.Guild.Id, Context.Channel.Id);
               Connections.Instance().DeleteRegistration(guild, channel);
               await ResponseMessage.SendInfoMessage(Context.Channel, $"Removed all registrations from this channel.");
               if (notify && messages != null)
               {
                  await Connections.ClearNotifyMessage(Context.Guild, Context.Channel, messages);
               }
            }
            else
            {
               List<ulong> messages = Connections.Instance().GetNotificationMessages(Context.Guild.Id, Context.Channel.Id);
               Connections.Instance().UpdateRegistration(guild, channel, registration);
               await ResponseMessage.SendInfoMessage(Context.Channel, $"Channel is now registered for the following command types {GenerateSummaryString(registration)}");
               if (notify && messages != null)
               {
                  await Connections.ClearNotifyMessage(Context.Guild, Context.Channel, messages);
               }
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "unregister", "This channel does not have any commands registered to it.");
         }
      }

      /// <summary>
      /// Handle check command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("check")]
      [Summary("Check command types channel is registerd for.")]
      public async Task Check()
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;

         string registration = Connections.Instance().GetRegistration(guild, channel);

         if (registration == null)
         {
            await ResponseMessage.SendInfoMessage(Context.Channel, $"This channel does not have any commands registered to it.");
         }
         else
         {
            await ResponseMessage.SendInfoMessage(Context.Channel, $"Channel is registered for the following command types {GenerateSummaryString(registration)}");
         }

      }

      /// <summary>
      /// Adds a registered command type from a channel register string.
      /// If the channel is already registed for a command type
      /// nothing will be changed.
      /// </summary>
      /// <param name="register">Command type to register for a channel.</param>
      /// <param name="existing">Channel's existing register string.</param>
      /// <returns>Updated register string.</returns>
      private static RegisterResult? GenerateRegistrationString(string register, string existing = "")
      {
         string add;
         bool CheckSetupComplete = false;

         if (Global.REGISTER_VALIE_STRING.ContainsKey(register))
         {
            add = Global.REGISTER_VALIE_STRING[register];
         }
         else
         {
            return null;
         }

         if (add.Equals(Global.FULL_REGISTER_STRING) ||
             add.Equals(Global.REGISTER_STRING_ROLE.ToString()))
         {
            CheckSetupComplete = true;
         }

         if (existing.Length == 0 || add.Equals(Global.FULL_REGISTER_STRING))
         {
            return new RegisterResult(add, CheckSetupComplete);
         }
         else if (existing.Contains(add))
         {
            return new RegisterResult(existing, CheckSetupComplete);
         }

         string s = existing + add;
         char[] a = s.ToCharArray();
         Array.Sort(a);
         return new RegisterResult(new string(a).ToUpper(), CheckSetupComplete);
      }

      /// <summary>
      /// Removes a registered command type from a channel register string.
      /// If the channel is not registed for a command type
      /// nothing will be changed.
      /// </summary>
      /// <param name="unregister">Command type to unregister from a channel.</param>
      /// <param name="existing">Channel's existing register string.</param>
      /// <returns>Updated register string.</returns>
      private static string GenerateUnregistrationString(string unregister, string existing)
      {
         string remove;
         if (Global.REGISTER_VALIE_STRING.ContainsKey(unregister))
         {
            remove = Global.REGISTER_VALIE_STRING[unregister];
         }
         else
         {
            return null;
         }

         if (remove.Equals(Global.FULL_REGISTER_STRING))
         {
            return string.Empty;
         }

         int index = existing.IndexOf(remove);
         return (index < 0) ? null : existing.Remove(index, remove.Length);
      }

      /// <summary>
      /// Generates a list of command types registered as a string.
      /// </summary>
      /// <param name="reg">Channel register string.</param>
      /// <returns>Channel register summary string.</returns>
      private static string GenerateSummaryString(string reg)
      {
         StringBuilder sb = new StringBuilder();

         foreach (char ch in reg.ToCharArray())
         {
            sb.Append($"{Global.REGISTER_STRING_TYPE[ch.ToString()]}, ");
         }
         return sb.ToString().TrimEnd().TrimEnd(',');
      }

      /// <summary>
      /// Checks if the channel is registered for a type of command.
      /// </summary>
      /// <param name="guild">Guild that has the channel.</param>
      /// <param name="channel">Channel to check for registered command type.</param>
      /// <param name="type">Type of command to check if the channel is registered for.</param>
      /// <returns>True if the channel is registed for the command type, else false.</returns>
      public static bool IsRegisteredChannel(ulong guild, ulong channel, char type)
      {
         string registration = Connections.Instance().GetRegistration(guild, channel);
         if (registration == null)
         {
            return false;
         }
         return registration.Contains(type.ToString().ToUpper());
      }
   }
}