CREATE TABLE IF NOT EXISTS session (
    id INTEGER NOT NULL,
	podcast_id INTEGER NULL,
	podcast_name NVARCHAR(2000) NOT NULL,
	episode_id INTEGER NULL,
	episode_name NVARCHAR(2000) NOT NULL,
	activity_start DATETIME NOT NULL,
	activity_end DATETIME NULL,
    PRIMARY KEY (id AUTOINCREMENT),
	FOREIGN KEY (podcast_id) REFERENCES podcast(id),
	FOREIGN KEY (episode_id) REFERENCES episode(id)
)
