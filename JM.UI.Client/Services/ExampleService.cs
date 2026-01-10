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
         new Example { Toc = [ new ()
         { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
             Name = "Customer", Icon = "\ue749", Children = new []
             {
                 new Example { Name = "List", Path = "CustomerList", Updated = true, Title = "Blazor Themes | Free UI Components by Radzen", Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.", Icon = "\ue40a", Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"} },
                 new Example { Name = "Add", Path = "CustomerAdd", Updated = true, Title = "Blazor Themes | Free UI Components by Radzen", Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.", Icon = "\ue40a", Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"} }, } },

      new Example
        {
            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "CustomerType",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "CustomerTypeList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
                new Example
                {
                    Name = "Add",
                    Path = "CustomerTypeAdd",
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
            Name = "Parcel",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "ParcelList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
                new Example
                {
                    Name = "Add",
                    Path = "ParcelAdd",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
                new Example
                {
                    Name = "Transfer List",
                    Path = "ParcelTransfersList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
            }
        },

         new Example
        {

            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Pickup",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "ParcelPickupList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
                new Example
                {
                    Name = "Add",
                    Path = "ParcelPickupAdd",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
                new Example
                {
                    Name = "Pending List",
                    Path = "PendingParcelPickupList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
            }
        },
         new Example
        {

            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Delivery",
            Icon = "\ue749",
            Children = new [] {
                new Example
                {
                    Name = "List",
                    Path = "ParcelDeliveryList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
                new Example
                {
                    Name = "Add",
                    Path = "ParcelDeliveryAdd",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
                new Example
                {
                    Name = "Pending List",
                    Path = "PendingParcelDeliveryList",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                },
            }
         },
         new Example
            {
                Toc = [ new () { Text = "Approval Management in Radzen Blazor Studio", Anchor = "#approval-management" } ],
                Name = "Approval",
                Icon = "\ue8e8",  // approval icon
                Children = new[]
                {
                    new Example
                    {
                        Toc = [ new () { Text = "Approval Levels", Anchor = "#approval-levels" } ],
                        Name = "Level",
                        Icon = "\ue749",  // layers or playlist_add_check
                        Children = new[]
                        {
                            new Example
                            {
                                Name = "List",
                                Path = "ApprovalLevelList",
                                Updated = true,
                                Title = "Approval Levels - List | Radzen Blazor Components",
                                Description = "Manage approval levels with filtering, sorting, paging and inline editing using Radzen Blazor DataGrid.",
                                Icon = "\ue40a",  // list icon
                                Tags = new[] { "approval", "level", "list", "datagrid", "crud", "filter", "sort", "paging" }
                            },
                            new Example
                            {
                                Name = "Add",
                                Path = "ApprovalLevelAdd",
                                Updated = true,
                                Title = "Approval Levels - Add New | Radzen Blazor Components",
                                Description = "Create new approval levels with validation, sequence ordering and role assignment using Radzen form components.",
                                Icon = "\ue145",  // add_circle
                                Tags = new[] { "approval", "level", "add", "form", "create", "validation" }
                            }
                        }
                    },
                    new Example
                    {
                        Toc = [ new () { Text = "Approval Levels", Anchor = "#approval-levels" } ],
                        Name = "Work Flow",
                        Icon = "\ue749",  // layers or playlist_add_check
                        Children = new[]
                        {
                            new Example
                            {
                                Name = "List",
                                Path = "ApprovalWorkflowList",
                                Updated = true,
                                Title = "Approval Levels - List | Radzen Blazor Components",
                                Description = "Manage approval levels with filtering, sorting, paging and inline editing using Radzen Blazor DataGrid.",
                                Icon = "\ue40a",  // list icon
                                Tags = new[] { "approval", "level", "list", "datagrid", "crud", "filter", "sort", "paging" }
                            },
                            new Example
                            {
                                Name = "Add",
                                Path = "ApprovalWorkflowAdd",
                                Updated = true,
                                Title = "Approval Levels - Add New | Radzen Blazor Components",
                                Description = "Create new approval levels with validation, sequence ordering and role assignment using Radzen form components.",
                                Icon = "\ue145",  // add_circle
                                Tags = new[] { "approval", "level", "add", "form", "create", "validation" }
                            }
                        }
                    },
                    new Example
                    {
                        Toc = [ new () { Text = "Approval Levels", Anchor = "#approval-levels" } ],
                        Name = "Approvers",
                        Icon = "\ue749",  // layers or playlist_add_check
                        Children = new[]
                        {
                            new Example
                            {
                                Name = "List",
                                Path = "ApprovalLevelApproverList",
                                Updated = true,
                                Title = "Approval Levels - List | Radzen Blazor Components",
                                Description = "Manage approval levels with filtering, sorting, paging and inline editing using Radzen Blazor DataGrid.",
                                Icon = "\ue40a",  // list icon
                                Tags = new[] { "approval", "level", "list", "datagrid", "crud", "filter", "sort", "paging" }
                            },
                            new Example
                            {
                                Name = "Add",
                                Path = "ApprovalLevelApproverAdd",
                                Updated = true,
                                Title = "Approval Levels - Add New | Radzen Blazor Components",
                                Description = "Create new approval levels with validation, sequence ordering and role assignment using Radzen form components.",
                                Icon = "\ue145",  // add_circle
                                Tags = new[] { "approval", "level", "add", "form", "create", "validation" }
                            }
                        }
                    }
                }
            },
            new Example
            {
                Toc = [ new() { Text = "Pending Approvals", Anchor = "#pending-approvals" } ],
                Name = "Pending Approvals",
                Icon = "\ue916", // pending_actions / schedule
                Children = new[]
                {
                    new Example
                    {
                        Name = "Pending Parcel",
                        Path = "ParcelPendinglList",
                        Updated = true,
                        Title = "Pending Approvals - List | Radzen Blazor Components",
                        Description = "View and manage pending approvals with filtering, sorting and paging using Radzen Blazor DataGrid.",
                        Icon = "\ue40a", // list icon
                        Tags = new[]
                        {
                            "approval",
                            "pending",
                            "workflow",
                            "datagrid",
                            "filter",
                            "sort",
                            "paging"
                        }
                    },
                    new Example
                    {
                        Name = "Pending Pickup",
                        Path = "PickupPendingList",
                        Updated = true,
                        Title = "Pending Approvals - List | Radzen Blazor Components",
                        Description = "View and manage pending approvals with filtering, sorting and paging using Radzen Blazor DataGrid.",
                        Icon = "\ue40a", // list icon
                        Tags = new[]
                        {
                            "approval",
                            "pending",
                            "workflow",
                            "datagrid",
                            "filter",
                            "sort",
                            "paging"
                        }
                    },
                    new Example
                    {
                        Name = "Pending Delivery",
                        Path = "DeliveryPendinglList",
                        Updated = true,
                        Title = "Pending Approvals - List | Radzen Blazor Components",
                        Description = "View and manage pending approvals with filtering, sorting and paging using Radzen Blazor DataGrid.",
                        Icon = "\ue40a", // list icon
                        Tags = new[]
                        {
                            "approval",
                            "pending",
                            "workflow",
                            "datagrid",
                            "filter",
                            "sort",
                            "paging"
                        }
                    }
                }
            },

            new Example
{
    Toc = [ new () { Text = "Manage delivery types in the system", Anchor = "#delivery-types" } ],
    Name = "Delivery Type",
    Icon = "\ue8e1",  // local_shipping icon (same style as your other modules)
    Children = new[]
    {
        new Example
        {
            Name = "List",
            Path = "DeliveryTypeList",
            Updated = true,
            Title = "Delivery Types | Parcel Management System",
            Description = "View, add, edit, activate/deactivate and manage all delivery types used in the parcel system.",
            Icon = "\ue40a",  // list icon
            Tags = new[] { "delivery", "type", "courier", "service", "management" }
        },
        new Example
        {
            Name = "Add",
            Path = "DeliveryTypeAdd",
            Updated = true,
            Title = "Add New Delivery Type | Parcel Management System",
            Description = "Create a new delivery type such as Standard, Express, Same-Day, Overnight, etc.",
            Icon = "\ue40a",
            Tags = new[] { "delivery", "type", "create", "add", "setup" }
        }
    }
},
            new Example
{
    Toc = [ new () { Text = "Manage parcel status types in the system", Anchor = "#parcel-status-types" } ],
    Name = "Parcel Status Type",
    Icon = "\ue917", // label_important icon – perfect for status types
    Children = new[]
    {
        new Example
        {
            Name = "List",
            Path = "ParcelStatusTypeList",
            Updated = true,
            Title = "Parcel Status Types | Parcel Management System",
            Description = "View, add, edit, activate/deactivate and manage all parcel status types (Pending, In Transit, Delivered, Returned, etc.).",
            Icon = "\ue40a", // list icon – consistent with all other list pages
            Tags = new[] { "parcel", "status", "type", "tracking", "lifecycle", "management" }
        },
        new Example
        {
            Name = "Add",
            Path = "ParcelStatusTypeAdd",
            Updated = true,
            Title = "Add New Parcel Status Type | Parcel Management System",
            Description = "Create a new parcel status type with custom name, display order, and description.",
            Icon = "\ue145", // add_circle icon – matches your "Add" pattern
            Tags = new[] { "parcel", "status", "create", "add", "setup", "tracking" }
        }
    }
},
            new Example
{
    Toc = [ new () { Text = "Manage delivery routes in the system", Anchor = "#routes" } ],
    Name = "Route",
    Icon = "\ue569", // route icon (Material Icons: "route")
    Children = new[]
    {
        new Example
        {
            Name = "List",
            Path = "RouteList",
            Updated = true,
            Title = "Routes | Parcel Management System",
            Description = "View, add, edit, activate/deactivate and manage all delivery routes (origin to destination) in the system.",
            Icon = "\ue40a", // list icon
            Tags = new[] { "route", "delivery", "origin", "destination", "logistics", "management" }
        },
        new Example
        {
            Name = "Add",
            Path = "RouteAdd",
            Updated = true,
            Title = "Add New Route | Parcel Management System",
            Description = "Create a new delivery route with code, from/to locations, distance, and estimated days.",
            Icon = "\ue40a",
            Tags = new[] { "route", "create", "add", "logistics", "setup" }
        }
    }
},
            new Example
{
    Toc = [ new () { Text = "Third Party Carriers Management", Anchor = "#carrier-management" } ],
    Name = "Carrier",
    Icon = "\ue916", // Material Icon: local_shipping
    Children = new[]
    {
        new Example
        {
            Name = "List",
            Path = "CarrierList",
            Updated = true,
            Title = "Third Party Carriers List | Blazor POS System",
            Description = "Manage all third-party carriers (DHL, FedEx, UPS, etc.) with full CRUD operations, status toggle, commission tracking, and advanced filtering.",
            Icon = "\ue916", // local_shipping
            Tags = new[] { "carrier", "logistics", "shipping", "third-party", "commission", "management" }
        },
        new Example
        {
            Name = "Add / Edit",
            Path = "CarrierAdd",
            Updated = true,
            Title = "Add or Edit Carrier | Blazor POS System",
            Description = "Create new carriers or update existing ones with contact details, commission rates, address, and activation status.",
            Icon = "\ue145", // add_circle
            Tags = new[] { "carrier", "add", "edit", "form", "commission", "logistics" }
        }
    }
},
            new Example
        {

            Toc = [ new () { Text = "Customize themes in Radzen Blazor Studio", Anchor = "#text-tag-name" } ],
            Name = "Agents",
            Icon = "\ue749",
            Children = new [] {
               new Example
                    {
                        Name = "List",
                        Path = "AgentList",
                        Updated = true,
                        Title = "Blazor Themes | Free UI Components by Radzen",
                        Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                        Icon = "\ue40a",
                        Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
                    },
                    new Example
                    {
                        Name = "Add",
                        Path = "AgentAdd",
                        Updated = true,
                        Title = "Blazor Themes | Free UI Components by Radzen",
                        Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                        Icon = "\ue40a",
                        Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var"}
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
