using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using DmShared;

namespace System.Collections
{
    public interface IDataCollection
    {
        int GetInteger(string Name);
        long GetLong(string Name);
        float GetFloat(string Namet);
        double GetDouble(string Name);
        string GetString(string Name);
        bool GetBoolean(string Name);

        int GetInteger(string Name, int Default);
        long GetLong(string Name, long Default);
        float GetFloat(string Name, float Default);
        double GetDouble(string Name, double Default);
        string GetString(string Name, string Default);
        bool GetBoolean(string Name, bool Default);

        bool SetInteger(string Name, int Value);
        bool SetLong(string Name, long Value);
        bool SetFloat(string Name, float Value);
        bool SetDouble(string Name, double Value);
        bool SetString(string Name, string Value);
        bool SetBoolean(string Name, bool Value);

        object GetValue(string Name);
        bool SetValue(string Name, object Value);

        ICollection<string> Keys { get; }

    }

    public class DataCollectionArray : List<DataCollection>
    {
    }

    public class DataCollection : Dictionary<string, object>, IDataCollection
    {
        public static bool DefaultCaseInsensitive = true;

        private bool CaseInsensitive = false;
        public char SeparatorCh = ',';

        public DataCollection()
        {
            CaseInsensitive = DefaultCaseInsensitive;
        }
        public DataCollection(bool IgnoreCase)
        {
            CaseInsensitive = IgnoreCase;
        }

