﻿
namespace PokeStar.DataModels
{
   /// <summary>
   /// Counter to a Pokémon.
   /// </summary>
   public class Counter
   {
      /// <summary>
      /// Name of the Pokémon.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Recommended Fast move.
      /// </summary>
      public Move FastAttack { get; set; }

      /// <summary>
      /// Recommended charge move.
      /// </summary>
      public Move ChargeAttack { get; set; }

      /// <summary>
      /// Gets the counter to a string.
      /// </summary>
      /// <returns>Counter string.</returns>
      public override string ToString()
      {
         return $@"**{Name}**: {FastAttack} / {ChargeAttack}";
      }
   }
}