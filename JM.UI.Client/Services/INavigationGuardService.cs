namespace JM.UI.Client.Services
{
    public interface INavigationGuardService
    {
        bool IsGuardActive { get; set; }
        Func<Task<bool>>? ConfirmCallback { get; set; }
    }
}
