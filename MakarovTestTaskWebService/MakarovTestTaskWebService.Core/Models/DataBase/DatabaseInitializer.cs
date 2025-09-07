using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MakarovTestTaskWebService.Core.Models.DataBase
{
    public class DatabaseInitializer
    {
        private readonly string _dbConnection;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(string dbConnection, ILogger<DatabaseInitializer> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Без EF core и автоматичяеских миграций
            // Для миграции я бы использовал Flyway(мой выбор и опыт ранее), FluentMigrator, dbup
            // Сейчас я не вижу смысла усложнять и не планируются изменения в базу и модели
            // поэтому просто создам таблицы если их нет

            try
            {
                using (var connection = new NpgsqlConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    var departmentTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Department (
                        Id SERIAL PRIMARY KEY,
                        Name VARCHAR(255),
                        Phone VARCHAR(50)
                    );";
                    await connection.ExecuteAsync(departmentTableQuery);

                    var passportTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Passport (
                        Type VARCHAR(50),
                        Number VARCHAR(50) PRIMARY KEY
                    );";
                    await connection.ExecuteAsync(passportTableQuery);

                    var employeeTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Employee (
                        Id SERIAL PRIMARY KEY,
                        Name VARCHAR(255),
                        Surname VARCHAR(255),
                        Phone VARCHAR(50),
                        CompanyId INT,
                        DepartmentId INT,
                        PassportNumber VARCHAR(50),
                        FOREIGN KEY (DepartmentId) REFERENCES Department(Id),
                        FOREIGN KEY (PassportNumber) REFERENCES Passport(Number)
                    );";
                    await connection.ExecuteAsync(employeeTableQuery);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка инициализации: {ex.Message}");
            }
        }
    }
}
