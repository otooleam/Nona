﻿using System;
using PokeStar.DataModels;

namespace PokeStar.Calculators
{
   /// <summary>
   /// Calculates type multipliers.
   /// </summary>
   public static class TypeCalculator
   {
      /// <summary>
      /// Calculate the type effectivness from the multipler.
      /// </summary>
      /// <param name="multiplier">Effectivness multiplier.</param>
      /// <returns>Effectivness of the type.</returns>
      public static double CalcTypeEffectivness(int multiplier)
      {
         double effectivness = Global.TYPE_COEFFICIENT;
         for (int i = 1; i < Math.Abs(multiplier); i++)
         {
            effectivness *= Global.TYPE_COEFFICIENT;
         }
         if (multiplier < 0)
         {
            effectivness = 1.0 / effectivness;
         }
         return effectivness;
      }

      /// <summary>
      /// Gets multiplier for specific type from relations.
      /// </summary>
      /// <param name="types">Type relations.</param>
      /// <param name="type">Type to find.</param>
      /// <returns>Multiplier of the type relationship.</returns>
      public static double GetMultiplier(TypeRelation types, string type)
      {
         if (types.Strong.ContainsKey(type))
         {
            return types.Strong[type];
         }
         else if (types.Weak.ContainsKey(type))
         {
            return types.Weak[type];
         }
         else
         {
            return 1.0;
         }
      }
   }
}