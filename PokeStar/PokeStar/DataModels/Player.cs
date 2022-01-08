using Discord.WebSocket;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Wrapper class for SocketGuildUser.
   /// </summary>
   public class Player
   {
      /// <summary>
      /// Wrapped SocketGuildUser object.
      /// </summary>
      public SocketGuildUser SocketPlayer { get; private set; }

      /// <summary>
      /// Creates a new Player.
      /// </summary>
      /// <param name="socketPlayer">SocketGuildUser object to wrap.</param>
      public Player(SocketGuildUser socketPlayer)
      {
         SocketPlayer = socketPlayer;
      }

      /// <summary>
      /// Checks if two players are the same.
      /// </summary>
      /// <param name="player">Player to check against.</param>
      /// <returns>True if the players are the same, otherwise false.</returns>
      public bool Equals(Player player)
      {
         return player != null && SocketPlayer.Id == player.SocketPlayer.Id;
      }

      /// <summary>
      /// Checks if this is equal to an object.
      /// </summary>
      /// <param name="obj">Object to check against.</param>
      /// <returns>True if the object is equal, otherwise false.</returns>
      public override bool Equals(object obj)
      {
         return obj != null && obj is Player && Equals(obj as Player);
      }

      /// <summary>
      /// Gets hash code for the player.
      /// Id is converted to an int.
      /// </summary>
      /// <returns>Hash code of the Player.</returns>
      public override int GetHashCode()
      {
         return (int)SocketPlayer.Id;
      }
   }
}