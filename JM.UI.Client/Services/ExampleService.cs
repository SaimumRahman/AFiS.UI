using JM.UI.Entities.Models;
namespace JM.UI.Client.Services
{
    public class ExampleService
    {
        private readonly Example[] allExamples = new[]
        {
           
            // 1. Dashboard / HomeA
           
            new Example
            {
                Name = "Dashboard",
                Path = "/",
                Icon = "\ue88a"  // home
            },

           
            // 2. Company & Organization Setup
           
            new Example
            {
                Name = "Company Setup",
                Icon = "\ue7f1",  // business / domain
                Children = new[]
                {
                    new Example
                    {
                        Name = "Companies",
                        Path = "/CompanyList",
                        Icon = "\ue7f1",
                        Title = "Companies",
                        Description = "Manage company profiles and basic settings",
                        Tags = new[] { "company", "setup", "organization" }
                    },
                    new Example
                    {
                        Name = "Branches / Stores",
                        Path = "/StoreList",
                        Icon = "\ue8f4",  // store
                        Title = "Branches",
                        Description = "Manage branches, stores or locations",
                        Tags = new[] { "branch", "store", "location" }
                    }
                }
            },

           
            // 3. Human Resources
           
            new Example
            {
                Name = "Human Resources",
                Icon = "\ue7fd",  // people / group
                Children = new[]
                {
                    new Example
                    {
                        Name = "Employees",
                        Path = "/EmployeeList",
                        Icon = "\ue7fd",
                        Title = "Employees",
                        Description = "Manage employee records, profiles and assignments",
                        Tags = new[] { "employee", "hr", "staff" }
                    },
                    new Example
                    {
                        Name = "Designations",
                        Path = "/DesignationList",
                        Icon = "\ue749",
                        Title = "Designations",
                        Description = "Define job titles and designations",
                        Tags = new[] { "designation", "job title", "role" }
                    },
                    new Example
                    {
                        Name = "Shifts",
                        Path = "/ShiftList",
                        Icon = "\ue8d6",  // schedule
                        Title = "Work Shifts",
                        Description = "Manage shift patterns and schedules",
                        Tags = new[] { "shift", "schedule", "roster" }
                    }
                }
            },
            new Example
            {
                Name = "Stock Report",
                Icon = "\ue7fd",  // people / group
                Children = new[]
                {
                    new Example
                    {
                        Name = "Current Stock",
                        Path = "/CurrentStockReport",
                        Icon = "\ue7fd",
                        Title = "Employees",
                        Description = "Manage employee records, profiles and assignments",
                        Tags = new[] { "employee", "hr", "staff" }
                    }
                }
            },

           
            // 4. Chart of Accounts & Finance Setup
           
            new Example
            {
                Name = "Accounting Setup",
                Icon = "\ue8b0",  // account balance
                Children = new[]
                {
                    new Example
                    {
                        Name = "Account Groups",
                        Path = "/AccountsGroupsList",
                        Icon = "\ue8b0",
                        Title = "Account Groups",
                        Description = "Organize accounts into categories and groups",
                        Tags = new[] { "account", "group", "finance", "coa" }
                    },
                    new Example
                    {
                        Name = "Accounts",
                        Path = "/AccountsList",
                        Icon = "\ue85d",
                        Title = "Chart of Accounts",
                        Description = "Manage ledger accounts and structure",
                        Tags = new[] { "account", "ledger", "chart", "finance" }
                    }
                }
            },

            // 5. Purchases / Suppliers / Payables
            new Example
            {
                Name = "Purchases",
                Icon = "\ue8cc",  // shopping cart
                Children = new[]
                {
                    new Example
                    {
                        Toc = [ new () { Text = "PurchaseReturnsList", Anchor = "#text-tag-name" } ],
                        Name = "Purchase Returns",
                        Path = "/PurchaseReturnsList",
                        Updated = false,
                        Title = "Purchase Returns List",
                        Description = "Manage purchase returns",
                        Icon = "\ue40a",
                        Tags = new[] { "purchase", "return", "finance" }
                    },
                   new Example {
                        Toc = [ new () { Text = "UndispatchedTransferList", Anchor = "#text-tag-name" } ],
                        Name = "Undispatched Transfers",
                        Path = "/UndispatchedTransferList",
                        Updated = false,
                        Title = "Undispatched Transfer List",
                        Description = "Manage undispatched transfers",
                        Icon = "\ue40a",
                        Tags = new[] { "transfer", "undispatched", "inventory" }
                    },
                    new Example
                    {
                        Name = "Purchase Orders",
                        Path = "/PurchaseOrdersList",
                        Icon = "\ue8cc",
                        Title = "Purchase Orders",
                        Description = "Create and track purchase orders",
                        Tags = new[] { "po", "order", "purchase" }
                    },
                    new Example
                    {
                        Name = "Purchase",
                        Path = "/PurchaseList",
                        Icon = "\ue8cc",
                        Title = "Purchase Entry",
                        Description = "Create and track purchase entry",
                        Tags = new[] { "po", "order", "purchase" }
                    },
                    new Example
                    {
                        Name = "Item Transfer",
                        Path = "/ItemTransfer",
                        Icon = "\ue0d1",
                        Title = "Item Transfer",
                        Description = "Transfer items between stores",
                        Tags = new[] { "transfer", "stock", "store", "issue" }
                    },
                    new Example {
                        Toc = [ new () { Text = "ItemsTransferList", Anchor = "#text-tag-name" } ],
                        Name = "Items Transfers",
                        Path = "/ItemsTransferList",
                        Updated = false,
                        Title = "Items Transfer List",
                        Description = "Manage items transfers",
                        Icon = "\ue40a",
                        Tags = new[] { "items", "transfer", "inventory" }
                    },
                    new Example
                    {
                        Name = "Stock Ledger",
                        Path = "/StockLedger",
                        Icon = "\ue8f9",
                        Title = "Stock Ledger",
                        Description = "View opening stock, receipts, issues and closing stock by store",
                        Tags = new[] { "stock", "ledger", "inventory", "opening", "closing", "receive", "issue" }
                    },
                    new Example
                    {
                        Name = "Purchase Draft",
                        Path = "/PurchaseDraftList",
                        Icon = "\ue8cc",
                        Title = "Purchase Entry",
                        Description = "Create and track purchase entry",
                        Tags = new[] { "po", "order", "purchase" }
                    },
                    new Example
                    {
                        Name = "Purchase Invoices",
                        Path = "/PurchasesList",
                        Icon = "\ue873",  // receipt
                        Title = "Purchase Invoices",
                        Description = "Record and manage received invoices",
                        Tags = new[] { "invoice", "bill", "purchase" }
                    },
                     new Example { Name = "Stock Opening", Path = "/StockOpeningList", Icon = "\ue14f", Title = "Stock Opening", Description = "Add new opening stock", Tags = new[] { "stock", "opening", "inventory" } },
                }
            },

            // 5a. Supplier Management
            new Example
            {
                Name = "Supplier",
                Icon = "\ue558", // business
                Children = new[]
                {
                    new Example
                    {
                        Name = "Suppliers",
                        Path = "/SuppliersList",
                        Icon = "\ue558",
                        Title = "Suppliers",
                        Description = "Manage vendor and supplier records",
                        Tags = new[] { "supplier", "vendor" }
                    },
                    new Example
                    {
                        Name = "Supplier Payments",
                        Path = "/SupplierPaymentsList",
                        Icon = "\ue8a1",
                        Title = "Supplier Payments",
                        Description = "Record and track payments to suppliers",
                        Tags = new[] { "payment", "supplier", "finance" }
                    },
                    new Example
                    {
                        Name = "Supplier Ledger",
                        Path = "/SupplierLedger",
                        Icon = "\ue8f4",
                        Title = "Supplier Ledger",
                        Description = "View supplier account statements",
                        Tags = new[] { "ledger", "supplier", "statement" }
                    },
                    new Example
                    {
                        Name = "Outstanding",
                        Path = "/SupplierOutstanding",
                        Icon = "\ue85d",
                        Title = "Supplier Outstanding",
                        Description = "View outstanding balances per supplier",
                        Tags = new[] { "outstanding", "balance", "supplier" }
                    }
                }
            },

           
            // 6. Vouchers & Journal Entries
           
            new Example
            {
                Name = "Vouchers",
                Icon = "\ue873",  // receipt_long
                Children = new[]
                {
                    new Example
                    {
                        Name = "Voucher List",
                        Path = "/VoucherList",
                        Icon = "\ue873",
                        Title = "Vouchers",
                        Description = "Create and manage accounting vouchers",
                        Tags = new[] { "voucher", "journal", "entry" }
                    },
                    new Example
                    {
                        Name = "Voucher Details",
                        Path = "/VoucherDetailsList",
                        Icon = "\ue85d",
                        Title = "Voucher Lines",
                        Description = "View and edit individual voucher transactions",
                        Tags = new[] { "voucher", "detail", "line", "debit", "credit" }
                    }
                }
            },

           
            // 7. Security & Access Control
           
            new Example
            {
                Name = "Security",
                Icon = "\ue897",  // shield
                Children = new[]
                {
                    new Example
                    {
                        Name = "Group Roles",
                        Path = "/GroupRoleList",
                        Icon = "\ue8d3",  // verified user
                        Title = "Group Roles",
                        Description = "Define roles and permissions",
                        Tags = new[] { "role", "permission", "security" }
                    },
                    new Example
                    {
                        Name = "User Groups",
                        Path = "/UserGroupList",
                        Icon = "\ue7fd",
                        Title = "User Group Assignment",
                        Description = "Assign users to security groups",
                        Tags = new[] { "user", "group", "assignment", "access" }
                    },
                    //new Example
                    //{
                    //    Name = "Action",
                    //    Path = "/ActionList",
                    //    Icon = "\ue7fd",
                    //    Title = "User Action List",
                    //    Description = "Create Action for Groups",
                    //    Tags = new[] { "user", "group", "assignment", "action" }
                    //},
                    //new Example
                    //{
                    //    Name = "Menu",
                    //    Path = "/RouteList",
                    //    Icon = "\ue7fd",
                    //    Title = "User Action List",
                    //    Description = "Create Action for Groups",
                    //    Tags = new[] { "user", "group", "assignment", "action" }
                    //},
                    new Example
                    {
                        Name = "Route Permission",
                        Path = "/GroupRoutePermissionAdd",
                        Icon = "\ue7fd",
                        Title = "User Action List",
                        Description = "Create Action for Groups",
                        Tags = new[] { "user", "group", "assignment", "action" }
                    },
                    new Example
                    {
                        Name = "Group Action Permissions",
                        Path = "/GroupActionPermissionsAdd",
                        Icon = "\ue7fd",
                        Title = "User Action List",
                        Description = "Create Action for Groups",
                        Tags = new[] { "user", "group", "assignment", "action" }
                    }
                }
            },

           
            // 8. Product / Inventory Master (optional / partial)
           
            new Example
            {
                Name = "Products & Inventory",
                Icon = "\ue8d2",  // layers / inventory
                Children = new[]
                {
                    //new Example
                    //{
                    //    Name = "Product Suppliers",
                    //    Path = "/ProductSuppliersList",
                    //    Icon = "\ue558", // business / handshake
                    //    Title = "Product Suppliers",
                    //    Description = "Manage suppliers for products",
                    //    Tags = new[] { "supplier", "product", "inventory" }
                    //},
                    new Example
                    {
                        Name = "Item Attributes",
                        Icon = "\ue8d2", // category
                        Title = "Item Attributes",
                        Description = "Manage product attributes like colors and groups",
                        Tags = new[] { "attribute", "product", "inventory" },
                        Children = new[]
                        {
                            new Example { Name = "Colors", Path = "/ColorsList", Icon = "\ue3e8", Title = "Colors", Description = "Manage product colors", Tags = new[] { "color", "attribute" } },
                            new Example { Name = "Groups", Path = "/GroupsList", Icon = "\ue7fd", Title = "Groups", Description = "Manage product groups", Tags = new[] { "group", "attribute" } },
                            new Example { Name = "SubGroups", Path = "/SubGroupsList", Icon = "\ue5db", Title = "SubGroups", Description = "Manage product sub-groups", Tags = new[] { "subgroup", "attribute" } },
                            new Example { Name = "Designs", Path = "/DesignsList", Icon = "\ue41f", Title = "Designs", Description = "Manage product designs per sub-product", Tags = new[] { "design", "attribute", "style" } },
                            new Example { Name = "MesurementUnits", Path = "/MesurementUnitsList", Icon = "\ue41c", Title = "Measurement Units", Description = "Manage measurement units", Tags = new[] { "unit", "attribute" } },
                            new Example { Name = "Items", Path = "/ItemsList", Icon = "\ue1bd", Title = "Items", Description = "Manage inventory items", Tags = new[] { "item", "inventory", "product" } },
                           
                        }
                    },
                    new Example
                    {
                        Name = "Sizes",
                        Path = "/SizesList",
                        Icon = "\ue8d2",
                        Title = "Sizes",
                        Description = "Manage product size variants",
                        Tags = new[] { "size", "variant", "inventory" }
                    },
                    new Example
                    {
                        Name = "Barcodes",
                        Path = "/BarcodeList",
                        Icon = "\ue6b8",  // qr_code_scanner
                        Title = "Barcodes",
                        Description = "Manage barcode assignments",
                        Tags = new[] { "barcode", "sku", "inventory" }
                    }
                }
            },

           
            // 9. Appearance (only if your app actually has theme customization)
           
            new Example
            {
                Name = "Appearance",
                Icon = "\ue3e8",  // palette
                Children = new[]
                {
                    new Example
                    {
                        Name = "Colors",
                        Path = "/ColorsList",
                        Icon = "\ue3e8",
                        Title = "Color Settings",
                        Description = "Customize application color theme",
                        Tags = new[] { "theme", "color", "appearance" }
                    }
                }
            }
        };

