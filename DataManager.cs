using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace DataHelper
{
    public static class DataManager 
    {
        public static DatabaseConnectionString DatabaseConn;
        public static DatabaseConnector Database;

        public static bool DatabaseConnected
        {
            get
            {
                if (Database == null)
                    return false;
                return Database.IsConnected();
            }
        }

        public static bool Open()
        {
            if (DatabaseConn != null)
            {
                Database = new DatabaseConnector(DatabaseConn);
                return Database.Open(DatabaseConn);
            }
            return false;
        }

        public static bool Open(string ConnectionString)
        {
            Database = new DatabaseConnector();
            return Database.Open(ConnectionString);
        }



        public static bool OpenFile(DatabaseConnectionString.ProviderType InpType, string FName)
        {
            DatabaseConn = new DatabaseConnectionString();
            DatabaseConn.SetConnectionLocalFile(InpType, FName);
            Database = new DatabaseConnector(DatabaseConn);
            return Database.Open(DatabaseConn);
        }

        public static List<string> DataTableToStringLists(DataTable Dt, string ColumnName)
        {
            List<string> Res = new List<string>();
            if (Dt != null)
            {
                foreach (DataRow Dr in Dt.Rows)
                {
                    Res.Add(Dr[ColumnName].ToString());
                }
            }
            return Res;
        }


    }
}
