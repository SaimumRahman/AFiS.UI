using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InvPosUtility
{
    public interface ICommonConnection
    {
        void ExecuteNonQuery(string sql);
        void BeginTransaction();
        void CommitTransaction();
        void RollBack();
        void Close();
        DataTable GetDataTable(string sql);
        DataSet GetDataSet(string sql);
        int GetScaler(string sql);
        List<T> Data<T>(string query);


    }
}
