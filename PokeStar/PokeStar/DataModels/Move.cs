﻿using System;
using Discord;

namespace PokeStar.DataModels
{
   public class Move
   {
      public string Name { get; set; }
      public string Type { get; set; }
      public bool IsLegacy { get; set; }

      public string ToString()
      {
         string typeString = Type;
         if (Environment.GetEnvironmentVariable("SETUP_EMOJI").Equals("TRUE", StringComparison.OrdinalIgnoreCase))
            typeString = Emote.Parse(Environment.GetEnvironmentVariable($"{Type.ToUpper()}_EMOTE")).ToString();
         string str = $@"{Name} {typeString}";
         if (IsLegacy)
            str += " !";
         return str;
      }

   }
}
