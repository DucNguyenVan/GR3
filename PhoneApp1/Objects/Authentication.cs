using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneApp1.Objects
{
    class Authentication
    {
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string Token { get; set; }

        //This will cause the application to request a token
        public Authentication(string publicKey, string secretKey)
        {
            PublicKey = publicKey;
            SecretKey = secretKey;
            Token = string.Empty;
        }

        //This will cause the application to request a token
        public Authentication(string publicKey, string secretKey, string token)
        {
            PublicKey = publicKey;
            SecretKey = secretKey;
            Token = token;
        }
    }
}
