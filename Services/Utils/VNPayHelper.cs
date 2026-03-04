using System.Security.Cryptography;
using System.Text;

namespace Services.Utils
{
    public class VNPayHelper
    {
        /// <summary>
        /// Get MD5 Hash for VNPay signature
        /// </summary>
        public static string GetMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Sort dictionary by key
        /// </summary>
        public static string BuildQueryString(SortedDictionary<string, string> data)
        {
            var queryString = new StringBuilder();
            var count = 0;
            foreach (var kv in data)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    if (count > 0) queryString.Append("&");
                    queryString.Append(kv.Key).Append("=").Append(kv.Value);
                    count++;
                }
            }
            return queryString.ToString();
        }

        /// <summary>
        /// Create request data for VNPay payment
        /// </summary>
        public static SortedDictionary<string, string> CreatePaymentRequest(
            string tmn_code,
            string hash_secret,
            string returnUrl,
            int orderid,
            long amount,
            string orderDescription,
            string clientIp)
        {
            var data = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmn_code },
                { "vnp_Amount", (amount * 100).ToString() }, // Amount in smallest unit
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", orderid.ToString() },
                { "vnp_OrderInfo", Uri.EscapeUriString(orderDescription) },
                { "vnp_Locale", "vn" },
                { "vnp_IpAddr", clientIp },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            // Create secure hash
            var queryString = BuildQueryString(data);
            var secureHash = GetMD5Hash(hash_secret + queryString);
            data.Add("vnp_SecureHash", secureHash);

            return data;
        }

        /// <summary>
        /// Verify VNPay response
        /// </summary>
        public static bool ValidateSignature(Dictionary<string, string> responseData, string hashSecret)
        {
            try
            {
                // Get secure hash from response
                var secureHash = responseData.ContainsKey("vnp_SecureHash") 
                    ? responseData["vnp_SecureHash"] 
                    : "";

                // Remove hash from data before validating
                var data = new SortedDictionary<string, string>();
                foreach (var kv in responseData.Where(x => x.Key != "vnp_SecureHash"))
                {
                    data.Add(kv.Key, kv.Value);
                }

                // Recreate hash
                var queryString = BuildQueryString(data);
                var newHash = GetMD5Hash(hashSecret + queryString);

                return secureHash.Equals(newHash, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get response code description
        /// </summary>
        public static string GetResponseCodeDescription(string code)
        {
            return code switch
            {
                "00" => "Giao dịch thành công",
                "01" => "Giao dịch bị từ chối",
                "02" => "Merchant đóng kết nối",
                "04" => "Server không phản hồi",
                "05" => "Giao dịch không thành công",
                "07" => "Trị số không hợp lệ",
                "08" => "Sai checksum",
                "09" => "IP/Tmncode không được phép truy cập",
                "10" => "Không có dữ liệu gửi lên",
                "11" => "Địa chỉ IP không đăng ký",
                "12" => "Sai mật khẩu",
                "13" => "Sai access code",
                "14" => "Sai merchant id",
                "20" => "Hết hạn khoá bảo mật",
                "99" => "Lỗi không xác định",
                _ => "Lỗi không xác định"
            };
        }
    }
}
