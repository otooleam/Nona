﻿using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Group within a raid.
   /// </summary>
   public class RaidGroup
   {
      private int PlayerLimit { get; set; }
      private int InviteLimit { get; set; }

      /// <summary>
      /// Dictionary of players attending the raid.
      /// key = player
      /// value = party size
      /// </summary>
      private Dictionary<SocketGuildUser, int> Attending { get; set; }

      /// <summary>
      /// Dictionary of players ready for the raid.
      /// key = player
      /// value = party size
      /// </summary>
      private Dictionary<SocketGuildUser, int> Ready { get; set; }

      /// <summary>
      /// Dictionary of players invited to the raid group.
      /// key = invited player
      /// value = player who invited
      /// </summary>
      private Dictionary<SocketGuildUser, SocketGuildUser> Invited { get; set; }

      /// <summary>
      /// Creates a new raid group.
      /// </summary>
      public RaidGroup(int playerLimit, int inviteLimit)
      {
         Attending = new Dictionary<SocketGuildUser, int>();
         Ready = new Dictionary<SocketGuildUser, int>();
         Invited = new Dictionary<SocketGuildUser, SocketGuildUser>();
         PlayerLimit = playerLimit;
         InviteLimit = inviteLimit;
      }

      /// <summary>
      /// Gets all attending players.
      /// </summary>
      /// <returns>Immutable dictionary of attending players.</returns>
      public ImmutableDictionary<SocketGuildUser, int> GetReadonlyAttending()
      {
         return Attending.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      /// <summary>
      /// Gets all ready players
      /// </summary>
      /// <returns>Immutable dictionary of ready players.</returns>
      public ImmutableDictionary<SocketGuildUser, int> GetReadonlyHere()
      {
         return Ready.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      /// <summary>
      /// Gets all invited players.
      /// </summary>
      /// <returns>Immutable dictionary of invited players.</returns>
      public ImmutableDictionary<SocketGuildUser, SocketGuildUser> GetReadonlyInvitedAll()
      {
         return Invited.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      public ImmutableDictionary<SocketGuildUser, SocketGuildUser> GetReadonlyInvitedReady()
      {
         return Invited.Where(invite => Ready.ContainsKey(invite.Value)).ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      public ImmutableDictionary<SocketGuildUser, SocketGuildUser> GetReadonlyInvitedAttending()
      {
         return Invited.Where(invite => Attending.ContainsKey(invite.Value)).ToImmutableDictionary(k => k.Key, v => v.Value);
      }


      /// <summary>
      /// Gets how many players are attending.
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
      /// Gets how many players are ready.
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
      /// Adds a player to the raid group.
      /// If the user is already in the raid group, their party size is updated.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="partySize">Number of accounts the user is bringing.</param>
      public void Add(SocketGuildUser player, int attendSize, int remoteSize)
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
      public List<SocketGuildUser> Remove(SocketGuildUser player)
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
            return new List<SocketGuildUser>();
         }

         List<SocketGuildUser> playerInvited = new List<SocketGuildUser>();
         foreach (KeyValuePair<SocketGuildUser, SocketGuildUser> invite in Invited.Where(x => x.Value.Equals(player)))
         {
            playerInvited.Add(invite.Key);
         }

         foreach (SocketGuildUser invite in playerInvited)
         {
            Invited.Remove(invite);
         }

         return playerInvited;
      }

      public Dictionary<SocketGuildUser, List<SocketGuildUser>> ClearEmptyPlayers()
      {
         Dictionary<SocketGuildUser, List<SocketGuildUser>> empty = new Dictionary<SocketGuildUser, List<SocketGuildUser>>();
         foreach (KeyValuePair<SocketGuildUser, int> user in Attending.Where(user => user.Value == 0))
         {
            empty.Add(user.Key, new List<SocketGuildUser>());
            empty[user.Key].AddRange(Invited.Where(x => x.Value.Equals(user)).Select(invite => invite.Key));
         }
         foreach (KeyValuePair<SocketGuildUser, int> user in Ready.Where(user => user.Value == 0))
         {
            empty.Add(user.Key, new List<SocketGuildUser>());
            empty[user.Key].AddRange(Invited.Where(x => x.Value.Equals(user)).Select(invite => invite.Key));
         }

         foreach (SocketGuildUser user in empty.Keys)
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
         foreach (SocketGuildUser user in empty.SelectMany(group => group.Value))
         {
            Invited.Remove(user);
         }

         return empty;
      }

      /// <summary>
      /// Marks a player as ready.
      /// </summary>
      /// <param name="player">Player to mark ready.</param>
      public bool PlayerReady(SocketGuildUser player)
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
      public void Invite(SocketGuildUser requester, SocketGuildUser accepter)
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
      public ImmutableList<SocketGuildUser> GetPingList()
      {
         return Ready.Keys.ToList().Union(Invited.Keys.ToList()).Distinct().ToImmutableList();
      }

      /// <summary>
      /// Gets a list of players to notify of an edit.
      /// </summary>
      /// <returns></returns>
      public ImmutableList<SocketGuildUser> GetNotifyList()
      {
         return Ready.Keys.ToList().Union(Invited.Keys.ToList()).Union(Attending.Keys.ToList()).Distinct().ToImmutableList();
      }

      /// <summary>
      /// Gets the total players in the raid group.
      /// </summary>
      /// <returns>Total players in raid group.</returns>
      public int TotalPlayers()
      {
         return GetAttendingCount() + GetReadyCount() + GetRemoteCount();
      }

      public int GetRemoteCount()
      {
         return GetAttendingRemoteCount() + GetReadyRemoteCount() + GetInviteCount();
      }

      /// <summary>
      /// Checks if the raid group has a desired user.
      /// </summary>
      /// <param name="player">Player to check.</param>
      /// <param name="checkInvite">If invited players should be checked.</param>
      /// <returns>True if the player is in the raid group, otherwise false.</returns>
      public bool HasPlayer(SocketGuildUser player, bool checkInvite = true)
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
         foreach (KeyValuePair<SocketGuildUser, int> player in Attending)
         {
            if ((newGroup.TotalPlayers() + player.Value) <= PlayerLimit / 2)
            {
               newGroup.Attending.Add(player.Key, player.Value);

               foreach (KeyValuePair<SocketGuildUser, SocketGuildUser> invite in Invited)
               {
                  if (invite.Value.Equals(player.Key))
                  {
                     newGroup.Invite(invite.Key, invite.Value);
                  }
               }
            }
         }

         if (newGroup.TotalPlayers() < PlayerLimit / 2)
         {
            foreach (KeyValuePair<SocketGuildUser, int> player in Ready)
            {
               if (newGroup.TotalPlayers() < PlayerLimit / 2)
               {
                  newGroup.Ready.Add(player.Key, player.Value);
                  foreach (KeyValuePair<SocketGuildUser, SocketGuildUser> invite in Invited)
                  {
                     if (invite.Value.Equals(player.Key))
                     {
                        newGroup.Invite(invite.Key, invite.Value);
                     }
                  }
               }
            }
         }

         foreach (SocketGuildUser player in newGroup.Attending.Keys)
         {
            Attending.Remove(player);
         }
         foreach (SocketGuildUser player in newGroup.Ready.Keys)
         {
            Ready.Remove(player);
         }
         foreach (SocketGuildUser player in newGroup.Invited.Keys)
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
      public static int GetTotalGroup(int value) => GetAttending(value) + GetRemote(value);
      private static int GetAttending(int value) => value & Global.ATTEND_MASK;
      private static int GetRemote(int value) => (value & Global.REMOTE_MASK) >> Global.REMOTE_SHIFT;
      private int SetValue(int attend, int remote) => (remote << Global.REMOTE_SHIFT) | attend;
   }
}