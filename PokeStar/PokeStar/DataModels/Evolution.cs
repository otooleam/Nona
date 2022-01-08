using System;
using System.Text;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Evolution of a Pokémon.
   /// </summary>
   public class Evolution
   {
      /// <summary>
      /// Initial Pokémon name.
      /// </summary>
      public string Start { get; set; }

      /// <summary>
      /// Final Pokémon name.
      /// </summary>
      public string End { get; set; }

      /// <summary>
      /// Amount of candy used.
      /// </summary>
      public int Candy { get; set; }

      /// <summary>
      /// Item used.
      /// </summary>
      public string Item { get; set; }

      /// <summary>
      /// Other methods used.
      /// </summary>
      public string OtherMethod { get; set; }

      /// <summary>
      /// Checks if two evolutions are the same.
      /// </summary>
      /// <param name="evo">Evolution to check against.</param>
      /// <returns>True if the evolutions are the same, otherwise false.</returns>
      public bool Equals(Evolution evo)
      {
         return evo != null && evo.Candy == Candy &&
            evo.Start.Equals(Start, StringComparison.OrdinalIgnoreCase) && 
            evo.End.Equals(End, StringComparison.OrdinalIgnoreCase);
      }

      /// <summary>
      /// Checks if this is equal to an object.
      /// </summary>
      /// <param name="obj">Object to check against.</param>
      /// <returns>True if the object is equal, otherwise false.</returns>
      public override bool Equals(object obj)
      {
         return obj != null && obj is Evolution && Equals(obj as Evolution);
      }

      /// <summary>
      /// Combines two evolutions together.
      /// This evolutions stays, the other one will
      /// be set to a bad evolution.
      /// </summary>
      /// <param name="evo">Evolution to combine into this one.</param>
      public void Combine(Evolution evo)
      {
         if (evo != null && Candy != 0 && evo.Candy == 0 &&
             evo.Start.Equals(Start, StringComparison.OrdinalIgnoreCase) &&
             evo.End.Equals(End, StringComparison.OrdinalIgnoreCase))
         {
            OtherMethod = evo.Item;
            evo.Candy = Global.BAD_EVOLUTION;
         }
      }

      /// <summary>
      /// Builds the method string for the evolution.
      /// </summary>
      /// <returns>Evolution method as a string.</returns>
      public string MethodToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append($"{Candy}");

         if (Item != null)
         {
            if (Item.Equals("mega", StringComparison.OrdinalIgnoreCase))
            {
               sb.Append(" mega candy");
            }
            else
            {
               sb.Append($" candy and {Item}");
            }
         }
         else
         {
            sb.Append(" candy");
         }
         if (OtherMethod != null)
         {
            sb.Append($" or {OtherMethod}");
         }
         return sb.ToString();
      }
   }
}