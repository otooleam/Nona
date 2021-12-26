using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Group within a raid.
   /// </summary>
   public class RaidGroup
   {
      /// <summary>
      /// Maximum number of players in the group.
      /// </summary>
      private int PlayerLimit { get; set; }

      /// <summary>
      /// Maximum number of invites in the group.
      /// </summary>
      private int InviteLimit { get; set; }

      /// <summary>
      /// Dictionary of players attending the raid.
      /// key = player
      /// value = party size
      /// </summary>
      private Dictionary<Player, int> Attending { get; set; }

      /// <summary>
      /// Dictionary of players ready for the raid.
      /// key = player
      /// value = party size
      /// </summary>
      private Dictionary<Player, int> Ready { get; set; }

      /// <summary>
      /// Dictionary of players invited to the raid group.
      /// key = invited player
      /// value = player who invited
      /// </summary>
      private Dictionary<Player, Player> Invited { get; set; }

      /// <summary>
      /// Creates a new raid group.
      /// </summary>
      /// <param name="playerLimit">Max number of players.</param>
      /// <param name="inviteLimit">Max number of invites.</param>
      public RaidGroup(int playerLimit, int inviteLimit)
      {
         Attending = new Dictionary<Player, int>();
         Ready = new Dictionary<Player, int>();
         Invited = new Dictionary<Player, Player>();
         PlayerLimit = playerLimit;
         InviteLimit = inviteLimit;
      }

      /// <summary>
      /// Gets all attending players.
      /// </summary>
      /// <returns>Immutable dictionary of attending players.</returns>
      public ImmutableDictionary<Player, int> GetReadonlyAttending()
      {
         return Attending.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      /// <summary>
      /// Gets all ready players.
      /// </summary>
      /// <returns>Immutable dictionary of ready players.</returns>
      public ImmutableDictionary<Player, int> GetReadonlyHere()
      {
         return Ready.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      /// <summary>
      /// Gets all invited players.
      /// </summary>
      /// <returns>Immutable dictionary of invited players.</returns>
      public ImmutableDictionary<Player, Player> GetReadonlyInvitedAll()
      {
         return Invited.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      /// <summary>
      /// Gets all invited players where the inviter is attending.
      /// </summary>
      /// <returns>Immutable dictionary of invited players.</returns>
      public ImmutableDictionary<Player, Player> GetReadonlyInvitedAttending()
      {
         return Invited.Where(invite => Attending.ContainsKey(invite.Value)).ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      /// <summary>
      /// Gets all invited players where the inviter is ready.
      /// </summary>
      /// <returns>Immutable dictionary of invited players.</returns>
      public ImmutableDictionary<Player, Player> GetReadonlyInvitedReady()
      {
         return Invited.Where(invite => Ready.ContainsKey(invite.Value)).ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      /// <summary>
      /// Gets how many players are attending in person.
      /// </summary>
      /// <returns>Number of attending players.</returns>
      public int GetAttendingCount()
      {
         int total = 0;
         foreach (int player in Attending.Values)
         {
            total += GetAttending(player);
         }
         return total;
      }

      /// <summary>
      /// Gets how many players are attending via remote.
      /// </summary>
      /// <returns>Number of attending players.</returns>
      public int GetAttendingRemoteCount()
      {
         int total = 0;
         foreach (int player in Attending.Values)
         {
            total += GetRemote(player);
         }
         return total;
      }

      /// <summary>
      /// Gets how many players are ready in person.
      /// </summary>
      /// <returns>Number of ready players.</returns>
      public int GetReadyCount()
      {
         int total = 0;
         foreach (int player in Ready.Values)
         {
            total += GetAttending(player);
         }
         return total;
      }

      /// <summary>
      /// Gets how many players are ready via remote.
      /// </summary>
      /// <returns>Number of ready players.</returns>
      public int GetReadyRemoteCount()
      {
         int total = 0;
         foreach (int player in Ready.Values)
         {
            total += GetRemote(player);
         }
         return total;
      }

      /// <summary>
      /// Gets how many players have been invited to the group.
      /// </summary>
      /// <returns>Number of invited players.</returns>
      public int GetInviteCount()
      {
         return Invited.Count;
      }

      /// <summary>
      /// Gets the total players in the raid group.
      /// </summary>
      /// <returns>Total players in raid group.</returns>
      public int TotalPlayers()
      {
         return GetAttendingCount() + GetReadyCount() + GetRemoteCount();
      }

      /// <summary>
      /// Gets the total remote players.
      /// Includes remote and intived players.
      /// </summary>
      /// <returns>Total amount of remote players.</returns>
      public int GetRemoteCount()
      {
         return GetAttendingRemoteCount() + GetReadyRemoteCount() + GetInviteCount();
      }

      /// <summary>
      /// Adds a player to the raid group.
      /// If the user is already in the raid group, their party size is updated.
      /// Will update attend size or remote size, not both
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="attendSize">Number of accounts attending in person.</param>
      /// <param name="remoteSize">Number of accounts attending via remote.</param>
      public void AddPlayer(Player player, int attendSize, int remoteSize)
      {
         if (!Invited.ContainsKey(player))
         {
            if (Attending.ContainsKey(player))
            {
               if (remoteSize == Global.NO_ADD_VALUE)
               {
                  Attending[player] = SetValue(attendSize, GetRemote(Attending[player]));
               }
               else if (attendSize == Global.NO_ADD_VALUE)
               {
                  Attending[player] = SetValue(GetAttending(Attending[player]), remoteSize);
               }
            }
            else if (Ready.ContainsKey(player))
            {
               if (remoteSize == Global.NO_ADD_VALUE)
               {
                  Ready[player] = SetValue(attendSize, GetRemote(Ready[player]));
               }
               else if (attendSize == Global.NO_ADD_VALUE)
               {
                  Ready[player] = SetValue(GetAttending(Ready[player]), remoteSize);
               }
            }
            else
            {
               int attend = (attendSize == Global.NO_ADD_VALUE) ? 0 : attendSize;
               int remote = (remoteSize == Global.NO_ADD_VALUE) ? 0 : remoteSize;
               int partySize = (remote << Global.REMOTE_SHIFT) | attend;
               if (partySize != 0)
               {
                  Attending.Add(player, partySize);
               }
            }
         }
      }

      /// <summary>
      /// Removes a player from the raid group.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>List of players invited by the player.</returns>
      public List<Player> RemovePlayer(Player player)
      {
         if (Attending.ContainsKey(player))
         {
            Attending.Remove(player);
         }
         else if (Ready.ContainsKey(player))
         {
            Ready.Remove(player);
         }
         else if (Invited.ContainsKey(player))
         {
            Invited.Remove(player);
            return new List<Player>();
         }

         List<Player> playerInvited = new List<Player>();
         foreach (KeyValuePair<Player, Player> invite in Invited.Where(x => x.Value.Equals(player)))
         {
            playerInvited.Add(invite.Key);
         }

         foreach (Player invite in playerInvited)
         {
            Invited.Remove(invite);
         }

         return playerInvited;
      }

      /// <summary>
      /// Removes all players with a party size of 0.
      /// </summary>
      /// <returns>Dictionary of all users invited by removed players.</returns>
      public Dictionary<Player, List<Player>> ClearEmptyPlayers()
      {
         Dictionary<Player, List<Player>> empty = new Dictionary<Player, List<Player>>();
         foreach (KeyValuePair<Player, int> user in Attending.Where(user => user.Value == 0))
         {
            empty.Add(user.Key, new List<Player>());
            empty[user.Key].AddRange(Invited.Where(x => x.Value.Equals(user.Key)).Select(invite => invite.Key));
         }
         foreach (KeyValuePair<Player, int> user in Ready.Where(user => user.Value == 0))
         {
            empty.Add(user.Key, new List<Player>());
            empty[user.Key].AddRange(Invited.Where(x => x.Value.Equals(user.Key)).Select(invite => invite.Key));
         }

         foreach (Player user in empty.Keys)
         {
            if (Attending.ContainsKey(user))
            {
               Attending.Remove(user);
            }
            else if (Ready.ContainsKey(user))
            {
               Ready.Remove(user);
            }
         }
         foreach (Player user in empty.SelectMany(group => group.Value))
         {
            Invited.Remove(user);
         }

         return empty;
      }

      /// <summary>
      /// Marks a player as ready.
      /// </summary>
      /// <param name="player">Player to mark ready.</param>
      public bool MarkPlayerReady(Player player)
      {
         if (Attending.ContainsKey(player))
         {
            Ready.Add(player, Attending[player]);
            Attending.Remove(player);
            return true;
         }
         return false;
      }

      /// <summary>
      /// Invites a player to the raid group.
      /// </summary>
      /// <param name="requester">Player that requested the invite.</param>
      /// <param name="accepter">Player that accepted the invite.</param>
      public void InvitePlayer(Player requester, Player accepter)
      {
         Invited.Add(requester, accepter);
      }

      /// <summary>
      /// Checks if all players are ready.
      /// </summary>
      /// <returns>True if all players are ready, otherwise false.</returns>
      public bool AllPlayersReady()
      {
         return Attending.Count == 0 && Ready.Count != 0;
      }

      /// <summary>
      /// Gets a list of players to ping.
      /// </summary>
      /// <returns>List of players that are here</returns>
      public ImmutableList<Player> GetPingList()
      {
         return Ready.Keys.ToList().Union(Invited.Keys.ToList()).Distinct().ToImmutableList();
      }

      /// <summary>
      /// Gets a list of players to notify of an edit.
      /// </summary>
      /// <returns>List of players to notify.</returns>
      public ImmutableList<Player> GetNotifyList()
      {
         return Ready.Keys.ToList().Union(Invited.Keys.ToList()).Union(Attending.Keys.ToList()).Distinct().ToImmutableList();
      }

      /// <summary>
      /// Checks if the raid group has a desired user.
      /// </summary>
      /// <param name="player">Player to check.</param>
      /// <param name="checkInvite">If invited players should be checked.</param>
      /// <returns>True if the player is in the raid group, otherwise false.</returns>
      public bool HasPlayer(Player player, bool checkInvite = true)
      {
         return Attending.ContainsKey(player) || Ready.ContainsKey(player) || (checkInvite && Invited.ContainsKey(player));
      }

      /// <summary>
      /// Checks if the group should be split.
      /// </summary>
      /// <returns>True if the total players is greater than the player limit, otherwise false.</returns>
      public bool ShouldSplit()
      {
         return TotalPlayers() > PlayerLimit || GetRemoteCount() > InviteLimit;
      }

      /// <summary>
      /// Attempts to split the raid group. 
      /// </summary>
      /// <returns>A new raid group if the raid can be split, else null.</returns>
      public RaidGroup SplitGroup()
      {
         RaidGroup newGroup = new RaidGroup(PlayerLimit, InviteLimit);
         foreach (KeyValuePair<Player, int> player in Attending)
         {
            if ((newGroup.TotalPlayers() + player.Value) <= PlayerLimit / 2)
            {
               newGroup.Attending.Add(player.Key, player.Value);

               foreach (KeyValuePair<Player, Player> invite in Invited)
               {
                  if (invite.Value.Equals(player.Key))
                  {
                     newGroup.InvitePlayer(invite.Key, invite.Value);
                  }
               }
            }
         }

         if (newGroup.TotalPlayers() < PlayerLimit / 2)
         {
            foreach (KeyValuePair<Player, int> player in Ready)
            {
               if (newGroup.TotalPlayers() < PlayerLimit / 2)
               {
                  newGroup.Ready.Add(player.Key, player.Value);
                  foreach (KeyValuePair<Player, Player> invite in Invited)
                  {
                     if (invite.Value.Equals(player.Key))
                     {
                        newGroup.InvitePlayer(invite.Key, invite.Value);
                     }
                  }
               }
            }
         }

         foreach (Player player in newGroup.Attending.Keys)
         {
            Attending.Remove(player);
         }
         foreach (Player player in newGroup.Ready.Keys)
         {
            Ready.Remove(player);
         }
         foreach (Player player in newGroup.Invited.Keys)
         {
            Invited.Remove(player);
         }
         return newGroup;
      }

      /// <summary>
      /// Merges this group and another group.
      /// </summary>
      /// <param name="group">Group to merge with this group.</param>
      public void MergeGroup(RaidGroup group)
      {
         if (!group.Equals(this) &&
            group.TotalPlayers() != 0 && TotalPlayers() != 0 &&
            (group.TotalPlayers() + TotalPlayers()) <= PlayerLimit)
         {
            Attending = Attending.Union(group.Attending).ToDictionary(k => k.Key, v => v.Value);
            Ready = Ready.Union(group.Ready).ToDictionary(k => k.Key, v => v.Value);
            Invited = Invited.Union(group.Invited).ToDictionary(k => k.Key, v => v.Value);
            group.Attending.Clear();
            group.Ready.Clear();
            group.Invited.Clear();
         }
      }

      /// <summary>
      /// Resets all ready users to attending.
      /// </summary>
      public void ResetReady()
      {
         Attending = Attending.Union(Ready).ToDictionary(k => k.Key, v => v.Value);
         Ready.Clear();
      }
      
      /// <summary>
      /// Get total number of players in a party.
      /// </summary>
      /// <param name="value">Encoded party value.</param>
      /// <returns>Full size of the party.</returns>
      public static int GetFullPartySize(int value) => GetAttending(value) + GetRemote(value);

      /// <summary>
      /// Get the number of players attending in person in a party.
      /// </summary>
      /// <param name="value">Encoded party value.</param>
      /// <returns>Number of in person players.</returns>
      private static int GetAttending(int value) => value & Global.ATTEND_MASK;

      /// <summary>
      /// Get the number of players attending via remote in a party.
      /// </summary>
      /// <param name="value">Encoded party value.</param>
      /// <returns>Number of remote players.</returns>
      private static int GetRemote(int value) => (value & Global.REMOTE_MASK) >> Global.REMOTE_SHIFT;

      /// <summary>
      /// Encodes a players party.
      /// The party is encoded into a 8-bit integer.
      /// The encodeing is 0YYY 0XXX where the in person players
      /// are in the lower 4 bits and the remote players are in
      /// the upper 4 bits. The 4th and 8th bits remain clear.
      /// </summary>
      /// <param name="attend">Number of in person players.</param>
      /// <param name="remote">Number of remote raiders.</param>
      /// <returns>Encoded party value.</returns>
      private static int SetValue(int attend, int remote) => (remote << Global.REMOTE_SHIFT) | attend;
   }
}