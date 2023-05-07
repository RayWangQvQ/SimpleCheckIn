using System.Security.Cryptography;
using System.Text;

namespace SimpleCheckIn.Ikuuu.Helpers
{
    public static class EncryptionHelper
    {
        #region DES Algorithm
        public static string DESToEncrypt(this string source, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(source)) { return ""; }
            byte[] inputByteArray = Encoding.UTF8.GetBytes(source);
            //var dCSP = new DESCryptoServiceProvider();
            var dCSP = DES.Create();
            var mStream = new MemoryStream();
            var cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }
        public static string DESToEncrypt(this string source, string encryptionKey)
        {
            byte[] rgbKey = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] rgbIV = Encoding.UTF8.GetBytes(encryptionKey);
            var result = source.DESToEncrypt(rgbKey, rgbIV);
            return result;
        }

        public static string DESToDecrypt(this string source, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(source)) { return ""; }
            byte[] inputByteArray = Convert.FromBase64String(source);
#pragma warning disable S5547 // Cipher algorithms should be robust
            //var DCSP = new DESCryptoServiceProvider();
            var DCSP = DES.Create();
#pragma warning restore S5547 // Cipher algorithms should be robust
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(mStream.ToArray());
        }
        public static string DESToDecrypt(this string source, string encryptionKey)
        {
            byte[] rgbKey = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] rgbIV = Encoding.UTF8.GetBytes(encryptionKey);
            var result = source.DESToDecrypt(rgbKey, rgbIV);
            return result;
        }
        #endregion

        #region AES Algorithm
        public static string AESToEncrypt(this string source, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(source)) { return ""; }
            var inputByteArray = Encoding.UTF8.GetBytes(source);
            //var serviceProvider = new AesCryptoServiceProvider();
            var serviceProvider = Aes.Create();
            var encryptor = serviceProvider.CreateEncryptor(rgbKey, rgbIV);
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }

        public static string AESToEncrypt(this string source, string encryptionKey)
        {
            var rgbKey = Encoding.UTF8.GetBytes(encryptionKey);
            var rgbIV = Encoding.UTF8.GetBytes(encryptionKey);
            var result = source.AESToEncrypt(rgbKey, rgbIV);
            return result;
        }

        public static string AESToEncrypt(this string source, string encryptionKey, string encryptionIV)
        {
            var encryptKey = Encoding.UTF8.GetBytes(encryptionKey);

            using var aesAlg = Aes.Create();
            var iv = Encoding.UTF8.GetBytes(encryptionIV);
            using var encryptor = aesAlg.CreateEncryptor(encryptKey, iv);
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(source);
            }

            var decryptedContent = msEncrypt.ToArray();

            return Convert.ToBase64String(decryptedContent);
        }


        public static string AESToDecrypt(this string source, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(source)) { return ""; }
            var inputByteArray = Encoding.UTF8.GetBytes(source);
            //var serviceProvider = new AesCryptoServiceProvider();
            var serviceProvider = Aes.Create();
            var encryptor = serviceProvider.CreateDecryptor(rgbKey, rgbIV);
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }

        public static string AESToDecrypt(this string source, string encryptionKey)
        {
            var rgbKey = Encoding.UTF8.GetBytes(encryptionKey);
            var rgbIV = Encoding.UTF8.GetBytes(encryptionKey);
            var result = source.AESToDecrypt(rgbKey, rgbIV);
            return result;
        }

        public static string AESToDecryptForJAVA(this string cipherText, string encryptionKey)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (encryptionKey == null || encryptionKey.Length <= 0)
                throw new ArgumentNullException(nameof(encryptionKey));

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = string.Empty;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(encryptionKey);
                aesAlg.Mode = CipherMode.ECB;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor();

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
        #endregion

        #region RSA Algorithm
        public static string RSAToEncrypt(this string source, string publicEcnyptionKey)
        {
            throw new NotImplementedException();
        }


        public static string RSAToDecrypt(this string source, string privateEcnyptionKey)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region TripleDES Algorithm
        private static string TripleDESToEncrypt(this string source, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(source)) { return ""; }
            var inputByteArray = Encoding.UTF8.GetBytes(source);
            //var serviceProvider = new TripleDESCryptoServiceProvider();
            var serviceProvider = TripleDES.Create();
            serviceProvider.Mode = CipherMode.ECB;
            var encryptor = serviceProvider.CreateEncryptor(rgbKey, rgbIV);
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());

        }

        private static string TripleDESToDecrypt(this string source, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(source)) { return ""; }
            var inputByteArray = Encoding.UTF8.GetBytes(source);
            //var serviceProvider = new TripleDESCryptoServiceProvider();
            var serviceProvider = TripleDES.Create();
            var encryptor = serviceProvider.CreateDecryptor(rgbKey, rgbIV);
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }
        #endregion
    }
}
