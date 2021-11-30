using System;
using System.Security.Cryptography;
using System.Text;

namespace LicentaWebApp.Shared
{
    public static class Utility
    {
        public static string Encode(string password)
        {
            var provider = SHA256.Create();
            const string salt = "N4U7jQcMZEvNvSDExRByizW4WxqMi5kZ";
            var bytes = provider.ComputeHash(Encoding.UTF32.GetBytes(salt + password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}