using JM.UI.Entities.Models;
namespace JM.UI.Client.Services
{
    public class ExampleService
    {
        private readonly Example[] allExamples = new[]
                {
            // ─────────────────────────────────────────
            // 1. Dashboard
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Dashboard",
                Path = "/",
                Icon = "\ue88a"
            },
 
            // ─────────────────────────────────────────
            // 2. Company Setup
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Company Setup",
                Icon = "\ue7f1",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Companies",
                        Path        = "/CompanyList",
                        Icon        = "\ue7f1",
                        Title       = "Companies",
                        Description = "Manage company profiles and basic settings",
                        Tags        = new[] { "company", "setup", "organization" }
                    },
                    new Example
                    {
                        Name        = "Branches / Stores",
                        Path        = "/StoreList",
                        Icon        = "\ue8f4",
                        Title       = "Branches",
                        Description = "Manage branches, stores or locations",
                        Tags        = new[] { "branch", "store", "location" }
                    }
                }
            },
            // ─────────────────────────────────────────
            // 2. Discount Setup
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Discount",
                Icon = "\ue8d2",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "General Discount",
                        Path        = "/DiscountManagerList",
                        Icon        = "\ue8d2",
                        Title       = "General Discount",
                        Description = "Manage general discount configurations",
                        Tags        = new[] { "discount", "general", "promotion" }
                    },
                    new Example
                    {
                        Name        = "Coupon Setup",
                        Path        = "/CouponList",
                        Icon        = "\ue8d2",
                        Title       = "Coupons",
                        Description = "Manage discount coupons and promotional codes",
                        Tags        = new[] { "coupon", "promotion", "discount" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 3. Human Resources
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Human Resources",
                Icon = "\ue7fd",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Employees",
                        Path        = "/EmployeeList",
                        Icon        = "\ue7fd",
                        Title       = "Employees",
                        Description = "Manage employee records, profiles and assignments",
                        Tags        = new[] { "employee", "hr", "staff" }
                    },
                    new Example
                    {
                        Name        = "Designations",
                        Path        = "/DesignationList",
                        Icon        = "\ue749",
                        Title       = "Designations",
                        Description = "Define job titles and designations",
                        Tags        = new[] { "designation", "job title", "role" }
                    },
                    new Example
                    {
                        Name        = "Shifts",
                        Path        = "/ShiftList",
                        Icon        = "\ue8d6",
                        Title       = "Work Shifts",
                        Description = "Manage shift patterns and schedules",
                        Tags        = new[] { "shift", "schedule", "roster" }
                    },
                    new Example
                    {
                        Name        = "Membership Types",
                        Path        = "/MembershipTypeList",
                        Icon        = "\ue7fd",
                        Title       = "Membership Types",
                        Description = "Manage membership type configurations",
                        Tags        = new[] { "membership", "type", "hr" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 4. Products & Inventory
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Products & Inventory",
                Icon = "\ue8d2",
                Children = new[]
                {
                    // ── Item Attributes (nested) ──
                    new Example
                    {
                        Name        = "Item Attributes",
                        Icon        = "\ue8d2",
                        Title       = "Item Attributes",
                        Description = "Manage product attributes like colors, groups and designs",
                        Tags        = new[] { "attribute", "product", "inventory" },
                        Children    = new[]
                        {
                            new Example
                            {
                                Name        = "Colors",
                                Path        = "/ColorsList",
                                Icon        = "\ue3e8",
                                Title       = "Colors",
                                Description = "Manage product colors",
                                Tags        = new[] { "color", "attribute" }
                            },
                            new Example
                            {
                                Name        = "Groups",
                                Path        = "/GroupsList",
                                Icon        = "\ue7fd",
                                Title       = "Groups",
                                Description = "Manage product groups",
                                Tags        = new[] { "group", "attribute" }
                            },
                            new Example
                            {
                                Name        = "SubGroups",
                                Path        = "/SubGroupsList",
                                Icon        = "\ue5db",
                                Title       = "SubGroups",
                                Description = "Manage product sub-groups",
                                Tags        = new[] { "subgroup", "attribute" }
                            },
                            new Example
                            {
                                Name        = "Designs",
                                Path        = "/DesignsList",
                                Icon        = "\ue41f",
                                Title       = "Designs",
                                Description = "Manage product designs per sub-product",
                                Tags        = new[] { "design", "attribute", "style" }
                            },
                            new Example
                            {
                                Name        = "Measurement Units",
                                Path        = "/MesurementUnitsList",
                                Icon        = "\ue41c",
                                Title       = "Measurement Units",
                                Description = "Manage measurement units",
                                Tags        = new[] { "unit", "measure", "attribute" }
                            },
                            new Example
                            {
                                Name        = "Items",
                                Path        = "/ItemsList",
                                Icon        = "\ue1bd",
                                Title       = "Items",
                                Description = "Manage inventory items",
                                Tags        = new[] { "item", "inventory", "product" }
                            }
                        }
                    },
 
                    // ── Variants & Codes ──
                    new Example
                    {
                        Name        = "Sizes",
                        Path        = "/SizesList",
                        Icon        = "\ue8d2",
                        Title       = "Sizes",
                        Description = "Manage product size variants",
                        Tags        = new[] { "size", "variant", "inventory" }
                    },
                   
                    new Example
                    {
                        Name        = "Barcodes",
                        Path        = "/BarcodeList",
                        Icon        = "\ue6b8",
                        Title       = "Barcodes",
                        Description = "Manage barcode assignments",
                        Tags        = new[] { "barcode", "sku", "inventory" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 5. Purchases
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Purchases",
                Icon = "\ue8cc",
                Children = new[]
                {
                    // ── Orders & Invoices ──
                    
                    new Example
                    {
                        Name        = "Purchase",
                        Path        = "/PurchaseList",
                        Icon        = "\ue8cc",
                        Title       = "Purchase Entry",
                        Description = "Create and track purchase entries",
                        Tags        = new[] { "purchase", "entry" }
                    },
                    new Example
                    {
                        Name        = "Purchase Drafts",
                        Path        = "/PurchaseDraftList",
                        Icon        = "\ue8cc",
                        Title       = "Purchase Drafts",
                        Description = "Manage draft purchase entries before posting",
                        Tags        = new[] { "draft", "purchase" }
                    },
                    
                    new Example
                    {
                        Name        = "Purchase Returns",
                        Path        = "/PurchaseReturnsList",
                        Icon        = "\ue40a",
                        Title       = "Purchase Returns",
                        Description = "Manage purchase returns to suppliers",
                        Tags        = new[] { "purchase", "return", "finance" },
                    },
 
                    // ── Stock Management ──
                    new Example
                    {
                        Name        = "Stock Opening",
                        Path        = "/StockOpeningList",
                        Icon        = "\ue14f",
                        Title       = "Stock Opening",
                        Description = "Add new opening stock entries",
                        Tags        = new[] { "stock", "opening", "inventory" }
                    },
                    new Example
                    {
                        Name        = "Stock Ledger",
                        Path        = "/StockLedger",
                        Icon        = "\ue8f9",
                        Title       = "Stock Ledger",
                        Description = "View opening stock, receipts, issues and closing stock by store",
                        Tags        = new[] { "stock", "ledger", "inventory", "opening", "closing", "receive", "issue" }
                    },
 
                    // ── Transfers ──
                   
                    new Example
                    {
                        Name        = "Transfer List",
                        Path        = "/ItemsTransferList",
                        Icon        = "\ue40a",
                        Title       = "Items Transfer List",
                        Description = "View all item transfers",
                        Tags        = new[] { "items", "transfer", "inventory" },
                    },
                    new Example
                    {
                        Name        = "Dispatch Goods",
                        Path        = "/UndispatchedTransferList",
                        Icon        = "\ue40a",
                        Title       = "Undispatched Transfer List",
                        Description = "Manage transfers pending dispatch",
                        Tags        = new[] { "transfer", "undispatched", "inventory" },
                    },
                    new Example
                    {
                        Name        = "Receive Transfers",
                        Path        = "/DispatchedTransferList",
                        Icon        = "\ue40a",
                        Title       = "Dispatched Transfer List",
                        Description = "Manage completed dispatched transfers",
                        Tags        = new[] { "transfer", "dispatched", "inventory" },
                    },
 
                    // ── Other ──
                    new Example
                    {
                        Name        = "Requisition",
                        Path        = "/InvRequisitionList",
                        Icon        = "\ue873",
                        Title       = "Requisition",
                        Description = "Manage inventory requisitions",
                        Tags        = new[] { "requisition", "order", "purchase" }
                    },
                    new Example
                    {
                        Name        = "Barcode Print",
                        Path        = "/BarcodePrint",
                        Icon        = "\ue873",
                        Title       = "Barcode Print",
                        Description = "Print barcodes for inventory items",
                        Tags        = new[] { "barcode", "print", "inventory" }
                    }
                }
            },

            // ─────────────────────────────────────────
            // 6. Sales
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Sales",
                Icon = "\ue558",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "POS",
                        Path        = "/SalesPOS",
                        Icon        = "\ue558",
                        Title       = "POS",
                        Description = "Manage vendor and supplier records",
                        Tags        = new[] { "supplier", "vendor" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 6. Suppliers
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Suppliers",
                Icon = "\ue558",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Suppliers",
                        Path        = "/SuppliersList",
                        Icon        = "\ue558",
                        Title       = "Suppliers",
                        Description = "Manage vendor and supplier records",
                        Tags        = new[] { "supplier", "vendor" }
                    },
                    new Example
                    {
                        Name        = "Supplier Payments",
                        Path        = "/SupplierPaymentsList",
                        Icon        = "\ue8a1",
                        Title       = "Supplier Payments",
                        Description = "Record and track payments to suppliers",
                        Tags        = new[] { "payment", "supplier", "finance" }
                    },
                    new Example
                    {
                        Name        = "Supplier Ledger",
                        Path        = "/SupplierLedger",
                        Icon        = "\ue8f4",
                        Title       = "Supplier Ledger",
                        Description = "View supplier account statements",
                        Tags        = new[] { "ledger", "supplier", "statement" }
                    },
                    new Example
                    {
                        Name        = "Outstanding",
                        Path        = "/SupplierOutstanding",
                        Icon        = "\ue85d",
                        Title       = "Supplier Outstanding",
                        Description = "View outstanding balances per supplier",
                        Tags        = new[] { "outstanding", "balance", "supplier" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 7. Accounting Setup
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Accounting Setup",
                Icon = "\ue8b0",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Account Groups",
                        Path        = "/AccountsGroupsList",
                        Icon        = "\ue8b0",
                        Title       = "Account Groups",
                        Description = "Organize accounts into categories and groups",
                        Tags        = new[] { "account", "group", "finance", "coa" }
                    },
                    new Example
                    {
                        Name        = "Accounts (COA)",
                        Path        = "/AccountsList",
                        Icon        = "\ue85d",
                        Title       = "Chart of Accounts",
                        Description = "Manage ledger accounts and structure",
                        Tags        = new[] { "account", "ledger", "chart", "finance" }
                    }
                }
            },
            new Example
            {
                Name = "CRM",
                Icon = "\ue8b0",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Registration",
                        Path        = "/CustomerDetailsList",
                        Icon        = "\ue8b0",
                        Title       = "Account Groups",
                        Description = "Organize accounts into categories and groups",
                        Tags        = new[] { "account", "group", "finance", "coa" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 8. Vouchers
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Vouchers",
                Icon = "\ue873",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Voucher List",
                        Path        = "/VoucherList",
                        Icon        = "\ue873",
                        Title       = "Vouchers",
                        Description = "Create and manage accounting vouchers",
                        Tags        = new[] { "voucher", "journal", "entry" }
                    },
                    new Example
                    {
                        Name        = "Voucher Details",
                        Path        = "/VoucherDetailsList",
                        Icon        = "\ue85d",
                        Title       = "Voucher Lines",
                        Description = "View and edit individual voucher transactions",
                        Tags        = new[] { "voucher", "detail", "line", "debit", "credit" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 9. Stock Reports
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Stock Reports",
                Icon = "\ue8f9",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Current Stock",
                        Path        = "/CurrentStockReport",
                        Icon        = "\ue8f9",
                        Title       = "Current Stock Report",
                        Description = "View current stock levels across all stores",
                        Tags        = new[] { "stock", "report", "inventory", "current" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 10. Security & Access
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Security & Access",
                Icon = "\ue897",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Group Roles",
                        Path        = "/GroupRoleList",
                        Icon        = "\ue8d3",
                        Title       = "Group Roles",
                        Description = "Define roles and permissions",
                        Tags        = new[] { "role", "permission", "security" }
                    },
                    new Example
                    {
                        Name        = "User Groups",
                        Path        = "/UserGroupList",
                        Icon        = "\ue7fd",
                        Title       = "User Group Assignment",
                        Description = "Assign users to security groups",
                        Tags        = new[] { "user", "group", "assignment", "access" }
                    },
                    new Example
                    {
                        Name        = "Route Permissions",
                        Path        = "/GroupRoutePermissionAdd",
                        Icon        = "\ue7fd",
                        Title       = "Route Permissions",
                        Description = "Control menu and page access per group",
                        Tags        = new[] { "route", "permission", "group", "access" }
                    },
                    new Example
                    {
                        Name        = "Action Permissions",
                        Path        = "/GroupActionPermissionsAdd",
                        Icon        = "\ue7fd",
                        Title       = "Action Permissions",
                        Description = "Control feature-level actions per group",
                        Tags        = new[] { "action", "permission", "group", "security" }
                    }
                }
            },
 
            // ─────────────────────────────────────────
            // 11. Appearance
            // ─────────────────────────────────────────
            new Example
            {
                Name = "Appearance",
                Icon = "\ue3e8",
                Children = new[]
                {
                    new Example
                    {
                        Name        = "Colors",
                        Path        = "/ColorsList",
                        Icon        = "\ue3e8",
                        Title       = "Color Settings",
                        Description = "Customize application color theme",
                        Tags        = new[] { "theme", "color", "appearance" }
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