        public IEnumerable<Example> Examples => allExamples;

        public IEnumerable<Example> Filter(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return allExamples;

            term = term.Trim();

            bool contains(string? value)
                => !string.IsNullOrEmpty(value) && value.Contains(term, StringComparison.OrdinalIgnoreCase);

            bool matchesItem(Example item)
                => contains(item.Name) ||
                   contains(item.Title) ||
                   contains(item.Description) ||
                   (item.Tags?.Any(contains) ?? false);

            bool matchesCategoryOrChildren(Example category)
            {
                if (matchesItem(category))
                    return true;

                if (category.Children == null)
                    return false;

                return category.Children.Any(matchesItem);
            }

            return allExamples
                .Where(matchesCategoryOrChildren)
                .Select(cat => new Example
                {
                    Name = cat.Name,
                    Path = cat.Path,
                    Icon = cat.Icon,
                    Title = cat.Title,
                    Description = cat.Description,
                    Tags = cat.Tags,
                    Expanded = true,
                    Children = cat.Children?
                        .Where(matchesItem)
                        .Select(child => new Example
                        {
                            Name = child.Name,
                            Path = child.Path,
                            Icon = child.Icon,
                            Title = child.Title,
                            Description = child.Description,
                            Tags = child.Tags,
                            Expanded = true
                        })
                        .ToArray()
                })
                .ToList();
        }

        public Example? FindCurrent(Uri uri)
        {
            string path = uri.AbsolutePath.TrimEnd('/');

            IEnumerable<Example> Flatten(IEnumerable<Example> items)
            {
                foreach (var item in items)
                {
                    yield return item;
                    if (item.Children != null)
                    {
                        foreach (var child in Flatten(item.Children))
                            yield return child;
                    }
                }
            }

            return Flatten(allExamples)
                .FirstOrDefault(e =>
                    e.Path != null &&
                    (string.Equals(e.Path.TrimStart('/'), path, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals("/" + e.Path.TrimStart('/'), path, StringComparison.OrdinalIgnoreCase)));
        }

        public string TitleFor(Example? example)
        {
            if (example == null || example.Name == "Dashboard")
            {
                return "ERP System | Dashboard";
            }

            return example.Title ?? $"{example.Name} | ERP System";
        }

        public string DescriptionFor(Example? example)
        {
            return example?.Description
                ?? "Modern ERP application built with Blazor – manage company, employees, accounts, purchases, vouchers and more.";
        }
    }
}