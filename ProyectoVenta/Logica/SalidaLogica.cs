using ProyectoVenta.Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoVenta.Logica
{
    public class SalidaLogica
    {

        private static SalidaLogica _instancia = null;

        public SalidaLogica()
        {

        }

        public static SalidaLogica Instancia
        {

            get
            {
                if (_instancia == null) _instancia = new SalidaLogica();
                return _instancia;
            }
        }


        public int reducirStock(int idproducto,int cantidad, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update PRODUCTO set Stock = Stock - @pcantidad where IdProducto = @pidproducto");
                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pcantidad", cantidad));
                    cmd.Parameters.Add(new SqlParameter("@pidproducto", idproducto));
                    cmd.CommandType = System.Data.CommandType.Text;
                    respuesta = cmd.ExecuteNonQuery();
                    if (respuesta < 1) {
                        mensaje = "no se puede reducir stock";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = 0;
                mensaje = ex.Message;
            }

            return respuesta;
        }

        public int aumentarStock(int idproducto, int cantidad, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update PRODUCTO set Stock = Stock + @pcantidad where IdProducto = @pidproducto");
                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pcantidad", cantidad));
                    cmd.Parameters.Add(new SqlParameter("@pidproducto", idproducto));
                    cmd.CommandType = System.Data.CommandType.Text;
                    respuesta = cmd.ExecuteNonQuery();
                    if (respuesta < 1)
                    {
                        mensaje = "no se puede aumentar stock";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = 0;
                mensaje = ex.Message;
            }

            return respuesta;
        }


        public int ObtenerCorrelativo(out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select count(*) + 1 from SALIDA");
                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.CommandType = System.Data.CommandType.Text;
                    respuesta = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                    if (respuesta < 1) {
                        mensaje = "No se pudo generar el correlativo";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = 0;
                mensaje = "No se pudo generar el correlativo\nMayor Detalle:\n" + ex.Message;
            }

            return respuesta;
        }


        public int Registrar(Salida obj, out string mensaje)
        {

            mensaje = string.Empty;
            int respuesta = 0;
            SqlTransaction objTransaccion = null;

            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    objTransaccion = conexion.BeginTransaction();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("CREATE TEMP TABLE _TEMP(id INTEGER);");
                    query.AppendLine(string.Format("Insert into SALIDA(NumeroDocumento,FechaRegistro,UsuarioRegistro,DocumentoCliente,NombreCliente,CantidadProductos,MontoTotal) values('{0}','{1}','{2}','{3}','{4}',{5},'{6}');",
                        obj.NumeroDocumento,
                        obj.FechaRegistro,
                        obj.UsuarioRegistro,
                        obj.DocumentoCliente,
                        obj.NombreCliente,
                        obj.CantidadProductos,
                        obj.MontoTotal));

                    query.AppendLine("INSERT INTO _TEMP (id) VALUES (last_insert_rowid());");

                    foreach (DetalleSalida de in obj.olistaDetalle)
                    {
                        query.AppendLine(string.Format("insert into DETALLE_SALIDA(IdSalida,IdProducto,CodigoProducto,DescripcionProducto,CategoriaProducto,AlmacenProducto,PrecioVenta,Cantidad,SubTotal) values({0},{1},'{2}','{3}','{4}','{5}','{6}',{7},'{8}');",
                            "(select id from _TEMP)",
                            de.IdProducto,
                            de.CodigoProducto,
                            de.DescripcionProducto,
                            de.CategoriaProducto,
                            de.AlmacenProducto,
                            de.PrecioVenta,
                            de.Cantidad,
                            de.SubTotal
                            ));

                    }

                    query.AppendLine("DROP TABLE _TEMP;");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Transaction = objTransaccion;
                    respuesta = cmd.ExecuteNonQuery();


                    if (respuesta < 1)
                    {
                        objTransaccion.Rollback();
                        mensaje = "No se pudo registrar la salida de los productos";
                    }

                    objTransaccion.Commit();

                }
                catch (Exception ex)
                {
                    objTransaccion.Rollback();
                    respuesta = 0;
                    mensaje = ex.Message;
                }
            }


            return respuesta;
        }

        public List<VistaSalida> Resumen(string fechainicio = "", string fechafin = "")
        {
            List<VistaSalida> oLista = new List<VistaSalida>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
               
                    query.AppendLine("select e.NumeroDocumento,strftime('%d/%m/%Y', date(e.FechaRegistro))[FechaRegistro],e.UsuarioRegistro,");
                    query.AppendLine("e.DocumentoCliente,e.NombreCliente,e.MontoTotal,");
                    query.AppendLine("de.CodigoProducto,de.DescripcionProducto,de.CategoriaProducto,de.AlmacenProducto,");
                    query.AppendLine("de.PrecioVenta,de.Cantidad,de.SubTotal");
                    query.AppendLine("from SALIDA e");
                    query.AppendLine("inner join DETALLE_SALIDA de on e.IdSalida = de.IdSalida");
                    query.AppendLine("where DATE(e.FechaRegistro) BETWEEN DATE(@pfechainicio) AND DATE(@pfechafin)");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pfechainicio", fechainicio));
                    cmd.Parameters.Add(new SqlParameter("@pfechafin", fechafin));
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new VistaSalida()
                            {
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                FechaRegistro = dr["FechaRegistro"].ToString(),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                DocumentoCliente = dr["DocumentoCliente"].ToString(),
                                NombreCliente = dr["NombreCliente"].ToString(),
                                MontoTotal = dr["MontoTotal"].ToString(),
                                CodigoProducto = dr["CodigoProducto"].ToString(),
                                DescripcionProducto = dr["DescripcionProducto"].ToString(),
                                CategoriaProducto = dr["CategoriaProducto"].ToString(),
                                AlmacenProducto = dr["AlmacenProducto"].ToString(),
                                PrecioVenta = dr["PrecioVenta"].ToString(),
                                Cantidad = dr["Cantidad"].ToString(),
                                SubTotal = dr["SubTotal"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAuditoria.Log.WriteLine(ex.Message);
                oLista = new List<VistaSalida>();
            }
            return oLista;
        }


        public Salida Obtener(string numerodocumento)
        {
            Salida objeto = null;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select IdSalida,NumeroDocumento, strftime('%d/%m/%Y', date(FechaRegistro))[FechaRegistro],UsuarioRegistro,DocumentoCliente,");
                    query.AppendLine("NombreCliente,CantidadProductos,MontoTotal from SALIDA");
                    query.AppendLine("where NumeroDocumento = @pnumero");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pnumero", numerodocumento));
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            objeto = new Salida()
                            {
                                IdSalida = Convert.ToInt32(dr["IdSalida"].ToString()),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                FechaRegistro = dr["FechaRegistro"].ToString(),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                DocumentoCliente = dr["DocumentoCliente"].ToString(),
                                NombreCliente = dr["NombreCliente"].ToString(),
                                CantidadProductos = Convert.ToInt32(dr["CantidadProductos"].ToString()),
                                MontoTotal = dr["MontoTotal"].ToString(),
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAuditoria.Log.WriteLine(ex.Message);
                objeto = null;
            }
            return objeto;
        }

        public List<DetalleSalida> ListarDetalle(int idsalida)
        {
            List<DetalleSalida> oLista = new List<DetalleSalida>();

            try
            {

                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select CodigoProducto, DescripcionProducto, CategoriaProducto,");
                    query.AppendLine("AlmacenProducto, PrecioVenta, Cantidad, SubTotal");
                    query.AppendLine("from DETALLE_SALIDA where IdSalida = @pidsalida");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pidsalida", idsalida));
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new DetalleSalida()
                            {
                                CodigoProducto = dr["CodigoProducto"].ToString(),
                                DescripcionProducto = dr["DescripcionProducto"].ToString(),
                                CategoriaProducto = dr["CategoriaProducto"].ToString(),
                                AlmacenProducto = dr["AlmacenProducto"].ToString(),
                                PrecioVenta = dr["PrecioVenta"].ToString(),
                                Cantidad = Convert.ToInt32(dr["Cantidad"].ToString()),
                                SubTotal = dr["SubTotal"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAuditoria.Log.WriteLine(ex.Message);
                oLista = new List<DetalleSalida>();
            }


            return oLista;
        }






    }
}
