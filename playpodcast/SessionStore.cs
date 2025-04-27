using Microsoft.Data.Sqlite;

namespace playpodcast;

public class SessionStore
{
    private const string SQL_SANITY_CHECK = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'session'";

    public DataStore RootStore { get; }

    public SessionStore(DataStore store)
    {
        RootStore = store;
    }

    public bool ValidateStore()
    {
        bool isValid = false;
                
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();
            
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_SANITY_CHECK;

            SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                string name = reader.GetString(0);
                isValid = name == "session";
            }

            RootStore.ReleaseConnection(connection);
        }

        return isValid;
    }
}
