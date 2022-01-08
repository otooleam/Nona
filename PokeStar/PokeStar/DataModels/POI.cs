using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// In game Point of Interest (POI).
   /// </summary>
   public class POI
   {
      /// <summary>
      /// Name of the POI.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Latitudinal coordinate of the POI.
      /// </summary>
      public string Latitude { get; set; }

      /// <summary>
      /// Longitudinal coordinate of the POI.
      /// </summary>
      public string Longitude { get; set; }

      /// <summary>
      /// Is the POI a gym.
      /// </summary>
      public bool IsGym { get; set; }

      /// <summary>
      /// Is the POI sponsored.
      /// </summary>
      public bool IsSponsored { get; set; }

      /// <summary>
      /// Is the POI an EX raid gym.
      /// This can only be true if IsGum is true.
      /// </summary>
      public bool IsExGym { get; set; }

      /// <summary>
      /// List of registered nicknames.
      /// </summary>
      public List<string> Nicknames { get; set; } = new List<string>();
   }
}