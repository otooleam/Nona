using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using PokeStar.Modules;
using PokeStar.ModuleParents;
using PokeStar.ImageProcessors;
using PokeStar.ConnectionInterface;

namespace PokeStar
{
   /// <summary>
   /// Runs the main thread of the system.
   /// </summary>
   public class Program
   {
      private static DiscordSocketClient client;
      private static CommandService commands;
      private static IServiceProvider services;

      private readonly int SizeMessageCashe = 100;
      private readonly LogSeverity DefaultLogLevel = LogSeverity.Info;

      private bool loggingInProgress;

      private Timer SilphUpdate;

      private static bool emoteSet = false;

      /// <summary>
      /// Main function for the system.
      /// Allows the system to run asyncronously.
      /// </summary>
      public static void Main()
         => new Program().MainAsync().GetAwaiter().GetResult();

      /// <summary>
      /// Runs main thread for the function.
      /// Task is blocked until the program is closed.
      /// </summary>
      /// <returns>No task is returned as function ends on system termination.</returns>
      public async Task MainAsync()
      {
         Global.PROGRAM_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         Global.ENV_FILE = JObject.Parse(File.ReadAllText($"{Global.PROGRAM_PATH}\\env.json"));

         string token = Global.ENV_FILE.GetValue("token").ToString();
         Global.VERSION = Global.ENV_FILE.GetValue("version").ToString();
         Global.HOME_SERVER = Global.ENV_FILE.GetValue("home_server").ToString();
         Global.EMOTE_SERVER = Global.ENV_FILE.GetValue("emote_server").ToString();
         Global.POGO_DB_CONNECTION_STRING = Global.ENV_FILE.GetValue("pogo_db_sql").ToString();
         Global.NONA_DB_CONNECTION_STRING = Global.ENV_FILE.GetValue("nona_db_sql").ToString();
         Global.DEFAULT_PREFIX = Global.ENV_FILE.GetValue("default_prefix").ToString();

         Global.USE_NONA_TEST = Global.ENV_FILE.GetValue("accept_nona_test").ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase);
         Global.USE_EMPTY_RAID = Global.ENV_FILE.GetValue("use_empty_raid").ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase);

         int logLevel = Convert.ToInt32(Global.ENV_FILE.GetValue("log_level").ToString());
         Global.LOG_LEVEL = !Enum.IsDefined(typeof(LogSeverity), logLevel) ? DefaultLogLevel : (LogSeverity)logLevel;

         DiscordSocketConfig clientConfig = new DiscordSocketConfig
         {
            MessageCacheSize = SizeMessageCashe,
            LogLevel = Global.LOG_LEVEL
         };
         client = new DiscordSocketClient(clientConfig);
         CommandServiceConfig commandConfig = new CommandServiceConfig
         {
            DefaultRunMode = RunMode.Async,
            LogLevel = Global.LOG_LEVEL
         };
         commands = new CommandService(commandConfig);

         services = new ServiceCollection()
             .AddSingleton(client)
             .AddSingleton(commands)
             .BuildServiceProvider();

         await HookEvents();
         await client.LoginAsync(TokenType.Bot, token);
         await client.StartAsync();
         await client.SetGameAsync($".help | v{Global.VERSION}");

         Global.COMMAND_INFO = commands.Commands.ToList().OrderBy(c => c.Name).ToList();

