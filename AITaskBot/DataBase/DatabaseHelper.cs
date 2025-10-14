using Microsoft.Data.Sqlite;
using SQLitePCL;

/// <summary>
/// Вспомогательный класс для работы с базой данных SQLite
/// </summary>
static class DatabaseHelper
{
    private static readonly string dbFile = "DailyReports.db";
    private static readonly string connectionString = $"Data Source={dbFile};";

    public static void InitializeDatabase()
    {
        Batteries.Init();
        bool needCreateFile = !File.Exists(dbFile);

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Таблица для отчётов
            string createReportsTableSql = @"
                CREATE TABLE IF NOT EXISTS Reports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId TEXT NOT NULL,
                    UserName TEXT NOT NULL,
                    ReportDate TEXT NOT NULL,
                    HoursWorked INTEGER NOT NULL,
                    ReceivedAt TEXT NOT NULL
                );
            ";
            using (var command = new SqliteCommand(createReportsTableSql, connection))
            {
                command.ExecuteNonQuery();
            }

            // Таблица для аккаунтов
            string createAccountsTableSql = @"
                CREATE TABLE IF NOT EXISTS Accounts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TelegramUserId TEXT NOT NULL UNIQUE,
                    Role TEXT NOT NULL
                );
            ";
            using (var command = new SqliteCommand(createAccountsTableSql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Работа с отчетами.
    /// </summary>
    public static void InsertReport(DailyReport report)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string insertSql = @"
                INSERT INTO Reports (UserId, UserName, ReportDate, HoursWorked, ReceivedAt)
                VALUES (@UserId, @UserName, @ReportDate, @HoursWorked, @ReceivedAt);
            ";
            using (var command = new SqliteCommand(insertSql, connection))
            {
                command.Parameters.AddWithValue("@UserId", report.UserId);
                command.Parameters.AddWithValue("@UserName", report.UserName);
                command.Parameters.AddWithValue("@ReportDate", report.ReportDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@HoursWorked", report.HoursWorked);
                command.Parameters.AddWithValue("@ReceivedAt", report.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Получить отчёты за указанный период (по полю ReportDate).
    /// </summary>
    public static List<DailyReport> GetReportsByDateRange(DateTime fromDate, DateTime toDate)
    {
        var result = new List<DailyReport>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectSql = @"
                SELECT UserId, UserName, ReportDate, HoursWorked, ReceivedAt
                FROM Reports
                WHERE date(ReportDate) >= date(@FromDate)
                  AND date(ReportDate) <= date(@ToDate)
                ORDER BY ReportDate ASC;
            ";
            using (var command = new SqliteCommand(selectSql, connection))
            {
                command.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var r = new DailyReport
                        {
                            UserId = reader.GetString(0),
                            UserName = reader.GetString(1),
                            ReportDate = DateTime.Parse(reader.GetString(2)),
                            HoursWorked = reader.GetInt32(3),
                            ReceivedAt = DateTime.Parse(reader.GetString(4))
                        };
                        result.Add(r);
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Получить  последний отчёты (по полю userId).
    /// </summary>
    public static DailyReport GetLastReportByUser(string userId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectSql = @"
                    SELECT UserId, UserName, ReportDate, HoursWorked, ReceivedAt
                    FROM Reports
                    WHERE UserId = @UserId
                    ORDER BY ReceivedAt DESC
                    LIMIT 1;
                ";
            using (var command = new SqliteCommand(selectSql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new DailyReport
                        {
                            UserId = reader.GetString(0),
                            UserName = reader.GetString(1),
                            ReportDate = DateTime.Parse(reader.GetString(2)),
                            HoursWorked = reader.GetInt32(3),
                            ReceivedAt = DateTime.Parse(reader.GetString(4))
                        };
                    }
                }
            }
        }

        return null;
    }

    public static Account GetAccountByTelegramId(string telegramUserId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectSql = @"
                SELECT Id, TelegramUserId, Role
                FROM Accounts
                WHERE TelegramUserId = @tgid;
            ";
            using (var command = new SqliteCommand(selectSql, connection))
            {
                command.Parameters.AddWithValue("@tgid", telegramUserId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Account
                        {
                            Id = reader.GetInt32(0),
                            TelegramUserId = reader.GetInt64(1),
                            Role = Enum.Parse<Role>(reader.GetString(2))
                        };
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Добавить или обновить (Upsert) аккаунт.
    /// </summary>
    public static void AddOrUpdateAccount(string telegramUserId, Role role)
    {
        var existing = GetAccountByTelegramId(telegramUserId);
        if (existing == null)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string insertSql = @"
                    INSERT INTO Accounts (TelegramUserId, Role)
                    VALUES (@tgid, @role);
                ";
                using (var command = new SqliteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@tgid", telegramUserId);
                    command.Parameters.AddWithValue("@role", role.ToString());
                    command.ExecuteNonQuery();
                }
            }
        }
        else
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string updateSql = @"
                    UPDATE Accounts
                    SET Role = @role
                    WHERE TelegramUserId = @tgid;
                ";
                using (var command = new SqliteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@tgid", telegramUserId);
                    command.Parameters.AddWithValue("@role", role.ToString());
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    /// <summary>
    /// Удалить аккаунт по TelegramUserId
    /// </summary>
    public static bool RemoveAccount(string telegramUserId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string deleteSql = @"
                DELETE FROM Accounts
                WHERE TelegramUserId = @tgid;
            ";
            using (var command = new SqliteCommand(deleteSql, connection))
            {
                command.Parameters.AddWithValue("@tgid", telegramUserId);
                int rows = command.ExecuteNonQuery();
                return (rows > 0);
            }
        }
    }

    /// <summary>
    /// Получить все аккаунты.
    /// </summary>
    public static List<Account> GetAllAccounts()
    {
        var result = new List<Account>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectSql = @"
                SELECT Id, TelegramUserId, Role
                FROM Accounts;
            ";
            using (var command = new SqliteCommand(selectSql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var acc = new Account
                        {
                            Id = reader.GetInt32(0),
                            TelegramUserId = reader.GetInt64(1),
                            Role = Enum.Parse<Role>(reader.GetString(2))
                        };
                        result.Add(acc);
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Получить всех админов.
    /// </summary>
    public static List<Account> GetAllAdmins()
    {
        return GetAllAccounts().Where(a => a.Role == Role.Admin).ToList();
    }
}
