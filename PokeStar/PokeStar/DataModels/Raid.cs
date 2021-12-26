using System.Linq;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Raid to fight against a raid boss.
   /// </summary>
   public class Raid : RaidParent
   {
      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="conductor">Conductor of the raid train.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public Raid(short tier, string time, string location, Player conductor, string boss = null) : 
         base(Global.LIMIT_RAID_GROUP, Global.LIMIT_RAID_PLAYER, Global.LIMIT_RAID_INVITE, tier, time, location, conductor, boss) { }

      /// <summary>
      /// Creates a new single stop raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public Raid(short tier, string time, string location, string boss = null) :
         base(Global.LIMIT_RAID_GROUP, Global.LIMIT_RAID_PLAYER, Global.LIMIT_RAID_INVITE, tier, time, location, boss) { }

      /// <summary>
      /// Adds a player to a raid.
      /// The player will not be added if splitting the group brings the number of
      /// raid groups over the group limit.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="partySize">Number of accounts the player is bringing.</param>
      /// <param name="invitedBy">Who invited the player.</param>
      /// <returns>True if the player was added, otherwise false.</returns>
      public override bool AddPlayer(Player player, int partySize, Player invitedBy = null)
      {
         int group;
         if (invitedBy == null) // Add in person
         {
            group = IsInRaid(player);
            if (group == Global.NOT_IN_RAID)
            {
               group = FindSmallestGroup();
            }
            if (group != InviteListNumber)
            {
               Groups.ElementAt(group).AddPlayer(player, partySize, Global.NO_ADD_VALUE);
            }
            else
            {
               return false;
            }
         }
         else if (player.Equals(invitedBy)) // Remote
         {
            group = IsInRaid(player);
            if (group == Global.NOT_IN_RAID)
            {
               group = FindSmallestGroup();
            }
            if (group != InviteListNumber)
            {
               Groups.ElementAt(group).AddPlayer(player, Global.NO_ADD_VALUE, partySize);
            }
            else
            {
               return false;
            }
         }
         else // accept invite
         {
            group = IsInRaid(invitedBy);
            if (group != Global.NOT_IN_RAID)
            {
               Groups.ElementAt(group).InvitePlayer(player, invitedBy);
               Invite.Remove(player);
            }
            else if (player.Equals(invitedBy))
            {
               group = FindSmallestGroup();
               Groups.ElementAt(group).InvitePlayer(player, invitedBy);
            }
            else
            {
               return false;
            }
         }

         bool shouldSplit = Groups.ElementAt(group).ShouldSplit();

         if (shouldSplit && Groups.Count < RaidGroupLimit)
         {
            RaidGroup newGroup = Groups.ElementAt(group).SplitGroup();
            Groups.Add(newGroup);
            CheckMergeGroups();
            return true;
         }
         else if (!shouldSplit)
         {
            CheckMergeGroups();
            return true;
         }

         Groups.ElementAt(group).RemovePlayer(player);
         if (invitedBy != null)
         {
            Invite.Add(player);
         }
         return false;
      }

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>RaidRemove with raid group and list of invited users.</returns>
      public override RaidRemoveResult RemovePlayer(Player player)
      {
         RaidRemoveResult returnValue = new RaidRemoveResult(Global.NOT_IN_RAID, new List<Player>());

         int group = IsInRaid(player);
         if (group == InviteListNumber)
         {
            Invite.Remove(player);
         }
         else
         {
            if (group != Global.NOT_IN_RAID)
            {
               RaidGroup foundGroup = Groups.ElementAt(group);
               List<Player> tempList = foundGroup.RemovePlayer(player);
               foreach (Player invite in tempList)
               {
                  returnValue.Users.Add(invite);
                  Invite.Add(invite);
               }
            }
         }
         return returnValue;
      }

      /// <summary>
      /// Requests an invite to a raid for a player.
      /// </summary>
      /// <param name="player">Player that requested the invite.</param>
      public override void RequestInvite(Player player)
      {
         if (IsInRaid(player) == Global.NOT_IN_RAID)
         {
            Invite.Add(player);
         }
      }

      /// <summary>
      /// Accepts an invite of a player.
      /// </summary>
      /// <param name="requester">Player that requested the invite.</param>
      /// <param name="accepter">Player that accepted the invite.</param>
      /// <returns>True if the requester was invited, otherwise false.</returns>
      public override bool InvitePlayer(Player requester, Player accepter)
      {
         if ((IsInRaid(requester) == InviteListNumber && IsInRaid(accepter, false) != Global.NOT_IN_RAID))
         {
            return AddPlayer(requester, 1, accepter);
         }
         return false;
      }

      /// <summary>
      /// Checks if a player is in the raid.
      /// This does not check the raid request invite list.
      /// </summary>
      /// <param name="player">Player to check.</param>
      /// <param name="checkInvite">If invited players should be checked.</param>
      /// <returns>Group number the player is in, else NotInRaid.</returns>
      public override int IsInRaid(Player player, bool checkInvite = true)
      {
         if (checkInvite && Invite.Contains(player))
         {
            return InviteListNumber;
         }
         for (int i = 0; i < Groups.Count; i++)
         {
            if (Groups.ElementAt(i).HasPlayer(player, checkInvite))
            {
               return i;
            }
         }
         return Global.NOT_IN_RAID;
      }

      /// <summary>
      /// Removes a player if their party size is zero.
      /// Any players invited by them are moved back to requesting invite.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>Return a dictionary of all users invited by the player.</returns>
      public Dictionary<Player, List<Player>> ClearEmptyPlayer(Player player)
      {
         int group = IsInRaid(player, false);
         if (group != Global.NOT_IN_RAID)
         {
            Dictionary<Player, List<Player>> empty = Groups.ElementAt(group).ClearEmptyPlayers();
            foreach (KeyValuePair<Player, List<Player>> user in empty)
            {
               Invite.AddRange(user.Value);
            }
            return empty;
         }
         return new Dictionary<Player, List<Player>>();
      }

      /// <summary>
      /// Marks a player as ready in the raid.
      /// </summary>
      /// <param name="player">Player to mark ready.</param>
      /// <returns>Group number the player is in, else NotInRaid.</returns>
      public int MarkPlayerReady(Player player)
      {
         int groupNum = IsInRaid(player, false);
         if (groupNum != Global.NOT_IN_RAID && groupNum != InviteListNumber)
         {
            RaidGroup group = Groups.ElementAt(groupNum);
            return (group.MarkPlayerReady(player) && group.AllPlayersReady()) ? groupNum : Global.NOT_IN_RAID;
         }
         return Global.NOT_IN_RAID;
      }

      /// <summary>
      /// Checks if all participants in the raid are ready.
      /// Checks all raid groups.
      /// </summary>
      /// <returns>True if all players in all raid groups are ready, otherwise false.</returns>
      public bool AllReady()
      {
         bool allReady = true;
         foreach (RaidGroup group in Groups)
         {
            allReady = allReady && group.AllPlayersReady();
         }
         return allReady;
      }
   }
}