         // Block this task until the program is closed.
         await Task.Delay(-1);
      }

      /// <summary>
      /// Hooks events to the Client and Command services.
      /// Runs asyncronously.
      /// </summary>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HookEvents()
      {
         client.Log += Log;
         commands.Log += Log;
         client.MessageReceived += HandleCommandAsync;
         await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
         client.ReactionAdded += HandleReactionAdded;
         client.ReactionRemoved += HandleReactionRemoved;
#if COMPONENTS
#if DROP_DOWNS
         client.SelectMenuExecuted += HandleSelectionMade;
#endif
         client.ButtonExecuted += HandleButtonPress;
#endif
         client.Ready += HandleReady;
         client.JoinedGuild += HandleJoinGuild;
         client.LeftGuild += HandleLeftGuild;
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Log event.
      /// </summary>
      /// <param name="msg">Message to log.</param>
      /// <returns>Task Complete.</returns>
      private Task Log(LogMessage msg)
      {
         string fileName = DateTime.Now.ToString("MM-dd-yyyy");
         string logFile = $"{Global.PROGRAM_PATH}\\Logs\\{fileName}.txt";

         string logText = $"{DateTime.Now:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";

         while (loggingInProgress) { }

         loggingInProgress = true;
         File.AppendAllText(logFile, logText + "\n");
         loggingInProgress = false;

         Console.WriteLine(msg.ToString());

         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Command event.
      /// Runs asyncronously.
      /// </summary>
      /// <param name="cmdMessage">Command message that was sent.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleCommandAsync(SocketMessage cmdMessage)
      {
         if (!Global.INIT_COMPLETE ||
            !(cmdMessage is SocketUserMessage message) ||
             (message.Author.IsBot &&
             (!Global.USE_NONA_TEST || !message.Author.Username.Equals("NonaTest", StringComparison.OrdinalIgnoreCase)))
             || cmdMessage.Channel is IPrivateChannel)
         {
            return Task.CompletedTask;
         }

         SocketCommandContext context = new SocketCommandContext(client, message);

         int argPos = 0;
         string prefix = Connections.Instance().GetPrefix(context.Guild.Id);

         if (message.Attachments.Count != 0)
         {
            if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, Global.REGISTER_STRING_ROLE))
            {
               RollImageProcess.RoleImageProcess(context);
            }
            else if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, Global.REGISTER_STRING_RAID))
            {
               //TODO: Add call for raid image processing
            }
            else if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, Global.REGISTER_STRING_EX))
            {
               //TODO: Add call for ex raid image processing
            }
         }
         else if (message.HasStringPrefix(prefix, ref argPos))
         {
            IResult result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
         }
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Reaction Added event.
      /// </summary>
      /// <param name="cachedMessage">Message that was reaction is on.</param>
      /// <param name="originChannel">Channel where the message is located.</param>
      /// <param name="reaction">Reaction made on the message.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage,
          Cacheable<IMessageChannel, ulong> originChannel, SocketReaction reaction)
      {
         ISocketMessageChannel channel = reaction.Channel ?? (ISocketMessageChannel)FindChannel(originChannel.Id);

         if (channel == null)
         {
            return Task.CompletedTask;
         }

         IMessage message = await channel.GetMessageAsync(cachedMessage.Id);

         SocketGuildChannel chnl = channel as SocketGuildChannel;
         ulong guild = chnl.Guild.Id;

         if (message != null && reaction.User.IsSpecified && !reaction.User.Value.IsBot)
         {
            if (Connections.IsNotifyMessage(message.Id))
            {
               await Connections.NotifyMessageReactionAddedHandle(reaction, chnl.Guild);
            }
#if !COMPONENTS
            else if (RaidCommandParent.IsRaidMessage(message.Id))
            {
               await RaidCommandParent.RaidMessageReactionHandle(message, reaction);
            }
            else if (RaidCommandParent.IsRaidSubMessage(message.Id))
            {
               await RaidCommandParent.RaidSubMessageReactionHandle(message, reaction);
            }
            else if (RaidCommandParent.IsRaidGuideMessage(message.Id))
            {
               await RaidCommandParent.RaidGuideMessageReactionHandle(message, reaction);
            }
            else if (DexCommandParent.IsDexSelectMessage(message.Id))
            {
               await DexCommandParent.DexSelectMessageReactionHandle(message, reaction, guild);
            }
            else if (DexCommandParent.IsDexMessage(message.Id))
            {
               await DexCommandParent.DexMessageReactionHandle(message, reaction, guild);
            }
            else if (DexCommandParent.IsCatchMessage(message.Id))
            {
               await DexCommandParent.CatchMessageReactionHandle(message, reaction);
            }
            else if (POICommands.IsPOISubMessage(message.Id))
            {
               await POICommands.POIMessageReactionHandle(message, reaction, guild);
            }
            else if (HelpCommands.IsHelpMessage(message.Id))
            {
               await HelpCommands.HelpMessageReactionHandle(message, reaction, guild);
            }
#endif
         }
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Reaction Removed event.
      /// </summary>
      /// <param name="cachedMessage">Message that was reaction is on.</param>
      /// <param name="originChannel">Channel where the message is located.</param>
      /// <param name="reaction">Reaction made on the message.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage,
          Cacheable<IMessageChannel, ulong> originChannel, SocketReaction reaction)
      {
         IMessage message = await reaction.Channel.GetMessageAsync(cachedMessage.Id);

         SocketGuildChannel chnl = message.Channel as SocketGuildChannel;

         if (message != null && reaction.User.IsSpecified && !reaction.User.Value.IsBot)
         {
            if (Connections.IsNotifyMessage(message.Id))
            {
               await Connections.NotifyMessageReactionRemovedHandle(reaction, chnl.Guild);
            }
         }
         return Task.CompletedTask;
      }

