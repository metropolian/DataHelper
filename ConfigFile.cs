using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DmShared;

namespace DataHelper
{
    public static class ConfigFile
    {
        public static string LoadDataFromUrl(string Url)
        {
            System.Net.WebClient req = new System.Net.WebClient();

            StreamReader reqstream = new StreamReader(req.OpenRead(Url));

            string res = reqstream.ReadToEnd();

            reqstream.Close();
            req.Dispose();


            return res;
        }

        public static bool SaveFile(string FName, string Data)
        {
            try
            {
                StreamWriter F = new StreamWriter(FName);
                F.Write(Data);
                F.Close();
                return true;
            }
            catch
            {
            }
            return false;
        }

        public static DataCollection LoadDataCollectionFile(string FName)
        {
            DataCollection res = new DataCollection();
            StreamReader F = new StreamReader(FName);
            res = LoadDataCollection(F.ReadToEnd());
            F.Close();
            return res;
        }

        public static DataCollection LoadDataCollection(string Data)
        {
            object r = JSON.JsonDecode(Data);
            if (r is DataCollection)
                return (DataCollection)r;
            return null;
        }
    }
}
