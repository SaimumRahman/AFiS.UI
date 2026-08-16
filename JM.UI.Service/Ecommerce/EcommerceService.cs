using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Ecommerce;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JM.UI.Service.Ecommerce
{
    public class EcommerceService : IEcommerceService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public EcommerceService(
            IRepositoryUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<EcommerceStoreDTO?> GetEcommerceStore(int? storeId) =>
            await _unitOfWork.EcommerceRepository.GetEcommerceStore(storeId);

        public async Task<IEnumerable<EcommerceItemDTO>> GetEcommerceItems(EcommerceFilterRequestDTO filter) =>
            await _unitOfWork.EcommerceRepository.GetEcommerceItems(filter);

        public async Task<EcommercePostResponseDTO> PostItemToProductApi(
            EcommerceItemDTO item,
            string currentUser,
            string userRole,
            CancellationToken ct = default)
        {
            var response = new EcommercePostResponseDTO();

            try
            {
                var client = _httpClientFactory.CreateClient("ProductApi");

                var product = BuildProductModel(item);
                var logModel = BuildLogModel(item, currentUser, userRole);

                var xHash = ComputeHash(item.Name ?? string.Empty);

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(JsonSerializer.Serialize(product, _jsonOptions)), "Data");
                content.Add(new StringContent(JsonSerializer.Serialize(logModel, _jsonOptions)), "Log");

                var imageFiles = BuildImageContent(item);
                foreach (var fileContent in imageFiles)
                {
                    content.Add(fileContent, "ImageFiles", fileContent.Headers.ContentDisposition?.FileName ?? "file.png");
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, "Demo/BDF/API/v1/Product") { Content = content };
                request.Headers.Add("x-hash", xHash);

                var httpResponse = await client.SendAsync(request, ct);
                var responseBody = await httpResponse.Content.ReadAsStringAsync(ct);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var id = ParseInsertedId(responseBody);
                    response.IsSuccess = true;
                    response.InsertedId = id;
                    response.Message = id.HasValue
                        ? $"Posted to Product API (Id={id})."
                        : "Posted to Product API.";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = $"Product API returned {(int)httpResponse.StatusCode}: {responseBody}";
                }

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Failed to post to Product API: {ex.Message}";
                return response;
            }
        }

        private ProductModel BuildProductModel(EcommerceItemDTO item)
        {
            var vendorId = GetIntConfig("ProductApiDefaultVendorId", 0);

            return new ProductModel
            {
                VendorId = vendorId,
                Title = item.Name ?? string.Empty,
                VendorName = "JHILMIL",
                ColorName = item.ColorName ?? string.Empty,
                SizeName = item.SizeName ?? string.Empty,
                BrandName = item.BrandName ?? string.Empty,
                ProductCode = item.Barcode,
                Price = item.SalePrice,
                PurchaseRate = item.LastCostPrice,
                Description = item.Barcode,
                IsDisplay = true,
                IsFeatured = false,
                IsFlashSale = false,
                Quantity = (int)(item.CurrentStock > 0 ? item.CurrentStock : 0),
                Images = item.ImageBase64 is { Length: > 0 } ? new List<string> { item.ImageBase64 } : new List<string>(),
                CategoryName = item.GroupName ?? string.Empty,
                SubCategoryName = item.SubGroupName ?? string.Empty,
                TitleBangla = item.Name ?? string.Empty
            };
        }

        private LogModel BuildLogModel(EcommerceItemDTO item, string currentUser, string userRole)
        {
            var ip = GetConfig("CurrentIP", "0.0.0.0");

            return new LogModel
            {
                UserName = currentUser,
                UserRole = userRole,
                IP = ip,
                TableName = "EcommerceItems",
                Action = "Create",
                ActionDateTime = DateTime.UtcNow,
                OldData = string.Empty,
                NewData = JsonSerializer.Serialize(item, _jsonOptions)
            };
        }

        private List<StreamContent> BuildImageContent(EcommerceItemDTO item)
        {
            var files = new List<StreamContent>();

            if (string.IsNullOrWhiteSpace(item.ImageBase64))
                return files;

            try
            {
                var imageBytes = Convert.FromBase64String(item.ImageBase64);
                using var ms = new MemoryStream(imageBytes);
                var content = new StreamContent(ms);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = "image.png"
                };
                files.Add(content);
            }
            catch
            {
                // ignored — image will simply be omitted
            }

            return files;
        }

        private string ComputeHash(string title)
        {
            var secret = _configuration["AppSettings:ProductApiHashSecret"] ?? "JHILMIL";
            var payload = $"{secret}{title}";
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private int GetIntConfig(string key, int defaultValue)
        {
            var raw = _configuration[$"AppSettings:{key}"];
            return int.TryParse(raw, out var v) ? v : defaultValue;
        }

        private string GetConfig(string key, string defaultValue)
        {
            var raw = _configuration[$"AppSettings:{key}"];
            return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
        }

        private static int? ParseInsertedId(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                if (root.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var id))
                    return id;
                if (root.TryGetProperty("Id", out idEl) && idEl.TryGetInt32(out id))
                    return id;
            }
            catch
            {
                return null;
            }

            return null;
        }

        public class ProductModel
        {
            public int VendorId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string VendorName { get; set; } = "JHILMIL";
            public string ColorName { get; set; } = string.Empty;
            public string SizeName { get; set; } = string.Empty;
            public string BrandName { get; set; } = string.Empty;
            public string? ProductCode { get; set; }
            public decimal? Price { get; set; }
            public decimal? PurchaseRate { get; set; }
            public string? Description { get; set; }
            public bool IsDisplay { get; set; } = true;
            public bool IsFeatured { get; set; }
            public bool IsFlashSale { get; set; }
            public int Quantity { get; set; }
            public List<string> Images { get; set; } = new();
            public string? CategoryName { get; set; }
            public string? SubCategoryName { get; set; }
            public string? TitleBangla { get; set; }
        }

        public class LogModel
        {
            public string UserName { get; set; } = string.Empty;
            public string UserRole { get; set; } = string.Empty;
            public string IP { get; set; } = "0.0.0.0";
            public string TableName { get; set; } = "EcommerceItems";
            public string Action { get; set; } = "Create";
            public DateTime ActionDateTime { get; set; }
            public string OldData { get; set; } = string.Empty;
            public string NewData { get; set; } = string.Empty;
        }
    }
}