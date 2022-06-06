using System.Security.Cryptography;
using System.Text;

namespace LicentaWebApp.Shared.Utils;

public static class CardCodeGenerator
{
    public static (int[],int[]) GenerateCode(string password)
    {
        var salt = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

#pragma warning disable CA1416
        var engine = new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes(password), salt, 512);
        var generatedBytes = engine.GetBytes(32);
#pragma warning restore CA1416
        
        int[] staticArray = { 101,49,82,115,75,80,111,83,84,31,99,8,70,126,31,40,85,90,109,74,61,49,1,56,15,87,
            36,118,123,31,123,35 };

        var code = new int[32];
        var helper = new int[32];


        for (var i = 0; i < 32; i++)
        {
            helper[i] = generatedBytes[i] % 127;
            code[i] = (helper[i] + staticArray[i]) % 127;
        }

        return (code, helper);
    }
}