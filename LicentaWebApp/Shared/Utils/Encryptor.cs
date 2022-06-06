using System.Text;

namespace LicentaWebApp.Shared.Utils;
using System.Security.Cryptography;

public static class Encryptor
{
    public static void DecryptFile(string filepath,string password)
    {
        var fileContent = System.IO.File.ReadAllBytes(filepath);
        var salt = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        var iv = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        var k1 = new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes(password), salt, 512);

        var encKey = k1.GetBytes(32);
        var result = AesEncryptor.DecryptStringFromBytes_Aes(fileContent, encKey, iv);
        System.IO.File.WriteAllText(filepath,result);
    }
        
    public static void EncryptFile(string filepath,string password)
    {
        var fileContent = System.IO.File.ReadAllText(filepath);
        var salt = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        var iv = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        var k1 = new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes(password), salt, 512);

        var encKey = k1.GetBytes(32);
        var result = AesEncryptor.EncryptStringToBytes_Aes(fileContent, encKey, iv);
        System.IO.File.WriteAllBytes(filepath,result);
    }
}