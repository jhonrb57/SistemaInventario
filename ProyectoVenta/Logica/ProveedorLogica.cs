using ProyectoVenta.Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoVenta.Logica
{
    public class ProveedorLogica
    {

        private static ProveedorLogica _instancia = null;

        public ProveedorLogica()
        {

        }

        public static ProveedorLogica Instancia
        {

            get
            {
                if (_instancia == null) _instancia = new ProveedorLogica();
                return _instancia;
            }
        }


        public List<Proveedor> Listar(out string mensaje)
        {
            mensaje = string.Empty;
            List<Proveedor> oLista = new List<Proveedor>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    string query = "select IdProveedor,NumeroDocumento,NombreCompleto from PROVEEDOR;";
                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new Proveedor()
                            {
                                IdProveedor = int.Parse(dr["IdProveedor"].ToString()),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                NombreCompleto = dr["NombreCompleto"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oLista = new List<Proveedor>();
                mensaje = ex.Message;
            }
            return oLista;
        }

        public int Existe(string numero, int defaultid, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select count(*)[resultado] from PROVEEDOR where upper(NumeroDocumento) = upper(@pnumero) and IdProveedor != @defaultid");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pnumero", numero));
                    cmd.Parameters.Add(new SqlParameter("@defaultid", defaultid));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                    if (respuesta > 0)
                        mensaje = "El numero de documento ya existe";

                }
                catch (Exception ex)
                {
                    respuesta = 0;
                    mensaje = ex.Message;
                }

            }
            return respuesta;
        }

        public int Guardar(Proveedor objeto, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;

            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {

                    conexion.Open();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("insert into PROVEEDOR(NumeroDocumento,NombreCompleto) values (@pnumero,@pnombre);");
                    query.AppendLine("select last_insert_rowid();");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pnumero", objeto.NumeroDocumento));
                    cmd.Parameters.Add(new SqlParameter("@pnombre", objeto.NombreCompleto));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                    if (respuesta < 1)
                        mensaje = "No se pudo registrar el proveedor";
                }
                catch (Exception ex)
                {
                    respuesta = 0;
                    mensaje = ex.Message;
                }
            }

            return respuesta;
        }

        public int Editar(Proveedor objeto, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;

            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update PROVEEDOR set NumeroDocumento = @pnumero,NombreCompleto = @pnombre where IdProveedor = @pidproveedor");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pidproveedor", objeto.IdProveedor));
                    cmd.Parameters.Add(new SqlParameter("@pnumero", objeto.NumeroDocumento));
                    cmd.Parameters.Add(new SqlParameter("@pnombre", objeto.NombreCompleto));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = cmd.ExecuteNonQuery();
                    if (respuesta < 1)
                        mensaje = "No se pudo editar el producto";
                }
                catch (Exception ex)
                {
                    respuesta = 0;
                    mensaje = ex.Message;
                }
            }

            return respuesta;
        }


        public int Eliminar(int id)
        {
            int respuesta = 0;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("delete from PROVEEDOR where IdProveedor = @id;");
                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.CommandType = System.Data.CommandType.Text;
                    respuesta = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogAuditoria.Log.WriteLine(ex.Message);
                respuesta = 0;
            }
            return respuesta;
        }

    }
}
