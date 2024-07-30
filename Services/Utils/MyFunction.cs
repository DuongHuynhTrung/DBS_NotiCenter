using System;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Services.Utils
{
    public static class MyFunction
    {
        public static string GetId(this ClaimsPrincipal user)
        {
            var idClaim = user.Claims.FirstOrDefault(i => i.Type.Equals("UserId"));
            if (idClaim != null)
            {
                return idClaim.Value;
            }
            return "";
        }

        public static async Task<string> UploadImageAsync(IFormFile file, string path)
        {

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var extension = Path.GetExtension(file.FileName);

            var imageName = DateTime.Now.ToBinary() + Path.GetFileName(file.FileName);

            string filePath = Path.Combine(path, imageName);

            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return filePath.Split("/app/wwwroot/")[1];
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

}

