using System.Security.Cryptography;

namespace Cpb.Application.Services;
public class PasswordHasher
{
    public string HashPassword(string password)
    {
        byte[] salt;
        byte[] buffer2;

        using Rfc2898DeriveBytes bytes = new(password, 0x10, 1000, HashAlgorithmName.SHA256);
        salt = bytes.Salt;
        buffer2 = bytes.GetBytes(0x20);
        
        var dst = new byte[0x31];
        Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
        Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
        return Convert.ToBase64String(dst);
    }

    public bool VerifyHashedPassword(string hashedPassword, string password)
    {
        byte[] buffer4;
        if (hashedPassword == null)
            return false;

        if (password == null)
            throw new ArgumentNullException(nameof(password));

        var src = Convert.FromBase64String(hashedPassword);
        if (src.Length != 0x31 || src[0] != 0)
            return false;
        
        var dst = new byte[0x10];
        Buffer.BlockCopy(src, 1, dst, 0, 0x10);
        var buffer3 = new byte[0x20];
        Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);

        using Rfc2898DeriveBytes bytes = new(password, dst, 1000, HashAlgorithmName.SHA256);
        buffer4 = bytes.GetBytes(0x20);

        return buffer3.SequenceEqual(buffer4);
    }
}
