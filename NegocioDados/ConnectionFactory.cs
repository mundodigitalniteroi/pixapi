using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NegocioDados
{
    public class ConnectionFactory
    {

        private static SqlCommand command = ConnectionFactory.ConnectionString();
        public static string connectionString;
        private static SqlTransaction transaction;

        private static SqlCommand ConnectionString()
        {
            return new SqlConnection(ConnectionFactory.connectionString).CreateCommand();
        }

        public static bool Conectado()
        {
            try
            {
                return ConnectionFactory.command.Connection.State == ConnectionState.Open;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public static void Conectar()
        {
            try
            {
                if (ConnectionFactory.command.Connection.ConnectionString == "")
                    ConnectionFactory.command = ConnectionFactory.ConnectionString();
                if (ConnectionFactory.command.Connection.State != ConnectionState.Closed)
                    return;
                ConnectionFactory.command.Connection.Open();
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public static void Desconectar()
        {
            try
            {
                ConnectionFactory.command.Connection.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static void IniciarTransacao()
        {
            try
            {
                ConnectionFactory.Conectar();
                if (ConnectionFactory.transaction != null)
                    return;
                ConnectionFactory.transaction = ConnectionFactory.command.Connection.BeginTransaction();
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public static void FinalizarTransacao()
        {
            try
            {
                ConnectionFactory.transaction.Commit();
                ConnectionFactory.transaction = (SqlTransaction)null;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public static void CancelarTransacao()
        {
            try
            {
                ConnectionFactory.transaction.Rollback();
                ConnectionFactory.transaction = (SqlTransaction)null;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public static DataTable Consultar(string SQL, bool desconectar = false)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionFactory.connectionString))
                {
                    connection.Open();

                    SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
                    using (SqlCommand sqlCommand = new SqlCommand(SQL, connection, transaction))
                    {
                        sqlCommand.CommandTimeout = 600;

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            dataTable.Load((IDataReader)sqlDataReader);
                            if (dataTable.Rows.Count > 0)
                                return dataTable;
                            return (DataTable)null;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                if (desconectar)
                    ConnectionFactory.Desconectar();
            }
        }


        public static int Executar(string SQL, bool desconectar = false)
        {
            try
            {
                ConnectionFactory.Conectar();
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;
                ConnectionFactory.command.CommandText = SQL;
                return ConnectionFactory.command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                if (desconectar)
                    ConnectionFactory.Desconectar();
            }
        }

        public static int ExecuteScalar(string SQL, bool desconectar = false)
        {
            int num = 0;
            try
            {
                ConnectionFactory.Conectar();
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;
                ConnectionFactory.command.CommandText = SQL;
                num = (int)ConnectionFactory.command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                if (desconectar)
                    ConnectionFactory.Desconectar();
            }
            return num;
        }

        public static int ExecutarScopeIdentity(string SQL, bool desconectar = false)
        {
            try
            {
                ConnectionFactory.Conectar();
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;
                ConnectionFactory.command.CommandText = SQL + "; SELECT CAST(scope_identity() AS int)";
                return (int)ConnectionFactory.command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (desconectar)
                    ConnectionFactory.Desconectar();
            }
        }

        public static int ExecutarScopeIdentity(string SQL, SqlParameter[] parameters, bool desconectar = false)
        {
            try
            {
                ConnectionFactory.Conectar();
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;

                ConnectionFactory.command.CommandText = SQL + "; SELECT CAST(scope_identity() AS int)";

                ConnectionFactory.command.Parameters.Clear();
                if (parameters.Length > 0)
                    ConnectionFactory.command.Parameters.AddRange(parameters);

                var retorno = ConnectionFactory.command.ExecuteScalar();

                return (int)retorno;
            }
            catch (SqlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (desconectar)
                    ConnectionFactory.Desconectar();
            }
        }

        public static void ExecutarStoredProcedure(string StoredProcedureName, bool disconnect = false)
        {
            try
            {
                ConnectionFactory.Conectar();
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;
                ConnectionFactory.command.CommandType = CommandType.StoredProcedure;
                ConnectionFactory.command.CommandText = StoredProcedureName;
                ConnectionFactory.command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                ConnectionFactory.command.CommandType = CommandType.Text;
                if (disconnect)
                    ConnectionFactory.Desconectar();
            }
        }

        public static void ExecutarStoredProcedureComParametros(string StoredProcedureName, SqlParameter[] parameters, bool disconnect = false)
        {
            try
            {
                ConnectionFactory.Conectar();
                ConnectionFactory.command.Parameters.Clear();
                if (parameters.Length <= 0)
                    throw new Exception("Ao usar \"Parâmetros\", deve-se informar os parâmetros.");
                ConnectionFactory.command.Parameters.AddRange(parameters);
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;
                ConnectionFactory.command.CommandType = CommandType.StoredProcedure;
                ConnectionFactory.command.CommandText = StoredProcedureName;
                ConnectionFactory.command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                ConnectionFactory.command.CommandType = CommandType.Text;
                if (disconnect)
                    ConnectionFactory.Desconectar();
            }
        }

        public static int ExecutarStoredProcedureComParametrosRetornarValor(string StoredProcedureName, SqlParameter[] parameters, bool disconnect = false)
        {
            try
            {
                ConnectionFactory.Conectar();
                ConnectionFactory.command.Parameters.Clear();
                if (parameters.Length > 0)
                    ConnectionFactory.command.Parameters.AddRange(parameters);
                ConnectionFactory.command.Parameters.Add(new SqlParameter("@RETURNVALUE", SqlDbType.Int));
                ConnectionFactory.command.Parameters["@RETURNVALUE"].Direction = ParameterDirection.Output;
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;
                ConnectionFactory.command.CommandType = CommandType.StoredProcedure;
                ConnectionFactory.command.CommandText = StoredProcedureName;
                ConnectionFactory.command.ExecuteNonQuery();
                return (int)ConnectionFactory.command.Parameters["@RETURNVALUE"].Value;
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                ConnectionFactory.command.CommandType = CommandType.Text;
                if (disconnect)
                    ConnectionFactory.Desconectar();
            }
        }

        public static int ExecutarStoredProcedureRetornarValor(string StoredProcedureName, bool disconnect = false)
        {
            try
            {
                ConnectionFactory.Conectar();
                ConnectionFactory.command.Parameters.Clear();
                if (ConnectionFactory.transaction != null)
                    ConnectionFactory.command.Transaction = ConnectionFactory.transaction;
                ConnectionFactory.command.CommandType = CommandType.StoredProcedure;
                ConnectionFactory.command.CommandText = StoredProcedureName;
                ConnectionFactory.command.Parameters.Add(new SqlParameter("@RETURNVALUE", SqlDbType.Int));
                ConnectionFactory.command.Parameters["@RETURNVALUE"].Direction = ParameterDirection.Output;
                ConnectionFactory.command.ExecuteNonQuery();
                return (int)ConnectionFactory.command.Parameters["@RETURNVALUE"].Value;
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                ConnectionFactory.command.CommandType = CommandType.Text;
                if (disconnect)
                    ConnectionFactory.Desconectar();
            }
        }
    }
}
