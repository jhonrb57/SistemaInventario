using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoVenta.LogAuditoria
{
    public class Log
    {
        public static void WriteLine(String text)
        {
            try
            {
                string logPath = ConfigurationManager.AppSettings["RutaLog"];

                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string fileName = string.Concat("LogInventario", DateTime.Now.ToString("ddMMyyyy"), ".txt");
                using (StreamWriter sw = new StreamWriter(Path.Combine(logPath, fileName), true))
                {
                    sw.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")));
                    sw.WriteLine("Mensaje: " + text);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
