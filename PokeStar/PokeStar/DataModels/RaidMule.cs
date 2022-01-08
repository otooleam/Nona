using System.Linq;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Invite only raid to fight against a raid boss.
   /// </summary>
   public class RaidMule : RaidParent
   {
      /// <summary>
      /// Group number used for Mule group.
      /// </summary>
      private readonly int MuleGroupNumber = 100;

      /// <summary>
      /// Raid group for raid mules.
      /// </summary>
      public RaidGroup Mules { get; private set; }

      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="conductor">Conductor of the mule train.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidMule(short tier, string time, string location, Player conductor, string boss = null) : 
         base(Global.LIMIT_RAID_MULE_GROUP, Global.LIMIT_RAID_MULE_INVITE, Global.LIMIT_RAID_MULE_INVITE, tier, time, location, conductor, boss)
      {
         Mules = new RaidGroup(Global.LIMIT_RAID_MULE_MULE, 0);
      }

      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidMule(short tier, string time, string location, string boss = null) :
         base(Global.LIMIT_RAID_MULE_GROUP, Global.LIMIT_RAID_MULE_INVITE, Global.LIMIT_RAID_MULE_INVITE, tier, time, location, boss)
      {
         Mules = new RaidGroup(Global.LIMIT_RAID_MULE_MULE, 0);
      }

      /// <summary>
      /// Adds a player to a raid.
      /// The player will not be added if splitting the group brings the number of
      /// raid groups over the group limit.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="partySize">Number of accounts the player is bringing. Should always be 1.</param>
      /// <param name="invitedBy">Who invited the user.</param>
      /// <returns>True if the user was added, otherwise false.</returns>
      public override bool AddPlayer(Player player, int partySize, Player invitedBy = null)
      {
         if (invitedBy == null)
         {
            if (IsInRaid(player) == Global.NOT_IN_RAID && Mules.GetAttendingCount() < Global.LIMIT_RAID_MULE_MULE)
            {
               Mules.AddPlayer(player, partySize, Global.NO_ADD_VALUE);
               return true;
            }
         }
         else // is invite
         {
            int group = FindSmallestGroup();
            Groups.ElementAt(group).InvitePlayer(player, invitedBy);
            Invite.Remove(player);

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

         int groupNum = IsInRaid(player);
         if (groupNum == InviteListNumber)
         {
            Invite.Remove(player);
         }
         else if (groupNum == MuleGroupNumber)
         {
            Mules.RemovePlayer(player);
            foreach (RaidGroup group in Groups)
            {
               returnValue.Users.AddRange(group.RemovePlayer(player));
            }
            foreach (Player invite in returnValue.Users)
            {
               Invite.Add(invite);
            }
            return returnValue;
         }
         else if (groupNum != Global.NOT_IN_RAID)
         {
            RaidGroup foundGroup = Groups.ElementAt(groupNum);
            foundGroup.RemovePlayer(player);
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
         if (Invite.Contains(requester) && Mules.HasPlayer(accepter, false))
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
         if (Mules.HasPlayer(player, false))
         {
            return MuleGroupNumber;
         }
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
      /// Check if any raid group has invited players.
      /// </summary>
      /// <returns>True if atleast 1 group has atleast 1 invited player, otherwise false.</returns>
      public bool HasInvites()
      {
         foreach (RaidGroup group in Groups)
         {
            if (!group.GetReadonlyInvitedAll().IsEmpty)
            {
               return true;
            }
         }
         return false;
      }
   }
}