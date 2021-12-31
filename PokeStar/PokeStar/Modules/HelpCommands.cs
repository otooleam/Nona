using System;
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
   /// <summary>
   /// Handles help commands.
   /// </summary>
   public class HelpCommands : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Saved help messages.
      /// </summary>
      private static readonly Dictionary<ulong, HelpMessage> helpMessages = new Dictionary<ulong, HelpMessage>();

      /// <summary>
      /// Max commands dispayed per page.
      /// </summary>
      private const int MAX_COMMANDS = 10;

      /// <summary>
      /// Emotes for a help message.
      /// </summary>
      private static readonly Emoji[] HELP_EMOJIS = {
         new Emoji("⬅️"),
         new Emoji("➡️")
      };

#if COMPONENTS
      /// <summary>
      /// Components for a help message.
      /// </summary>
      private static readonly string[] HELP_COMPONENTS = {
         "help_back",
         "help_forward",
      };
#endif

      /// <summary>
      /// Index of emotes on a help message.
      /// </summary>
      private enum HELP_EMOJI_INDEX
      {
         BACK_ARROW,
         FORWARD_ARROR,
      }

      /// <summary>
      /// Handle help command.
      /// </summary>
      /// <param name="command">(Optional) Get help with this command.</param>
      /// <returns>Completed Task.</returns>
      [Command("help")]
      [Summary("Displays info about commands." +
               "Leave blank to get a list of all commands.")]
      public async Task Help([Summary("(Optional) Get help with this command.")] string command = null)
      {

         SocketGuildUser user = Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id);
         bool isAdmin = (user.Roles.Where(role => role.Permissions.Administrator).ToList().Count != 0 || Context.Guild.OwnerId == user.Id);
         bool isNona = Context.Guild.Name.Equals(Global.HOME_SERVER, StringComparison.OrdinalIgnoreCase);
         string prefix = Connections.Instance().GetPrefix(Context.Guild.Id);

         if (command == null)
         {
            List<CommandInfo> validCommands = Global.COMMAND_INFO.Where(cmdInfo => CheckShowCommand(cmdInfo.Name, isAdmin, isNona)).ToList();
#if COMPONENTS
#if DROP_DOWNS
            List<CommandInfo> commands = validCommands.Take(MAX_COMMANDS).ToList();
            IUserMessage msg = await ReplyAsync(embed: BuildGeneralHelpEmbed(commands, prefix, 1),
                                                components: Global.BuildSelectionMenuWithButtons(GetCommandNames(commands).ToArray(), 
                                                                                                 Global.DEFAULT_MENU_PLACEHOLDER,
                                                                                                 HELP_EMOJIS, HELP_COMPONENTS));
#else
            IUserMessage msg = await ReplyAsync(embed: BuildGeneralHelpEmbed(validCommands.Take(MAX_COMMANDS).ToList(), prefix, 1), 
                                                components: Global.BuildButtons(HELP_EMOJIS, HELP_COMPONENTS));
#endif
#else
            IUserMessage msg = await ReplyAsync(embed: BuildGeneralHelpEmbed(validCommands.Take(MAX_COMMANDS).ToList(), prefix, 1));
#endif
            helpMessages.Add(msg.Id, new HelpMessage(validCommands));
#if !COMPONENTS
            msg.AddReactionsAsync(HELP_EMOJIS);
#endif
         }
         else if (Global.COMMAND_INFO.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase) || x.Aliases.Contains(command)) is CommandInfo cmdInfo
            && CheckShowCommand(cmdInfo.Name, isAdmin, isNona))
         {
            await ReplyAsync(embed: BuildCommandHelpEmbed(cmdInfo, prefix));
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "help", $"Command \'{command}\' does not exist. Run the '.help' command to get a list of valid commands.");
         }
      }

      /// <summary>
      /// Checks if the command should be shown to the user.
      /// Uses the following check:
      /// Z = (NOT(A) AND NOT(B) AND NOT(C)) OR (B AND D) OR (C AND D AND E)
      /// 
      /// Input:
      /// A: Is hidden command
      /// B: Is admin command
      /// C: Is nona admin command
      /// D: User is an admin on the server
      /// E: Server is the home server
      /// </summary>
      /// <param name="command">Command to check.</param>
      /// <param name="isAdmin">If the user has the admin permission.</param>
      /// <param name="isNona">If ther server is the home server.</param>
      /// <returns>True if the command should be shown, otherwise false.</returns>
      public static bool CheckShowCommand(string command, bool isAdmin, bool isNona)
      {
         return (!Global.HIDDEN_COMMANDS.Contains(command) && !Global.ADMIN_COMMANDS.Contains(command) && !Global.NONA_ADMIN_COMMANDS.Contains(command))
                  || (isAdmin && Global.ADMIN_COMMANDS.Contains(command)) || (isAdmin && isNona && Global.NONA_ADMIN_COMMANDS.Contains(command));
      }

      /// <summary>
      /// Checks if a message is a help message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a help message, otherwise false.</returns>
      public static bool IsHelpMessage(ulong id)
      {
         return helpMessages.ContainsKey(id);
      }

