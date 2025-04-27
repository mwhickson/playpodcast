using Microsoft.Data.Sqlite;

namespace playpodcast;

public class PodcastStore
{
    private const string SQL_SANITY_CHECK = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'podcast'";
    private const string SQL_DELETE_BY_ID = "DELETE FROM podcast WHERE id = $id";
    private const string SQL_DELETE_BY_URL = "DELETE FROM podcast WHERE url = $url";
    private const string SQL_GET_BY_ID = "SELECT TOP 1 * FROM podcast WHERE id = $id";
    private const string SQL_GET_BY_URL = "SELECT TOP 1 * FROM podcast WHERE url = $url";
    private const string SQL_GET_LIST = "SELECT * FROM podcast ORDER BY title, id";
    private const string SQL_UPSERT = @"
        INSERT INTO podcast(id, title, url, description, subscribed_on, updated_on)
        VALUES (null, $title, $url, $description, $subscribed_on, $updated_on)
            ON CONFLICT(url)
            DO UPDATE SET title = excluded.title, description = excluded.description, updated_on = excluded.updated_on WHERE url = excluded.url
    ";

    public DataStore RootStore { get; }

    public PodcastStore(DataStore store)
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
                isValid = name == "podcast";
            }

            RootStore.ReleaseConnection(connection);
        }

        return isValid;
    }    

    public bool DeleteByID(int id) {
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_DELETE_BY_ID;
            command.Parameters.AddWithValue("$id", id);

            int affected = command.ExecuteNonQuery();

            RootStore.ReleaseConnection(connection);

            return affected > 0;
        }
        return false;
    }

    public bool DeleteByUrl(string url) 
    {
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_DELETE_BY_URL;
            command.Parameters.AddWithValue("$url", url);

            int affected = command.ExecuteNonQuery();

            RootStore.ReleaseConnection(connection);

            return affected > 0;
        }
        return false;
    }

    public Podcast? GetByID(int id) 
    {
        Podcast? podcast = null;
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_GET_BY_ID;
            command.Parameters.AddWithValue("$id", id);

            using(SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    podcast = new Podcast(
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("url"))
                    );
                }
            }

            RootStore.ReleaseConnection(connection);
        }
        return podcast;
    }

    public Podcast? GetByUrl(string url) 
    {
        Podcast? podcast = null;
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_GET_BY_URL;
            command.Parameters.AddWithValue("$url", url);

            using(SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    podcast = new Podcast(
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("url"))
                    );
                }
            }

            RootStore.ReleaseConnection(connection);
        }
        return podcast;
    }

    public List<Podcast> GetList() 
    {
        List<Podcast> podcasts = [];
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_GET_LIST;

            using(SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    podcasts.Add(new Podcast(
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("url"))
                    ));
                }
            }

            RootStore.ReleaseConnection(connection);
        }
        return podcasts;
    }

    public bool InsertOrUpdate(Podcast podcast) 
    {
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_UPSERT;
            command.Parameters.AddWithValue("$title", podcast.Title);
            command.Parameters.AddWithValue("$url", podcast.Url);

            // TODO:
            command.Parameters.AddWithValue("$description", "");
            command.Parameters.AddWithValue("$subscribed_on", DateTime.Now);
            command.Parameters.AddWithValue("$updated_on", DateTime.MinValue);

            int affected = command.ExecuteNonQuery();

            RootStore.ReleaseConnection(connection);

            return affected > 0;
        }
        return false;
    }
}
