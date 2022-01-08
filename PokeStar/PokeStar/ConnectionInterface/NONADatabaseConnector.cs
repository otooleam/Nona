using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Connects to the NONA database.
   /// </summary>
   public class NONADatabaseConnector : DatabaseConnector
   {
      /// <summary>
      /// Creates a new NONA database connector.
      /// </summary>
      /// <param name="connectionString">Connections string for the NONA database.</param>
      public NONADatabaseConnector(string connectionString) : base(connectionString) { }

      // Guild Settings *******************************************************

      /// <summary>
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>The guild prefix.</returns>
      public string GetPrefix(ulong guild)
      {
         string prefix = null;
         string queryString = $@"SELECT Prefix 
                                 FROM GuildSettings 
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  prefix = Convert.ToString(reader["Prefix"]);
               }
            }
            conn.Close();
         }
         return prefix;
      }

      /// <summary>
      /// Checks if setup has been completed for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>True if setup has been complete, otherwise false.</returns>
      public bool GetSetupComplete(ulong guild)
      {
         bool setupComplete = false;
         string queryString = $@"SELECT SetupComplete 
                                 FROM GuildSettings 
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  setupComplete = Convert.ToInt32(reader["SetupComplete"]) == TRUE;
               }
            }
            conn.Close();
         }
         return setupComplete;
      }

      /// <summary>
      /// Gets the player limit for raids set for the guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>Limit of players for a raid.</returns>
      public int GetLimitRaidPlayer(ulong guild)
      {
         int player_limit = Global.LIMIT_RAID_PLAYER;
         string queryString = $@"SELECT LimitRaidPlayer 
                                 FROM GuildSettings 
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  player_limit = Convert.ToInt32(reader["LimitRaidPlayer"]);
               }
            }
            conn.Close();
         }
         return player_limit;
      }

      /// <summary>
      /// Gets the invite limit for raids set for the guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>Limit of invites for a raid.</returns>
      public int GetLimitRaidInvite(ulong guild)
      {
         int invite_limit = Global.LIMIT_RAID_INVITE;
         string queryString = $@"SELECT LimitRaidInvite
                                 FROM GuildSettings 
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  invite_limit = Convert.ToInt32(reader["LimitRaidInvite"]);
               }
            }
            conn.Close();
         }
         return invite_limit;
      }

      /// <summary>
      /// Adds the guild with default settings.
      /// Default is default prefix and setup is incomplete.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void AddSettings(ulong guild)
      {
         string queryString = $@"INSERT INTO GuildSettings (GuildID, Prefix, SetupComplete, LimitRaidPlayer, LimitRaidInvite)
                                 VALUES ({guild}, '{Global.DEFAULT_PREFIX}', 0, {Global.LIMIT_RAID_PLAYER}, {Global.LIMIT_RAID_INVITE})";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="prefix">New prefix value.</param>
      public void UpdatePrefix(ulong guild, string prefix)
      {
         string queryString = $@"UPDATE GuildSettings 
                                 SET Prefix='{prefix}'
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Mark setup as completed for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void CompleteSetup(ulong guild)
      {
         string queryString = $@"UPDATE GuildSettings 
                                 SET SetupComplete=1
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Deletes the settings saved for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void DeleteSettings(ulong guild)
      {
         string queryString = $@"DELETE FROM GuildSettings
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      // Channel Registration *************************************************

      /// <summary>
      /// Gets the registration for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <returns>The channel registration string, otherwise null.</returns>
      public string GetRegistration(ulong guild, ulong channel)
      {
         string registration = null;
         string queryString = $@"SELECT Register 
                                 FROM ChannelRegistration 
                                 WHERE GuildID={guild}
                                 AND ChannelID={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  registration = Convert.ToString(reader["Register"]);
               }
            }
            conn.Close();

         }
         return registration;
      }

      /// <summary>
      /// Checks if guild has a channel registered for raid notifications.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>True if registered channel exists, otherwise false.</returns>
      public bool CheckNotificationRegister(ulong guild)
      {
         bool registerFound = false;
         string queryString = $@"SELECT COUNT(*) AS count 
                                 FROM ChannelRegistration
                                 WHERE Register LIKE '%{Global.REGISTER_STRING_NOTIFICATION}%'
                                 AND GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  registerFound = Convert.ToInt32(reader["count"]) != 0;
               }
            }
            conn.Close();
         }
         return registerFound;
      }

      /// <summary>
      /// Checks if guild has a channel registered for raid notifications.
      /// </summary>
      /// <param name="message">Id of the message.</param>
      /// <returns>True if message is a raid notification message, otherwise false.</returns>
      public bool CheckNotificationMessage(ulong message)
      {
         bool registerFound = false;
         string queryString = $@"SELECT COUNT(*) AS count 
                                 FROM ChannelRegistration
                                 WHERE NotifyMessages LIKE '%{message}%';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  registerFound = Convert.ToInt32(reader["count"]) != 0;
               }
            }
            conn.Close();
         }
         return registerFound;
      }

      /// <summary>
      /// Get all channels registerd for raid notifications.
      /// </summary>
      /// <returns>Dictionary of channels where key is the guild id and value is channel id.</returns>
      public Dictionary<ulong, ulong> GetNotificationChannels()
      {
         Dictionary<ulong, ulong> channels = new Dictionary<ulong, ulong>();
         string queryString = $@"SELECT GuildID, ChannelID 
                                 FROM ChannelRegistration
                                 WHERE Register LIKE '%{Global.REGISTER_STRING_NOTIFICATION}%';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  channels.Add(Convert.ToUInt64(reader["GuildID"]), Convert.ToUInt64(reader["ChannelID"]));
               }
            }
            conn.Close();
         }
         return channels;
      }

      /// <summary>
      /// Get notification messages for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <returns>Channel message string.</returns>
      public string GetNotificationMessages(ulong guild, ulong channel)
      {
         string NotifyMessages = null;
         string queryString = $@"SELECT NotifyMessages 
                                 FROM ChannelRegistration 
                                 WHERE GuildID={guild}
                                 AND ChannelID={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  if (reader["NotifyMessages"].GetType() != typeof(DBNull))
                  {
                     NotifyMessages = Convert.ToString(reader["NotifyMessages"]);
                  }
               }
            }
            conn.Close();

         }
         return NotifyMessages;
      }

      /// <summary>
      /// Adds a new registration to a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <param name="registration">New registration value.</param>
      public void AddRegistration(ulong guild, ulong channel, string registration)
      {
         string queryString = $@"INSERT INTO ChannelRegistration (GuildID, ChannelID, Register, NotifyMessages)
                                 VALUES ({guild}, {channel}, '{registration}', NULL)";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates the registration for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <param name="registration">New registration value.</param>
      public void UpdateRegistration(ulong guild, ulong channel, string registration)
      {
         string queryString = $@"UPDATE ChannelRegistration 
                                 SET Register='{registration}'
                                 WHERE GuildID={guild}
                                 AND ChannelID={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates the notification message for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <param name="messages">Id of the messages as comma separated string.</param>
      public void UpdateNotificationMessage(ulong guild, ulong channel, string messages = null)
      {

         string messageID = $@"'{messages}'" ?? "NULL";
         string queryString = $@"UPDATE ChannelRegistration 
                                 SET NotifyMessages={messageID}
                                 WHERE GuildID={guild}
                                 AND ChannelID={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Deletes the registration for all channels in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void DeleteAllRegistration(ulong guild)
      {
         string queryString = $@"DELETE FROM ChannelRegistration
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Deletes the registration for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      public void DeleteRegistration(ulong guild, ulong channel)
      {
         string queryString = $@"DELETE FROM ChannelRegistration
                                 WHERE GuildID={guild}
                                 AND ChannelID={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      // Nicknames ************************************************************

      /// <summary>
      /// Get a Pokémon by nickname.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of the Pokémon.</param>
      /// <returns>Name of the Pokémon with the nickname, otherwiase null.</returns>
      public string GetPokemonByNickname(ulong guild, string nickname)
      {
         string pokemonName = null;
         string queryString = $@"SELECT BaseName 
                                 FROM Nickname 
                                 WHERE GuildID={guild}
                                 AND Nickname='{nickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  pokemonName = Convert.ToString(reader["BaseName"]);
               }
            }
            conn.Close();
         }
         return pokemonName;
      }

      /// <summary>
      /// Gets all nicknames for a Pokémon in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="pokemon">Name of the Pokémon.</param>
      /// <returns>List of all nicknames for the Pokémon in the guild.</returns>
      public List<string> GetNicknames(ulong guild, string pokemon)
      {
         List<string> nicknames = new List<string>();
         string queryString = $@"SELECT Nickname 
                                 FROM Nickname 
                                 WHERE GuildID={guild}
                                 AND BaseName='{pokemon}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  nicknames.Add(Convert.ToString(reader["Nickname"]));
               }
            }
            conn.Close();
         }
         return nicknames;
      }

      /// <summary>
      /// Adds a nickname to a Pokémon for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="nickname">Nickname of the Pokémon.</param>
      public void AddNickname(ulong guild, string pokemonName, string nickname)
      {
         string queryString = $@"INSERT INTO Nickname (GuildID, BaseName, Nickname)
                                 VALUES ({guild}, '{pokemonName}', '{nickname}')";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates a nickname for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="newNickname">New nickname.</param>
      /// <param name="originalNickname">Old nickname.</param>
      public void UpdateNickname(ulong guild, string newNickname, string originalNickname)
      {
         string queryString = $@"UPDATE Nickname 
                                 SET Nickname='{newNickname}'
                                 WHERE GuildID={guild}
                                 AND Nickname='{originalNickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Delete a nickname from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of a Pokémon.</param>
      public void DeleteNickname(ulong guild, string nickname)
      {
         string queryString = $@"DELETE FROM Nickname
                                 WHERE GuildID={guild}
                                 AND Nickname='{nickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      // POI ******************************************************************

      /// <summary>
      /// Gets a POI by name.
      /// </summary>
      /// <param name="poiName">Name of the POI.</param>
      /// <returns>A gym if the name is in the database, otherwise null.</returns>
      public POI GetPOI(ulong guild, string poiName)
      {
         POI poi = null;
         string queryString = $@"SELECT *
                                 FROM POI 
                                 WHERE GuildID={guild}
                                 AND Name='{poiName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  poi = new POI
                  {
                     Name = Convert.ToString(reader["Name"]),
                     Latitude = Convert.ToString(reader["Latitude"]),
                     Longitude = Convert.ToString(reader["Longitude"]),
                     IsGym = Convert.ToInt32(reader["IsGym"]) == TRUE,
                     IsSponsored = Convert.ToInt32(reader["IsSponsored"]) == TRUE,
                     IsExGym = Convert.ToInt32(reader["IsEx"]) == TRUE,
                  };
               }
            }
            conn.Close();
         }
         return poi;
      }

      /// <summary>
      /// Get a gym by nickname.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of the gym.</param>
      /// <returns>Name of the gym with the nickname, otherwiase null.</returns>
      public string GetPOIByNickname(ulong guild, string nickname)
      {
         string poi = null;
         string queryString = $@"SELECT BaseName 
                                 FROM POINickname 
                                 WHERE GuildID={guild}
                                 AND Nickname='{nickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  poi = Convert.ToString(reader["BaseName"]);
               }
            }
            conn.Close();
         }
         return poi;
      }

      /// <summary>
      /// Gets all nicknames for a POI in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poi">Name of the POI.</param>
      /// <returns>List of all nicknames for the POI in the guild.</returns>
      public List<string> GetPOINicknames(ulong guild, string poi)
      {
         List<string> nicknames = new List<string>();
         string queryString = $@"SELECT Nickname 
                                 FROM POINickname 
                                 WHERE GuildID={guild}
                                 AND BaseName='{poi}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  nicknames.Add(Convert.ToString(reader["Nickname"]));
               }
            }
            conn.Close();
         }
         return nicknames;
      }

      /// <summary>
      /// Gets all POIs in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>List of all POIs in the guild.</returns>
      public List<string> GetGuildPOIs(ulong guild)
      {
         List<string> pois = new List<string>();
         string queryString = $@"SELECT Name 
                                 FROM POI 
                                 WHERE GuildID={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  pois.Add(Convert.ToString(reader["Name"]));
               }
            }
            conn.Close();
         }
         return pois;
      }

      /// <summary>
      /// Adds a Point of Interest to a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      /// <param name="latitude">Latitudinal coordinate.</param>
      /// <param name="longitude">Longitudinal coordinate.</param>
      /// <param name="gym">Is the Point of Interest a gym.</param>
      /// <param name="sponsored">Is the Point of Interest sponsored.</param>
      /// <param name="ex">Is the Point of Interest an EX Gym.</param>
      public void AddPOI(ulong guild, string poiName, float latitude, float longitude, int gym, int sponsored, int ex)
      {
         if ((gym != TRUE && gym != FALSE) ||
             (sponsored != TRUE && sponsored != FALSE) ||
             (ex != TRUE && ex != FALSE))
         {
            return;
         }

         string queryString = $@"INSERT INTO POI (GuildID, Name, Latitude, Longitude, IsGym, IsSponsored, IsEx)
                                 VALUES ({guild}, '{poiName}', '{latitude}', '{longitude}', {gym}, {sponsored}, {ex})";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates an attribute of a Point of Interest.
      /// Only updates true/false values.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      /// <param name="attribute">Attribute to change.</param>
      /// <param name="value">New value of the attribute</param>
      public void UpdatePOI(ulong guild, string poiName, string attribute, int value)
      {
         if (value != TRUE && value != FALSE)
         {
            return;
         }

         string queryString = $@"UPDATE POI 
                                 SET {attribute}={value}
                                 WHERE GuildID={guild}
                                 AND Name='{poiName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Delete a Point of Interest from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      public void RemovePOI(ulong guild, string poiName)
      {
         string queryString = $@"DELETE FROM POI
                                 WHERE GuildID={guild}
                                 AND Name='{poiName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Adds a nickname to a Point of Interest for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      /// <param name="nickname">Nickname of the Point of Interest.</param>
      public void AddPOINickname(ulong guild, string poiName, string nickname)
      {
         string queryString = $@"INSERT INTO POINickname (GuildID, BaseName, Nickname)
                                 VALUES ({guild}, '{poiName}', '{nickname}')";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates a Point of Interest nickname for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="newNickname">New nickname.</param>
      /// <param name="originalNickname">Old nickname.</param>
      public void UpdatePOINickname(ulong guild, string newNickname, string originalNickname)
      {
         string queryString = $@"UPDATE POINickname 
                                 SET Nickname='{newNickname}'
                                 WHERE GuildID={guild}
                                 AND Nickname='{originalNickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Delete a Point of Interest nickname from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of a Pokémon.</param>
      public void DeletePOINickname(ulong guild, string nickname)
      {
         string queryString = $@"DELETE FROM POINickname
                                 WHERE GuildID={guild}
                                 AND Nickname='{nickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      // Profile **************************************************************

      /// <summary>
      /// Creates an empty profile.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      public void CreateProfile(ulong account)
      {
         string queryString = $@"INSERT INTO Profile (UserID, Exp, TrainerCode, ReferalCode, Location)
                                 VALUES ({account}, -1, 0, NULL, NULL)";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Gets a profile.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      /// <returns>Profile if one has been created, otherwise null.</returns>
      public Profile GetProfile(ulong account)
      {
         Profile profile = null;
         string queryString = $@"SELECT Exp, TrainerCode, ReferalCode, Location 
                                 FROM Profile 
                                 WHERE UserID={account};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  profile = new Profile()
                  {
                     Exp = Convert.ToInt32(reader["Exp"]),
                     TrainerCode = Convert.ToInt64(reader["TrainerCode"]),
                     ReferalCode = Convert.ToString(reader["ReferalCode"]),
                     Location = Convert.ToString(reader["Location"])
                  };
               }
            }
            conn.Close();
         }
         return profile;
      }

      /// <summary>
      /// Update the total experiance of a profie.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      /// <param name="exp">Total experiance.</param>
      public void UpdateProfileExp(ulong account, uint exp)
      {
         string queryString = $@"UPDATE Profile 
                                 SET Exp={exp}
                                 WHERE UserID={account};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Update friend code of a profile.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      /// <param name="code">Friend code.</param>
      public void UpdateFriendCode(ulong account, long code)
      {
         string queryString = $@"UPDATE Profile 
                                 SET TrainerCode={code}
                                 WHERE UserID={account};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Update the referal code of a profile.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      /// <param name="code">Referal Code.</param>
      public void UpdateReferalCode(ulong account, string code)
      {
         string queryString = $@"UPDATE Profile 
                                 SET ReferalCode='{code}'
                                 WHERE UserID={account};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Update the location of a profile.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      /// <param name="location">Country name.</param>
      public void UpdateLocation(ulong account, string location)
      {
         string queryString = $@"UPDATE Profile 
                                 SET Location='{location}'
                                 WHERE UserID={account};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Clear the location of a profile.
      /// </summary>
      /// <param name="account">Id of the account.</param>
      public void ClearLocation(ulong account)
      {
         string queryString = $@"UPDATE Profile 
                                 SET Location=NULL
                                 WHERE UserID={account};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }
   }
}