#if COMPONENTS
#if DROP_DOWNS
      /// <summary>
      /// Handles a selection made on a help message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that the selection was made on.</param>
      /// <param name="guildId">Id of the guild that the message was sent in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task HelpMessageMenuHandle(IMessage message, SocketMessageComponent component, ulong guildId)
      {
         HelpMessage helpMessage = helpMessages[message.Id];
         int option = Global.GetOptionIndex(component.Data.Values.ElementAt(0));
         string prefix = Connections.Instance().GetPrefix(guildId);
         CommandInfo cmdInfo = helpMessage.Commands.ElementAt((helpMessage.Page * MAX_COMMANDS) + option);
         
         await component.Channel.SendMessageAsync(embed: BuildCommandHelpEmbed(cmdInfo, prefix));
         await message.DeleteAsync();
      }
#endif
      /// <summary>
      /// Handles a button press on a help message.
      /// </summary>
      /// <param name="message">Message that the component is on.</param>
      /// <param name="component">Component that was pressed.</param>
      /// <param name="guildId">Id of the guild that the message was sent in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task HelpMessageButtonHandle(IMessage message, SocketMessageComponent component, ulong guildId)
      {
         HelpMessage helpMessage = helpMessages[message.Id];
         int offset = helpMessage.Page;
         string prefix = Connections.Instance().GetPrefix(guildId);

         if (component.Data.CustomId.Equals(HELP_COMPONENTS[(int)HELP_EMOJI_INDEX.BACK_ARROW]) && offset > 0)
         {
            offset--;
         }
         else if (component.Data.CustomId.Equals(HELP_COMPONENTS[(int)HELP_EMOJI_INDEX.FORWARD_ARROR]) && helpMessage.Commands.Count > (offset + 1) * MAX_COMMANDS)
         {
            offset++;
         }

         if (helpMessage.Page != offset)
         {
            await ((SocketUserMessage)message).ModifyAsync(x =>
            {
               x.Embed = BuildGeneralHelpEmbed(helpMessage.Commands.Skip(offset * MAX_COMMANDS).Take(MAX_COMMANDS).ToList(), prefix, offset + 1);
#if DROP_DOWNS
               x.Components = Global.BuildSelectionMenuWithButtons(GetCommandNames(helpMessage.Commands.Skip(offset * MAX_COMMANDS).Take(MAX_COMMANDS).ToList()).ToArray(),
                                                                   Global.DEFAULT_MENU_PLACEHOLDER, HELP_EMOJIS, HELP_COMPONENTS);
#endif
            });
         }

         helpMessages[message.Id] = new HelpMessage(helpMessage.Commands, offset);
      }
