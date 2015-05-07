using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataHelper
{
    public class UpdatableDataTable : DataTable
    {
        public string CurSqlQuery;
        public System.Data.OleDb.OleDbConnection SqlConn;
        public System.Data.OleDb.OleDbCommandBuilder SqlCmdBld;
        public System.Data.OleDb.OleDbDataAdapter SqlAdapter;

        public delegate void SetLastErrorHandler(Exception Ex);
        public event SetLastErrorHandler SetLastError;

        public UpdatableDataTable(string InpSqlQuery, System.Data.OleDb.OleDbConnection InpSqlConn)
        {
            CurSqlQuery = InpSqlQuery;
            SqlConn = InpSqlConn;
            SqlAdapter = new System.Data.OleDb.OleDbDataAdapter(CurSqlQuery, SqlConn);
            SqlCmdBld = new System.Data.OleDb.OleDbCommandBuilder(SqlAdapter);
            SqlCmdBld.QuotePrefix = "[";
            SqlCmdBld.QuoteSuffix = "]";
            SqlAdapter.Fill(this);
        }

        public bool Reload()
        {
            try
            {
                this.Clear();
                SqlAdapter.Fill(this);
                return true;
            }
            catch (Exception Ex)
            {
                if (SetLastError != null)
                    SetLastError(Ex);
            }
            return false;
        }

        public bool Update()
        {
            try
            {
                SqlAdapter.Update(this);
                return true;
            }
            catch (Exception Ex)
            {
                if (SetLastError != null)
                    SetLastError(Ex);
            }
            return false;
        }
    }

}
