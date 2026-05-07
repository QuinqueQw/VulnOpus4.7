using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text;

namespace Vullnerability.db
{
    /// <summary>
    /// Создаёт файл-БД SQLite при первом запуске приложения и накатывает на него
    /// схему из <c>01_schema.sqlite.sql</c>.
    /// 
    /// Должно вызываться ДО любого использования <see cref="VulnDbContext"/> —
    /// как правило, в <c>Program.Main()</c> перед <c>Application.Run(...)</c>.
    /// 
    /// Файл БД хранится по пути <c>%LOCALAPPDATA%\VulnDb\vulndb.sqlite</c>.
    /// Этот же путь подставляется в <c>|DataDirectory|</c> connection string'а
    /// из App.config — поэтому EF6 автоматически открывает именно созданный нами файл.
    /// 
    /// При желании можно перевести в "portable" режим (БД лежит рядом с .exe):
    /// в <see cref="ResolveDbDirectory"/> вернуть <see cref="AppDomain.BaseDirectory"/>.
    /// </summary>
    public static class SqliteBootstrap
    {
        /// <summary>Имя файла БД в каталоге пользователя.</summary>
        private const string DbFileName = "vulndb.sqlite";

        /// <summary>
        /// Имя SQL-скрипта со схемой. Лежит в каталоге сборки (Output Directory)
        /// — в проекте файл должен иметь свойство «Copy to Output Directory:
        /// Copy if newer». См. README_SQLITE.md.
        /// </summary>
        private const string SchemaFileName = "01_schema.sqlite.sql";

        /// <summary>
        /// Создаёт БД (если её нет) и накатывает схему. Возвращает полный путь
        /// к файлу .sqlite — на случай, если приложению нужно показать его
        /// пользователю в UI («База данных хранится здесь: …»).
        /// </summary>
        public static string EnsureDatabase()
        {
            string dbDir = ResolveDbDirectory();
            string dbPath = Path.Combine(dbDir, DbFileName);

            // |DataDirectory| в connection string'е (App.config) подставится этим значением.
            // Должно быть установлено ДО первого открытия EF6-контекста, иначе будет
            // искать файл в каталоге .exe и/или %TEMP%.
            AppDomain.CurrentDomain.SetData("DataDirectory", dbDir);

            Directory.CreateDirectory(dbDir);

            if (!File.Exists(dbPath))
            {
                // Пустой файл .sqlite — обязательная подготовка перед накатыванием схемы.
                SQLiteConnection.CreateFile(dbPath);

                string schemaSql = LoadSchemaScript();
                ExecuteSchemaOnFreshDb(dbPath, schemaSql);
            }
            else
            {
                // Файл уже существует — на каждом запуске включаем foreign_keys
                // (это per-connection настройка, но ставим явно — на случай, если
                // App.config забыли поправить).
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

        /// <summary>
        /// %LOCALAPPDATA%\VulnDb. Туда же будет писаться SQLite WAL-журнал, кэши и т.д.
        /// </summary>
        private static string ResolveDbDirectory()
        {
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(baseDir, "VulnDb");
        }

        /// <summary>
        /// Читает SQL-скрипт из каталога Output (рядом с .exe). Если файла нет —
        /// бросает с понятной диагностикой, чтобы юзер не получил мутный
        /// SQLiteException на первом же SELECT.
        /// </summary>
        private static string LoadSchemaScript()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, SchemaFileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Не найден файл схемы '{SchemaFileName}' в каталоге '{baseDir}'. " +
                    "Убедитесь, что файл добавлен в проект и его свойство " +
                    "'Copy to Output Directory' = 'Copy if newer'.",
                    path);
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Накатывает SQL-скрипт целиком одним батчем. SQLite понимает несколько
        /// statement'ов через ';' внутри одного <see cref="SQLiteCommand"/> — внешняя
        /// транзакция гарантирует атомарность инициализации.
        /// </summary>
        private static void ExecuteSchemaOnFreshDb(string dbPath, string schemaSql)
        {
            using (var conn = new SQLiteConnection(BuildConnectionString(dbPath)))
            {
                conn.Open();

                // foreign_keys в схеме включается явным PRAGMA, продублируем здесь —
                // на старых версиях провайдера PRAGMA в скрипте не всегда применяется
                // до начала транзакции.
                using (var pragma = conn.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA foreign_keys = ON;";
                    pragma.ExecuteNonQuery();
                }

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

        /// <summary>
        /// Минимальная корректная connection string для прямого использования
        /// в SqliteBootstrap. Пакет EF6 (App.config) использует свою — там
        /// добавлены Pooling и Journal Mode=WAL.
        /// </summary>
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
