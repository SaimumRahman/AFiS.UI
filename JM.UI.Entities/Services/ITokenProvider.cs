using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Entities.Services
{
    public interface ITokenProvider
    {
        string GetToken();
        void SetToken(string token);
        void ClearToken();
    }
}
