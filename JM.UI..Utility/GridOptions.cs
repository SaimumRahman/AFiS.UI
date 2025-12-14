using System;
using System.Collections.Generic;
using System.Text;

namespace InvPosUtility
{
    public class GridOptions
    {
        public int skip { get; set; }
        public int take { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public List<AzFilter.GridSort> sort { get; set; }
        public AzFilter.GridFilters filter { get; set; }
    }

    public class AutoCompOptions
    {
        public int skip { get; set; }
        public int take { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public AzFilter.AutoCompFilters filter { get; set; }
    }

    public class MultiSelectOptions
    {
        public int skip { get; set; }
        public int take { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public AzFilter.AutoCompFilters filter { get; set; }
    }

    public class GridColumns
    {
        public string aggregates;
        public string field { get; set; }
        public string title { get; set; }
        public string width { get; set; }
        public string footerTemplate { get; set; }
        public string template { get; set; }
        public bool filterable { get; set; }
        public bool sortable { get; set; }
        public bool hidden { get; set; }

        public object editor { get; set; }

        public string groupHeaderTemplate { get; set; }
    }

    public class GridColumnCollection : List<GridColumns>
    {

    }

    public class GridResult<T>
    {
        public GridEntity<T> Data(List<T> list, int totalCount)
        {
            var dEntity = new GridEntity<T>();
            dEntity.Items = list ?? new List<T>();
            dEntity.TotalCount = totalCount;
            dEntity.Columnses = new List<GridColumns>();
            return dEntity;
        }
    }

    public class GridEntity<T>
    {
        public IList<T> Items { get; set; }
        public int TotalCount { get; set; }
        public IList<GridColumns> Columnses { get; set; }
    }

    public class GridModel
    {
        public string id { get; set; }
        public GridFields fields { get; set; }
    }

    public class GridFields
    {

    }

    public class GridFieldAggregates : List<GridFieldAggregate>
    {

    }
    public class GridFieldAggregate
    {
        public string field { get; set; }
        public string aggregate { get; set; }

    }
}
