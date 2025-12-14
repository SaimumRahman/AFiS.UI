using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InvPosUtility
{
    public class ListConversion
    {
        public static DataTable ConvertTo<T>(IList<T> list)
        {
            DataTable table = CreateTable<T>();
            Type entityType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            foreach (T item in list)
            {
                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    if (prop.PropertyType.Name == "DateTime")
                    {
                        var value = prop.GetValue(item);
                        if (value != null)
                        {
                            var date = (DateTime)value;
                            if (date == DateTime.MinValue)
                            {
                                row[prop.Name.ToUpper()] = DBNull.Value;
                            }
                            else
                            {
                                row[prop.Name.ToUpper()] = prop.GetValue(item);
                            }
                        }
                    }
                    else
                    {
                        var isType = TypeArray().Contains(prop.PropertyType.Name);
                        if (isType)
                        {
                            row[prop.Name.ToUpper()] = prop.GetValue(item);
                        }
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public static IList<T> ConvertTo<T>(IList<DataRow> rows)
        {
            IList<T> list = null;

            if (rows != null)
            {
                list = new List<T>();

                foreach (DataRow row in rows)
                {
                    T item = CreateItem<T>(row);
                    list.Add(item);
                }
            }

            return list;
        }


        public static IList<T> ConvertTo<T>(DataTable table)
        {
            if (table == null)
            {
                return null;
            }

            var rows = table.Rows.Cast<DataRow>().ToList();

            return ConvertTo<T>(rows);
        }


        public static T CreateItem<T>(DataRow row)
        {
            T obj = default;
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();


                var properties = obj.GetType().GetProperties();
                foreach (var propertyInfo in properties)
                {
                    PropertyInfo prop = propertyInfo;
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        if (column.ColumnName.ToLower() != "rowindex")
                        {
                            if (propertyInfo.Name.ToLower() == column.ColumnName.ToLower())
                            {
                                try
                                {
                                    if (row[column.ColumnName] != DBNull.Value)
                                    {
                                        dynamic value = row[column.ColumnName];
                                        value = ValueConverter(prop, value);
                                        prop.SetValue(obj, value);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // You can log something here
                                    throw new Exception(
                                        "Please check your entity and query column name or DataType mismatch in this column is " +
                                        propertyInfo.Name + "." + ex.Message);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return obj;
        }

        public static DataTable CreateTable<T>()
        {
            Type entityType = typeof(T);
            var table = new DataTable(entityType.Name.ToUpper());
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            foreach (PropertyDescriptor prop in properties)
            {
                var isType = TypeArray().Contains(prop.PropertyType.Name);
                if (isType)
                {
                    table.Columns.Add(prop.Name.ToUpper(), prop.PropertyType);
                }
            }

            return table;
        }

        private static dynamic ValueConverter(PropertyInfo prop, dynamic value)
        {
            try
            {
                switch (prop.PropertyType.Name)
                {
                    case "Int16":
                        value = Convert.ToInt16(value);
                        break;

                    case "Int32":
                        value = Convert.ToInt32(value);
                        break;

                    case "Int64":
                        value = Convert.ToInt64(value);
                        break;

                    case "Double":
                        value = Convert.ToDouble(value);
                        break;

                    case "Decimal":
                        value = Convert.ToDecimal(value);
                        break;
                    case "SByte":
                        value = Convert.ToSByte(value);
                        break;
                    case "Single":
                        value = Convert.ToSingle(value);
                        break;

                    case "String":
                        string sValue = Convert.ToString(value);
                        value = sValue.Replace("^", "'");

                        break;

                    case "Char":
                        value = Convert.ToChar(value);
                        break;

                    case "Byte":
                        value = Convert.ToByte(value);
                        break;

                    case "Boolean":
                        value = Convert.ToBoolean(value);
                        break;

                    case "DateTime":
                        value = Convert.ToDateTime(value);
                        break;
                }
            }
            catch (Exception)
            {
                return value = string.Empty;
            }
            return value;
        }

        private static string[] TypeArray()
        {
            string[] strArray = new string[]
            {
                "String", "Int32", "Int64", "Double", "Decimal", "Single", "Char", "Byte", "Boolean", "DateTime"
            };

            return strArray;
        }
    }
}
