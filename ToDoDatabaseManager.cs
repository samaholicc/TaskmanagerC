using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ToDoList
{
    public class ToDoDatabaseManager
    {
        private readonly string _connectionString;

        public ToDoDatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Add a new ToDoItem to the database
        public void AddToDoItem(ToDoItem item)
        {
            string sql = "INSERT INTO tasks (ID, ItemText, IsDone, DoneDate, Priority) VALUES (@ID, @ItemText, @IsDone, @DoneDate, @Priority)";

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", item.ID.ToString());
                        command.Parameters.AddWithValue("@ItemText", item.ItemText);
                        command.Parameters.AddWithValue("@IsDone", item.IsDone);
                        command.Parameters.AddWithValue("@DoneDate", item.DoneDate.HasValue ? (object)item.DoneDate.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@Priority", item.Priority.ToString());

                        command.ExecuteNonQuery();
                        Console.WriteLine("Task added successfully.");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"An error occurred while adding the task: {ex.Message}");
                }
            }
        }

        // Read all tasks from the database
        public List<ToDoItem> ReadToDoList()
        {
            var tasks = new List<ToDoItem>();
            string sql = "SELECT ID, ItemText, IsDone, DoneDate, Priority FROM tasks";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var task = new ToDoItem
                            {
                                ID = Guid.Parse(reader["ID"].ToString()),
                                ItemText = reader["ItemText"].ToString().Trim(),
                                IsDone = Convert.ToBoolean(reader["IsDone"]),
                                DoneDate = reader["DoneDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DoneDate"]) : null,
                                Priority = Enum.TryParse<TaskPriority>(reader["Priority"].ToString(), out var priority) ? priority : TaskPriority.Low
                            };

                            tasks.Add(task);
                        }
                    }
                }
            }

            return tasks;
        }

        // Update an existing ToDoItem in the database
        public void UpdateToDoItem(ToDoItem item)
        {
            string sql = "UPDATE tasks SET ItemText = @ItemText, IsDone = @IsDone, DoneDate = @DoneDate, Priority = @Priority WHERE ID = @ID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", item.ID.ToString());
                        command.Parameters.AddWithValue("@ItemText", item.ItemText);
                        command.Parameters.AddWithValue("@IsDone", item.IsDone);
                        command.Parameters.AddWithValue("@DoneDate", item.DoneDate.HasValue ? (object)item.DoneDate.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@Priority", item.Priority.ToString());

                        command.ExecuteNonQuery();
                        Console.WriteLine("Task updated successfully.");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"An error occurred while updating the task: {ex.Message}");
                }
            }
        }

        // Delete a ToDoItem from the database
        public void DeleteToDoItem(ToDoItem item)
        {
            string sql = "DELETE FROM tasks WHERE ID = @ID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", item.ID.ToString());
                        command.ExecuteNonQuery();
                        Console.WriteLine("Task deleted successfully.");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"An error occurred while deleting the task: {ex.Message}");
                }
            }
        }
    }

    // TaskPriority enum
    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    // ToDoItem class
    public class ToDoItem
    {
        public Guid ID { get; set; }
        public string ItemText { get; set; }
        public bool IsDone { get; set; }
        public DateTime? DoneDate { get; set; }  // Nullable DateTime
        public TaskPriority Priority { get; set; }
    }
}
