using APIBase.Utils;
using Newtonsoft.Json;
using APIBase.Utils.Encryption;
using APIBase.Utils.Encryption.Internal;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using APIBase.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace APIBase.Services
{

    public interface IEncMaster
    {
        AppSettings AppSettings { get; }
        RSAKeyPEM RsaKeyPemAPI { get; }
        //AESKey AesKeyPemLocal { get; }
        void init();
        string GetHeaderValue(HttpContext httpContext, string KeyName, bool isIV, string IV = null);
    }
    public class EncMaster : IEncMaster
    {
        public AppSettings AppSettings { set; get; }
        public RSAKeyPEM RsaKeyPemAPI { get; set; }
        public AESKey AesKeyPemLocal { get; set; }
        public EncMaster(AppSettings appSettings)
        {
            this.AppSettings = appSettings;
            init();
        }

        #region methods

        public void init()
        {
            RsaKeyPemAPI = CreateRsa();
            //AesKeyPemLocal = CreateAes();
        }
        private RSAKeyPEM CreateRsa()
        {
            RSAKeyPEM rsakeypem;
            string encPublicKey;
            string encPrivateKey;

            string decPrivateKey;
            string decPublicKey;

            try
            {
                encPublicKey = IOHelper.ReadAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.RSAAPIPublicKeyFileName, this);
                encPrivateKey = IOHelper.ReadAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.RSAAPIPrivateKeyFileName, this);
                if (encPublicKey == null || encPrivateKey == null)
                {
                    var newRSApem = EncryptProvider.RSAToPem(true);
                    rsakeypem = new RSAKeyPEM() { PublicKey = newRSApem.publicPem, PrivateKey = newRSApem.privatePem };

                    encPublicKey = EncryptProvider.AESEncrypt(rsakeypem.PublicKey, AppSettings.Encryptkey);
                    encPrivateKey = EncryptProvider.AESEncrypt(rsakeypem.PrivateKey, AppSettings.Encryptkey);

                    decPublicKey = EncryptProvider.AESDecrypt(JsonConvert.DeserializeObject(encPublicKey).ToString(), AppSettings.Encryptkey);
                    decPrivateKey = EncryptProvider.AESDecrypt(JsonConvert.DeserializeObject(encPrivateKey).ToString(), AppSettings.Encryptkey);


                    IOHelper.CreateOrReplaceAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.RSAAPIPublicKeyFileName, JsonConvert.SerializeObject(encPublicKey), this);
                    IOHelper.CreateOrReplaceAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.RSAAPIPrivateKeyFileName, JsonConvert.SerializeObject(encPrivateKey), this);

                    return rsakeypem;
                }
                else
                {
                    decPublicKey = EncryptProvider.AESDecryptNet6(JsonConvert.DeserializeObject(encPublicKey).ToString(), AppSettings.Encryptkey);
                    decPrivateKey = EncryptProvider.AESDecryptNet6(JsonConvert.DeserializeObject(encPrivateKey).ToString(), AppSettings.Encryptkey);
                    return new RSAKeyPEM() { PublicKey = decPublicKey, PrivateKey = decPrivateKey };
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        //private AESKey CreateAes()
        //{
        //    AESKey rsakeypem;
        //    string encKey;
        //    string encIV;

        //    string decIV;
        //    string decKey;

        //    try
        //    {
        //        encKey = IOHelper.ReadAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.AesKeyFileName, this);
        //        encIV = IOHelper.ReadAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.AesIVFileName, this);
        //        if (encKey == null || encIV == null)
        //        {
        //            var newRSApem = EncryptProvider.CreateAesKey();
        //            rsakeypem = new AESKey() { Key = newRSApem.Key, IV = newRSApem.IV };

        //            encKey = EncryptProvider.AESEncrypt(rsakeypem.Key, AppSettings.Encryptkey);
        //            encIV = EncryptProvider.AESEncrypt(rsakeypem.IV, AppSettings.Encryptkey);
        //            IOHelper.CreateOrReplaceAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.AesKeyFileName, JsonConvert.SerializeObject(encKey), this);
        //            IOHelper.CreateOrReplaceAppDataFileUsingEncryptionKey(Path.Combine(AppSettings.AppDataFolderName, AppSettings.KeysFolderName), AppSettings.AesIVFileName, JsonConvert.SerializeObject(encIV), this);

        //            return rsakeypem;
        //        }
        //        else
        //        {
        //            decKey = EncryptProvider.AESDecrypt(JsonConvert.DeserializeObject(encKey).ToString(), AppSettings.Encryptkey);
        //            decIV = EncryptProvider.AESDecrypt(JsonConvert.DeserializeObject(encIV).ToString(), AppSettings.Encryptkey);
        //            return new AESKey() { Key = decKey, IV = decIV };
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //}

        public string GetHeaderValue(HttpContext httpContext, string KeyName, bool isIV, string IV = null)
        {
            Microsoft.Extensions.Primitives.StringValues encClinetPublicKEY;
            httpContext.Request.Headers.TryGetValue(KeyName, out encClinetPublicKEY);
            string result = "";
            if (encClinetPublicKEY.Count == 1)
            {
                if (isIV)
                {
                    result = EncryptProvider.AESDecrypt(this.AppSettings.Encryptkey, encClinetPublicKEY[0], IV);
                }
                else
                {
                    result = EncryptProvider.RSADecryptWithPem(this.RsaKeyPemAPI.PrivateKey, encClinetPublicKEY[0]);
                }
                return result;
            }
            else
            {
                return null;
            }
        }
        #endregion methods
    }
}