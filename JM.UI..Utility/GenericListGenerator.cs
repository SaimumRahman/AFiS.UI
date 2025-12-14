using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InvPosUtility
{
    public class GenericListGenerator
    {
        public static IList<T> GetList<T>(DataTable table)
        {
            if (table == null)
            {
                return null;
            }

            var rows = table.Rows.Cast<DataRow>().ToList();

            return ConvertTo<T>(rows);
        }

        private static IList<T> ConvertTo<T>(IList<DataRow> rows)
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

        private static T CreateItem<T>(DataRow row)
        {
            T obj = default;
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();


                var properties = obj.GetType().GetProperties();
                bool getType;
                foreach (var propertyInfo in properties)
                {
                    PropertyInfo prop = propertyInfo;
                    var objChild = new object();

                    getType = TypeArray().Contains(prop.PropertyType.Name);
                    if (!getType)
                    {
                        Type type = prop.PropertyType;
                        if (type.FullName == "System.Byte[]")
                        {
                            continue;
                        }
                        objChild = Activator.CreateInstance(type);
                    }

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
                                        if (!getType && propertyInfo.PropertyType.BaseType != null && propertyInfo.PropertyType.BaseType.Name == "Object")
                                        {

                                        }
                                        else
                                        {
                                            if (prop.GetSetMethod() != null)
                                            {
                                                dynamic value = row[column.ColumnName];
                                                value = ValueConverter(prop, value);
                                                prop.SetValue(obj, value);
                                            }

                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    // You can log something here
                                    throw new Exception("Please check your entity and query column name " + ex.Message);
                                }
                                break;
                            }

                            if (!getType)
                            {
                                var propties = objChild.GetType().GetProperties();
                                foreach (var prInf in propties)
                                {
                                    if (prInf.Name.ToLower() == column.ColumnName.ToLower())
                                    {
                                        try
                                        {
                                            if (row[column.ColumnName] != DBNull.Value)
                                            {
                                                dynamic value = row[column.ColumnName];
                                                value = ValueConverter(prInf, value);
                                                prInf.SetValue(objChild, value);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            // You can log something here
                                            //throw new Exception("Please check your entity and query column name " + ex.Message);
                                        }
                                        break;
                                    }
                                }

                                prop.SetValue(obj, objChild);
                            }
                        }
                    }
                }
            }

            return obj;
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
                        value = Convert.ToString(value);
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