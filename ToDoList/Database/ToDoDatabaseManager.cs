using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using static ToDoItem;

namespace ToDoList
{
    public class ToDoDatabaseManager
    {
        private readonly string _connectionString;  // Chaîne de connexion à la base de données

        // Constructeur du gestionnaire de base de données
        public ToDoDatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Ajouter une nouvelle tâche dans la base de données
        public void AddToDoItem(ToDoItem item)
        {
            // Requête SQL pour insérer une nouvelle tâche
            string sql = "INSERT INTO tasks (ID, ItemText, IsDone, DoneDate, Priority) VALUES (@ID, @ItemText, @IsDone, @DoneDate, @Priority)";

            // Connexion à la base de données
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();  // Ouvrir la connexion
                using (var command = new MySqlCommand(sql, connection))
                {
                    // Assigner les valeurs des paramètres à partir de l'objet `ToDoItem`
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());
                    command.Parameters.AddWithValue("@ItemText", item.ItemText);
                    command.Parameters.AddWithValue("@IsDone", item.IsDone);
                    command.Parameters.AddWithValue("@DoneDate", item.DoneDate.HasValue ? (object)item.DoneDate.Value : DBNull.Value);  // Gérer la date si elle existe
                    command.Parameters.AddWithValue("@Priority", item.Priority.ToString());

                    // Exécuter la commande SQL
                    command.ExecuteNonQuery();
                }
            }
        }

        // Lire toutes les tâches depuis la base de données
        public List<ToDoItem> ReadToDoList()
        {
            var tasks = new List<ToDoItem>();  // Liste qui va contenir toutes les tâches
            // Requête SQL pour récupérer toutes les tâches
            string sql = "SELECT ID, ItemText, IsDone, DoneDate, Priority FROM tasks";

            // Connexion à la base de données
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();  // Ouvrir la connexion
                using (var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())  // Lire les résultats de la requête
                    {
                        while (reader.Read())
                        {
                            // Pour chaque ligne lue, créer un objet `ToDoItem`
                            var task = new ToDoItem
                            {
                                ID = Guid.Parse(reader["ID"].ToString()),  // Récupérer l'ID
                                ItemText = reader["ItemText"].ToString().Trim(),  // Récupérer le texte de la tâche
                                IsDone = Convert.ToBoolean(reader["IsDone"]),  // Vérifier si la tâche est terminée
                                DoneDate = reader["DoneDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DoneDate"]) : null,  // Récupérer la date si elle existe
                                Priority = Enum.TryParse<TaskPriority>(reader["Priority"].ToString(), out var priority) ? priority : TaskPriority.Low  // Récupérer la priorité
                            };

                            tasks.Add(task);  // Ajouter la tâche à la liste
                        }
                    }
                }
            }

            return tasks;  // Retourner la liste des tâches
        }

        // Mettre à jour une tâche existante dans la base de données
        public void UpdateToDoItem(ToDoItem item)
        {
            // Requête SQL pour mettre à jour une tâche
            string sql = "UPDATE tasks SET ItemText = @ItemText, IsDone = @IsDone, DoneDate = @DoneDate, Priority = @Priority WHERE ID = @ID";

            // Connexion à la base de données
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();  // Ouvrir la connexion
                using (var command = new MySqlCommand(sql, connection))
                {
                    // Assigner les valeurs des paramètres
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());
                    command.Parameters.AddWithValue("@ItemText", item.ItemText);
                    command.Parameters.AddWithValue("@IsDone", item.IsDone);
                    command.Parameters.AddWithValue("@DoneDate", item.DoneDate.HasValue ? (object)item.DoneDate.Value : DBNull.Value);  // Gérer la date si elle existe
                    command.Parameters.AddWithValue("@Priority", item.Priority.ToString());

                    // Exécuter la commande SQL
                    command.ExecuteNonQuery();
                }
            }
        }

        // Supprimer une tâche de la base de données
        public void DeleteToDoItem(ToDoItem item)
        {
            // Requête SQL pour supprimer une tâche
            string sql = "DELETE FROM tasks WHERE ID = @ID";

            // Connexion à la base de données
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();  // Ouvrir la connexion
                using (var command = new MySqlCommand(sql, connection))
                {
                    // Assigner l'ID de la tâche à supprimer
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());

                    // Exécuter la commande SQL
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
