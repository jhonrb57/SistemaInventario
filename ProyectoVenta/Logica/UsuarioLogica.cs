using ProyectoVenta.LogAuditoria;
using ProyectoVenta.Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace ProyectoVenta.Logica
{
    public class UsuarioLogica
    {
        private static UsuarioLogica _instancia = null;

        public UsuarioLogica()
        {

        }

        public static UsuarioLogica Instancia
        {
            get
            {
                if (_instancia == null) _instancia = new UsuarioLogica();
                return _instancia;
            }
        }

        public int resetear()
        {
            int respuesta = 0;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update USUARIO set NombreUsuario = 'Admin', Clave = '123' where IdUsuario = 1;");
                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
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


        public List<Usuario> Listar(out string mensaje)
        {
            mensaje = string.Empty;
            List<Usuario> oLista = new List<Usuario>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    

                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select u.IdUsuario,u.NombreCompleto,u.NombreUsuario,u.Clave,u.IdPermisos,p.Descripcion from USUARIO u");
                    query.AppendLine("inner join PERMISOS p on p.IdPermisos = u.IdPermisos;");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new Usuario()
                            {
                                IdUsuario = int.Parse(dr["IdUsuario"].ToString()),
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                NombreUsuario = dr["NombreUsuario"].ToString(),
                                Clave = dr["Clave"].ToString(),
                                IdPermisos = Convert.ToInt32(dr["IdPermisos"].ToString()),
                                Descripcion = dr["Descripcion"].ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oLista = new List<Usuario>();
                mensaje = ex.Message;
            }
            return oLista;
        }

        public int Existe(string usuario, int defaultid, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select count(*)[resultado] from USUARIO where upper(NombreUsuario) = upper(@pnombreusuario)");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pnombreusuario", usuario));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                    if (respuesta > 0)
                        mensaje = "El usuario ya existe";

                }
                catch (Exception ex)
                {
                    //mensaje = "No se pudo registrar el usuario, para \nmayor de talle revise el log de la \naplicación";
                    MessageBox.Show("No se pudo registrar el usuario, para mayor detalle revise el log de la aplicación", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.WriteLine(ex.Message);
                }

            }
            return respuesta;
        }

        public int AsignarId(out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select MAX(IdUsuario) from USUARIO");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = Convert.ToInt32(cmd.ExecuteScalar().ToString()) + 1;
                }
                catch (Exception ex)
                {
                    respuesta = 0;
                    mensaje = ex.Message;
                }

            }
            return respuesta;
        }

        public int Guardar(Usuario objeto, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;

            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("insert into USUARIO(IdUsuario,NombreCompleto,NombreUsuario,Clave,IdPermisos) values (@pidusuario,@pnombrecompleto,@pnombreusuario,@pclave,@pidpermisos);");
                    //query.AppendLine("select last_insert_rowid();");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pidusuario", objeto.IdUsuario));
                    cmd.Parameters.Add(new SqlParameter("@pnombrecompleto", objeto.NombreCompleto));
                    cmd.Parameters.Add(new SqlParameter("@pnombreusuario", objeto.NombreUsuario));
                    cmd.Parameters.Add(new SqlParameter("@pclave", objeto.Clave));
                    cmd.Parameters.Add(new SqlParameter("@pidpermisos", objeto.IdPermisos));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = cmd.ExecuteNonQuery();
                    if (respuesta < 1)
                        mensaje = "No se pudo registrar el usuario";
                }
                catch (Exception ex)
                {
                    //mensaje = "No se pudo registrar el usuario, para \nmayor de talle revise el log de la \naplicación";
                    MessageBox.Show("No se pudo registrar el usuario, para mayor detalle revise el log de la aplicación", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.WriteLine(ex.Message);
                }
            }

            return respuesta;
        }

        public int Editar(Usuario objeto, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;

            using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
            {
                try
                {
                    conexion.Open();
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update USUARIO set NombreCompleto = @pnombrecompleto,Clave = @pclave,IdPermisos = @pidpermisos  where NombreUsuario = @pnombreusuario");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@pnombrecompleto", objeto.NombreCompleto));
                    cmd.Parameters.Add(new SqlParameter("@pnombreusuario", objeto.NombreUsuario));
                    cmd.Parameters.Add(new SqlParameter("@pclave", objeto.Clave));
                    cmd.Parameters.Add(new SqlParameter("@pidpermisos", objeto.IdPermisos));
                    cmd.Parameters.Add(new SqlParameter("@pid", objeto.IdUsuario));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = cmd.ExecuteNonQuery();
                    if (respuesta < 1)
                        mensaje = "No se pudo editar el usuario";
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
                    query.AppendLine("delete from USUARIO where IdUsuario= @id;");
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
