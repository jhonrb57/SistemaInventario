using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoVenta.Logica
{
    public class Conexion:DbContext
    {
        public static string BDInventario = ConfigurationManager.ConnectionStrings["BDInventario"].ToString();
    }
}
