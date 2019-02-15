using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin
{

    public abstract class SQLQueryData
    {
        //private static SqlConnection Instance = null;

        public abstract string GetQuery();

        public static void WriteToDatabase(SQLQueryData data)
        {
            //if (Instance == null)
            //{
            //    SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(ActivityCollector.Config.Data.DB_Connection_String)
            //    {
            //        MultipleActiveResultSets = true,
            //        AsynchronousProcessing = true
            //    };

            //    ActivityCollector.Log.Error(connectionStringBuilder.ConnectionString);

            //    Instance = new SqlConnection(connectionStringBuilder.ConnectionString);
            //    Instance.Open();

            //}

            //try
            //{
            //    if (!string.IsNullOrWhiteSpace(data.GetQuery()))
            //    {
            //        SqlCommand cmd = new SqlCommand(data.GetQuery(), Instance);
            //        cmd.CommandTimeout = int.MaxValue;
            //        cmd.ExecuteNonQueryAsync().ContinueWith(after);
            //    }
            //}
            //catch (Exception e)
            //{
            //    ActivityCollector.Log.Warn(e);
            //    ActivityCollector.Log.Warn(data.GetQuery());
            //}

            using (SqlConnection conn = new SqlConnection(ActivityCollector.Config.Data.DB_Connection_String))
            {
                conn.OpenAsync().ContinueWith((t) => 
                {
                    SqlCommand command = new SqlCommand(data.GetQuery(), conn);
                    command.ExecuteNonQueryAsync().ContinueWith(after);
                });
            }

        }

        private static void after(Task<int> t)
        {
            if (t.Exception != null)
            {
                ActivityCollector.Log.Error(t.Exception);
            }
        }

        //public static void Close()
        //{
        //    if (Instance != null)
        //    {
        //        Instance.Close();
        //    }
        //}
    }
}
