using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Services.Utils
{
    public class VNPayHelper
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        private string GetUrlEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return WebUtility.UrlEncode(value).Replace("+", "%20");
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(GetUrlEncode(kv.Key) + "=" + GetUrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();

            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(data.Length - 1, 1);
            }

            string signData = queryString;
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);

            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(GetUrlEncode(kv.Key) + "=" + GetUrlEncode(kv.Value) + "&");
                }
            }
            string rspRaw = data.ToString();
            if (rspRaw.Length > 0)
            {
                rspRaw = rspRaw.Remove(rspRaw.Length - 1, 1);
            }
            string myChecksum = HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}