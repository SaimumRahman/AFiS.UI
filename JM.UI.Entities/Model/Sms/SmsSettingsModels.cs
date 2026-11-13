using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Entities.Model.Sms
{
    public class SmsSettingsDTO
    {
        public int Id { get; set; } = 1;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Sid { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
        public string ApiLink { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<SmsTemplateDTO> Templates { get; set; } = new();
    }

    public class SmsTemplateDTO
    {
        public int Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string TemplateText { get; set; } = string.Empty;
        public string DefaultText { get; set; } = string.Empty;
        public string AvailableTags { get; set; } = string.Empty;

        public List<string> TagList =>
            string.IsNullOrWhiteSpace(AvailableTags)
                ? new List<string>()
                : AvailableTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public class SmsTestRequestDTO
    {
        public SmsSettingsDTO Settings { get; set; } = new();
        public string MobileNumber { get; set; } = string.Empty;
        public string TestMessage { get; set; } = string.Empty;
    }

    public class SmsSendResultDTO
    {
        public bool Success { get; set; }
        public int HttpStatusCode { get; set; }
        public string ProviderMessage { get; set; } = string.Empty;
    }
}
