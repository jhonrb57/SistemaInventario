﻿using ProyectoVenta.Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoVenta.Logica
{
    public class DatoLogica
    {

        private static DatoLogica _instancia = null;

        public DatoLogica()
        {

        }

        public static DatoLogica Instancia
        {

            get
            {
                if (_instancia == null) _instancia = new DatoLogica();
                return _instancia;
            }
        }

   

        public Datos Obtener()
        {
            Datos obj = new Datos();
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    string query = "select IdDato, RazonSocial, RUC, Direccion from DATOS where IdDato = 1";
                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            obj = new Datos()
                            {
                                IdDato = int.Parse(dr["IdDato"].ToString()),
                                RazonSocial = dr["RazonSocial"].ToString(),
                                RUC = dr["RUC"].ToString(),
                                Direccion = dr["Direccion"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAuditoria.Log.WriteLine(ex.Message);
                obj = new Datos();
            }
            return obj;
        }

        public int Guardar(Datos objeto, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            try
            {

                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {

                    conexion.Open();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("update DATOS set RazonSocial = @prazonsocial,");
                    query.AppendLine("RUC = @pruc,");
                    query.AppendLine("Direccion = @pdireccion");
                    query.AppendLine("where IdDato = 1;");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    cmd.Parameters.Add(new SqlParameter("@prazonsocial", objeto.RazonSocial));
                    cmd.Parameters.Add(new SqlParameter("@pruc", objeto.RUC));
                    cmd.Parameters.Add(new SqlParameter("@pdireccion", objeto.Direccion));
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = cmd.ExecuteNonQuery();
                    if (respuesta < 1)
                        mensaje = "No se pudo actualizar los datos";

                }
            }
            catch (Exception ex)
            {

                respuesta = 0;
                mensaje = ex.Message;
            }

            return respuesta;
        }

        public int ActualizarLogo(byte[] imagen, out string mensaje)
        {
            mensaje = string.Empty;
            int respuesta = 0;
            try
            {

                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {

                    conexion.Open();
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("update DATOS set Logo = @pimagen");
                    query.AppendLine("where IdDato = 1;");

                    SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                    SqlParameter parameter = new SqlParameter("@pimagen", System.Data.DbType.Binary);
                    parameter.Value = imagen;
                    cmd.Parameters.Add(parameter);
                    cmd.CommandType = System.Data.CommandType.Text;

                    respuesta = cmd.ExecuteNonQuery();
                    if (respuesta < 1)
                        mensaje = "No se pudo actualizar el logo";

                }
            }
            catch (Exception ex)
            {

                respuesta = 0;
                mensaje = ex.Message;
            }

            return respuesta;
        }

        public byte[] ObtenerLogo(out bool obtenido)
        {
            obtenido = false;
            byte[] obj = new byte[0];
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.BDInventario))
                {
                    conexion.Open();
                    string query = "select Logo from DATOS where IdDato = 1";
                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            obj = (byte[])dr["Logo"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAuditoria.Log.WriteLine(ex.Message);
                obtenido = false;
                obj = new byte[0];
            }
            return obj;
        }


    }
}