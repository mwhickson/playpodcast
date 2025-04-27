using Microsoft.Data.Sqlite;

namespace playpodcast;

public class EpisodeStore
{
    private const string SQL_SANITY_CHECK = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'episode'";
    private const string SQL_DELETE_BY_ID = "DELETE FROM episode WHERE id = $id";
    private const string SQL_DELETE_BY_PODCAST_ID = "DELETE FROM episode WHERE podcast_id = $podcast_id";
    private const string SQL_DELETE_BY_URL = "DELETE FROM episode WHERE url = $url";
    private const string SQL_GET_BY_ID = "SELECT * FROM episode WHERE id = $id LIMIT 1";
    private const string SQL_GET_BY_URL = "SELECT * FROM episode WHERE url = $url LIMIT 1";
    private const string SQL_GET_LIST_BY_PODCAST_ID = "SELECT * FROM episode WHERE podcast_id = $podcast_id ORDER BY published_on DESC, id DESC, title";
    private const string SQL_UPSERT = @"
        INSERT INTO episode(id, podcast_id, title, url, description, published_on, is_played, position)
        VALUES (null, $podcast_id, $title, $url, $description, $published_on, $is_played, $position)
            ON CONFLICT(url)
            DO UPDATE SET podcast_id = excluded.podcast_id, title = excluded.title, description = excluded.description, published_on = excluded.published_on, is_played = excluded.is_played, position = excluded.position WHERE url = excluded.url
    ";

    public DataStore RootStore { get; }

    public EpisodeStore(DataStore store)
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
                isValid = name == "episode";
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

    public bool DeleteByPodcastID(int podcastId) {
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_DELETE_BY_PODCAST_ID;
            command.Parameters.AddWithValue("$podcast_id", podcastId);

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

    public Episode? GetByID(int id) 
    {
        Episode? episode = null;
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
                    episode = new Episode(
                        reader.GetInt32(reader.GetOrdinal("podcast_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("url"))
                    );

                    episode.Id = reader.GetInt32(reader.GetOrdinal("id"));
                    episode.Description = reader.GetString(reader.GetOrdinal("description"));
                    episode.PublishedOn = reader.GetDateTime(reader.GetOrdinal("published_on"));
                    episode.IsPlayed = reader.GetBoolean(reader.GetOrdinal("is_played"));
                    episode.Position = reader.GetInt32(reader.GetOrdinal("position"));
                }
            }

            RootStore.ReleaseConnection(connection);
        }
        return episode;
    }

    public Episode? GetByUrl(string url) 
    {
        Episode? episode = null;
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
                    episode = new Episode(
                        reader.GetInt32(reader.GetOrdinal("podcast_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("url"))
                    );

                    episode.Id = reader.GetInt32(reader.GetOrdinal("id"));
                    episode.Description = reader.GetString(reader.GetOrdinal("description"));
                    episode.PublishedOn = reader.GetDateTime(reader.GetOrdinal("published_on"));
                    episode.IsPlayed = reader.GetBoolean(reader.GetOrdinal("is_played"));
                    episode.Position = reader.GetInt32(reader.GetOrdinal("position"));
                }
            }

            RootStore.ReleaseConnection(connection);
        }
        return episode;
    }

    public List<Episode> GetListByPodcastId(int podcastId) 
    {
        List<Episode> episodes = [];
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_GET_LIST_BY_PODCAST_ID;
            command.Parameters.AddWithValue("$podcast_id", podcastId);

            using(SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Episode e = new(
                        podcastId,
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("url"))
                    );

                    e.Id = reader.GetInt32(reader.GetOrdinal("id"));
                    e.Description = reader.GetString(reader.GetOrdinal("description"));
                    e.PublishedOn = reader.GetDateTime(reader.GetOrdinal("published_on"));
                    e.IsPlayed = reader.GetBoolean(reader.GetOrdinal("is_played"));
                    e.Position = reader.GetInt32(reader.GetOrdinal("position"));

                    episodes.Add(e);
                }
            }

            RootStore.ReleaseConnection(connection);
        }
        return episodes;
    }

    public bool InsertOrUpdate(Podcast podcast, Episode episode) 
    {
        SqliteConnection? connection = RootStore.GetConnection();
        if (connection != null)
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = SQL_UPSERT;
            command.Parameters.AddWithValue("$podcast_id", podcast.Id);
            command.Parameters.AddWithValue("$title", episode.Title);
            command.Parameters.AddWithValue("$url", episode.Url);
            command.Parameters.AddWithValue("$description", episode.Description);
            command.Parameters.AddWithValue("$published_on", episode.PublishedOn);
            command.Parameters.AddWithValue("$is_played", episode.IsPlayed);
            command.Parameters.AddWithValue("$position", episode.Position);

            int affected = command.ExecuteNonQuery();

            RootStore.ReleaseConnection(connection);

            return affected > 0;
        }
        return false;
    }
}
