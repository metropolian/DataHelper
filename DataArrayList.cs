using System;
using System.Collections.Generic;
using System.Data;

namespace System.Collections
{
    public class DataArrayList : List<object>
    {
        public void AddItems(params object[] Src)
        {
            foreach (object item in Src)
                base.Add(item);
        }

        public string[] ToArrayString(string Def)
        {
            List<string> res = new List<string>();
            foreach (object item in this)
            {
                if (item == null)
                    res.Add(Def);
                else
                    res.Add(item.ToString());
            }
            return res.ToArray();
        }


        public static DataArrayList From(params object[] Src)
        {
            DataArrayList res = new DataArrayList();
            res.AddItems(Src);
            return res;
        }

        public static DataArrayList FromDataTable(DataTable Tb, string Col)
        {
            DataArrayList res = new DataArrayList();

            foreach (DataRow Tr in Tb.Rows)
            {
                res.Add(Tr[Col]);
            }
            return res;
        }

        public bool RemoveLast()
        {
            if (this.Count > 0)
            {
                this.RemoveAt(this.Count - 1);
                return true;
            }
            return false;
        }

        public string[] ToArrayString()
        {
            List<string> res = new List<string>();
            foreach (object item in this)
            {
                if (item == null)
                    res.Add(null);
                else
                    res.Add(item.ToString());
            }
            return res.ToArray();

        }
    }
}