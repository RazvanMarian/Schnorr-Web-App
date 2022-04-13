namespace LicentaWebApp.Shared.PayloadModels;

public class VerifyFilePayload
{
    public string FileHash { get; set; }
    
    public byte[] SignatureContent { get; set; }
}