#if COMPONENTS
#if DROP_DOWNS
      public async Task<Task> HandleSelectionMade(SocketMessageComponent component)
      {
         if (component.Channel == null || component.Message == null)
         {
            return Task.CompletedTask;
         }

         SocketGuildChannel chnl = component.Channel as SocketGuildChannel;
         ulong guild = chnl.Guild.Id;

         if (component.Message != null && component.User != null && !component.User.IsBot)
         {
            SocketUserMessage message = component.Message;

            if (DexCommandParent.IsDexSelectMessage(message.Id))
            {
               await DexCommandParent.DexSelectMessageMenuHandle(message, component, guild);
            }
            else if (DexCommandParent.IsDexMessage(message.Id))
            {
               await DexCommandParent.DexMessageMenuHandle(message, component, guild);
            }
            else if (HelpCommands.IsHelpMessage(message.Id))
            {
               await HelpCommands.HelpMessageMenuHandle(message, component, guild);
            }
            await component.DeferAsync();
         }
         return Task.CompletedTask;
      }
#endif
      /// <summary>
      /// Handles the Button Pressed event.
      /// </summary>
      /// <param name="component">Component pressed on the message.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleButtonPress(SocketMessageComponent component)
      {
         if (component.Channel == null || component.Message == null)
         {
            return Task.CompletedTask;
         }

         SocketGuildChannel chnl = component.Channel as SocketGuildChannel;
         ulong guild = chnl.Guild.Id;

         if (component.Message != null && component.User != null && !component.User.IsBot)
         {
            SocketUserMessage message = component.Message;

            if (RaidCommandParent.IsRaidMessage(message.Id))
            {
               await RaidCommandParent.RaidMessageButtonHandle(message, component);
            }
            else if (RaidCommandParent.IsRaidSubMessage(message.Id))
            {
               await RaidCommandParent.RaidSubMessageButtonHandle(message, component);
            }
            else if (RaidCommandParent.IsRaidGuideMessage(message.Id))
            {
               await RaidCommandParent.RaidGuideMessageButtonHandle(message, component);
            }
#if !DROP_DOWNS
            else if (DexCommandParent.IsDexSelectMessage(message.Id))
            {
               await DexCommandParent.DexSelectMessageButtonHandle(message, component, guild);
            }
            else if (DexCommandParent.IsDexMessage(message.Id))
            {
               await DexCommandParent.DexMessageButtonHandle(message, component, guild);
            }
#endif
            else if (DexCommandParent.IsCatchMessage(message.Id))
            {
               await DexCommandParent.CatchMessageButtonHandle(message, component);
            }
            else if (POICommands.IsPOISubMessage(message.Id))
            {
               await POICommands.POIMessageButtonHandle(message, component, guild);
            }
            else if (HelpCommands.IsHelpMessage(message.Id))
            {
               await HelpCommands.HelpMessageButtonHandle(message, component, guild);
            }
            await component.DeferAsync();
         }
         return Task.CompletedTask;
      }
