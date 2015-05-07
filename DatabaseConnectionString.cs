using System;
using System.Collections.Generic;
using System.Text;

namespace DataHelper
{
    public class DatabaseConnectionString
    {
        public enum ProviderType
        {
            None = 0,
            MSJet = 1,
            MySQL = 2,
            ODBC = 3,
            MSOLAP = 4,
            MSOracle = 5,
            Oracle = 6,
            MSSQL_Server = 7
        }

        public ProviderType Provider;

        public string ProviderFullname = "";
        public string Server = "(local)";
        public string Username = "";
        public string Password = "";
        public string Catalog = "";
        public int PacketSize = 0;
        public int ConnectionTimeout = 10;
        public bool UseTrustedConnection = false;
        public bool UsePersistSecurityInfo = false;

        public DatabaseConnectionString()
        {
        }

        public DatabaseConnectionString(string InpServer, string InpUsername, string InpPassword, string InpCatalog)
        {
            SetConnectionSqlServer(InpServer, InpUsername, InpPassword, InpCatalog);
        }

        public void SetProvider(ProviderType InpType)
        {
            Provider = InpType;

            switch (InpType)
            {
                case ProviderType.MSJet:
                    ProviderFullname = "Microsoft.Jet.OLEDB.4.0";
                    break;
                case ProviderType.MySQL:
                    break;
                case ProviderType.ODBC:
                    break;
                case ProviderType.MSOLAP:
                    break;
                case ProviderType.MSOracle:
                    break;
                case ProviderType.Oracle:
                    break;
                case ProviderType.MSSQL_Server:
                    ProviderFullname = "SQLNCLI11";
                    break;
                default:
                    Provider = ProviderType.None;
                    break;
            }

        }

        public void SetConnectionLocalFile(ProviderType InpType, string FName)
        {
            SetProvider(InpType);
            Server = FName;
        }

        public void SetConnectionSqlServer(string InpServer, string InpUsername, string InpPassword, string InpCatalog)
        {
            SetProvider(ProviderType.MSSQL_Server);
            Server = InpServer;
            Username = InpUsername;
            Password = InpPassword;
            Catalog = InpCatalog;
        }

        public override string ToString()
        {
            string Res = string.Format("data source={0};user id={1};password={2};", Server, Username, Password);

            if (Catalog.Length > 0)
                Res += string.Format("initial catalog={0};", Catalog);

            if (ProviderFullname.Length > 0)
                Res += string.Format("provider={0};", ProviderFullname);

            if ((UseTrustedConnection))
                Res += "Trusted_Connection=true";

            if ((UsePersistSecurityInfo))
                Res += "Persist Security Info=true";

            if ((PacketSize > 0))
                Res += "Packet Size=" + PacketSize.ToString();
            return Res;
        }

    }

}
