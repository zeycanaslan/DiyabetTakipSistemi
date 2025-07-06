using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
public static class AESAlgoritmasi
{
    public static readonly byte[] Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("bu1sabahgizli1key1234567890123456"));
    public static readonly byte[] IV = Encoding.UTF8.GetBytes("bu1baslangicivkey123").Take(16).ToArray();

    public static byte[] Sifrele(string data, byte[] key, byte[] iv)
    {
        using (Aes aesalgoritmasi = Aes.Create())
        {
            aesalgoritmasi.Key = key;
            aesalgoritmasi.IV = iv;
            aesalgoritmasi.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesalgoritmasi.CreateEncryptor(aesalgoritmasi.Key, aesalgoritmasi.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }
                }
                return ms.ToArray();
            }
        }
    }

    public static string SifreCoz(byte[] encryptedData, byte[] key, byte[] iv)
    {
        using (Aes aesalgoritmasi = Aes.Create())
        {
            aesalgoritmasi.Key = key;
            aesalgoritmasi.IV = iv;
            aesalgoritmasi.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesalgoritmasi.CreateDecryptor(aesalgoritmasi.Key, aesalgoritmasi.IV);
            using (MemoryStream ms = new MemoryStream(encryptedData))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
