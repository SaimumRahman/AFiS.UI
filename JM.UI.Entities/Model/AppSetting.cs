using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Entities.Model
{
    public class AppSetting
    {
        public string BaseUrl { get; set; }
        public string BaseUrlAuth { get; set; }
        public double Timeout { get; set; }
        public string Secret { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string FileUploadPath { get; set; }

        public string DbServer { get; set; }

        public string? ConnectionString { get; set; }

        public string? ProductApiUrl { get; set; }
        public string? ProductApiHashSecret { get; set; }

        public double AccessTokenExpirationMinutes { get; set; }

        public string RefreshTokenSecret { get; set; }

        public double RefreshTokenExpirationMinutes { get; set; }
    }
}
