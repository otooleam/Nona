﻿using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Rocket boss.
   /// </summary>
   public class Rocket
   {
      /// <summary>
      /// Title of a rocket leader.
      /// </summary>
      private readonly string LEADER_TITLE = "Leader";

      /// <summary>
      /// Title of a rocket grunt.
      /// </summary>
      private readonly string GRUNT_TITLE = "Grunt";

      /// <summary>
      /// Name of the rocket.
      /// </summary>
      public string Name { get; private set; }

      /// <summary>
      /// Phrase said by the rocket.
      /// Only set for grunts.
      /// </summary>
      public string Phrase { get; set; } = null;

      /// <summary>
      /// Pokémon used by rockets.
      /// Length is always 3.
      /// </summary>
      public List<string>[] Slots  =
      {
         new List<string>(),
         new List<string>(),
         new List<string>()
      };

      /// <summary>
      /// Sets the name for a rocket leader.
      /// </summary>
      /// <param name="name">Name of the rocket leader.</param>
      public void SetLeader(string name)
      {
         Name = name.Replace(LEADER_TITLE, string.Empty).Replace(GRUNT_TITLE, string.Empty).Trim();
      }

      /// <summary>
      /// Sets the name for a rocket grunt.
      /// </summary>
      /// <param name="type">Type of rocket grunt.</param>
      public void SetGrunt(string type)
      {
         Name = $"{type} {GRUNT_TITLE}";
      }
   }
}