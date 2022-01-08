using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Player's in game information.
   /// </summary>
   public class Profile
   {
      /// <summary>
      /// Current total experiance.
      /// No value set if value is -1.
      /// </summary>
      public int Exp { get; set; } = -1;

      /// <summary>
      /// Trainer friend code.
      /// No value set if value is 0.
      /// </summary>
      public long TrainerCode { get; set; }

      /// <summary>
      /// Trainer referal code.
      /// </summary>
      public string ReferalCode { get; set; }

      /// <summary>
      /// Country typically located in.
      /// </summary>
      public string Location { get; set; }

      /// <summary>
      /// Gets the location and flag as a string.
      /// </summary>
      /// <returns>Location and flag as a string.</returns>
      public string LocationToString()
      {
         if(string.IsNullOrEmpty(Location))
         {
            return Global.EMPTY_FIELD;
         }

         List<RegionInfo> regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(culture => new RegionInfo(culture.LCID)).ToList();
         RegionInfo rInfo = regions.FirstOrDefault(region => region.EnglishName.Equals(Location, StringComparison.OrdinalIgnoreCase));
         string code = string.Concat(rInfo.TwoLetterISORegionName.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
         return $"{code} {rInfo.EnglishName}";
      }

      /// <summary>
      /// Gets and converts Exp to trainer level as a string.
      /// </summary>
      /// <returns>Trainer level as a string.</returns>
      public string GetLevel()
      {
         if (Exp == -1)
         {
            return Global.EMPTY_FIELD;
         }
         for (int i = 0; i < Global.LEVEL_UP_EXP.Length; i++)
         {
            if (Global.LEVEL_UP_EXP[i] == Exp)
            {
               return $"{i + 1} ({string.Format("{0:n0}", Exp)} xp)";
            }
            else if (Global.LEVEL_UP_EXP[i] > Exp)
            {
               return $"{i} ({string.Format("{0:n0}", Exp)} xp)";
            }
         }
         return $"{Global.LEVEL_UP_EXP.Length} ({string.Format("{0:n0}", Exp)} xp)";
      }

      /// <summary>
      /// Gets the trainer code as a string.
      /// </summary>
      /// <returns>Trainer code as a string.</returns>
      public string TrainerCodeToString()
      {
         if (TrainerCode == 0)
         {
            return Global.EMPTY_FIELD;
         }
         string codeStr = TrainerCode.ToString();
         List<string> code = Enumerable.Range(0, codeStr.Length / 4).Select(i => codeStr.Substring(i * 4, 4)).ToList();

         StringBuilder sb = new StringBuilder();
         foreach (string str in code)
         {
            sb.Append($"{str} ");
         }
         return sb.ToString().Trim();
      }
   }
}