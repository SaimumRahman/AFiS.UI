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
                new Example
                {
                    Name = "Add",
                    Path = "DesignationAdd",
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
                    Name = "Add",
                    Path = "CompanyAdd",
                    Updated = true,
                    Title = "Blazor Themes | Free UI Components by Radzen",
                    Description = "The Radzen Blazor Components package features an array of both free and premium themes, allowing you to choose the style that best suits your project's requirements.",
                    Icon = "\ue40a",
                    Tags = new[] { "theme", "color", "background", "border", "utility", "css", "var" }
                },
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
