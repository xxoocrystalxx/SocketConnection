using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CryptoHelper
{
    public static class Crypto
    {
        private static string _cryptoData = "classe5D2021-22";
        public static string Encrypt(string dText)
        {

            byte[] bytes = Encoding.Unicode.GetBytes(dText);
            PasswordDeriveBytes passwordDeriveByte =
                new PasswordDeriveBytes(Crypto._cryptoData,
                    new byte[] { 0x44, 0x6f, 0x74, 0x4e, 0x65, 0x74, 0x57,
                0x6f, 0x72, 0x6b, 0x65, 0x72, 0x73, 0x2e, 0x47,
                0x6f, 0x74, 0x74, 0x61 });
            byte[] encryptedArray = Crypto.Encrypt(bytes, passwordDeriveByte.GetBytes(32), passwordDeriveByte.GetBytes(16));
            return Convert.ToBase64String(encryptedArray);

        }

        private static byte[] Encrypt(byte[] dData, byte[] outerKey, byte[] IV)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Aes key = Aes.Create();
                key.Key = outerKey;
                key.IV = IV;

                using (CryptoStream cryptoStream =
                    new CryptoStream(memoryStream, key.CreateEncryptor(),
                        CryptoStreamMode.Write))
                {
                    cryptoStream.Write(dData, 0, (int)dData.Length);
                    cryptoStream.Close();
                }
                byte[] array = memoryStream.ToArray();
                return array;
            }
        }

        public static string Decrypt(string eText)
        {

            byte[] dataArray = Convert.FromBase64String(eText);
            PasswordDeriveBytes passwordDeriveByte =
                new PasswordDeriveBytes(Crypto._cryptoData,
                    new byte[] { 0x44, 0x6f, 0x74, 0x4e, 0x65, 0x74, 0x57,
                0x6f, 0x72, 0x6b, 0x65, 0x72, 0x73, 0x2e, 0x47, 0x6f,
                0x74, 0x74, 0x61 });
            byte[] hiddenArray = Crypto.Decrypt(dataArray,
                passwordDeriveByte.GetBytes(32), passwordDeriveByte.GetBytes(16));
            return Encoding.Unicode.GetString(hiddenArray);

        }

        private static byte[] Decrypt(byte[] eData, byte[] outerKey, byte[] IV)
        {

            using (MemoryStream memoryStream = new MemoryStream())
            {
                Aes key = Aes.Create();
                key.Key = outerKey;
                key.IV = IV;
                using (CryptoStream cryptoStream =
                    new CryptoStream(memoryStream, key.CreateDecryptor(),
                        CryptoStreamMode.Write))
                {
                    cryptoStream.Write(eData, 0, (int)eData.Length);
                    cryptoStream.Close();
                }
                byte[] array = memoryStream.ToArray();
                return array;
            }
        }

        //cifratura one way
        public static string HashSHA256(string rawData)
        {

            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }

        }

    }
}
