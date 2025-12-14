using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Data.SqlClient;

namespace InvPosUtility
{
    public class CommonConnection : ICommonConnection
    {
        public string ConnectionString { get; set; }
        private DbCommand dbCommand;
        private DbConnection dbConnection;
        private IsolationLevel rIsolationLevel;
        private bool isIsolation { get; set; }
        private DbTransaction dbTransaction;
        public IConfiguration Configuration { get; }
        public CommonConnection(IConfiguration configuration)
        {
            Configuration = configuration;
            ConnectionString = Configuration["ConnectionStrings:SqlConnectionString"];
            dbConnection = new SqlConnection(ConnectionString);
            // dbCommand = dbConnection.CreateCommand();
        }

        public void ExecuteNonQuery(string sql)
        {
            dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = sql;
            dbCommand.Connection = dbConnection;
            if (dbTransaction != null)
            {
                dbCommand.Transaction = dbTransaction;
            }
            if (dbConnection.State == ConnectionState.Closed)
                dbConnection.Open();
            dbCommand.ExecuteNonQuery();
            if (dbTransaction == null)
            {
                dbCommand.Dispose();
                dbConnection.Close();
                dbConnection.Dispose();
            }


        }

        public void BeginTransaction()
        {
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            dbTransaction = dbConnection.BeginTransaction(rIsolationLevel);
            dbCommand = dbConnection.CreateCommand();
            dbCommand.Transaction = dbTransaction;
        }



        public void CommitTransaction()
        {
            dbTransaction.Commit();

        }
        public void RollBack()
        {
            dbTransaction.Rollback();
        }

        public void Close()
        {
            if (dbConnection.State == ConnectionState.Open)
                dbConnection.Close();
            if (dbCommand != null) { dbCommand.Dispose(); }

            if (dbTransaction != null)
            {
                dbTransaction.Dispose();
            }



        }

        public DataTable GetDataTable(string sql)
        {
            DataTable returnDT = new DataTable();

            DbDataAdapter adapter = new SqlDataAdapter(sql, ConnectionString);

            adapter.Fill(returnDT);
            adapter.Dispose();

            return returnDT;
        }
        public DataSet GetDataSet(string sql)
        {
            DataSet returnDT = new DataSet();
            DbDataAdapter adapter = new SqlDataAdapter(sql, ConnectionString);
            adapter.Fill(returnDT);
            adapter.Dispose();

            return returnDT;
        }
        public int GetScaler(string sql)
        {
            if (dbConnection.State == ConnectionState.Closed)
                dbConnection.Open();
            if (dbCommand == null)
            {
                dbCommand = new SqlCommand();
            }
            dbCommand.CommandText = sql;
            dbCommand.Connection = dbConnection;
            int returnCount = 0;

            returnCount = Convert.ToInt32(dbCommand.ExecuteScalar());

            if (dbConnection.State == ConnectionState.Open)
                dbConnection.Close();
            return returnCount;
        }

        public List<T> Data<T>(string query)
        {

            DataTable dataTable = GetDataTable(query);

            var objData = (List<T>)ListConversion.ConvertTo<T>(dataTable);
            return objData;

        }





    }
}
