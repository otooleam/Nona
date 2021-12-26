using System;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Generic;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Connects to the POGO database.
   /// </summary>
   public class POGODatabaseConnector : DatabaseConnector
   {
      /// <summary>
      /// Creates a new POGO database connector.
      /// </summary>
      /// <param name="connectionString">Connection string for the POGO database.</param>
      public POGODatabaseConnector(string connectionString) : base(connectionString) { }

      /// <summary>
      /// Gets a list of all Pokémon names.
      /// </summary>
      /// <returns>List of Pokémon names.</returns>
      public List<string> GetPokemonNameList()
      {
         List<string> names = new List<string>();
         using (SqlConnection conn = GetConnection())
         {
            string queryString = $@"SELECT Name 
                                    FROM Pokemon
                                    WHERE Name<>'{Global.DUMMY_POKE_NAME}';";
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  names.Add(Convert.ToString(reader["Name"]));
               }
            }
            conn.Close();
         }
         return names;
      }

      /// <summary>
      /// Gets a list of all Move names.
      /// </summary>
      /// <returns>List of Move names.</returns>
      public List<string> GetMoveNameList()
      {
         List<string> names = new List<string>();
         using (SqlConnection conn = GetConnection())
         {
            string queryString = $@"Select Name 
                                    FROM Move;";
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  names.Add(Convert.ToString(reader["Name"]));
               }
            }
            conn.Close();
         }
         return names;
      }

      /// <summary>
      /// Gets list of Pokémon to use in counter sims.
      /// </summary>
      /// <returns>List of Pokémon to use.</returns>
      public List<Pokemon> GetSimPokemon()
      {
         List<Pokemon> pokemon = new List<Pokemon>();
         using (SqlConnection conn = GetConnection())
         {
            string queryString = $@"SELECT Number, Name, Type1, Type2, Attack, Defense, Stamina, IsShadow, IsReleased 
                                    FROM Pokemon
                                    ORDER BY Number;";
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  Pokemon pkmn = new Pokemon
                  {
                     Number = Convert.ToInt32(reader["Number"]),
                     Name = Convert.ToString(reader["Name"]),
                     Attack = Convert.ToInt32(reader["Attack"]),
                     Defense = Convert.ToInt32(reader["Defense"]),
                     Stamina = Convert.ToInt32(reader["Stamina"]),
                     Shadow = Convert.ToInt32(reader["IsShadow"]) == 1,
                     Released = Convert.ToInt32(reader["IsReleased"]) == 1,
                  };
                  pkmn.Type.Add(Convert.ToString(reader["Type1"]));
                  if (reader["Type2"].GetType() != typeof(DBNull))
                  {
                     pkmn.Type.Add(Convert.ToString(reader["Type2"]));
                  }
                  pokemon.Add(pkmn);
               }
            }
            conn.Close();
         }
         return pokemon;
      }

      /// <summary>
      /// Gets a Pokémon by name.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>A Pokémon if the name is in the database, otherwise null.</returns>
      public Pokemon GetPokemon(string pokemonName)
      {
         Pokemon pokemon = null;
         string queryString = $@"SELECT *
                                 FROM Pokemon 
                                 WHERE Name='{pokemonName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  pokemon = new Pokemon
                  {
                     Number = Convert.ToInt32(reader["Number"]),
                     Name = Convert.ToString(reader["Name"]),
                     Description = Convert.ToString(reader["Description"]),
                     Attack = Convert.ToInt32(reader["Attack"]),
                     Defense = Convert.ToInt32(reader["Defense"]),
                     Stamina = Convert.ToInt32(reader["Stamina"]),
                     Region = Convert.ToString(reader["Region"]),
                     Category = Convert.ToString(reader["Category"]),
                     BuddyDistance = Convert.ToInt32(reader["BuddyDistance"]),
                     Shadow = Convert.ToInt32(reader["IsShadow"]) == TRUE,
                     Shiny = Convert.ToInt32(reader["IsShiny"]) == TRUE,
                     Released = Convert.ToInt32(reader["IsReleased"]) == TRUE,
                     Regional = (reader["Regional"].GetType() == typeof(DBNull)) ? null : Convert.ToString(reader["regional"]),
                     CatchRate = Convert.ToDouble(reader["CatchRate"]),
                     FleeRate = Convert.ToDouble(reader["FleeRate"]),
                     SecondMoveCandy = Convert.ToInt32(reader["SecondMoveCandy"]),
                     SecondMoveStardust = Convert.ToInt32(reader["SecondMoveStardust"]),
                     Height = Convert.ToDouble(reader["Height"]),
                     Weight = Convert.ToDouble(reader["Weight"]),
                  };
                  pokemon.Type.Add(Convert.ToString(reader["Type1"]));
                  if (reader["Type2"].GetType() != typeof(DBNull))
                  {
                     pokemon.Type.Add(Convert.ToString(reader["Type2"]));
                  }
               }
            }
            conn.Close();
         }
         return pokemon;
      }

      /// <summary>
      /// Gets a list of Pokémon by number
      /// </summary>
      /// <param name="pokemonNumber">Number of the Pokémon.</param>
      /// <returns>List of all Pokémon with the number.</returns>
      public List<string> GetPokemonByNumber(int pokemonNumber)
      {
         List<string> pokemon = new List<string>();
         string order = (pokemonNumber == Global.ARCEUS_NUMBER || pokemonNumber == Global.UNOWN_NUMBER) ? "ORDER BY NEWID()" : "";

         string queryString = $@"SELECT Name 
                                 FROM Pokemon 
                                 WHERE Number={pokemonNumber}
                                 {order};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  pokemon.Add(Convert.ToString(reader["Name"]));
               }
            }
            conn.Close();
         }
         return pokemon;
      }

      /// <summary>
      /// Gets all weather that boosts the given types.
      /// </summary>
      /// <param name="types">List of types.</param>
      /// <returns>List of weather that boosts the givent types.</returns>
      public List<string> GetWeather(List<string> types)
      {
         List<string> weather = new List<string>();
         string queryString = $@"SELECT Weather 
                                 FROM Weather 
                                 WHERE {GetTypeWhere(types, "Type")}
                                 GROUP BY Weather;";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  if (!weather.Contains(Convert.ToString(reader["Weather"])))
                  {
                     weather.Add(Convert.ToString(reader["Weather"]));
                  }
               }
            }
            conn.Close();
         }
         return weather;
      }

      /// <summary>
      /// Gets defensive type relations for a Pokémon's type.
      /// </summary>
      /// <param name="types">List of the Pokémon's types.</param>
      /// <returns>Dictionary of types and modifiers.</returns>
      public Dictionary<string, int> GetTypeDefenseRelations(List<string> types)
      {
         Dictionary<string, int> relations = new Dictionary<string, int>();
         string queryString = $@"SELECT Attacker, SUM(Modifier) AS TotalRelation 
                                 FROM (
                                 SELECT Attacker, Modifier
                                 FROM TypeMatchUp
                                 WHERE {GetTypeWhere(types, "Defender")}
                                 ) AS Relations
                                 GROUP BY Attacker
                                 HAVING SUM(Modifier) <> 0
                                 ORDER BY TotalRelation;";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  relations.Add(Convert.ToString(reader["Attacker"]), Convert.ToInt32(reader["TotalRelation"]));
               }
            }
            conn.Close();
         }
         return relations;
      }

      /// <summary>
      /// Gets offensive type relations for a move's type.
      /// </summary>
      /// <param name="type">Move type.</param>
      /// <returns>Dictionary of types and modifiers.</returns>
      public Dictionary<string, int> GetTypeAttackRelations(string type)
      {
         Dictionary<string, int> relations = new Dictionary<string, int>();
         string queryString = $@"SELECT Defender, Modifier
                                 FROM TypeMatchUp
                                 WHERE Attacker='{type}'
                                 ORDER BY Modifier;";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  relations.Add(Convert.ToString(reader["Defender"]), Convert.ToInt32(reader["Modifier"]));
               }
            }
            conn.Close();
         }
         return relations;
      }

      /// <summary>
      /// Gets moves of a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="category">Category of moves.</param>
      /// <param name="shadowable">Is the Pokémon shadowable.</param>
      /// <returns>List of moves of the Pokémon.</returns>
      public List<Move> GetPokemonMoves(string pokemonName, string category, bool shadowable = false)
      {
         List<Move> moves = new List<Move>();

         string queryString = $@"SELECT Name, Type, IsLegacy
                                 FROM MoveSet
                                 INNER JOIN Move 
                                 ON MoveSet.Move=Move.Name
                                 WHERE Pokemon='{GetMegaBase(pokemonName)}'
                                 AND Category='{category}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  moves.Add(new Move
                  {
                     Name = Convert.ToString(reader["Name"]),
                     Type = Convert.ToString(reader["Type"]),
                     IsLegacy = Convert.ToInt32(reader["IsLegacy"]) == TRUE
                  });
               }
            }
            conn.Close();
         }
         if (shadowable && category.Equals(Global.CHARGE_MOVE_CATEGORY, StringComparison.OrdinalIgnoreCase))
         {
            moves.AddRange(Global.SHADOW_MOVES);
         }
         return moves;
      }

      /// <summary>
      /// Gets the top counters of a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of counters to a Pokémon.</returns>
      public List<Counter> GetCounters(string pokemonName, bool IsSpecial)
      {
         List<Counter> counters = new List<Counter>();
         int special = IsSpecial ? TRUE : FALSE;
         string queryString = $@"SELECT Counter, Fast, Charge, Value
                                 FROM Counter
                                 WHERE Pokemon='{pokemonName}' 
                                 AND IsSpecial = {special}
                                 ORDER BY Value DESC, Counter;";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  string name = Convert.ToString(reader["Counter"]);
                  Counter counter = new Counter
                  {
                     Name = name.Contains($"{Global.MEGA_TAG} ") || !IsSpecial ? name : $"{Global.SHADOW_TAG} {name}",
                     FastAttack = new Move { Name = Convert.ToString(reader["Fast"]) },
                     ChargeAttack = new Move { Name = Convert.ToString(reader["Charge"]) },
                     Rating = Convert.ToDouble(reader["value"])
                  };
                  counters.Add(counter);
               }
            }
            conn.Close();
         }
         return counters;
      }

      /// <summary>
      /// Gets a move that a Pokémon can learn.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="moveName">Name of the move.</param>
      /// <returns>Move that the pokemon can learn, otherwise null.</returns>
      public Move GetPokemonMove(string pokemonName, string moveName)
      {
         Move move = null;
         string queryString = $@"SELECT Move, Type, IsLegacy
                                 FROM MoveSet
                                 INNER JOIN Move
                                 ON MoveSet.Move=Move.Name
                                 WHERE Pokemon='{GetMegaBase(pokemonName)}' 
                                 AND Move='{moveName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  move = new Move
                  {
                     Name = Convert.ToString(reader["Move"]),
                     Type = Convert.ToString(reader["Type"]),
                     IsLegacy = Convert.ToInt32(reader["IsLegacy"]) == TRUE,
                  };
               }
            }
            conn.Close();
         }
         return move;
      }

      /// <summary>
      /// Gets a Move by name.
      /// </summary>
      /// <param name="moveName">Name of the Move.</param>
      /// <returns>A Move if the name is in the database, otherwise null.</returns>
      public Move GetMove(string moveName)
      {
         Move move = null;
         string queryString = $@"SELECT *
                                 FROM Move 
                                 WHERE Name='{moveName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  move = new Move
                  {
                     Name = Convert.ToString(reader["Name"]),
                     Type = Convert.ToString(reader["Type"]),
                     Category = Convert.ToString(reader["Category"]),
                     PvEPower = Convert.ToInt32(reader["PvePower"]),
                     PvEEnergy = Convert.ToInt32(reader["PveEnergy"]),
                     PvPPower = Convert.ToInt32(reader["PvpPower"]),
                     PvPEnergy = Convert.ToInt32(reader["PvpEnergy"]),
                     PvPTurns = Convert.ToInt32(reader["PvpTurns"]),
                     Cooldown = Convert.ToInt32(reader["PveCooldown"]),
                     DamageWindowStart = Convert.ToInt32(reader["PveDmgWinStart"]),
                     DamageWindowEnd = Convert.ToInt32(reader["PveDmgWinEnd"]),
                     AttackerAtk = Convert.ToInt32(reader["AttackerAtk"]),
                     AttackerDef = Convert.ToInt32(reader["AttackerDef"]),
                     TargetAtk = Convert.ToInt32(reader["TargetAtk"]),
                     TargetDef = Convert.ToInt32(reader["TargetDef"]),
                     BuffChance = Convert.ToDouble(reader["BuffChance"]),
                  };
               }
            }
            conn.Close();
         }
         return move;
      }

      /// <summary>
      /// Gets the number of Pokémon with a given move.
      /// </summary>
      /// <param name="moveName">Name of the move.</param>
      /// <returns>Number of Pokémon that learn the move.</returns>
      public int GetPokemonWithMove(string moveName)
      {
         int count = 0;
         string queryString = $@"SELECT COUNT(*) as Count
                                 FROM MoveSet 
                                 WHERE Move='{moveName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  count = Convert.ToInt32(reader["Count"]);
               }
            }
            conn.Close();
         }
         return count;
      }

      /// <summary>
      /// Gets all moves of a given type.
      /// </summary>
      /// <param name="type">Type of the move.</param>
      /// <param name="category">Category of the move.</param>
      /// <returns>List of moves.</returns>
      public List<string> GetMoveByType(string type, string category)
      {
         List<string> moves = new List<string>();
         string queryString = $@"SELECT Name
                                 FROM Move 
                                 WHERE Type='{type}'
                                 AND Category='{category}'
                                 ORDER BY Category DESC, Name;";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  moves.Add(Convert.ToString(reader["Name"]));
               }
            }
            conn.Close();
         }
         return moves;
      }

      /// <summary>
      /// Get all evolutions a Pokemon is part of.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of evolutions the Pokémon is part of.</returns>
      public List<Evolution> GetEvolutions(string pokemonName)
      {
         List<Evolution> evolutions = new List<Evolution>();
         string queryString = $@"SELECT * 
                                 FROM Evolution 
                                 WHERE StartPokemon='{pokemonName}' 
                                 OR EndPokemon='{pokemonName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  evolutions.Add(new Evolution
                  {
                     Start = Convert.ToString(reader["StartPokemon"]),
                     End = Convert.ToString(reader["EndPokemon"]),
                     Candy = Convert.ToInt32(reader["Candy"]),
                     Item = (reader["Item"].GetType() == typeof(DBNull)) ? null : Convert.ToString(reader["Item"])
                  });
               }
            }
            conn.Close();
         }
         return evolutions;
      }

      /// <summary>
      /// Checks if a Pokemon is the start of its evolutionary family.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokemon.</param>
      /// <returns>True if the Pokemon is not evolved and has an evolution, otherwise false.</returns>
      public bool IsBaseForm(string pokemonName)
      {
         bool isBaseForm = false;
         string queryString = $@"SELECT COUNT(*) AS Base
                                 FROM Evolution
                                 WHERE (StartPokemon='{pokemonName}'
                                 OR EndPokemon='{pokemonName}')
                                 AND '{pokemonName}' NOT IN (
                                 SELECT EndPokemon
                                 FROM Evolution);";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  isBaseForm = Convert.ToInt32(reader["Base"]) == TRUE;
               }
            }
            conn.Close();
         }
         return isBaseForm;
      }

      /// <summary>
      /// Get form tags for a Pokémon.
      /// </summary>
      /// <param name="pokemonName">name of the Pokémon.</param>
      /// <returns>Struct containing tag information.</returns>
      public Form GetFormTags(string pokemonName)
      {
         List<string> formList = new List<string>();
         string default_form = null;
         string queryString = $@"SELECT Tag, IsDefault
                                 FROM Form 
                                 WHERE Base='{pokemonName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  formList.Add(Convert.ToString(reader["tag"]));
                  if (Convert.ToInt32(reader["IsDefault"]) == TRUE)
                  {
                     default_form = Convert.ToString(reader["tag"]);
                  }
               }
            }
            conn.Close();
         }
         return new Form(formList, default_form);
      }

      /// <summary>
      /// Gets the base forms of all Pokémon with form differences.
      /// </summary>
      /// <returns>List of Pokémon names.</returns>
      public List<string> GetPokemonWithTags()
      {
         List<string> pokemon = new List<string>();
         string queryString = $@"SELECT Base 
                                 FROM Form 
                                 GROUP BY Base;";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  pokemon.Add(Convert.ToString(reader["Base"]));
               }
            }
            conn.Close();
         }
         return pokemon;
      }

      /// <summary>
      /// Updates an attribute of a Pokémon.
      /// Only updates true/false values.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="attribute">Attribute to change.</param>
      /// <param name="value">New value of the attribute.</param>
      public void SetPokemonAttribute(string pokemonName, string attribute, int value)
      {
         if (value == TRUE || value == FALSE)
         {
            string queryString = $@"UPDATE Pokemon 
                                 SET {attribute}={value}
                                 WHERE Name='{pokemonName}';";

            using (SqlConnection conn = GetConnection())
            {
               conn.Open();
               _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
               conn.Close();
            }
         }
      }

      /// <summary>
      /// Sets a move for a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="moveName">Name of the move.</param>
      /// <param name="isLegacy">Is the move a legacy move.</param>
      public void SetPokemonMove(string pokemonName, string moveName, int isLegacy)
      {
         if (isLegacy == TRUE || isLegacy == FALSE)
         {
            string queryString = $@"INSERT INTO MoveSet
                                 VALUES ('{pokemonName}', '{moveName}', {isLegacy});";

            using (SqlConnection conn = GetConnection())
            {
               conn.Open();
               _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
               conn.Close();
            }
         }
      }

      /// <summary>
      /// Update the legacy status of a move for a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="moveName">Name of the move.</param>
      /// <param name="isLegacy">Is the move a legacy move.</param>
      public void UpdatePokemonMove(string pokemonName, string moveName, int isLegacy)
      {
         if (isLegacy == TRUE || isLegacy == FALSE)
         {
            string queryString = $@"UPDATE MoveSet
                                 SET IsLegacy={isLegacy}
                                 WHERE Pokemon='{pokemonName}'
                                 AND Move='{moveName}';";

            using (SqlConnection conn = GetConnection())
            {
               conn.Open();
               _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
               conn.Close();
            }
         }
      }

      /// <summary>
      /// Add a counter to a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="counterName">Name of the counter.</param>
      /// <param name="fastName">Name of the counter's fast move.</param>
      /// <param name="chargeName">Name of the counter's charge move.</param>
      /// <param name="rating">Rating value of the counter.</param>
      /// <param name="isSpecial">Is the counter a special counter.</param>
      public void AddCounter(string pokemonName, string counterName, string fastName, string chargeName, double rating, bool isSpecial)
      {
         int special = isSpecial ? TRUE : FALSE;
         string queryString = $@"INSERT INTO Counter
                                 VALUES ('{pokemonName}', '{counterName}', '{fastName}', '{chargeName}', {rating}, {special})";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Remove all counters for a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      public void ClearPokemonCounters(string pokemonName)
      {
         string queryString = $@"DELETE FROM Counter
                                 WHERE Pokemon='{pokemonName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Generates the where string for types.
      /// </summary>
      /// <param name="types">List of pokemon types.</param>
      /// <param name="variableName">Name of the variable to check.</param>
      /// <returns>SQL where string for pokemon types.</returns>
      private static string GetTypeWhere(List<string> types, string variableName)
      {
         StringBuilder sb = new StringBuilder();

         sb.Append($@"({variableName}='{types[0]}'");
         if (types.Count == 2)
         {
            sb.Append($@" OR {variableName}='{types[1]}'");
         }
         sb.Append(')');
         return sb.ToString();
      }

      /// <summary>
      /// Gets the base name of a Mega Pokémon
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Base name of the Mega Pokémon.</returns>
      private static string GetMegaBase(string pokemonName)
      {
         int index = pokemonName.IndexOf(' ');
         string name = pokemonName;
         if (index != -1 && pokemonName.Substring(0, index).Equals(Global.MEGA_TAG, StringComparison.OrdinalIgnoreCase))
         {
            name = pokemonName.Substring(index);
            if (pokemonName.Split(' ').Length == Global.MAX_LEN_MEGA)
            {
               name = name.TrimEnd(name[name.Length - 1]);
            }
         }
         return name.Trim();
      }
   }
}