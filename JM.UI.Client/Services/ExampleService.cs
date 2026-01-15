using JM.UI.Entities.Models;

namespace JM.UI.Client.Services.Services
{
    public class KeyboardShortcut
    {
        public string Key { get; set; }
        public string Action { get; set; }
    }
    public class ExampleService
    {
        Example[] allExamples = new[] {
        new Example
        {
            Name = "Overview",
            Path = "/",
            Icon = "\ue88a"
        },
       
      new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Designations",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "DesignationList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
            }
        },
      new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Colors",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "ColorsList",
                    Updated = false,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
              
            }
        },

      new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Shift",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "ShiftList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
              
            }
        },
     
      new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Accounts Groups",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "AccountsGroupsList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
              
            }
        },
     
      new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Banks",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "BanksList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
               
            }
        },
     
            new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Company",
            Icon = "\ue749",
            Children = new [] {
               
                new Example
                {
                    Name = "List",
                    Path = "CompanyList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
            }
        },
            new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Employee",
            Icon = "\ue749",
            Children = new [] {
              
                new Example
                {
                    Name = "List",
                    Path = "EmployeeList ",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
            }
        },
            new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Group Role",
            Icon = "\ue749",
            Children = new [] {
               
                new Example
                {
                    Name = "List",
                    Path = "GroupRoleList ",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
            }
        },
            new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Branch",
            Icon = "\ue749",
            Children = new [] {
                
                new Example
                {
                    Name = "List",
                    Path = "StoreList ",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
            }
        },
             new Example
        {
            Toc = [ new () { Text = "SizesList", Anchor = "#text-tag-name" } ],
            Name = "Size",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/SizesList",
                    Updated = false,
                    Title = "SizesList Title",
                    Description = "SizesList Description",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                }
            
            }
        },
          new Example
        {
            Toc = [ new () { Text = "BarcodesList", Anchor = "#text-tag-name" } ],
            Name = "Barcode",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/BarcodeList",
                    Updated = false,
                    Title = "BarcodesList Title",
                    Description = "BarcodesList Description",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                }
            
            }
        },
        new Example
        {
            Toc = [ new () { Text = "AccountsGroupsList", Anchor = "#text-tag-name" } ],
            Name = "Accounts Groups",
            Icon = "\ue8b0",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/AccountsGroupsList",
                    Updated = false,
                    Title = "Accounts Group List",
                    Description = "Manage group categorizations for accounts",
                    Icon = "\ue40a",
                    Tags = new[] { "accounts", "group", "finance" }
                }
            }
        },
        new Example
        {
            Toc = [ new () { Text = "AccountsList", Anchor = "#text-tag-name" } ],
            Name = "Accounts",
            Icon = "\ue85d",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/AccountsList",
                    Updated = false,
                    Title = "Chart of Accounts",
                    Description = "Manage your chart of accounts",
                    Icon = "\ue40a",
                    Tags = new[] { "accounts", "chart", "finance" }
                }
            }
        },
        new Example
        {
            Toc = [ new () { Text = "SuppliersList", Anchor = "#text-tag-name" } ],
            Name = "Suppliers",
            Icon = "\ue558",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/SuppliersList",
                    Updated = false,
                    Title = "Suppliers List",
                    Description = "Manage supplier records",
                    Icon = "\ue40a",
                    Tags = new[] { "suppliers", "master", "finance" }
                }
            }
        },
        new Example
        {
            Toc = [ new () { Text = "PurchasesList", Anchor = "#text-tag-name" } ],
            Name = "Purchases",
            Icon = "\ue8cc",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/PurchasesList",
                    Updated = false,
                    Title = "Purchases List",
                    Description = "Manage purchases",
                    Icon = "\ue40a",
                    Tags = new[] { "purchase", "buying", "finance" }
                }
            }
        },
        new Example
        {
            Toc = [ new () { Text = "PurchaseOrdersList", Anchor = "#text-tag-name" } ],
            Name = "Purchase Orders",
            Icon = "\ue8cc",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/PurchaseOrdersList",
                    Updated = false,
                    Title = "Purchase Orders List",
                    Description = "Manage purchase orders",
                    Icon = "\ue40a",
                    Tags = new[] { "purchase", "order", "finance" }
                }
            }
        },
        new Example
        {
            Toc = [ new () { Text = "VoucherList", Anchor = "#text-tag-name" } ],
            Name = "Vouchers",
            Icon = "\ue8b0",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/VoucherList",
                    Updated = false,
                    Title = "Voucher List",
                    Description = "Manage master voucher records",
                    Icon = "\ue40a",
                    Tags = new[] { "voucher", "master", "finance" }
                }
            }
        },
        new Example
        {
            Toc = [ new () { Text = "VoucherDetailsList", Anchor = "#text-tag-name" } ],
            Name = "Voucher Detail",
            Icon = "\ue85d",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "/VoucherDetailsList",
                    Updated = false,
                    Title = "Voucher Detail List",
                    Description = "Manage individual voucher line items",
                    Icon = "\ue40a",
                    Tags = new[] { "voucher", "account", "debit", "credit" }
                }
            }
        },

            new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "User Group Assign",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "Add",
                    Path = "GroupUserAssignment",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
            }
        },
    };

        public IEnumerable<Example> Examples
        {
            get
            {
                return allExamples;
            }
        }

        public IEnumerable<Example> Filter(string term)
        {
            if (string.IsNullOrEmpty(term))
                return allExamples;

            bool contains(string value) => value != null && value.Contains(term, StringComparison.OrdinalIgnoreCase);

            bool filter(Example example) => contains(example.Name) || (example.Tags != null && example.Tags.Any(contains));

            bool deepFilter(Example example) => filter(example) || example.Children?.Any(filter) == true;

            return Examples.Where(category => category.Children?.Any(deepFilter) == true || filter(category))
                           .Select(category => new Example
                           {
                               Name = category.Name,
                               Path = category.Path,
                               Icon = category.Icon,
                               Expanded = true,
                               Children = category.Children?.Where(deepFilter).Select(example => new Example
                               {
                                   Name = example.Name,
                                   Path = example.Path,
                                   Icon = example.Icon,
                                   Expanded = true,
                                   Children = example.Children
                               }
                               ).ToArray()
                           }).ToList();
        }

        public Example FindCurrent(Uri uri)
        {
            IEnumerable<Example> Flatten(IEnumerable<Example> e)
            {
                return e.SelectMany(c => c.Children != null ? Flatten(c.Children) : new[] { c });
            }

            return Flatten(Examples)
                        .FirstOrDefault(example => example.Path == uri.AbsolutePath || $"/{example.Path}" == uri.AbsolutePath);
        }

        public string TitleFor(Example example)
        {
            if (example != null && example.Name != "Overview")
            {
                return example.Title ?? $"Blazor {example.Name} Component | Free UI Components by Radzen";
            }

            return "Free Blazor Components | 100+ UI controls by Radzen";
        }

        public string DescriptionFor(Example example)
        {
            return example?.Description ?? "The Radzen Blazor component library provides more than 100 UI controls for building rich ASP.NET Core web applications.";
        }
    }
}