#else
      /// <summary>
      /// Handles a reaction on a help message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="guildId">Id of the guild that the message was sent in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task HelpMessageReactionHandle(IMessage message, SocketReaction reaction, ulong guildId)
      {
         HelpMessage helpMessage = helpMessages[message.Id];
         int offset = helpMessage.Page;
         string prefix = Connections.Instance().GetPrefix(guildId);

         if (reaction.Emote.Equals(HELP_EMOJIS[(int)HELP_EMOJI_INDEX.BACK_ARROW]) && offset > 0)
         {
            offset--;
         }
         else if (reaction.Emote.Equals(HELP_EMOJIS[(int)HELP_EMOJI_INDEX.FORWARD_ARROR]) && helpMessage.Commands.Count > (offset + 1) * MAX_COMMANDS)
         {
            offset++;
         }

         if (helpMessage.Page != offset)
         {
            await ((SocketUserMessage)message).ModifyAsync(x =>
            {
               x.Embed = BuildGeneralHelpEmbed(helpMessage.Commands.Skip(offset * MAX_COMMANDS).Take(MAX_COMMANDS).ToList(), prefix, offset + 1);
            });
         }

         helpMessages[message.Id] = new HelpMessage(helpMessage.Commands, offset);
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }
#endif
      /// <summary>
      /// Builds a general help embed.
      /// </summary>
      /// <param name="commands">List of commands to display.</param>
      /// <param name="prefix">Prefix used for the server.</param>
      /// <param name="page">Current page number.</param>
      /// <returns>Embed for viewing a general list of commands.</returns>
      private static Embed BuildGeneralHelpEmbed(List<CommandInfo> commands, string prefix, int page)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle("**Command List**");
         embed.WithDescription($"List of commands supported by Nona.\n**Current Page:** {page}");
         foreach (CommandInfo cmdInfo in commands)
         {
            embed.AddField($"**{prefix}{cmdInfo.Name}**", cmdInfo.Summary ?? "No description available");
         }
         embed.WithColor(Global.EMBED_COLOR_HELP_RESPONSE);
         embed.WithFooter($"Run \"{prefix}help <command name>\" to get help for a specific command.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a command help embed.
      /// </summary>
      /// <param name="cmdInfo">Command info to display.</param>
      /// <param name="prefix">Prefix used for the server.</param>
      /// <returns>Emebed for viewing a single command. </returns>
      private static Embed BuildCommandHelpEmbed(CommandInfo cmdInfo, string prefix)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_HELP_RESPONSE);
         embed.WithTitle($"**{prefix}{cmdInfo.Name} command help**");
         embed.WithDescription(cmdInfo.Summary ?? "No description available");
         if (cmdInfo.Aliases.Count > 1)
         {
            StringBuilder sb = new StringBuilder();
            foreach (string alias in cmdInfo.Aliases)
            {
               if (!alias.Equals(cmdInfo.Name, StringComparison.OrdinalIgnoreCase))
               {
                  sb.Append($"{alias}, ");
               }
            }
            embed.AddField("Alternate Command:", sb.ToString().TrimEnd().TrimEnd(','));
         }
         if (cmdInfo.Remarks != null)
         {
            embed.AddField("**Additional Information:**", cmdInfo.Remarks);
         }

         if (cmdInfo.Parameters.Count == 0)
         {
            embed.WithFooter("*This command does not take any parameters.");
         }
         else
         {
            StringBuilder sb = new StringBuilder();
            foreach (ParameterInfo param in cmdInfo.Parameters)
            {
               embed.AddField($"**<{param.Name}>**", param.Summary ?? "No description available");
               sb.Append($" {param.Name}");
            }
            embed.AddField($"**Example:**", $"{prefix}{cmdInfo.Name}{sb}");
         }

         return embed.Build();
      }

#if COMPONENTS && DROP_DOWNS
      /// <summary>
      /// Gets all command names in a given list.
      /// </summary>
      /// <param name="commands">List of command info.</param>
      /// <returns>List of command names.</returns>
      private static List<string> GetCommandNames(List<CommandInfo> commands)
      {
         List<string> commandNames = new List<string>();
         foreach (CommandInfo cmd in commands)
         {
            commandNames.Add(cmd.Name);
         }
         return commandNames;
      }
#endif
   }
}