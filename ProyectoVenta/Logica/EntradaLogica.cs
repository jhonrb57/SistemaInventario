using ProyectoVenta.Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoVenta.Logica
{
    public class EntradaLogica
    {

        private static EntradaLogica _instancia = null;

        public EntradaLogica()
        {

        }

        public static EntradaLogica Instancia
        {

            get
            {
                if (_instancia == null) _instancia = new EntradaLogica();
                return _instancia;
            }
        }


        public int Existe(string numerodocumento, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select count(*)[resultado] from ENTRADA where upper(NumeroDocumento) = upper(@pnumero)");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pnumero", numerodocumento));
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


        public int Registrar(Entrada obj, out string mensaje)
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
                    query.AppendLine(string.Format("Insert into ENTRADA(NumeroDocumento,FechaRegistro,UsuarioRegistro,DocumentoProveedor,NombreProveedor,CantidadProductos,MontoTotal) values('{0}','{1}','{2}','{3}','{4}',{5},'{6}');",
                        obj.NumeroDocumento,
                        obj.FechaRegistro,
                        obj.UsuarioRegistro,
                        obj.DocumentoProveedor,
                        obj.NombreProveedor,
                        obj.CantidadProductos,
                        obj.MontoTotal));

                    query.AppendLine("INSERT INTO _TEMP (id) VALUES (last_insert_rowid());");

                    foreach (DetalleEntrada de in obj.olistaDetalle)
                    {
                        query.AppendLine(string.Format("insert into DETALLE_ENTRADA(IdEntrada,IdProducto,CodigoProducto,DescripcionProducto,CategoriaProducto,AlmacenProducto,PrecioCompra,PrecioVenta,Cantidad,SubTotal) values({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}',{8},'{9}');",
                            "(select id from _TEMP)",
                            de.IdProducto,
                            de.CodigoProducto,
                            de.DescripcionProducto,
                            de.CategoriaProducto,
                            de.AlmacenProducto,
                            de.PrecioCompra,
                            de.PrecioVenta,
                            de.Cantidad,
                            de.SubTotal
                            ));

                        query.AppendLine(string.Format("UPDATE PRODUCTO set PrecioCompra = '{0}', PrecioVenta = '{1}', Stock = (Stock + {2}) where IdProducto = {3};",de.PrecioCompra,de.PrecioVenta,de.Cantidad,de.IdProducto));
                    }

                    query.AppendLine("DROP TABLE _TEMP;");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Transaction = objTransaccion;
                    respuesta = cmd.ExecuteNonQuery();


                    if (respuesta < 1)
                    {
                        objTransaccion.Rollback();
                        mensaje = "No se pudo registrar la entrada de los productos";
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

        public List<VistaEntradas> Resumen(string fechainicio = "", string fechafin = "")
        {
            List<VistaEntradas> oLista = new List<VistaEntradas>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("select e.NumeroDocumento,strftime('%d/%m/%Y', date(e.FechaRegistro))[FechaRegistro],e.UsuarioRegistro,");
                    query.AppendLine("e.DocumentoProveedor,e.NombreProveedor,e.MontoTotal,");
                    query.AppendLine("de.CodigoProducto,de.DescripcionProducto,de.CategoriaProducto,de.AlmacenProducto,de.PrecioCompra,");
                    query.AppendLine("de.PrecioVenta,de.Cantidad,de.SubTotal");
                    query.AppendLine("from ENTRADA e");
                    query.AppendLine("inner join DETALLE_ENTRADA de on e.IdEntrada = de.IdEntrada");
                    query.AppendLine("where DATE(e.FechaRegistro) BETWEEN DATE(@pfechainicio) AND DATE(@pfechafin)");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pfechainicio", fechainicio));
                    cmd.Parameters.Add(new SqlParameter("@pfechafin", fechafin));
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new VistaEntradas()
                            {
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                FechaRegistro = dr["FechaRegistro"].ToString(),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                DocumentoProveedor = dr["DocumentoProveedor"].ToString(),
                                NombreProveedor = dr["NombreProveedor"].ToString(),
                                MontoTotal = dr["MontoTotal"].ToString(),
                                CodigoProducto = dr["CodigoProducto"].ToString(),
                                DescripcionProducto = dr["DescripcionProducto"].ToString(),
                                CategoriaProducto = dr["CategoriaProducto"].ToString(),
                                AlmacenProducto = dr["AlmacenProducto"].ToString(),
                                PrecioCompra = dr["PrecioCompra"].ToString(),
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
                oLista = new List<VistaEntradas>();
            }
            return oLista;
        }


        public Entrada Obtener(string numerodocumento)
        {
            Entrada objeto = null;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select IdEntrada,NumeroDocumento, strftime('%d/%m/%Y', date(FechaRegistro))[FechaRegistro],UsuarioRegistro,DocumentoProveedor,");
                    query.AppendLine("NombreProveedor,CantidadProductos,MontoTotal from ENTRADA");
                    query.AppendLine("where NumeroDocumento = @pnumero");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pnumero", numerodocumento));
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            objeto = new Entrada()
                            {
                                IdEntrada = Convert.ToInt32(dr["IdEntrada"].ToString()),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                FechaRegistro = dr["FechaRegistro"].ToString(),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                DocumentoProveedor = dr["DocumentoProveedor"].ToString(),
                                NombreProveedor = dr["NombreProveedor"].ToString(),
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

        public List<DetalleEntrada> ListarDetalle(int identrada)
        {
            List<DetalleEntrada> oLista = new List<DetalleEntrada>();

            try
            {

                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select CodigoProducto, DescripcionProducto, CategoriaProducto,");
                    query.AppendLine("AlmacenProducto, PrecioCompra, PrecioVenta, Cantidad, SubTotal");
                    query.AppendLine("from DETALLE_ENTRADA where IdEntrada = @pidentrada");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pidentrada", identrada));
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new DetalleEntrada()
                            {
                                CodigoProducto = dr["CodigoProducto"].ToString(),
                                DescripcionProducto = dr["DescripcionProducto"].ToString(),
                                CategoriaProducto = dr["CategoriaProducto"].ToString(),
                                AlmacenProducto = dr["AlmacenProducto"].ToString(),
                                PrecioCompra = dr["PrecioCompra"].ToString(),
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
                oLista = new List<DetalleEntrada>();
            }


            return oLista;
        }






    }
}
