using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Datack.Agent.Service.Helpers
{
    public static class SqlHelper
    {
        private static String GetConnectionString(String server, String userName, String password, Int32 timeout = 60000)
        {
            return $"Server={server};User Id={userName};Password={password}";
        }

        /// <summary>
        /// Test a databas connection.
        /// </summary>
        /// <param name="server">The server to connect to.</param>
        /// <param name="userName">The username of the connection.</param>
        /// <param name="password">The password of the connection.</param>
        /// <returns>NULL if successful, otherwise an error message.</returns>
        public static async Task<String> TestDatabaseConnection(String server, String userName, String password)
        {
            await using var sqlConnection = new SqlConnection(GetConnectionString(server, userName, password, 1000));

            try
            {
                sqlConnection.Open();

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
