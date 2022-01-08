using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles region commands.
   /// </summary>
   public class RegionCommands : DexCommandParent
   {
      /// <summary>
      /// Handle region command.
      /// </summary>
      /// <param name="region">(Optional) Get information about this region.</param>
      /// <returns>Completed Task.</returns>
      [Command("region")]
      [Summary("Gets information for a given region.")]
      [RegisterChannel('D')]
      public async Task Region([Summary("(Optional) Get information about this region.")] string region = null)
      {
         if (region == null)
         {
            Region regionInfo = Connections.Instance().GetRegionInfo();
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < Global.REGION.Count; i++)
            {
               sb.AppendLine($"Gen {i + 1}: {Global.REGION[i]}");
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"All Regions");
            embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
            embed.WithDescription($"{regionInfo.NumReleasedToString()} Pokémon Available in game.");
            embed.AddField("**Shiny Pokémon Available**", regionInfo.NumShinyToString());
            embed.AddField("**Shadow Pokémon Available**", regionInfo.NumShadowToString());
            embed.AddField("**Pokémon With Form Differences**", regionInfo.NumFormToString());
            embed.AddField("**Region Names**", sb.ToString());
            await ReplyAsync(embed: embed.Build());
         }
         else if (!CheckValidRegion(region))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "Region", $"{region} is not a valid region.");
         }
         else
         {
            Region regionInfo = Connections.Instance().GetRegionInfo(region);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"{region.ToUpper()} Region");
            embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
            embed.WithDescription($"{regionInfo.NumReleasedToString()} Pokémon Available in game.");
            embed.AddField("**Shiny Pokémon Available**", regionInfo.NumShinyToString());
            embed.AddField("**Shadow Pokémon Available**", regionInfo.NumShadowToString());
            embed.AddField("**Pokémon With Form Differences**", regionInfo.NumFormToString());
            await ReplyAsync(embed: embed.Build());
         }
      }
   }
}