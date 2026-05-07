using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text;

namespace Vullnerability.db
{
    // Создаёт файл БД при первом запуске и накатывает на него схему
    // из 01_schema.sqlite.sql. Должно вызываться до того, как EF откроет контекст.
    public static class SqliteBootstrap
    {
        private const string DbFileName = "vulndb.sqlite";
        private const string SchemaFileName = "01_schema.sqlite.sql";

        // Возвращает полный путь к .sqlite (вдруг пригодится показать в UI).
        public static string EnsureDatabase()
        {
            string dbDir = ResolveDbDirectory();
            string dbPath = Path.Combine(dbDir, DbFileName);

            // подставится в |DataDirectory| из App.config
            AppDomain.CurrentDomain.SetData("DataDirectory", dbDir);

            Directory.CreateDirectory(dbDir);

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);

                string schemaSql = LoadSchemaScript();
                ExecuteSchemaOnFreshDb(dbPath, schemaSql);
            }
            else
            {
                // на всякий случай включаем foreign_keys и для уже существующей БД
                using (var conn = new SQLiteConnection(BuildConnectionString(dbPath)))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA foreign_keys = ON;";
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return dbPath;
        }

        // %LOCALAPPDATA%\VulnDb
        private static string ResolveDbDirectory()
        {
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(baseDir, "VulnDb");
        }

        private static string LoadSchemaScript()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, SchemaFileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Не найден файл схемы '{SchemaFileName}' в каталоге '{baseDir}'. " +
                    "Проверь, что у файла стоит 'Copy to Output Directory: Copy if newer'.",
                    path);
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static void ExecuteSchemaOnFreshDb(string dbPath, string schemaSql)
        {
            using (var conn = new SQLiteConnection(BuildConnectionString(dbPath)))
            {
                conn.Open();

                using (var pragma = conn.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA foreign_keys = ON;";
                    pragma.ExecuteNonQuery();
                }

                // весь скрипт схемы накатываем одной транзакцией
                using (var tx = conn.BeginTransaction())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = schemaSql;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
        }

        private static string BuildConnectionString(string dbPath)
        {
            return new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                ForeignKeys = true,
                JournalMode = SQLiteJournalModeEnum.Wal,
                SyncMode = SynchronizationModes.Normal,
            }.ConnectionString;
        }
    }
}
