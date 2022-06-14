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
        
        int[] key = { 101,49,82,115,75,80,111,83,84,31,99,8,70,126,31,40,85,90,109,74,61,49,1,56,15,87,
            36,118,123,31,123,35 };

        
        var helper = new int[32];
        for (var i = 0; i < 32; i++)
        {
            helper[i] = generatedBytes[i] % 127;
        }
        
        const int blocksize = 64;
        const int hashsize = 20;
        var code = new int[hashsize];
        var hmacBuffer = new byte[128];

        //xor cu 0x36
        for(var i = 0; i< key.Length;i++)
        {
            hmacBuffer[i] = (byte)(key[i] ^ 0x36);
        }

        //fill array cu 0
        for(var i = key.Length; i < blocksize-key.Length;i++)
        {
            hmacBuffer[i] = 0;
        }

        //copiere mesaj
        for (var i = 0; i < helper.Length; i++)
        {
            hmacBuffer[i+blocksize] = (byte)helper[i];
        }

        var sha1 = SHA1.Create();
        var firstHash = sha1.ComputeHash(hmacBuffer,0,blocksize+helper.Length);

        for(var i = blocksize;i<blocksize+hashsize;i++)
        {
            hmacBuffer[i] = firstHash[i-blocksize];
        }

        for (var i = 0; i < key.Length; i++)
        {
            hmacBuffer[i] = (byte)(key[i] ^ 0x5c);
        }

        //fill array cu 0
        for (var i = key.Length; i < blocksize - key.Length; i++)
        {
            hmacBuffer[i] = 0;
        }

        var result = sha1.ComputeHash(hmacBuffer, 0, blocksize + hashsize);

        for (var i = 0; i < hashsize;i++)
        {
            if(result[i]>127)
            {
                result[i] -= 129;
            }
        }

        for (var i = 0; i < hashsize; i++)
        {
            code[i] = result[i];
        }


        return (code, helper);
    }
}