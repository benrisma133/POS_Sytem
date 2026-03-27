using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace POS_WPF.Helpers
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.txt");

        public static void Log(Exception ex)
        {
            try
            {

                string technicalMessage = ex.InnerException != null
                ? ex.InnerException.Message // الرسالة التقنية القصيرة
                : ex.Message;

                File.AppendAllText(
                    LogPath,
                    $"\n[{DateTime.Now}]\n{ex}\n======================================================\n"
                );
            }
            catch
            {
                // ما نطيّحوش البرنامج إلا ما قدرناش نسجلو
            }
        }

        public static void Log(string ex)
        {
            try
            {


                File.AppendAllText(
                    LogPath,
                    $"\n[{DateTime.Now}]\n{ex}\n======================================================\n"
                );
            }
            catch
            {
                // ما نطيّحوش البرنامج إلا ما قدرناش نسجلو
            }
        }
    }

}
