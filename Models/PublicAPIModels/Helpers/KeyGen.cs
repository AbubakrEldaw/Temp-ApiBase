using System.Security.Cryptography;

namespace APIBase.PublicAPIModels.Helpers
{
    public class KeyGen
    {


        public static string GenerateKey()
        {
            byte[] randomBytes = new byte[64];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            SHA256 ShaHashFunction = new SHA256Managed();
            byte[] hashedBytes = ShaHashFunction.ComputeHash(randomBytes);
            string randomString = string.Empty;
            foreach (byte b in hashedBytes)
            {
                randomString += string.Format("{0:x2}", b);
            }
            return randomString;
        }
    }
}
