namespace JM.UI.Entities.Model.Accounting_D
{
    /// <summary>
    /// Preview of the account number the server would assign to a new head.
    /// <see cref="Message"/> carries the reason when the number cannot be generated
    /// automatically (parent too deep, group series exhausted, parent inactive...).
    /// </summary>
    public class NextAccountNoDTO
    {
        public string? Code { get; set; }

        /// <summary>False when a manual account number is required.</summary>
        public bool IsAutoAllowed { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
