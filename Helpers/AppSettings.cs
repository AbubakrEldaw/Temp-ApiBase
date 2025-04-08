using APIBase.Utils;
using Newtonsoft.Json;
using APIBase.Utils.Encryption;
using APIBase.Utils.Encryption.Internal;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace APIBase.Helpers
{
#nullable disable
    public class AppSettings
    {
        public string Client_DB { get; set; }
        public string SecretKey { get; set; }
        public string Encryptkey { get; set; }
        public string TEncryptkey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int NotBeforeMinutes { get; set; }
        public int ExpirationMinutes { get; set; }
        public int RefreshTokenExpirationMinutes { get; set; }
        public string AppDataFolderName { get; set; }
        public string KeysFolderName { get; set; }
        public string RSAAPIPublicKeyFileName { get; set; }
        public string RSAAPIPrivateKeyFileName { get; set; }
        public string AesKeyFileName { get; set; }
        public string AesIVFileName { get; set; }

        //Forkpos Env Urls
        public string ForkPosProdUrl { get; set; }
        public string ForkPosStagingUrl { get; set; }
        public string ForkPosDevUrl { get; set; }

        //Deliverect Env Urls
        public string DeliverectHMACkey { get; set; }
        public string DeliverectProdUrl { get; set; }
        public string DeliverectStagingUrl { get; set; }

        //Jahez Env Urls
        public string JahezProdUrl { get; set; }
        public string JahezStagingUrl { get; set; }

        //SmartLive Env Urls
        public string SmartLiveProdUrl { get; set; }
        public string SmartLiveStagingUrl { get; set; }

        //MicrosoftDBC Env Urls
        public string MicrosoftDBCProdUrl { get; set; }
        public string MicrosoftDBCStagingUrl { get; set; }

        //Taxizer Env Urls
        public string TaxizerProdUrl { get; set; }
        public string TaxizerStagingUrl { get; set; }

        //Xero Env Urls
        public string XeroProdUrl { get; set; }
        public string XeroStagingUrl { get; set; }

        //Koinz Env Urls
        public string KoinzProdUrl { get; set; }
        public string KoinzStagingUrl { get; set; }

        //Foodizone Env Urls
        public string FoodizoneProdUrl { get; set; }
        public string FoodizoneStagingUrl { get; set; }

    }
}