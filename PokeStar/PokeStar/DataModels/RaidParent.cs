using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Parent class for raid types.
   /// </summary>
   public abstract class RaidParent
   {
      /// <summary>
      /// Index of the current location.
      /// </summary>
      private int CurrentLocation;

      /// <summary>
      /// List of all locations the train will visit.
      /// </summary>
      private readonly List<RaidTrainLoc> Locations;

      /// <summary>
      /// Maximum number of raid groups.
      /// </summary>
      protected int RaidGroupLimit { get; private set; }

      /// <summary>
      /// Maximum number of players per group.
      /// </summary>
      protected int PlayerLimit { get; private set; }

      /// <summary>
      /// Maximum number of invites per group.
      /// </summary>
      protected int InviteLimit { get; private set; }

      /// <summary>
      /// Group number used for request invite list.
      /// </summary>
      protected const int InviteListNumber = 101;

      /// <summary>
      /// List of raid groups in the raid.
      /// </summary>
      protected List<RaidGroup> Groups { get; private set; }

      /// <summary>
      /// List of players looking for an invite to the raid.
      /// </summary>
      protected List<SocketGuildUser> Invite { get; private set; }

      /// <summary>
      /// Tier of the raid.
      /// </summary>
      public short Tier { get; set; }

      /// <summary>
      /// Tier of the raid for boss selection.
      /// </summary>
      public short SelectionTier { get; set; }

      /// <summary>
      /// Id of the current station message.
      /// Only 1 should exist at a time.
      /// </summary>
      public ulong? StationMessageId { get; set; } = null;

      /// <summary>
      /// Current page of Invite Embed.
      /// </summary>
      public int InvitePage { get; protected set; }

      /// <summary>
      /// When the raid was created at.
      /// </summary>
      public DateTime CreatedAt { get; protected set; }

      /// <summary>
      /// Player who is in the process of inviting someone.
      /// </summary>
      public SocketGuildUser InvitingPlayer { get; set; }

      /// <summary>
      /// Player in charge of the train.
      /// </summary>
      public SocketGuildUser Conductor { get; set; }

      /// <summary>
      /// Player editing the boss of the raid.
      /// Only used for single stop raids.
      /// </summary>
      public SocketGuildUser BossEditingPlayer { get; set; }

      /// <summary>
      /// List of all current raid bosses.
      /// </summary>
      public Dictionary<int, List<string>> AllBosses { get; set; }

      /// <summary>
      /// Current page for selecting a boss.
      /// </summary>
      public int BossPage { get; set; }

      /// <summary>
      /// Creates a new RaidParent.
      /// </summary>
      /// <param name="groupLimit">Max number of groups.</param>
      /// <param name="playerLimit">Max number of players per group.</param>
      /// <param name="inviteLimit">Max number of invites per group.</param>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="conductor">Conductor of the raid train.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidParent(int groupLimit, int playerLimit, int inviteLimit, short tier, string time, string location, SocketGuildUser conductor, string boss = null)
      {
         RaidGroupLimit = groupLimit;
         PlayerLimit = playerLimit;
         InviteLimit = inviteLimit;
         Tier = tier;
         SelectionTier = tier;
         Conductor = conductor;

         Locations = new List<RaidTrainLoc>
         {
            new RaidTrainLoc(time, location, boss)
         };
         Groups = new List<RaidGroup>
         {
            new RaidGroup(PlayerLimit, InviteLimit)
         };
         Invite = new List<SocketGuildUser>();
         CurrentLocation = 0;
         CreatedAt = DateTime.Now;
         InvitingPlayer = null;
      }

      /// <summary>
      /// Creates a new single stop RaidParent.
      /// </summary>
      /// <param name="groupLimit">Max number of groups.</param>
      /// <param name="playerLimit">Max number of players per group.</param>
      /// <param name="inviteLimit">Max number of invites per group.</param>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidParent(int groupLimit, int playerLimit, int inviteLimit, short tier, string time, string location, string boss = null)
      {
         RaidGroupLimit = groupLimit;
         PlayerLimit = playerLimit;
         InviteLimit = inviteLimit;
         Tier = tier;
         SelectionTier = tier;

         Locations = new List<RaidTrainLoc>
         {
            new RaidTrainLoc(time, location, boss)
         };
         Groups = new List<RaidGroup>
         {
            new RaidGroup(PlayerLimit, InviteLimit)
         };
         Invite = new List<SocketGuildUser>();
         CurrentLocation = 0;
         CreatedAt = DateTime.Now;
         InvitingPlayer = null;
         Conductor = null;
      }

      /// <summary>
      /// Gets the number of raid groups.
      /// </summary>
      /// <returns>Current number of raid groups.</returns>
      public int GetTotalGroups()
      {
         return Groups.Count;
      }

      /// <summary>
      /// Gets a specific group by number.
      /// </summary>
      /// <param name="groupNumber">Group number to get</param>
      /// <returns>Group at the intended posision</returns>
      public RaidGroup GetGroup(int groupNumber)
      {
         return Groups.ElementAt(groupNumber);
      }

      /// <summary>
      /// Gets all users in all raid groups and who requested invites.
      /// </summary>
      /// <returns>A list of all users.</returns>
      public List<SocketGuildUser> GetAllUsers()
      {
         List<SocketGuildUser> allUsers = new List<SocketGuildUser>();
         allUsers.AddRange(Invite);
         foreach (RaidGroup group in Groups)
         {
            allUsers.AddRange(group.GetNotifyList());
         }
         return allUsers;
      }

      /// <summary>
      /// Gets all users that want an invite to the raid.
      /// </summary>
      /// <returns>List of users.</returns>
      public ImmutableList<SocketGuildUser> GetReadonlyInviteList()
      {
         return Invite.ToImmutableList();
      }

      /// <summary>
      /// Checks if there is an active invite message.
      /// </summary>
      /// <returns>True if there is an active message, otherwise false.</returns>
      public bool HasActiveInvite()
      {
         return InvitingPlayer != null;
      }

      /// <summary>
      /// Updates the invite page.
      /// </summary>
      /// <param name="isPositiveChage">If the change is positive, otherwise negative.</param>
      /// <param name="pageMaxOptions">Total options per page.</param>
      public void ChangeInvitePage(bool isPositiveChage, int pageMaxOptions)
      {
         if (isPositiveChage)
         {
            if ((InvitePage + 1) * pageMaxOptions < Invite.Count)
            {
               InvitePage++;
            }
         }
         else
         {
            if (InvitePage != 0)
            {
               InvitePage--;
            }
         }
      }

      /// <summary>
      /// Adds a player to a raid.
      /// The player will not be added if splitting the group brings the number of
      /// raid groups over the group limit.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="partySize">Number of accounts the player is bringing.</param>
      /// <param name="invitedBy">Who invited the user.</param>
      /// <returns>True if the user was added, otherwise false.</returns>
      public abstract bool AddPlayer(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null);

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>RaidRemove with raid group and list of invited users.</returns>
      public abstract RaidRemoveResult RemovePlayer(SocketGuildUser player);

      /// <summary>
      /// Requests an invite to a raid for a player.
      /// </summary>
      /// <param name="player">Player that requested the invite.</param>
      public abstract void RequestInvite(SocketGuildUser player);

      /// <summary>
      /// Accepts an invite of a player.
      /// </summary>
      /// <param name="requester">Player that requested the invite.</param>
      /// <param name="accepter">Player that accepted the invite.</param>
      /// <returns>True if the requester was invited, otherwise false.</returns>
      public abstract bool InvitePlayer(SocketGuildUser requester, SocketGuildUser accepter);

      /// <summary>
      /// Checks if a user is in the raid.
      /// </summary>
      /// <param name="player">Player to check.</param>
      /// <param name="checkInvite">Should invites be checked.</param>
      /// <returns>Raid Group number if the user is in the raid, otherwise -1.</returns>
      public abstract int IsInRaid(SocketGuildUser player, bool checkInvite = true);

      /// <summary>
      /// Is the RaidParent a single stop.
      /// </summary>
      /// <returns>True if the parent can only have one stop.</returns>
      public bool IsSingleStop()
      {
         return Conductor == null;
      }

      /// <summary>
      /// Check if the current location is the first location.
      /// </summary>
      /// <returns>True if first location, otherwise false.</returns>
      public bool IsFirstLocation()
      {
         return CurrentLocation == 0;
      }

      /// <summary>
      /// Gets the name of the current gym count over the total count.
      /// </summary>
      /// <returns>Current raid count as a string.</returns>
      public string GetCurrentRaidCount()
      {
         return $"{CurrentLocation + 1} / {Locations.Count}";
      }

      /// <summary>
      /// Gets the time for the current raid.
      /// </summary>
      /// <returns>Time of the current raid.</returns>
      public string GetCurrentTime()
      {
         return Locations.ElementAt(CurrentLocation).Time;
      }

      /// <summary>
      /// Gets the location for the current raid.
      /// </summary>
      /// <returns>Location of the current raid.</returns>
      public string GetCurrentLocation()
      {
         return $"{Locations.ElementAt(CurrentLocation).Location}";
      }

      /// <summary>
      /// Gets the name of the current raid boss.
      /// </summary>
      /// <returns>Name of the current raid boss.</returns>
      public string GetCurrentBoss()
      {
         return Locations.ElementAt(CurrentLocation).BossName;
      }

      /// <summary>
      /// Gets the information for the next raid.
      /// </summary>
      /// <returns>Information for the next raid.</returns>
      public string GetNextRaid()
      {
         return (CurrentLocation + 1 == Locations.Count) ? Global.EMPTY_FIELD : $"{Locations.ElementAt(CurrentLocation + 1).Time} at {Locations.ElementAt(CurrentLocation + 1).Location} ({Locations.ElementAt(CurrentLocation + 1).BossName})";
      }

      /// <summary>
      /// Gets information for all raids that are not finished.
      /// </summary>
      /// <returns>List of all incomplete raids.</returns>
      public List<RaidTrainLoc> GetIncompleteRaids()
      {
         return Locations.GetRange(CurrentLocation, Locations.Count - CurrentLocation);
      }

      /// <summary>
      /// Adds a raid to the list of locations.
      /// </summary>
      /// <param name="time">Time of the raid.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="boss">Boss of the raid.</param>
      public void AddRaid(string time, string location, string boss = null)
      {
         Locations.Add(new RaidTrainLoc(time, location, boss ?? Locations.Last().BossName));
      }

      /// <summary>
      /// Update information for a location.
      /// </summary>
      /// <param name="time">New time of the raid.</param>
      /// <param name="location">New location of the raid.</param>
      public void UpdateRaidInformation(string time = null, string location = null)
      {
         if (time != null)
         {
            Locations[CurrentLocation] = new RaidTrainLoc(time, Locations[CurrentLocation].Location, Locations[CurrentLocation].BossName);
         }
         if (location != null)
         {
            Locations[CurrentLocation] = new RaidTrainLoc(Locations[CurrentLocation].Time, location, Locations[CurrentLocation].BossName);
         }
      }

      /// <summary>
      /// Update the name of the boss at the current location.
      /// Selection tier should be updated first to ensure correct
      /// raid boss is selected.
      /// </summary>
      /// <param name="index">Index of the new boss in the tier list.</param>
      public void UpdateBoss(int index)
      {
         string bossName = AllBosses[SelectionTier].ElementAt(index);
         Locations[CurrentLocation] = new RaidTrainLoc(Locations[CurrentLocation].Time, Locations[CurrentLocation].Location, bossName);
         Tier = SelectionTier;
      }

      /// <summary>
      /// Counts forward to the next location.
      /// If the count can be moved all players are reset to attending.
      /// </summary>
      /// <returns>True if the location changed, otherwise false.</returns>
      public bool NextLocation()
      {
         int OldLocation = CurrentLocation;
         if (CurrentLocation < Locations.Count - 1)
         {
            CurrentLocation++;
         }
         if (OldLocation != CurrentLocation)
         {
            foreach (RaidGroup group in Groups)
            {
               group.ResetReady();
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// Counts back to the previous location.
      /// </summary>
      /// <returns>True if the location changed, otherwise false.</returns>
      public bool PreviousLocation()
      {
         int OldLocation = CurrentLocation;
         CurrentLocation = Math.Max(--CurrentLocation, 0);
         return OldLocation != CurrentLocation;
      }

      /// <summary>
      /// Finds the smallest group.
      /// </summary>
      /// <returns>Group number of the smallest group.</returns>
      protected int FindSmallestGroup()
      {
         int minSize = int.MaxValue;
         int minGroup = 0;
         for (int i = 0; i < Groups.Count; i++)
         {
            int groupSize = Groups.ElementAt(i).TotalPlayers();
            if (groupSize < minSize)
            {
               minSize = groupSize;
               minGroup = i;
            }
         }
         return minGroup;
      }

      /// <summary>
      /// Attempts to merge groups.
      /// </summary>
      protected void CheckMergeGroups()
      {
         foreach (RaidGroup group in Groups)
         {
            foreach (RaidGroup check in Groups)
            {
               group.MergeGroup(check);
            }
         }
         Groups.RemoveAll(x => x.TotalPlayers() == 0);
         if (Groups.Count == 0)
         {
            Groups.Add(new RaidGroup(PlayerLimit, InviteLimit));
         }
      }
   }
}