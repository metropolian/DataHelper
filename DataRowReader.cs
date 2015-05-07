using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace DataHelper
{
    // DataRow Reader - For DataRow Safty Reading Value .............................................................
    public class DataRowReader
    {
        public DataRow CurDataRow;

        public DataRowReader(DataRow Inp)
        {
            CurDataRow = Inp;
        }

        public object GetData(string Field, object Def)
        {
            try
            {
                object Res = CurDataRow[Field];
                if (((Res == null) | (Res is System.DBNull)))
                    return Def;
                return Res;
            }
            catch { }
            return Def;
        }

        public object GetData(int Field, object Def)
        {
            try
            {
                object Res = CurDataRow[Field];
                if (((Res == null) | (Res is System.DBNull)))
                    return Def;
                return Res;
            }
            catch { }
            return Def;
        }

        public string GetString(int Field, string Def)
        {
            return GetData(Field, Def).ToString();
        }

        public string GetString(string Field, string Def)
        {
            return GetData(Field, Def).ToString();
        }

        public string GetString(string Field)
        {
            return GetString(Field, "");
        }
    }

}
