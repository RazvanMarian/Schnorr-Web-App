using System;

namespace LicentaWebApp.Shared;

public static class OTPGenerator
{
    
    public static string GenerateOTP()
    {
        
        const string characters = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        

        const int length = 6;
        var otp = string.Empty;
        for (var i = 0; i < length; i++)
        {
            string character;
            do
            {
                var index = new Random().Next(0, characters.Length);
                character = characters.ToCharArray()[index].ToString();
            } while (otp.IndexOf(character, StringComparison.Ordinal) != -1);
            otp += character;
        }

        return otp;
    }

}