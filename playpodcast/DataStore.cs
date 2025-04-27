using Microsoft.Data.Sqlite;

namespace playpodcast;

public class DataStore {
    public string StoreLocation { get; }
    public SessionStore Sessions { get; }
    public PodcastStore Podcasts { get; }
    public EpisodeStore Episodes { get; }

    public DataStore(string location)
    {
        StoreLocation = location;

        Sessions = new SessionStore(this);
        Podcasts = new PodcastStore(this);
        Episodes = new EpisodeStore(this);      
    }

    public SqliteConnection? GetConnection()
    {
        try
        {
            SqliteConnection connection = new(string.Format("Data Source=\"{0}\"", StoreLocation));
            connection.Open();
            connection.Close();
            return connection;
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: {0}", ex.Message);
        }

        return null;
    }

    public void ReleaseConnection(SqliteConnection connection)
    {
        if (connection.State != System.Data.ConnectionState.Closed)
        {
            connection.Close();
        }
        connection.Dispose();
    }

    public bool ValidateStore()
    {
        bool isValid = false;
        
        SqliteConnection? connection = GetConnection();
        if (connection != null)
        {
            isValid = true;
            ReleaseConnection(connection);
        }

        return isValid;
    }

}