﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Reflection;
using Discord;
using Discord.Commands;
using Patagames.Ocr;
using Patagames.Ocr.Enums;
using System.Collections.Generic;

namespace PokeStar.ImageProcessors
{
   public static class RollImageProcess
   {
      public static async void RoleImageProcess(SocketCommandContext context)
      {
         var attachments = context.Message.Attachments;
         var user = context.Guild.Users.FirstOrDefault(x => x.Username.ToString().Equals(context.Message.Author.Username, StringComparison.OrdinalIgnoreCase));
         string url = attachments.ElementAt(0).Url;
         string imagePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Images\profile\{user.Username}.png";
         string plainText = null;
         System.Drawing.Color teamColor = System.Drawing.Color.White;

         using (WebClient client = new WebClient())
         {
            client.DownloadFile(new Uri(url), imagePath);
         }

         using (System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath))
         {
            using (Bitmap bitmap = ImageProcess.ScaleImage(image, 495, 880))
            {
               using (var api = OcrApi.Create())
               {
                  api.Init(Languages.English);
                  plainText = api.GetTextFromImage(bitmap, new Rectangle(10, 120, 480, 100));
               }
               System.Drawing.Color[] teamColors = { System.Drawing.Color.Red, System.Drawing.Color.Blue, System.Drawing.Color.Yellow };
               System.Drawing.Color avgColor = GetAvgColor(bitmap, new Rectangle(410, 60, 10, 10));
               teamColor = teamColors.ElementAt(ClosestColor(new List<System.Drawing.Color>(teamColors), avgColor));
            }
         }

         if (plainText != null)
         {
            int nameEndIndex = Math.Min(plainText.IndexOf('\n'), plainText.IndexOf(' '));
            try
            {
               string nickname = plainText.Substring(0, nameEndIndex);
               await user.ModifyAsync(x => { x.Nickname = nickname; }).ConfigureAwait(false);

               await user.SendMessageAsync($"{user.Username} now has the nickname {nickname}").ConfigureAwait(false);
            }
            catch(Discord.Net.HttpException e)
            {
               Console.WriteLine(e.Message);
               await user.SendMessageAsync($"Unable to set nickname for {user.Username}. Please set your nickname to your in game name in \"{context.Guild.Name}\"").ConfigureAwait(false);
            }
         }

         if (!teamColor.Equals(System.Drawing.Color.White))
         {
            var valor = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase));
            var mystic = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase));
            var instinct = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase));
            if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase)) == null)
             await user.RemoveRoleAsync(valor).ConfigureAwait(false);
            else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase)) == null)
               await user.RemoveRoleAsync(mystic).ConfigureAwait(false);
            else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase)) == null)
               await user.RemoveRoleAsync(instinct).ConfigureAwait(false);

            string teamName = "";
            if (teamColor.Equals(System.Drawing.Color.Red))
               teamName = "Valor";
            if (teamColor.Equals(System.Drawing.Color.Blue))
               teamName = "Mystic";
            if (teamColor.Equals(System.Drawing.Color.Yellow))
               teamName = "Instinct";

            var team = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(teamName, StringComparison.OrdinalIgnoreCase));
            await user.AddRoleAsync(team).ConfigureAwait(false);

            var role = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase));
            await user.AddRoleAsync(role).ConfigureAwait(false);

            await user.SendMessageAsync($"{user.Username} now has the Trainer role and the {teamName} role").ConfigureAwait(false);
         }
      }

      private static System.Drawing.Color GetAvgColor(Bitmap bitmap, Rectangle rect)
      {
         int[] avgRGB = { 0, 0, 0 };
         for (int x = rect.X; x < (rect.X + rect.Width); x++)
            for (int y = rect.Y; y < (rect.Y + rect.Height); y++)
            {
               System.Drawing.Color c = bitmap.GetPixel(x, y);
               avgRGB[0] += c.R;
               avgRGB[1] += c.G;
               avgRGB[2] += c.B;
            }
         int pixelCount = rect.Width * rect.Height;
         for (int i = 0; i < avgRGB.Length; i++)
            avgRGB[i] /= pixelCount;
         return System.Drawing.Color.FromArgb(avgRGB[0], avgRGB[1], avgRGB[2]);
      }

      private static int ClosestColor(List<System.Drawing.Color> colors, System.Drawing.Color target)
      {
         var hue1 = target.GetHue();
         var diffs = colors.Select(n => GetHueDistance(n.GetHue(), hue1));
         var diffMin = diffs.Min(n => n);
         return diffs.ToList().FindIndex(n => n == diffMin);
      }

      private static float GetHueDistance(float hue1, float hue2)
      {
         float d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
      }
   }
}