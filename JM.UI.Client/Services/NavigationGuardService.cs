namespace JM.UI.Client.Services
{
    public class NavigationGuardService : INavigationGuardService
    {
        public bool IsGuardActive { get; set; }
        public Func<Task<bool>>? ConfirmCallback { get; set; }
    }
}
