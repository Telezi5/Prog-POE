using System;
using System.Collections.Generic;
using MySqlConnector;

namespace CyberBot
{
    public class Database
    {
        // ── Change these if your XAMPP MySQL uses a different user/password ──
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=cyberbot;User ID=root;Password=;";

        // ── Ensure the tasks table exists when the app starts ────────────────
        public static void Initialise()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand(@"
                    CREATE TABLE IF NOT EXISTS tasks (
                        id            INT AUTO_INCREMENT PRIMARY KEY,
                        title         VARCHAR(255) NOT NULL,
                        description   TEXT,
                        reminder_date DATETIME     NULL,
                        is_completed  BOOLEAN      NOT NULL DEFAULT FALSE,
                        created_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );", conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialisation failed: {ex.Message}");
            }
        }

        // ── Add a new task and return the new auto-increment id ──────────────
        public static int AddTask(string title, string description, DateTime? reminderDate)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(@"
                INSERT INTO tasks (title, description, reminder_date)
                VALUES (@title, @desc, @reminder);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@desc", description ?? "");
            cmd.Parameters.AddWithValue("@reminder", reminderDate.HasValue ? (object)reminderDate.Value : DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Retrieve all tasks ordered by completion then created date ────────
        public static List<TaskItem> GetAllTasks()
        {
            var list = new List<TaskItem>();

            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT id, title, description, reminder_date, is_completed, created_at
                FROM tasks
                ORDER BY is_completed ASC, created_at DESC;", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TaskItem
                {
                    Id          = reader.GetInt32("id"),
                    Title       = reader.GetString("title"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime("reminder_date"),
                    IsCompleted = reader.GetBoolean("is_completed"),
                    CreatedAt   = reader.GetDateTime("created_at")
                });
            }
            return list;
        }

        // ── Mark a task as completed ──────────────────────────────────────────
        public static void MarkCompleted(int id)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE tasks SET is_completed = TRUE WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ── Delete a task by id ───────────────────────────────────────────────
        public static void DeleteTask(int id)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(
                "DELETE FROM tasks WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ── Retrieve tasks that have a reminder due today or earlier ──────────
        public static List<TaskItem> GetDueTasks()
        {
            var list = new List<TaskItem>();

            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT id, title, description, reminder_date, is_completed, created_at
                FROM tasks
                WHERE reminder_date IS NOT NULL
                  AND reminder_date <= @now
                  AND is_completed = FALSE;", conn);

            cmd.Parameters.AddWithValue("@now", DateTime.Now);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TaskItem
                {
                    Id           = reader.GetInt32("id"),
                    Title        = reader.GetString("title"),
                    Description  = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime("reminder_date"),
                    IsCompleted  = reader.GetBoolean("is_completed"),
                    CreatedAt    = reader.GetDateTime("created_at")
                });
            }
            return list;
        }
    }
}