        public void SetDefaultString(string Def)
        {
            this.SetString("__default_string", Def);
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public string Name
        {
            get
            {
                return this.GetString("NAME");
            }
        }

        public DataCollection Value
        {
            get
            {
                return this;
            }
        }

        public override string ToString()
        {
            if (this.ContainsKey("__default_string"))
                return this.GetString("__default_string");

            string Res = "";
            foreach (object Key in base.Keys)
            {
                Res += Key.ToString() + " = " + GetValue(Key.ToString(), "<null>") + "\r\n";
            }
            return Res;
        }

        public new object this[string Key]
        {
            get
            {
                return this.GetValue(Key);
            }

            set
            {
                this.SetValue(Key, value);
            }
        }

        public string ToString(string Format)
        {
            return "";
        }

        public DataCollection ToDataCollection()
        {
            return (DataCollection)this;
        }

        public IDataCollection ToIDataCollection()
        {
            return (IDataCollection)this;
        }

        public object AddDataCollection(IDataCollection Inp)
        {
            foreach (string Key in Inp.Keys)
            {
                SetValue(Key.ToString(), Inp.GetValue(Key));
            }
            return true;
        }

        public int AddDataRow(DataRow Src)
        {
            int Cnt = -1;
            try
            {
                foreach (DataColumn Col in Src.Table.Columns)
                {
                    SetValue(Col.ColumnName, Src[Col]);
                    Cnt++;
                }
            }
            catch
            {
            }
            return Cnt;
        }

        public int AddDataTable(DataTable Src, string ColKeyName, string ColValName)
        {
            int Cnt = -1;
            try
            {
                int RowsCount = Src.Rows.Count;
                int ColKeyIndex = Src.Columns.IndexOf(ColKeyName);
                int ColValIndex = Src.Columns.IndexOf(ColValName);
                for (Cnt = 0; Cnt < RowsCount; Cnt++)
                {
                    DataRow CurRow = Src.Rows[Cnt];

                    SetValue(CurRow[ColKeyIndex].ToString(), CurRow[ColValIndex]);
                }
            }
            catch
            {
            }
            return Cnt;
        }

        public int AddParamPair(params object[] Inp)
        {
            return AddArrayPair(Inp);
        }

        public int AddArrayPair(object[] Inp)
        {
            int Index = 0;
            int Cnt = 0;
            string PropName = null;
            object PropVal = null;

            try
            {
                while ((Index + 2 <= Inp.Length))
                {

                    PropName = Inp[Index].ToString();
                    Index += 1;

                    PropVal = Inp[Index];
                    Index += 1;

                    SetValue(PropName, PropVal);
                    Cnt += 1;
                }
            }
            catch
            {
            }

            return Cnt;
        }




        public int GetSubValueCount(string Key)
        {
            string Value = GetString(Key);
            int Cnt = 0;
            for (int Index = 0; Index <= Value.Length - 1; Index++)
            {
                if ((Value[Index] == SeparatorCh))
                {
                    Cnt = Cnt + 1;
                }
            }
            return Cnt;
        }

        public object GetValue(string Key)
        {
            return GetValue(Key, null);
        }

        public T GetValue<T>(string Key)
        {
            object Res = this.GetValue(Key, null);
            if (Res is T)
                return (T)Res;
            /*if (T is typeof(string))
                return Res.ToString(); */
            return default(T);

        }

        public object GetValue(string Key, object Def)
        {
            if (CaseInsensitive)
                Key = Key.ToUpper();

            if ((base.ContainsKey(Key)))
            {
                return base[Key];
            }
            return Def;
        }

        public DateTime GetDateTime(string Key)
        {
            return GetDateTime(Key, DateTime.MinValue);
        }

        public DateTime GetDateTime(string Key, DateTime Def)
        {
            object Value = GetValue(Key, Def);

            if (Value is DateTime)
                return (DateTime)Value;

            if ((Value == null) || (Value is DBNull))
                return Def;

            DateTime Res = Def;
            DateTime.TryParse(Value.ToString(), out Res);
            return Res;
        }



        public string GetString(string Key)
        {
            return GetString(Key, "");
        }

        public string GetString(string Key, string Def)
        {
            object Res = GetValue(Key, Def);
            if (Res == null)
                return Def;
            if (Res is string)
                return (string)Res;
            if (Res is bool)
                return ((bool)Res ? "TRUE" : "FALSE");
            return Res.ToString();
        }


        #region Integer Conversions
        public int GetInteger(string Key)
        {
            return GetInteger(Key, 0);
        }

        public int GetInteger(string Key, int Def)
        {
            object Val = GetValue(Key, Def);
            if (Val == null)
                return Def;
            if ((Val is bool))
                return ((bool)Val ? 1 : 0);
            if ((Val is int) || (Val is uint) || (Val is ulong) || (Val is long))
                unchecked
                {
                    return (int)Val;
                }
            if ((Val is float) || (Val is double) || (Val is decimal))
                return Convert.ToInt32(Val);
            int Res;
            if (int.TryParse(Val.ToString(), out Res))
                return Res;
            return Def;
        }

        public long GetLong(string Key)
        {
            return GetLong(Key, 0);
        }

        public long GetLong(string Key, long Def)
        {
            object Val = GetValue(Key, Def);
            if (Val == null)
                return Def;
            if ((Val is bool))
                return ((bool)Val ? 1 : 0);
            if ((Val is int) || (Val is uint) || (Val is ulong) || (Val is long))
                unchecked
                {
                    return (long)Val;
                }
            if ((Val is float) || (Val is double) || (Val is decimal))
                return Convert.ToInt64(Val);

            long Res;
            if (long.TryParse(Val.ToString(), out Res))
                return Res;
            return Def;
        }
        #endregion


        #region "Floating Point Conversion"
        public float GetFloat(string Key)
        {
            return GetFloat(Key, 0);
        }

        public float GetFloat(string Key, float Def)
        {
            object Val = GetValue(Key, Def);
            if (Val == null)
                return Def;
            if ((Val is bool))
                return ((bool)Val ? 1 : 0);
            if ((Val is int) || (Val is uint) || (Val is ulong) || (Val is long))
                return Convert.ToSingle(Val);
            if ((Val is float) || (Val is double) || (Val is decimal))
                return Convert.ToSingle(Val);

            float Res;
            if (float.TryParse(Val.ToString(), out Res))
                return Res;
            return Def;
        }


        public double GetDouble(string Key)
        {
            return GetDouble(Key, 0);
        }

        public double GetDouble(string Key, double Def)
        {
            object Val = GetValue(Key, Def);
            if (Val == null)
                return Def;
            if ((Val is bool))
                return ((bool)Val ? 1 : 0);
            if ((Val is int) || (Val is uint) || (Val is ulong) || (Val is long))
                return Convert.ToDouble(Val);
            if (Val is float)
                return Convert.ToDouble(Val);
            if ((Val is double) || (Val is decimal))
                unchecked
                {
                    return (double)Val;
                }

            double Res;
            if (double.TryParse(Val.ToString(), out Res))
                return Res;
            return Def;
        }




        public decimal GetDecimal(string Key)
        {
            return GetDecimal(Key, 0);
        }

        public decimal GetDecimal(string Key, decimal Def)
        {
            object Val = GetValue(Key, Def);
            if (Val == null)
                return Def;
            if ((Val is bool))
                return ((bool)Val ? 1 : 0);
            if ((Val is int) || (Val is uint) || (Val is ulong) || (Val is long))
                return Convert.ToDecimal(Val);
            if ((Val is float) || (Val is double) || (Val is decimal))
                unchecked
                {
                    return (decimal)Val;
                }

            decimal Res;
            if (decimal.TryParse(Val.ToString(), out Res))
                return Res;
            return Def;
        }
        #endregion


        public bool GetBoolean(string Key)
        {
            return GetBoolean(Key, false);
        }

        public bool GetBoolean(string Key, bool Def)
        {
            object Val = GetValue(Key, Def);
            if (Val == null)
                return Def;
            if (Val is bool)
                return (bool)Val;
            if ((Val is int) || (Val is uint) || (Val is ulong) || (Val is long))
                return ((int)Val > 0);
            if ((Val is float) || (Val is double) || (Val is decimal))
                return (Convert.ToSingle(Val) > 0);

            string ResStr = Val.ToString().Trim().ToUpper();

            if (((ResStr.IndexOf("TRUE") >= 0) ||
                (ResStr.IndexOf("YES") >= 0) ||
                (ResStr.IndexOf("ON") >= 0) ||
                (ResStr == "T") ||
                (ResStr == "Y") ||
                (ResStr == "1")))
            {
                return true;
            }
            else if (((ResStr.IndexOf("FALSE") >= 0) ||
                (ResStr.IndexOf("NO") >= 0) ||
                (ResStr.IndexOf("OFF") >= 0) ||
                (ResStr == "F") ||
                (ResStr == "N") ||
                (ResStr == "0")))
            {
                return false;
            }

            return Def;
        }

        public bool Contains(string Key)
        {
            return ContainsKey(Key);
        }

        public new bool ContainsKey(string Key)
        {
            if (CaseInsensitive)
                Key = Key.ToUpper();

            return base.ContainsKey(Key);
        }

        public bool SetValue(string Key, object Value)
        {
            return SetValue(Key, Value, false);
        }

        public bool SetValue(string Key, object Value, bool NoOverwriting)
        {
            if (CaseInsensitive)
                Key = Key.ToUpper();

            if (base.ContainsKey(Key))
            {
                if (NoOverwriting)
                    return false;

                base[Key] = Value;
                return true;
            }
            base.Add(Key, Value);
            return true;
        }



        public bool SetString(string Key, string Val)
        {
            return SetValue(Key, Val);
        }

        public bool SetBoolean(string Key, bool Val)
        {
            return SetValue(Key, Val);
        }

        public bool SetLong(string Key, long Val)
        {
            return SetValue(Key, Val);
        }

        public bool SetInteger(string Key, int Val)
        {
            return SetValue(Key, Val);
        }

        public bool SetFloat(string Key, float Val)
        {
            return SetValue(Key, Val);
        }

        public bool SetDouble(string Key, double Val)
        {
            return SetValue(Key, Val);
        }

        public bool SetDecimal(string Key, decimal Val)
        {
            return SetValue(Key, Val);
        }

        public bool SetDataArrayList(string Key, params object[] Vals)
        {
            return SetValue(Key, DataArrayList.From(Vals));
        }



        public System.Drawing.Font GetFontFromString(string Key, System.Drawing.Font Def)
        {
            string StrRes = GetString(Key, null);
            if (((StrRes != null)))
            {
                string Face = Def.Name;
                float Size = Def.Size;
                string[] Params = StrRes.Split(',');
                System.Drawing.FontStyle Style = System.Drawing.FontStyle.Regular;

                Face = Params[0].Trim();
                if ((Params.Length >= 2))
                {

                    try
                    {
                        Size = float.Parse(Params[1].Trim());
                    }
                    catch
                    {
                    }


                    if ((Params.Length >= 3))
                    {
                        StrRes = Params[2].ToUpper();

                        if ((StrRes.IndexOf("BOLD") >= 0))
                        {
                            Style = System.Drawing.FontStyle.Bold;
                        }
                        else if ((StrRes.IndexOf("ITALIC") >= 0))
                        {
                            Style = System.Drawing.FontStyle.Italic;
                        }
                        else if ((StrRes.IndexOf("UNDERLINE") >= 0))
                        {
                            Style = System.Drawing.FontStyle.Underline;
                        }
                        else if ((StrRes.IndexOf("STRIKEOUT") >= 0))
                        {
                            Style = System.Drawing.FontStyle.Strikeout;
                        }
                    }
                }
                return new System.Drawing.Font(Face, Size, Style);
            }
            return Def;
        }

        public System.Drawing.Color GetColorFromString(string Key, System.Drawing.Color Def)
        {
            string StrRes = GetString(Key, "");
            if (((StrRes != null)))
            {
                string[] StrChl = StrRes.Split(',');

                if ((StrChl.Length >= 3))
                {
                    try
                    {
                        if ((StrChl.Length >= 4))
                        {
                            return System.Drawing.Color.FromArgb(int.Parse(StrChl[3]), int.Parse(StrChl[0]), int.Parse(StrChl[1]), int.Parse(StrChl[2]));
                        }

                        return System.Drawing.Color.FromArgb(int.Parse(StrChl[0]), int.Parse(StrChl[1]), int.Parse(StrChl[2]));
                    }
                    catch
                    {
                        return Def;
                    }
                }

                return System.Drawing.ColorTranslator.FromHtml(StrRes);
            }
            return Def;
        }



        public static DataCollection FromJSON(string Src)
        {
            if (Src != null)
            {
                object Res = JSON.JsonDecode(Src);
                if (Res is DataCollection)
                    return (DataCollection)Res;
            }
            return null;
        }

        public static DataCollection FromPairs(params object[] Values)
        {
            DataCollection Res = new DataCollection();
            Res.AddParamPair(Values);
            return Res;
        }


        #region IDataCollection Members
        public new ICollection<string> Keys
        {
            get { return base.Keys; }
        }
        #endregion

        public bool IsEmpty()
        {
            return (this.Count == 0);
        }

        public bool IsEmpty(string Key)
        {
            if (this.ContainsKey(Key))
            {
                string Res = this.GetString(Key);
                if (Res.Length > 0)
                    return false;

            }
            return true;
        }

        public static DataCollection From(DataCollection Src)
        {
            DataCollection Res = new DataCollection();
            Res.AddDataCollection(Src);
            return Res;
        }

        public static DataCollection FromDataRow(DataRow Src)
        {
            if (Src != null)
            {
                DataCollection Res = new DataCollection();

                for (int Index = 0; Index < Src.Table.Columns.Count; Index++)
                {
                    Res.SetValue(Src.Table.Columns[Index].ColumnName, Src.ItemArray[Index]);
                }

                return Res;
            }
            return null;
        }


    }


}
