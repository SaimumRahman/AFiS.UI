using JM.UI.Client.Services;
using Microsoft.AspNetCore.Components;

namespace JM.UIWeb.CustomBase
{
    public class AddEditPageBase : PosComponentBase, IDisposable
    {
        private bool _guardInitialized;

        public override Task SetParametersAsync(ParameterView parameters)
        {
            if (!_guardInitialized)
            {
                _guardInitialized = true;
                NavigationGuard.IsGuardActive = true;
            }
            return base.SetParametersAsync(parameters);
        }

        public void Dispose()
        {
            NavigationGuard.IsGuardActive = false;
        }
    }
}
