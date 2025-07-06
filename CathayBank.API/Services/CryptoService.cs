using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CathayBank.API.Services
{
    public class CryptoSettings
    {
        public string Key { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
    }

    public interface ICryptoService
    {
        /// <summary>
        /// 將字串使用 AES 加密
        /// </summary>
        /// <param name="plainText">要加密的明文</param>
        /// <returns>Base64 編碼的加密後字串</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// 將 AES 加密的字串解密
        /// </summary>
        /// <param name="cipherText">Base64 編碼的加密字串</param>
        /// <returns>解密後的明文</returns>
        string Decrypt(string cipherText);
    }

    /// <summary>
    /// 使用 AES-256 加解密
    /// </summary>
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public CryptoService(IOptions<CryptoSettings> cryptoSettings)
        {
            _key = Encoding.UTF8.GetBytes(cryptoSettings.Value.Key);
            _iv = Encoding.UTF8.GetBytes(cryptoSettings.Value.IV);
        }

        public string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        var encrypted = msEncrypt.ToArray();
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
