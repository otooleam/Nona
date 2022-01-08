using System;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;
using Patagames.Ocr;
using Patagames.Ocr.Enums;
using PokeStar.ConnectionInterface;
using PokeStar.DataModels;

namespace PokeStar.ImageProcessors
{
   /// <summary>
   /// Processes an image of a user's role.
   /// </summary>
   public static class RollImageProcess
   {
      /// <summary>
      /// Processes an image of a user's profile page.
      /// Assigns the user's nickname and team.
      /// </summary>
      /// <param name="context">Command context that has the image.</param>
      public static async void RoleImageProcess(SocketCommandContext context)
      {
         IReadOnlyCollection<Discord.Attachment> attachments = context.Message.Attachments;
         SocketGuildUser user = (SocketGuildUser)context.Message.Author;
         if (!Connections.Instance().GetSetupComplete(context.Guild.Id))
         {
            await ResponseMessage.SendWarningMessage(context.Channel, "Roll image proccessing", "Setup has not been completed for this server.");
         }
         else
         {
            string url = attachments.ElementAt(0).Url;
            string imagePath = $@"{Global.PROGRAM_PATH}\Images\profile\{user.Username}.png";
            string plainText = null;
            int colorIndex = -1;
            Color[] teamColors = { Global.ROLE_COLOR_VALOR, Global.ROLE_COLOR_MYSTIC, Global.ROLE_COLOR_INSTINCT };

            using (WebClient client = new WebClient())
            {
               client.DownloadFile(new Uri(url), imagePath);
            }

            using (Image image = Image.FromFile(imagePath))
            {
               using (Bitmap bitmap = ImageProcess.ScaleImage(image, Global.SCALE_WIDTH, Global.SCALE_HEIGHT))
               {
                  using (OcrApi api = OcrApi.Create())
                  {
                     api.Init(Languages.English);
                     plainText = api.GetTextFromImage(bitmap, Global.IMAGE_RECT_NICKNAME);
                  }
                  Color avgColor = GetAvgColor(bitmap, Global.IMAGE_RECT_TEAM_COLOR);
                  colorIndex = ClosestColor(new List<Color>(teamColors), avgColor);
               }
            }

            if (plainText != null)
            {
               int nameEndIndex = Math.Min(plainText.IndexOf('\n'), plainText.IndexOf(' '));
               string nickname = plainText.Substring(0, nameEndIndex);
               try
               {
                  await user.ModifyAsync(x => { x.Nickname = nickname; });

                  await ResponseMessage.SendInfoMessage(context.Channel, $"{user.Username} now has the nickname {nickname}");
               }
               catch (Discord.Net.HttpException e)
               {
                  
                  Console.WriteLine(e.Message);
                  await ResponseMessage.SendWarningMessage(context.Channel, "Roll image proccessing", $"Unable to set nickname for {user.Username} to {nickname}. Please set your nickname to your in game name in \"{context.Guild.Name}\"");
               }
            }

            if (colorIndex == Global.ROLE_INDEX_NO_TEAM_FOUND)
            {
               await ResponseMessage.SendWarningMessage(context.Channel, "Roll image proccessing", $"An error occured while attempting to determine a team for {user.Username}.");
            }
            else
            {
               SocketRole valor = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase));
               SocketRole mystic = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase));
               SocketRole instinct = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase));
               if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase)) != null)
               {
                  await user.RemoveRoleAsync(valor);
               }
               else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase)) != null)
               {
                  await user.RemoveRoleAsync(mystic);
               }
               else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase)) != null)
               {
                  await user.RemoveRoleAsync(instinct);
               }

               string teamName = "";
               if (colorIndex == Global.ROLE_INDEX_VALOR)
               {
                  teamName = Global.ROLE_VALOR;
               }
               else if (colorIndex == Global.ROLE_INDEX_MYSTIC)
               {
                  teamName = Global.ROLE_MYSTIC;
               }
               else if (colorIndex == Global.ROLE_INDEX_INSTINCT)
               {
                  teamName = Global.ROLE_INSTINCT;
               }

               Discord.IRole team = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(teamName, StringComparison.OrdinalIgnoreCase));
               await (user as Discord.IGuildUser).AddRoleAsync(team);

               SocketRole role = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_TRAINER, StringComparison.OrdinalIgnoreCase));
               await (user as Discord.IGuildUser).AddRoleAsync(role);

               await ResponseMessage.SendInfoMessage(context.Channel, $"{user.Username} now has the {Global.ROLE_TRAINER} role and the {teamName} role");
            }
         }
      }

      /// <summary>
      /// Get the average color of a bitmap within a rectangle.
      /// </summary>
      /// <param name="bitmap">Bitmap to get the average color of.</param>
      /// <param name="rect">Rectangle to get average color within.</param>
      /// <returns>Average color of the bitmap within a rectangle.</returns>
      private static Color GetAvgColor(Bitmap bitmap, Rectangle rect)
      {
         int[] avgRGB = { 0, 0, 0 };
         for (int x = rect.X; x < (rect.X + rect.Width); x++)
         {
            for (int y = rect.Y; y < (rect.Y + rect.Height); y++)
            {
               Color c = bitmap.GetPixel(x, y);
               avgRGB[0] += c.R;
               avgRGB[1] += c.G;
               avgRGB[2] += c.B;
            }
         }
         int pixelCount = rect.Width * rect.Height;
         for (int i = 0; i < avgRGB.Length; i++)
         {
            avgRGB[i] /= pixelCount;
         }
         return Color.FromArgb(avgRGB[0], avgRGB[1], avgRGB[2]);
      }

      /// <summary>
      /// Gets the closest color from a list of colors to a given color.
      /// </summary>
      /// <param name="colors">List of colors to check against.</param>
      /// <param name="target">Color to check against list.</param>
      /// <returns>Closest color to the target color.</returns>
      private static int ClosestColor(List<Color> colors, Color target)
      {
         float hue1 = target.GetHue();
         IEnumerable<float> diffs = colors.Select(n => GetHueDistance(n.GetHue(), hue1));
         float diffMin = diffs.Min(n => n);
         return diffs.ToList().FindIndex(n => n == diffMin);
      }

      /// <summary>
      /// Gets the distance between two hue values
      /// </summary>
      /// <param name="hue1">First hue to check</param>
      /// <param name="hue2">Second hue to check.</param>
      /// <returns>Distance from 0-360 between the given hues.</returns>
      private static float GetHueDistance(float hue1, float hue2)
      {
         float d = Math.Abs(hue1 - hue2);
         return d > 180 ? 360 - d : d;
      }
   }
}