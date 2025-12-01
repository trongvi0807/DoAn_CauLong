using DoAn_CauLong.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions; // Cần thêm thư viện này để xử lý chuỗi "dưới 3 triệu"
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DoAn_CauLong.Controllers
{
    public class ChatbotController : Controller
    {
        private QLDN_CAULONGEntities db = new QLDN_CAULONGEntities();
        private readonly string apiKey = "AIzaSyD1f2fdpA7kkGB2YnZ5lQqhXlJmQkDTvaI";

        [HttpPost]
        public async Task<ActionResult> Ask(string message)
        {
            try
            {
                // Kích hoạt bảo mật TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string messageLower = message.ToLower();
                decimal? maxPrice = null;

                // --- BƯỚC 1: PHÂN TÍCH GIÁ TỪ CÂU HỎI (REGEX) ---
                // Tìm các cụm từ như: "dưới 3 triệu", "dưới 3tr", "tầm 3 triệu"
                var priceMatch = Regex.Match(messageLower, @"(dưới|tầm|khoảng)\s*(\d+)\s*(triệu|tr)");
                if (priceMatch.Success)
                {
                    // Lấy con số (ví dụ: 3)
                    if (decimal.TryParse(priceMatch.Groups[2].Value, out decimal priceNumber))
                    {
                        maxPrice = priceNumber * 1000000; // Đổi ra tiền VNĐ (3 -> 3.000.000)
                    }
                }

                // --- BƯỚC 2: TẠO TRUY VẤN LỌC DỮ LIỆU ---

                // Lấy tất cả sản phẩm ra memory (hoặc dùng IQueryable nếu muốn tối ưu SQL)
                var query = db.SanPhams.AsEnumerable();

                // A. LỌC THEO GIÁ (Nếu khách có yêu cầu giá)
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.GiaGoc.HasValue && p.GiaGoc.Value <= maxPrice.Value);
                }

                // B. LỌC THEO TỪ KHÓA
                // Loại bỏ các từ thừa như "dưới", "triệu", "muốn", "mua" để tìm tên chính xác hơn
                string[] stopWords = { "dưới", "trên", "triệu", "tr", "đồng", "muốn", "tìm", "mua", "cần" };
                var keywords = messageLower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Where(w => !stopWords.Contains(w) && !Regex.IsMatch(w, @"^\d+$")) // Bỏ luôn số
                                           .ToArray();

                if (keywords.Length > 0)
                {
                    // Chỉ lấy sản phẩm chứa ít nhất 1 từ khóa quan trọng (ví dụ: "vợt", "yonex")
                    query = query.Where(p => keywords.Any(k => p.TenSanPham.ToLower().Contains(k)));
                }

                // C. CHỌN 5 SẢN PHẨM PHÙ HỢP NHẤT
                var products = query.Take(5)
                                    .Select(p => new
                                    {
                                        Ten = p.TenSanPham,
                                        Gia = p.GiaGoc,
                                        // Kiểm tra null cho collection trước khi Sum để tránh lỗi
                                        TonKho = p.ChiTietSanPhams != null ? p.ChiTietSanPhams.Sum(ct => ct.SoLuongTon) : 0,
                                        MoTa = p.MoTa
                                    }).ToList();

                // Lấy thông tin khuyến mãi
                string promoInfo = "";
                if (messageLower.Contains("khuyến mãi") || messageLower.Contains("giảm giá"))
                {
                    var promos = db.KhuyenMais.Where(k => k.NgayKetThuc >= DateTime.Now).ToList();
                    promoInfo = "Chương trình khuyến mãi: " + string.Join(", ", promos.Select(k => k.TenChuongTrinh + " giảm " + k.PhanTramGiam + "%"));
                }

                // --- BƯỚC 3: TẠO CONTEXT CHO GEMINI ---
                string contextData = "Hệ thống không tìm thấy sản phẩm nào khớp với tiêu chí lọc.";
                if (products.Count > 0)
                {
                    contextData = $"Dữ liệu sản phẩm {(maxPrice.HasValue ? $"dưới {maxPrice:N0} VNĐ" : "")} tìm thấy:\n";
                    foreach (var p in products)
                    {
                        contextData += $"- {p.Ten}: {p.Gia:N0} VNĐ (Tồn: {p.TonKho})\n";
                    }
                }
                contextData += "\n" + promoInfo;

                // --- BƯỚC 4: GỌI API GEMINI ---
                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"Bạn là nhân viên tư vấn shop cầu lông. \nDữ liệu đã lọc từ hệ thống: \n{contextData}\n\nKhách hỏi: {message}\n\nYêu cầu: Trả lời dựa trên dữ liệu trên. Nếu có sản phẩm phù hợp, hãy liệt kê ra kèm giá. Nếu không có, hãy khéo léo giới thiệu sản phẩm khác hoặc mời liên hệ hotline. Trả lời ngắn gọn, tiếng Việt." }
                            }
                        }
                    }
                };

                using (var client = new HttpClient())
                {
                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Sử dụng Model 2.5 Flash như code cũ của bạn
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, reply = $"Lỗi Google API ({response.StatusCode}): {responseString}" });
                    }

                    JObject jsonResponse = JObject.Parse(responseString);
                    string botReply = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                    return Json(new { success = true, reply = botReply ?? "Xin lỗi, tôi không tìm thấy thông tin phù hợp." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, reply = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}