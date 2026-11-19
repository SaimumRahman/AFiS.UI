using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Entities.Model.MembershipType;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.CustomerDetails
{
    public partial class CustomerDetailsAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public DialogService DialogService { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected CustomerDetailsDTO Customer { get; set; } = new();
        protected List<MembershipTypeDTO> MembershipTypes { get; set; } = new();
        protected List<CustomerCompanyDTO> CompanyResults { get; set; } = new();
        protected List<CustomerDetailsDTO> FoundCustomers { get; set; } = new();

        protected bool IsLoading { get; set; } = false;
        protected bool IsProcessing { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected bool IsAdmin { get; set; } = false;
        protected int CurrentUserId { get; set; } = 0;
        protected bool ShowForm { get; set; } = false;
        protected bool LookupDone { get; set; } = false;
        protected string LookupMobile { get; set; } = string.Empty;
        protected string LookupMobileInput { get; set; } = string.Empty;
        protected string CompanyText { get; set; } = string.Empty;
        protected bool SameAsMobile { get; set; } = true;
        protected string SavedMsg { get; set; } = string.Empty;
        protected string PageTitle => IsEditMode ? "Edit Customer" : "Add New Customer";
        protected string PageIcon => IsEditMode ? "edit" : "person_add";

        // Admin remarks (admin only) - stored as a single column; we keep a list for display.
        protected List<(string Text, string By, DateTime At)> PendingRemarks { get; set; } = new();
        protected string RemarkInput { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            NavigationGuard.IsGuardActive = true;
            await TokenService.InitializeTokenAsync();
            await LoadUserContext();

            MembershipTypes = (await _serviceUnitOfWork.MembershipTypeService.GetAll()).ToList();

            if (IsEditMode)
            {
                await LoadCustomer();
                ShowForm = true;
            }
            else
            {
                Customer = _serviceUnitOfWork.CustomerDetailsService.CreateNew();
                if (Id == null) { /* new */ }
            }
        }

        private async Task LoadUserContext()
        {
            try
            {
                var uid = await sessionStorage.GetAsync<string>("UserId");
                if (!string.IsNullOrEmpty(uid.Value) && int.TryParse(uid.Value, out var parsed)) CurrentUserId = parsed;
                IsAdmin = await _serviceUnitOfWork.UserGroupService.GetSystemGroupCountByUserId(CurrentUserId) > 0;
            }
            catch { IsAdmin = false; }
        }

        private async Task LoadCustomer()
        {
            try
            {
                IsLoading = true;
                var customer = await _serviceUnitOfWork.CustomerDetailsService.GetCustomerById(Id!.Value);
                if (customer == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Customer not found.");
                    NavigationManager.NavigateTo("/CustomerDetailsList");
                    return;
                }
                Customer = customer;
                CompanyText = customer.CompanyName ?? string.Empty;
                ShowForm = true;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load customer: {ex.Message}");
                NavigationManager.NavigateTo("/CustomerDetailsList");
            }
            finally { IsLoading = false; }
        }

        // ---------- Step 1: mobile lookup ----------
        protected async Task RunLookup()
        {
            var m = NormalizeMobile(LookupMobileInput);
            if (!ValidMobile(m))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", "Enter a valid mobile number (e.g. 01711000000).");
                return;
            }
            LookupMobile = m;
            LookupMobileInput = m;
            FoundCustomers = (await _serviceUnitOfWork.CustomerDetailsService.GetCustomersByMobile(m)).ToList();
            LookupDone = true;
            StateHasChanged();
        }

        protected async Task OpenNewForm()
        {
            // Same mobile can hold multiple customers, but only after explicit confirmation.
            if (!string.IsNullOrWhiteSpace(LookupMobile))
            {
                var existing = (await _serviceUnitOfWork.CustomerDetailsService.GetCustomersByMobile(LookupMobile)).ToList();
                if (existing.Count > 0)
                {
                    var names = string.Join(", ", existing.Take(5).Select(c => $"{c.Name} (#{c.Id})"));
                    var confirmed = await DialogService.Confirm(
                        $"Mobile {LookupMobile} is already registered to: {names}. " +
                        $"Multiple customers can share the same mobile number. Do you want to add another customer with this number?",
                        "Mobile Number Already Registered",
                        new ConfirmOptions
                        {
                            OkButtonText = "Yes, Add Another",
                            CancelButtonText = "No",
                            CloseDialogOnEsc = true
                        });
                    if (confirmed != true) return;
                }
            }

            Customer = _serviceUnitOfWork.CustomerDetailsService.CreateNew();
            Customer.Phone = LookupMobile;
            Customer.WhatsApp = LookupMobile;
            Customer.IsForceAdd = true;
            LookupDone = false;
            ShowForm = true;
            StateHasChanged();
        }

        protected async Task OpenEditForm(CustomerDetailsDTO c)
        {
            Customer = await _serviceUnitOfWork.CustomerDetailsService.GetCustomerById(c.Id) ?? c;
            CompanyText = Customer.CompanyName ?? string.Empty;
            LookupDone = false;
            ShowForm = true;
            StateHasChanged();
        }

        // ---------- Company search ----------
        protected async Task OnCompanyInput(string value)
        {
            CompanyText = value;
            Customer.CompanyName = value;
            Customer.CompanyId = null;
            if (string.IsNullOrWhiteSpace(value)) { CompanyResults = new List<CustomerCompanyDTO>(); return; }
            CompanyResults = (await _serviceUnitOfWork.CustomerDetailsService.SearchCompanies(value)).ToList();
            StateHasChanged();
        }

        protected void SelectCompany(CustomerCompanyDTO c)
        {
            Customer.CompanyId = c.Id;
            Customer.CompanyName = c.Name;
            Customer.CompanyCode = c.Code;
            CompanyText = c.Name;
            CompanyResults = new List<CustomerCompanyDTO>();
            if (c.DefaultCategoryId.HasValue)
            {
                Customer.MemberTypeId = c.DefaultCategoryId;
                notificationService.Notify(NotificationSeverity.Info, "Info", $"Membership category auto-selected from company. You can change it.");
            }
            StateHasChanged();
        }

        protected async Task RegisterCompany()
        {
            var name = (CompanyText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", "Type a company name first.");
                return;
            }
            var code = SuggestCode(name);
            var newCompany = new CustomerCompanyDTO { Name = name, Code = code };
            var confirm = await DialogService.OpenAsync<CustomerCompanyRegisterDialog>(
                "Register New Company",
                new Dictionary<string, object> { { "Company", newCompany } });
            if (confirm is CustomerCompanyDTO saved && saved.Id > 0)
            {
                Customer.CompanyId = saved.Id;
                Customer.CompanyName = saved.Name;
                Customer.CompanyCode = saved.Code;
                CompanyText = saved.Name;
                if (saved.DefaultCategoryId.HasValue) Customer.MemberTypeId = saved.DefaultCategoryId;
                StateHasChanged();
            }
            else if (confirm is CustomerCompanyDTO saved2)
            {
                // fallback: try create directly
                var res = await _serviceUnitOfWork.CustomerDetailsService.CreateCompany(saved2);
                if (res.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", "Company saved.");
                }
            }
        }

        // ---------- Save ----------
        protected async Task Save()
        {
            await DoSave(false);
        }

        protected async Task SaveAndNew()
        {
            await DoSave(true);
        }

        private async Task DoSave(bool andNew)
        {
            // normalize mobile
            Customer.Phone = NormalizeMobile(Customer.Phone);
            if (!string.IsNullOrWhiteSpace(Customer.WhatsApp)) Customer.WhatsApp = NormalizeMobile(Customer.WhatsApp);

            // company must resolve if text typed
            if (!string.IsNullOrWhiteSpace(CompanyText) && !Customer.CompanyId.HasValue)
            {
                var matches = await _serviceUnitOfWork.CustomerDetailsService.SearchCompanies(CompanyText);
                var exact = matches.FirstOrDefault(c => c.Name.Equals(CompanyText, StringComparison.OrdinalIgnoreCase));
                if (exact != null) SelectCompany(exact);
                else
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Validation", "This company is not registered yet. Please register it or clear the field.");
                    await RegisterCompany();
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(CompanyText)) { Customer.CompanyId = null; Customer.CompanyName = null; Customer.CompanyCode = null; }

            var validation = await _serviceUnitOfWork.CustomerDetailsService.Validate(Customer);
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            // duplicate soft-warning: the user already confirmed at form open that this
            // mobile may hold multiple customers, so do not ask a second time.
            if (Customer.Id == 0)
            {
                var existing = await _serviceUnitOfWork.CustomerDetailsService.GetCustomersByMobile(Customer.Phone);
                var dup = existing.Any(c => c.Name.Equals(Customer.Name, StringComparison.OrdinalIgnoreCase));
                if (dup) Customer.IsForceAdd = true;
            }

            // remarks (admin): append to Remarks column (simplest: prepend new entries)
            if (IsAdmin && PendingRemarks.Count > 0)
            {
                var existing = Customer.Remarks ?? string.Empty;
                var appended = string.Join("\n", PendingRemarks.Select(r => $"[{r.At:dd/MM/yyyy HH:mm}] {r.By}: {r.Text}"));
                Customer.Remarks = string.IsNullOrWhiteSpace(existing) ? appended : appended + "\n" + existing;
                PendingRemarks.Clear();
            }

            if (Customer.Id > 0 && CurrentUserId > 0)
                Customer.UpdateBy = CurrentUserId;

            IsProcessing = true;
            try
            {
                var result = await _serviceUnitOfWork.CustomerDetailsService.InsertUpdateCustomer(Customer);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        Customer.Id == 0 ? "Customer created successfully!" : "Customer updated successfully!");
                    if (andNew)
                    {
                        Customer = _serviceUnitOfWork.CustomerDetailsService.CreateNew();
                        Customer.Phone = LookupMobile;
                        Customer.WhatsApp = LookupMobile;
                        CompanyText = string.Empty;
                        CompanyResults = new List<CustomerCompanyDTO>();
                        StateHasChanged();
                    }
                    else
                    {
                        NavigationManager.NavigateTo("/CustomerDetailsList");
                    }
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save customer: {ex.Message}");
            }
            finally { IsProcessing = false; }
        }

        protected void AddRemark()
        {
            if (!IsAdmin) return;
            var text = (RemarkInput ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) return;
            PendingRemarks.Insert(0, (text, "Admin", DateTime.Now));
            RemarkInput = string.Empty;
            StateHasChanged();
        }

        protected void SyncWhatsApp()
        {
            if (Customer == null) return;
            // tracked via a separate flag; keep simple: if WhatsApp equals phone, treat as same
        }

        // ---------- helpers ----------
        protected string NormalizeMobile(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var d = new string(s.Where(char.IsDigit).ToArray());
            if (d.StartsWith("880")) d = "0" + d.Substring(3);
            return d;
        }

        protected bool ValidMobile(string m) => System.Text.RegularExpressions.Regex.IsMatch(m ?? string.Empty, @"^01[3-9]\d{8}$");

        protected string SuggestCode(string name)
        {
            var words = (name ?? string.Empty).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var code = new string(words.Select(w => w[0]).ToArray()).ToUpperInvariant();
            code = new string(code.Where(char.IsLetterOrDigit).ToArray());
            return code.Length > 6 ? code.Substring(0, 6) : (code.Length < 2 ? "XX" : code);
        }

        protected async Task Cancel() => NavigationManager.NavigateTo("/CustomerDetailsList");

        protected async Task Reset()
        {
            if (IsEditMode) await LoadCustomer();
            else { Customer = _serviceUnitOfWork.CustomerDetailsService.CreateNew(); CompanyText = string.Empty; CompanyResults = new List<CustomerCompanyDTO>(); }
            StateHasChanged();
        }

        public void Dispose()
        {
            NavigationGuard.IsGuardActive = false;
        }
    }
}
