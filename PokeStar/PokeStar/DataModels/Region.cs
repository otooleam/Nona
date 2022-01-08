using System;

namespace PokeStar.DataModels
{
   /// <summary>
   /// 
   /// </summary>
   public class Region
   {
      /// <summary>
      /// Total number of Pokémon in the region.
      /// </summary>
      public int Total { get; set; }

      /// <summary>
      /// Number of Pokémon released in the region.
      /// </summary>
      public int Released { get; set; }

      /// <summary>
      /// Number of shiny Pokémon in the region.
      /// </summary>
      public int Shiny { get; set; }

      /// <summary>
      /// Number of shadow Pokémon in the region.
      /// </summary>
      public int Shadow { get; set; }

      /// <summary>
      /// Number of Pokémon with form differences in the region.
      /// </summary>
      public int Form { get; set; }

      /// <summary>
      /// Number of Pokémon released in the region as a string.
      /// </summary>
      /// <returns>Pokémon released string.</returns>
      public string NumReleasedToString()
      {
         return $"{Released} / {Total} ({Math.Round((float)Released / Total * 100.0, 2)}%)";
      }

      /// <summary>
      /// Number of shiny Pokémon in the region as a string.
      /// </summary>
      /// <returns>Shiny Pokémon string.</returns>
      public string NumShinyToString()
      {
         return $"{Shiny} / {Total} ({Math.Round((float)Shiny / Total * 100.0,2)}%)";
      }

      /// <summary>
      /// Number of shadow Pokémon in the region as a string.
      /// </summary>
      /// <returns>Shadow Pokémon string.</returns>
      public string NumShadowToString()
      {
         return $"{Shadow} / {Total} ({Math.Round((float)Shadow / Total * 100.0, 2)}%)";
      }

      /// <summary>
      /// Number of Pokémon with form differences in the region as a string.
      /// </summary>
      /// <returns>Form difference string.</returns>
      public string NumFormToString()
      {
         return $"{Form} / {Total} ({Math.Round((float)Form / Total * 100.0, 2)}%)";
      }
   }
}