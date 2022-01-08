using System;
using System.Linq;
using Discord;
using PokeStar.Calculators;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Simulates catching Pokémon.
   /// </summary>
   public class CatchSimulation
   {
      /// <summary>
      /// Pokémon to simulate catching.
      /// </summary>
      public Pokemon Pokemon { get; private set; }

      /// <summary>
      /// Chance to catch the Pokémon.
      /// </summary>
      public double CatchChance { get; private set; }

      /// <summary>
      /// Custom radius value.
      /// </summary>
      public double CustomRadius { get; private set; }

      /// <summary>
      /// Current modifer index.
      /// </summary>
      private int CurrentModifier;

      /// <summary>
      /// Array of modifer indices.
      /// </summary>
      private readonly int[] Modifiers = new int[Global.MODIFIER_STATS.Count];

      /// <summary>
      /// Modifer list index.
      /// </summary>
      private enum MODIFIER_INDEX
      {
         LEVEL,
         BALL,
         BERRY,
         THROW,
         CURVEBALL,
         MEDAL_1,
         MEDAL_2,
         ENCOUNTER,
      };

      /// <summary>
      /// Creates a new CatchSimulation.
      /// </summary>
      /// <param name="pokemon">Pokemon to simulate catching.</param>
      public CatchSimulation(Pokemon pokemon)
      {
         Pokemon = pokemon;
         CurrentModifier = (int)MODIFIER_INDEX.LEVEL;
         CustomRadius = 0;
         CalcCatchChance();
      }

      /// <summary>
      /// Gets the name of the current modifier to edit.
      /// </summary>
      /// <returns>Name of the current modifier.</returns>
      public string GetCurrentModifier()
      {
         return Global.MODIFIER_STATS.Keys.ElementAt(CurrentModifier);
      }

      /// <summary>
      /// Get level text.
      /// </summary>
      /// <returns>Level as a string.</returns>
      public string GetLevel()
      {
         return $"{Modifiers[(int)MODIFIER_INDEX.LEVEL] + 1}";
      }

      /// <summary>
      /// Get Pokéball text.
      /// </summary>
      /// <returns>Pokéball as a string.</returns>
      public string GetBall()
      {
         return Global.POKE_BALL_RATE.Keys.ElementAt(Modifiers[(int)MODIFIER_INDEX.BALL]);
      }

      /// <summary>
      /// Get berry text.
      /// </summary>
      /// <returns>Berry as a string.</returns>
      public string GetBerry()
      {
         return Global.BERRY_RATE.Keys.ElementAt(Modifiers[(int)MODIFIER_INDEX.BERRY]);
      }

      /// <summary>
      /// Get throw text.
      /// Will display custom radius if set.
      /// </summary>
      /// <returns>Throw radius as a string.</returns>
      public string GetThrow()
      {
         return CustomRadius == 0 ? Global.THROW_RATE.Keys.ElementAt(Modifiers[(int)MODIFIER_INDEX.THROW]) : $"Custom ({CustomRadius})";
      }

      /// <summary>
      /// Get curveball text.
      /// </summary>
      /// <returns>Curveball as a string.</returns>
      public string GetCurveball()
      {
         return Global.CURVEBALL_RATE.Keys.ElementAt(Modifiers[(int)MODIFIER_INDEX.CURVEBALL]);
      }

      /// <summary>
      /// Get medal 1 text.
      /// </summary>
      /// <returns>Medal 1 as a string.</returns>
      public string GetMedal1()
      {
         return Global.MEDAL_RATE.Keys.ElementAt(Modifiers[(int)MODIFIER_INDEX.MEDAL_1]);
      }

      /// <summary>
      /// Get medal 2 text.
      /// </summary>
      /// <returns>Medal 2 as a string.</returns>
      public string GetMedal2()
      {
         return Global.MEDAL_RATE.Keys.ElementAt(Modifiers[(int)MODIFIER_INDEX.MEDAL_2]);
      }

      /// <summary>
      /// Get encounter text.
      /// </summary>
      /// <returns>Encounter type as a string.</returns>
      public string GetEncounter()
      {
         return Global.ENCOUNTER_RATE.Keys.ElementAt(Modifiers[(int)MODIFIER_INDEX.ENCOUNTER]);
      }

      /// <summary>
      /// Set a custom level.
      /// </summary>
      /// <param name="newLevel">New level value.</param>
      /// <returns>True if the custom value is set, otherwise false.</returns>
      public bool SetCustomLevel(int newLevel)
      {
         if (newLevel >= Global.MIN_WILD_LEVEL && newLevel <= Global.MAX_WILD_LEVEL)
         {
            Modifiers[(int)MODIFIER_INDEX.LEVEL] = newLevel - 1;
            CalcCatchChance();
            return true;
         }
         return false;
      }

      /// <summary>
      /// Set a custom throw radius.
      /// </summary>
      /// <param name="newRadius">New radius value.</param>
      /// <returns>True if the custom value is set, otherwise false.</returns>
      public bool SetCustomRadius(double newRadius)
      {
         if (newRadius >= 1.0 && newRadius <= 2.0)
         {
            CustomRadius = Math.Round(newRadius, 2);
            CalcCatchChance();
            return true;
         }
         return false;
      }

      /// <summary>
      /// Cycles the modifier.
      /// Getting to the end of the list, the counter restarts.
      /// </summary>
      public void UpdateModifier()
      {
         if (CurrentModifier == Modifiers.Length - 1)
         {
            CurrentModifier = 0;
         }
         else
         {
            CurrentModifier++;
            if (CurrentModifier == (int)MODIFIER_INDEX.MEDAL_2 && Pokemon.Type.Count == 1)
            {
               CurrentModifier++;
            }
         }
      }

      /// <summary>
      /// Increment the current modifier.
      /// </summary>
      public void IncrementModifierValue()
      {
         if (Modifiers[CurrentModifier] == Global.MODIFIER_STATS.Values.ElementAt(CurrentModifier))
         {
            Modifiers[CurrentModifier] = 0;
         }
         else
         {
            Modifiers[CurrentModifier]++;
         }

         if (CurrentModifier == (int)MODIFIER_INDEX.THROW)
         {
            CustomRadius = 0;
         }
         CalcCatchChance();
      }

      /// <summary>
      /// Decrement the current modifier.
      /// </summary>
      public void DecrementModifierValue()
      {
         if (Modifiers[CurrentModifier] == 0)
         {
            Modifiers[CurrentModifier] = Global.MODIFIER_STATS.Values.ElementAt(CurrentModifier);
         }
         else
         {
            Modifiers[CurrentModifier]--;
         }

         if (CurrentModifier == (int)MODIFIER_INDEX.THROW)
         {
            CustomRadius = 0;
         }
         CalcCatchChance();
      }

      /// <summary>
      /// Calculates the color of the catch ring.
      /// </summary>
      /// <returns>Expected catch ring color.</returns>
      public Color CalcRingColor()
      {
         if (CatchChance > 50.0)
         {
            uint offset = 255 - (uint)((CatchChance - 50.0) / 50.0 * 255.0);
            return new Color(Global.MAX_CATCH_COLOR | (offset << 16));
         }
         else if (CatchChance < 50.0)
         {
            uint offset = 255 - (uint)((50.0 - CatchChance) / 50.0 * 255.0);
            return new Color(Global.MIN_CATCH_COLOR | (offset << 8));
         }
         else
         {
            return new Color(Global.MID_CATCH_COLOR);
         }
      }

      /// <summary>
      /// Calculates the chance the pokemon will be caught.
      /// </summary>
      private void CalcCatchChance()
      {
         double medal = Global.MEDAL_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.MEDAL_1]).Value;
         if (Pokemon.Type.Count != 1)
         {
            medal = CatchCalculator.CalculateMultiMedalMultiplier(
               Global.MEDAL_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.MEDAL_1]).Value,
               Global.MEDAL_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.MEDAL_2]).Value
            );
         }

         CatchChance = Math.Round(Math.Max(0.0, Math.Min(100.0,
            CatchCalculator.CalcCatchChance(
               Pokemon.CatchRate,
               Modifiers[(int)MODIFIER_INDEX.LEVEL] + 1,
               Global.POKE_BALL_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.BALL]).Value,
               Global.BERRY_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.BERRY]).Value,
               CustomRadius == 0 ? Global.THROW_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.THROW]).Value : CustomRadius,
               Global.CURVEBALL_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.CURVEBALL]).Value,
               medal,
               Global.ENCOUNTER_RATE.ElementAt(Modifiers[(int)MODIFIER_INDEX.ENCOUNTER]).Value
            ) * 100.0
         )), 2);
      }
   }
}