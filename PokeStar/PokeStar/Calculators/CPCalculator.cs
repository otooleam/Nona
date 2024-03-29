﻿using System;
using PokeStar.DataModels;

namespace PokeStar.Calculators
{
   /// <summary>
   /// Calculates CP of a Pokémon.
   /// </summary>
   public static class CPCalculator
   {
      /// <summary>
      /// Calculates the CPM of a half level (X.5).
      /// It is assumed that the level is a half level.
      /// </summary>
      /// <param name="halfLevel">HalfLevel to calculate CPM for.</param>
      /// <returns>CPM for the half level</returns>
      private static double CalcHalfLevelCPM(double halfLevel)
      {
         double lowerCPM = Global.DISCRETE_CPM[(int)(halfLevel - Global.LEVEL_STEP) - 1];
         double upperCPM = Global.DISCRETE_CPM[(int)(halfLevel + Global.LEVEL_STEP) - 1];

         double cpmStep = ((upperCPM * upperCPM) - (lowerCPM * lowerCPM)) / 2;
         return Math.Sqrt((lowerCPM * lowerCPM) + cpmStep);
      }

      /// <summary>
      /// Calculates the CPM for a level.
      /// </summary>
      /// <param name="level">Level to calculate CPM for.</param>
      /// <returns>CPM of the level.</returns>
      private static double CalcCPM(double level)
      {
         if (level % 1.0 != 0)
         {
            return CalcHalfLevelCPM(level);
         }
         else
         {
            return Global.DISCRETE_CPM[(int)(level - 1)];
         }
      }

      /// <summary>
      /// Calculates the attack value for a Pokémon.
      /// </summary>
      /// <param name="attackStat">Attack stat of the Pokémon.</param>
      /// <param name="attackIv">Attack IV of the Pokémon.</param>
      /// <param name="level">Level of the Pokémon.</param>
      /// <returns>Attack value of the Pokémon.</returns>
      private static double CalcAttack(int attackStat, int attackIv, double level)
      {
         return (attackStat + attackIv) * CalcCPM(level);
      }

      /// <summary>
      /// Calculates the defense value for a Pokémon.
      /// </summary>
      /// <param name="defenseStat">Defense stat of the Pokémon.</param>
      /// <param name="defenseIv">Defense IV of the Pokémon.</param>
      /// <param name="level">Level of the Pokémon.</param>
      /// <returns>Defense value of the Pokémon.</returns>
      private static double CalcDefense(int defenseStat, int defenseIv, double level)
      {
         return (defenseStat + defenseIv) * CalcCPM(level);
      }

      /// <summary>
      /// Calculates the stamina (HP) for a pokemon.
      /// </summary>
      /// <param name="staminaStat">Stamina stat of the pokemon.</param>
      /// <param name="staminaIv">Stamina IV of the pokemon.</param>
      /// <param name="level">Level of the pokemon.</param>
      /// <returns>Stamina value of the pokemon.</returns>
      private static double CalcStamina(int staminaStat, int staminaIv, double level)
      {
         return (staminaStat + staminaIv) * CalcCPM(level);
      }

      /// <summary>
      /// Calculates the CP of Pokémon at a given level.
      /// </summary>
      /// <param name="attackStat">Attack stat of the Pokémon.</param>
      /// <param name="defenseStat">Defense stat of the Pokémon.</param>
      /// <param name="staminaStat">Stamina stat of the Pokémon.</param>
      /// <param name="attackIv">Attack IV of the Pokémon.</param>
      /// <param name="defenseIv">Defense IV of the Pokémon.</param>
      /// <param name="staminaIv">Stamina IV of the Pokémon.</param>
      /// <param name="level">Level of the Pokémon.</param>
      /// <returns>CP of the Pokémon.</returns>
      public static int CalcCPPerLevel(int attackStat, int defenseStat, int staminaStat,
                                       int attackIv, int defenseIv, int staminaIv, double level)
      {
         double attack = CalcAttack(attackStat, attackIv, level);
         double defense = CalcDefense(defenseStat, defenseIv, level);
         double stamina = CalcStamina(staminaStat, staminaIv, level);
         return (int)(attack * Math.Sqrt(defense) * Math.Sqrt(stamina) / 10.0);
      }

      /// <summary>
      /// Calculates the rank 1 PvP IVs for a Pokémon in a given league.
      /// </summary>
      /// <param name="attackStat">Attack stat of the Pokémon.</param>
      /// <param name="defenseStat">Defense stat of the Pokémon.</param>
      /// <param name="staminaStat">Stamina stat of the Pokémon.</param>
      /// <param name="leagueCap">Max CP of the league.</param>
      /// <param name="maxLevel">Max level to calculate to.</param>
      /// <returns>LeagueIV object with the best IVs for the league.</returns>
      public static LeagueIV CalcPvPIVsPerLeague(int attackStat, int defenseStat, int staminaStat, int leagueCap, int maxLevel)
      {
         LeagueIV bestIV = new LeagueIV();
         int bestTotal = -1;
         double bestProduct = -1;

         for (double level = 1; level <= maxLevel; level += Global.LEVEL_STEP)
         {
            for (int attack = 0; attack <= Global.MAX_IV; attack++)
            {
               for (int defense = 0; defense <= Global.MAX_IV; defense++)
               {
                  for (int stamina = 0; stamina <= Global.MAX_IV; stamina++)
                  {
                     int calcCP = CalcCPPerLevel(attackStat, defenseStat, staminaStat, attack, defense, stamina, level);
                     int total = attack + defense + stamina;
                     double product = CalcAttack(attackStat, attack, level) * CalcDefense(defenseStat, defense, level) * Math.Floor(CalcStamina(staminaStat, stamina, level));
                     if (calcCP <= leagueCap && (product > bestProduct || (product == bestProduct && total > bestTotal)))
                     {
                        bestIV.Attack = attack;
                        bestIV.Defense = defense;
                        bestIV.Stamina = stamina;
                        bestIV.Level = level;
                        bestIV.CP = calcCP;
                        bestTotal = total;
                        bestProduct = product;
                     }
                  }
               }
            }
         }
         return bestIV;
      }
   }
}