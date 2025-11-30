using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DoAn_CauLong.Payment
{
    public class MomoSecurity
    {
        public string signSHA256(string message, string key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(key);
            string messageCode = "";
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashMessage = hmacsha256.ComputeHash(messageBytes);
                string hex = BitConverter.ToString(hashMessage);
                hex = hex.Replace("-", "").ToLower();
                messageCode = hex;
            }
            return messageCode;
        }
    }
}