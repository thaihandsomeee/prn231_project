using System.Security.Cryptography;
using System.Text;

namespace MyRazorPage.Common
{
    public static class HashPassword
    {
        public static string encryptPassword(String secretKey, String password)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(secretKey);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using(MemoryStream memoryStream = new MemoryStream())
                {
                    using(CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream
                                                                       , encryptor
                                                                       , CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(password);
                        }

                        array = memoryStream.ToArray();
                    }
                }

            }
            return Convert.ToBase64String(array);
        }

        public static string DecryptString(String secretKey, String password)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(password);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(secretKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream
                                                                        , decryptor
                                                                        , CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
