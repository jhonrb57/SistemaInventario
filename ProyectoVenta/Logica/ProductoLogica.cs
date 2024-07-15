using ProyectoVenta.Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoVenta.Logica
{
    public class ProductoLogica
    {
        private static ProductoLogica _instancia = null;

        public ProductoLogica()
        {

        }

        public static ProductoLogica Instancia
        {

            get
            {
                if (_instancia == null) _instancia = new ProductoLogica();
                return _instancia;
            }
        }


        public List<Producto> Listar(out string mensaje)
        {
            mensaje = string.Empty;
            List<Producto> oLista = new List<Producto>();

            try
            {

                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();

                    string query = "select IdProducto,Codigo,Descripcion,Categoria,Almacen,Stock,PrecioVenta from PRODUCTO;";
                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new Producto()
                            {
                                IdProducto = int.Parse(dr["IdProducto"].ToString()),
                                Codigo = dr["Codigo"].ToString(),
                                Descripcion = dr["Descripcion"].ToString(),
                                Categoria = dr["Categoria"].ToString(),
                                Almacen = dr["Almacen"].ToString(),
                                Stock = Convert.ToInt32(dr["Stock"].ToString()),
                                PrecioVenta = dr["PrecioVenta"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oLista = new List<Producto>();
                mensaje = ex.Message;
            }


            return oLista;
        }

        public int Existe(string codigo, int defaultid, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select count(*)[resultado] from PRODUCTO where upper(Codigo) = upper(@pcodigo) and IdProducto != @defaultid");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pcodigo", codigo));
                    cmd.Parameters.Add(new SqlParameter("@defaultid", defaultid));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                    if (respuesta > 0)
                        mensaje = "El codigo de producto ya existe";

                }
                catch (Exception ex)
                {
                    respuesta = 0;
                    mensaje = ex.Message;
                }

            }
            return respuesta;
        }

        public int Guardar(Producto objeto, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            
            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {

                    conexion.Open();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("insert into PRODUCTO(Codigo,Descripcion,Categoria,Almacen) values (@pcodigo,@pdescripcion,@pcategoria,@palmacen);");
                    query.AppendLine("select last_insert_rowid();");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pcodigo", objeto.Codigo));
                    cmd.Parameters.Add(new SqlParameter("@pdescripcion", objeto.Descripcion));
                    cmd.Parameters.Add(new SqlParameter("@pcategoria", objeto.Categoria));
                    cmd.Parameters.Add(new SqlParameter("@palmacen", objeto.Almacen));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                    if (respuesta < 1)
                        mensaje = "No se pudo registrar el producto";
                }
                catch (Exception ex)
                {
                    respuesta = 0;
                    mensaje = ex.Message;
                }
            }

            return respuesta;
        }

        public int Editar(Producto objeto, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;

            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {

                    conexion.Open();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("update PRODUCTO set Codigo = @pcodigo,Descripcion = @pdescripcion,Categoria =@pcategoria ,Almacen = @palmacen where IdProducto = @pidproducto");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pidproducto", objeto.IdProducto));
                    cmd.Parameters.Add(new SqlParameter("@pcodigo", objeto.Codigo));
                    cmd.Parameters.Add(new SqlParameter("@pdescripcion", objeto.Descripcion));
                    cmd.Parameters.Add(new SqlParameter("@pcategoria", objeto.Categoria));
                    cmd.Parameters.Add(new SqlParameter("@palmacen", objeto.Almacen));
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
                    query.AppendLine("delete from PRODUCTO where IdProducto= @id;");
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
