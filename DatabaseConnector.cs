using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace DataHelper
{

    // BaseEx Database - Database Application Class ........................................................................
    public class DatabaseConnector
    {
        public DatabaseConnectionString ConnString;

        public System.Data.OleDb.OleDbConnection SqlConn;
        public System.Data.OleDb.OleDbCommand SqlCmd;
        public System.Data.OleDb.OleDbCommandBuilder SqlCmdBld;
        public System.Data.OleDb.OleDbDataAdapter SqlAdapter;

        public bool DisableTransx = false;
        public bool EnTransx = false;
        public System.Data.OleDb.OleDbTransaction CurTransx;
        public DataTable Data;

        public event EventHandler OnError;

        public string CurSqlQuery;
        public string MsgErr;

        public DatabaseConnector()
        {
        }

        public DatabaseConnector(DatabaseConnectionString InpConn)
        {
            ConnString = InpConn;
        }

        public DatabaseConnector(DatabaseConnectionString InpConn, bool EnAuto)
        {
            ConnString = InpConn;

            if ((EnAuto))
                Open();
        }

        public void Reset()
        {
            SqlConn = null;
            SqlCmd = null;
            SqlCmdBld = null;
            SqlAdapter = null;
            MsgErr = "";
            CurSqlQuery = "";
        }

        public bool Open()
        {
            return Open(ConnString.ToString());
        }

        public bool Open(DatabaseConnectionString InpConn)
        {
            ConnString = InpConn;
            return Open();
        }

        public bool Open(string CS)
        {
            try
            {
                CurSqlQuery = "Open Connection";
                SqlConn = new System.Data.OleDb.OleDbConnection();
                SqlConn.ConnectionString = CS;
                SqlConn.Open();

                SqlCmd = new System.Data.OleDb.OleDbCommand();
                SqlCmd.Connection = SqlConn;
                SqlCmdBld = new System.Data.OleDb.OleDbCommandBuilder();
                return true;
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return false;
        }

        public bool IsConnected()
        {
            if (((SqlConn != null)))
            {
                if ((SqlConn.State != ConnectionState.Closed))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Close()
        {
            try
            {
                if (((SqlConn != null)))
                {
                    CurSqlQuery = "Close Connection";
                    SqlConn.Close();
                    SqlConn = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return false;
        }

        public bool Transaction_Begin()
        {
            return Transaction_Begin(IsolationLevel.Serializable);
        }

        public bool Transaction_Begin(System.Data.IsolationLevel Level)
        {
            try
            {
                if ((!DisableTransx))
                {
                    CurSqlQuery = "Transaction Begin";
                    CurTransx = SqlConn.BeginTransaction(Level);
                    EnTransx = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return false;
        }


        public bool Transaction_Commit()
        {
            if ((EnTransx))
            {
                try
                {
                    CurSqlQuery = "Transaction Commit";
                    CurTransx.Commit();
                    EnTransx = false;
                    return true;
                }
                catch (Exception ex)
                {
                    SetLastError(ex);
                }
            }
            return false;
        }


        public bool Transaction_Rollback()
        {
            if ((EnTransx))
            {
                try
                {
                    CurSqlQuery = "Transaction Rollback";
                    CurTransx.Rollback();
                    EnTransx = false;
                    return true;
                }
                catch (Exception ex)
                {
                    SetLastError(ex);
                }
            }
            return false;
        }


        // Select and Result as a Single Value
        public object SelectValue(string SqlQuery,  object Def)
        {
            try
            {
                CurSqlQuery = SqlQuery;
                SqlCmd.CommandText = SqlQuery;
                object Res = SqlCmd.ExecuteScalar();
                if (((Res == null) | (Res is System.DBNull)))
                {
                    return Def;
                }
                return Res;
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return Def;
        }

        public UpdatableDataTable RetriveUpdatableDataTable(string SqlQuery)
        {
            try
            {
                CurSqlQuery = SqlQuery;
                UpdatableDataTable Res = new UpdatableDataTable(SqlQuery, SqlConn);
                Res.SetLastError += this.SetLastError;
                MsgErr = "";
                return Res;
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return null;
        }

        // Select and Result as a DataTable Type
        public DataTable SelectTable(string SqlQuery)
        {
            return SelectTable(SqlQuery, "");
        }

        public DataTable SelectTable(string SqlQuery, string TblName)
        {
            try
            {
                CurSqlQuery = SqlQuery;
                SqlAdapter = new System.Data.OleDb.OleDbDataAdapter(SqlQuery, SqlConn);
                Data = new DataTable(TblName);
                SqlAdapter.Fill(Data);
                SqlAdapter.Dispose();
                MsgErr = "";
                return Data;
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return null;
        }

        // Select and Result as a DataRow Type
        public DataRow SelectTableRow(string SqlQuery)
        {
            return SelectTableRow(SqlQuery, 0);
        }

        public DataRow SelectTableRow(string SqlQuery, int Row)
        {
            DataTable ResTable = SelectTable(SqlQuery, null);
            if (((ResTable != null)))
            {
                if (((ResTable.Rows.Count > 0) & (Row < ResTable.Rows.Count)))
                {
                    return ResTable.Rows[Row];
                }
            }
            return null;
        }

        public int ExecCommand(string SqlQuery)
        {
            try
            {
                CurSqlQuery = SqlQuery;
                if ((EnTransx))
                {
                    SqlCmd.Transaction = CurTransx;
                }
                else
                {
                    SqlCmd.Transaction = null;
                }

                SqlCmd.CommandType = CommandType.Text;
                SqlCmd.CommandText = SqlQuery;
                MsgErr = "";
                return SqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return 0;
        }



        public int Insert_Objects(string TableName, params object[] Inps)
        {
            return Insert_Values(TableName, GetPairedObjToSqlStr(Inps));
        }

        public int Insert_Values(string TableName, params string[] Inps)
        {
            int MaxArgs;
            string SqlQuery;
            string Names;
            string Vals;
            Names = "";
            Vals = "";
            MaxArgs = Inps.Length - 1;
            if ((MaxArgs >= 1))
            {
                for (int Index = 0; Index <= MaxArgs; Index++)
                {
                    if ((Index % 2) == 0)
                    {
                        if ((Index == MaxArgs))
                        {
                            break; // TODO: might not be correct. Was : Exit For
                        }
                        Names += Inps[Index];

                        if ((Index < MaxArgs - 2))
                        {
                            Names += ",";
                        }
                    }
                    else
                    {
                        Vals += Inps[Index];
                        if ((Index < MaxArgs - 1))
                        {
                            Vals += ",";
                        }
                    }
                }
                SqlQuery = "Insert into " + TableName + " (" + Names + ") Values (" + Vals + ")";
                try
                {
                    return ExecCommand(SqlQuery);
                }
                catch (Exception ex)
                {
                    SetLastError(ex);
                }
            }
            return 0;
        }

        public int Update_Objects(string TableName, object[] Inps, string Cond)
        {
            List<object> Src = new List<object>(Inps);
            Src.Add(Cond);
            return Update_Objects(TableName, Src.ToArray());
        }

        public int Update_Objects(string TableName, params object[] Inps)
        {
            return Update_Values(TableName, GetPairedObjToSqlStr(Inps));
        }

        public int Update_Values(string TableName, params string[] Inps)
        {
            int MaxArgs = Inps.Length - 1;
            string SqlQuery;
            string Vals;
            Vals = "";
            if ((MaxArgs >= 1))
            {
                for (int Index = 0; Index <= MaxArgs; Index++)
                {
                    if ((Index % 2) == 0)
                    {
                        // even
                        if ((Index == MaxArgs))
                        {
                            Vals += " " + Inps[Index];
                            break; // TODO: might not be correct. Was : Exit For
                        }
                        Vals += Inps[Index] + " = ";
                    }
                    else
                    {
                        // odd
                        Vals += Inps[Index];
                        if ((Index < MaxArgs - 1))
                        {
                            Vals += ", ";
                        }
                    }
                }
                SqlQuery = "Update " + TableName + " set " + Vals;
                try
                {
                    return ExecCommand(SqlQuery);
                }
                catch (Exception ex)
                {
                    SetLastError(ex);
                }
            }
            return 0;
        }


        public int Delete_From(string TableName, string Where)
        {
            string SqlQuery = string.Format("Delete From {0} Where {1} ", TableName, Where);
            try
            {
                return ExecCommand(SqlQuery);
            }
            catch (Exception ex)
            {
                SetLastError(ex);
            }
            return 0;
        }


        public bool ClearTable(string TableName)
        {
            return TrancateTable(TableName);
        }

        public bool TrancateTable(string TableName)
        {
            int Res = ExecCommand("Truncate Table " + TableName);
            return (Res > 0) | (Res == -1);
        }




        public object GetDateTimeNowToSqlStr()
        {
            return GetDateTimeToSqlStr(DateTime.Now);
        }

        public char SqlQuote = '\'';

        public string GetDateTimeToSqlStr(DateTime Inp, bool Standard)
        {
            string res;
            if (Standard)
                res = Inp.ToString("s");
            else
                res = string.Format("{2:0000}-{1:00}-{0:00} {3:00}:{4:00}:{5:00}", Inp.Day, Inp.Month, Inp.Year, Inp.Hour, Inp.Minute, Inp.Second);

            if (ConnString != null)
            {
                if (ConnString.Provider == DatabaseConnectionString.ProviderType.MSJet)
                    return "#" + res + "#";
                if (ConnString.Provider == DatabaseConnectionString.ProviderType.MSSQL_Server)
                    return "'" + res + "'";
            }
            return SqlQuote + res + SqlQuote;
        }

        public string GetDateTimeToSqlStr(DateTime Inp)
        {
            return GetDateTimeToSqlStr(Inp, true);
        }

        public string[] GetPairedObjToSqlStr(object[] Inps)
        {
            int MaxArgs = Inps.Length - 1;
            string[] Res = new string[MaxArgs + 1];
            object InpVal;
            string ResVal;

            for (int Index = 0; Index <= MaxArgs; Index++)
            {

                InpVal = Inps[Index];
                ResVal = "";
                if (((Index % 2) == 0))
                {
                    ResVal = InpVal.ToString();
                }
                else
                {
                    if ((Inps[Index] == null))
                    {
                        ResVal = "null";
                    }
                    else
                    {
                        if ((InpVal is string))
                        {
                            ResVal = "'" + (string)InpVal + "'";
                        }
                        else if ((InpVal is char))
                        {
                            ResVal = "'" + (string)InpVal + "'";
                        }
                        else if ((InpVal is DateTime))
                        {
                            if ((InpVal == null))
                            {
                                ResVal = "null";
                            }
                            else
                            {
                                ResVal = GetDateTimeToSqlStr((DateTime)InpVal, false);
                            }
                        }

                        else
                        {
                            ResVal = InpVal.ToString();
                        }
                    }

                }
                Res[Index] = ResVal;
            }
            return Res;
        }

        public string GetLastError()
        {
            return MsgErr;
        }

        public void SetLastError(Exception Ex)
        {
            MsgErr = "Command: " + CurSqlQuery + "\r\n" + Ex.Message + "\r\n" + " (" + Ex.Source + ") " + "\r\n";
            if (OnError != null)
            {
                EventArgs a = new EventArgs();                
                OnError(this, a);
            }
        }

    }


}
