using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_DAL
{
    public static class clsDataAccessSettigs
    {

        //public static string ConnectionString = @"Server=.;
        //                                    Database=POS2;
        //                                    User Id=sa;
        //                                    Password=sa123456;
        //                                    TrustServerCertificate=True;";

        //public static string ConnectionString = @"Server=(localdb)\POS2LocalDB;
        //                                 AttachDbFilename=|DataDirectory|\Data\POS2.mdf;
        //                                 Integrated Security=True;
        //                                 Connect Timeout=30;";

        //    public static string ConnectionString =
        //@"Data Source=C:\Users\DELL\Desktop\My Prog. Career Path\Projects\C#\WinForms\POS\POS_DAL\Data\POS.db;";

        public static string ConnectionString =
    $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "POS.db")};";





    }
}
