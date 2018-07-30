using System;
using System.Data.SqlClient;

namespace ActivityCollectorPlugin
{

    public abstract class SQLQueryData
    {
        private static SqlConnection Instance = null;

        public abstract string GetQuery();

        public static void WriteToDatabase(SQLQueryData data)
        {

            if (Instance == null)
            {
                Instance = new SqlConnection(ActivityCollector.Config.Data.DB_Connection_String);
                Instance.Open();
            }

            SqlCommand cmd = new SqlCommand(data.GetQuery(), Instance);

            try
            {
                cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                ActivityCollector.Log.Error(e.ToString() +  $"\n{cmd.CommandText}");
            }

        }

        public static void Close()
        {
            if (Instance != null)
            {
                Instance.Close();
            }
        }
    }
}
