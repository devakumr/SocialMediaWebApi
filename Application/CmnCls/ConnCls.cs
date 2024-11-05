using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CmnCls
{
    public class ConnCls
    {
        public static SqlDataReader dr;
        public static DataSet ds = new DataSet();
        public static DataTable dt = new DataTable();
        byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool");

        public ConnCls() { }
    
        public static string Environ = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("EnvName").Value;

        public static string Con = new ConfigurationBuilder().AddJsonFile($"appsettings.{Environ}.json").Build().GetSection("ConnectionStrings")["DefaultConnection"]; 
        
        public static SqlConnection connOpen()
        {
            SqlConnection conn = new SqlConnection();
            conn.Open();
            return conn;
        }
        public static SqlConnection connClose()
        {
            SqlConnection conn = new SqlConnection();
            conn.Close();
            return conn;
        }

        public static SqlDataReader ret_dr(String pSP, string Con)
        {

            using (SqlConnection con = new SqlConnection(Con))
            {
                SqlCommand cmd = new SqlCommand(pSP, con);
                cmd.Connection = connOpen();
                cmd.CommandType = CommandType.StoredProcedure;
                return cmd.ExecuteReader();
            }
        }

        public static DataSet ret_ds(string pSP, string Con)
        {
            using (SqlConnection con = new SqlConnection(Con))
            {
                SqlDataAdapter da = new SqlDataAdapter(pSP, con);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }

        public static DataTable ret_dt(string pSP, string Con)
        {
            using (SqlConnection con = new SqlConnection(Con))
            {
                SqlDataAdapter da = new SqlDataAdapter(pSP, con);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
                return dt;
            }
        }

        public static void exec(String str, string Con)
        {
            using (SqlConnection con = new SqlConnection(Con))
            {
                SqlCommand cmd = new SqlCommand(str, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
        }


        //Functions which executes With Parameters

        public static void execCmd(SqlCommand cmd, string Con)
        {
            using (SqlConnection con = new SqlConnection(Con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static SqlDataReader ret_dr_Cmd(SqlCommand cmd, string Con)
        {
            using (SqlConnection con = new SqlConnection(Con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connOpen();
                return cmd.ExecuteReader();
            }
        }

        public static DataSet ret_ds_Cmd(SqlCommand cmd, string Con)
        {
            using (SqlConnection con = new SqlConnection(Con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }

        public static DataTable ret_dt_Cmd(SqlCommand cmd, string Con)
        {
            using (SqlConnection con = new SqlConnection(Con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
                return dt;
            }
        }

        public static int execCmdScalar(SqlCommand cmd, string Con)
        {
            int scalarVal;
            using (SqlConnection con = new SqlConnection(Con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                con.Open();
                scalarVal = (int)cmd.ExecuteScalar();
            }
            return scalarVal;
        }
    }
}
