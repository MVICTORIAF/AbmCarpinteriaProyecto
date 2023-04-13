using CarpinteriaApp.dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CarpinteriaApp.datos
{
    class HelperDB
    {
        private SqlConnection conn;
        private string CadenaConexion = @"Data Source=DESKTOP-64M431K\SQLEXPRESS;Initial Catalog=carpinteria_db;Integrated Security=True";
        private static HelperDB instancia;

        public HelperDB()
        {
            conn = new SqlConnection(CadenaConexion);
        }

        public static HelperDB ObtenerInstancia()
        {
            if (instancia == null)

                instancia = new HelperDB();
            return instancia;

        }

        public DataTable ConsultarDB(string NomProc)
        {
            DataTable tabla = new DataTable();
            SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = NomProc;
            cmd.CommandType = CommandType.StoredProcedure;
            tabla.Load(cmd.ExecuteReader());
            conn.Close();
            return tabla;
        }



        public bool ConfirmarPresupuesto(Presupuesto oPresupuesto)
        {
            bool resultado = true;

            SqlConnection cnn = new SqlConnection();
            SqlTransaction trans = null;

       try {

            cnn.ConnectionString = @"Data Source=DESKTOP-64M431K\SQLEXPRESS;Initial Catalog=carpinteria_db;Integrated Security=True";
            cnn.Open();
            trans = cnn.BeginTransaction();
            SqlCommand cmd = new SqlCommand();

            cmd.Transaction = trans;
            cmd.Connection = cnn;
            cmd.CommandText = "SP_INSERTAR_MAESTRO";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@cliente", oPresupuesto.Cliente);
            cmd.Parameters.AddWithValue("@dto", oPresupuesto.Descuento);
            cmd.Parameters.AddWithValue("@total", oPresupuesto.CalcularTotal());

            //parámetro de salida:
            SqlParameter pOut = new SqlParameter();
            pOut.ParameterName = "@presupuesto_nro";
            pOut.DbType = DbType.Int32;
            pOut.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(pOut);

            cmd.ExecuteNonQuery();

            int presupuestoNro = (int)pOut.Value;

                   //si es ident esto no va 
                int detalleNro = 1; //esto tampoco

                foreach (DetallePresupuesto item in oPresupuesto.Detalles)
                {
                    SqlCommand cmdDet = new SqlCommand();
                    cmdDet.Connection = cnn;
                    cmdDet.Transaction = trans;

                    cmdDet.CommandText = "SP_INSERTAR_DETALLE";
                    cmdDet.CommandType = CommandType.StoredProcedure;
                    cmdDet.Parameters.AddWithValue("@presupuesto_nro", presupuestoNro);
                    cmdDet.Parameters.AddWithValue("@detalle", detalleNro);  //si es identity no va 
                    cmdDet.Parameters.AddWithValue("@id_producto", item.Producto.ProductoNro);
                    cmdDet.Parameters.AddWithValue("@cantidad", item.Cantidad);

                    cmd.ExecuteNonQuery();
                    detalleNro++;  //si es identity no va 
                }
                trans.Commit();
                cnn.Close();
                
            }
            catch(Exception)
            {

                trans.Rollback();
                resultado = false;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open) cnn.Close();
            }

            return resultado;
        }
    }
}

