using MySqlConnector;
using System;
using System.Threading.Tasks;
using t_project.Database;

namespace t_project.ViewModels
{
    public static class DatabaseHelper
    {
        public static async Task<bool> ValidateUserAsync(string username, string password)
        {
            string query = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";

            using (MySqlConnection connection = new MySqlConnection(Config.connection))
            {
                try
                {
                    await connection.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@login", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    int userCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return userCount > 0;
                }
                catch (Exception ex)
                {
                    // Можно записать ошибку в лог
                    Console.WriteLine($"Ошибка базы данных: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