#endif

      /// <summary>
      /// Handles the Ready event.
      /// </summary>
      /// <returns>Task Complete.</returns>
      private Task HandleReady()
      {
         SocketGuild server = client.Guilds.FirstOrDefault(x => x.Name.ToString().Equals(Global.HOME_SERVER, StringComparison.OrdinalIgnoreCase));
         SetEmotes(server);
         RaidCommandParent.SetInitialEmotes();
         DexCommandParent.SetInitialEmotes();

         foreach (SocketGuild guild in client.Guilds)
         {
            if (Connections.Instance().GetPrefix(guild.Id) == null)
            {
               Connections.Instance().InitSettings(guild.Id);
            }
         }

         SilphUpdate = new Timer(async _ => await Connections.Instance().RunSilphUpdate(client.Guilds.ToList()), new AutoResetEvent(false), 0, 300000);

         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Join Guild event.
      /// </summary>
      /// <param name="guild">Guild that the bot joined.</param>
      /// <returns>Task Complete.</returns>
      private Task HandleJoinGuild(SocketGuild guild)
      {
         Connections.Instance().InitSettings(guild.Id);
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Left Guild event.
      /// </summary>
      /// <param name="guild">Guild that the bot left.</param>
      /// <returns>Task Complete.</returns>
      private Task HandleLeftGuild(SocketGuild guild)
      {
         Connections.Instance().DeleteRegistration(guild.Id);
         Connections.Instance().DeleteSettings(guild.Id);
         return Task.CompletedTask;
      }

      /// <summary>
      /// Sets the emotes from a JSON file
      /// </summary>
      /// <param name="server">Server that the emotes are on.</param>
      private static void SetEmotes(SocketGuild server)
      {
         if (!emoteSet)
         {
            List<string> nonaEmojiKeys = Global.NONA_EMOJIS.Keys.ToList();
            foreach (string emote in nonaEmojiKeys)
            {
               Global.NONA_EMOJIS[emote] = Emote.Parse(
                  server.Emotes.FirstOrDefault(
                     x => x.Name.Equals(
                        Global.NONA_EMOJIS[emote],
                        StringComparison.OrdinalIgnoreCase)
                     ).ToString()).ToString();
            }

            List<string> noumEmojiKeys = Global.NUM_EMOJI_NAMES.Keys.ToList();
            foreach (string emote in noumEmojiKeys)
            {
               Global.NUM_EMOJIS.Add(Emote.Parse(
                  server.Emotes.FirstOrDefault(
                     x => x.Name.Equals(
                        Global.NUM_EMOJI_NAMES[emote],
                        StringComparison.OrdinalIgnoreCase)
                     ).ToString()));
            }

            for (int i = 0; i < Global.NUM_SELECTIONS; i++)
            {
               Global.SELECTION_EMOJIS[i] = Global.NUM_EMOJIS[i];
            }
            emoteSet = true;
         }
      }

      /// <summary>
      /// Finds a channel in a guild by id.
      /// </summary>
      /// <param name="id">Id of the channel.</param>
      /// <returns>Channel if it exists, otherwise null.</returns>
      private static SocketGuildChannel FindChannel(ulong id)
      {
         foreach (SocketGuild guild in client.Guilds)
         {
            SocketGuildChannel channel = guild.Channels.FirstOrDefault(x => x.Id == id);
            if (channel != null)
            {
               return channel;
            }
         }
         return null;
      }

      /// <summary>
      /// Get the name of the bot.
      /// </summary>
      /// <returns>Bot name as a string.</returns>
      public static string GetName()
      {
         return client.CurrentUser.Username;
      }

      /// <summary>
      /// Get the status of the bot.
      /// </summary>
      /// <returns>Bot status as a string.</returns>
      public static string GetStatus()
      {
         return client.Status.ToString();
      }

      /// <summary>
      /// Get the connection status of the bot.
      /// </summary>
      /// <returns>Bot connection state as a string.</returns>
      public static string GetConnectionState()
      {
         return client.ConnectionState.ToString();
      }

      /// <summary>
      /// Get the number of guilds the bot is in.
      /// </summary>
      /// <returns>Number of guilds the bot is in.</returns>
      public static int GetGuildCount()
      {
         return client.Guilds.Count;
      }

      /// <summary>
      /// Get the latency of the bot and the server in milliseconds (ms).
      /// </summary>
      /// <returns>Latency of the bot and server.</returns>
      public static int GetLatency()
      {
         return client.Latency;
      }
   }
}