-- ============================================================================
-- 3НФ-схема БД для справочника уязвимостей БДУ ФСТЭК (SQLite-вариант)
-- Источник: vullist.xlsx
-- СУБД: SQLite 3.x (через System.Data.SQLite + EF6)
-- ----------------------------------------------------------------------------
-- Отличия от 01_schema.sql (T-SQL → SQLite):
--   * нет схемы `dbo.` и `USE`/`CREATE DATABASE` — БД это сам файл .sqlite;
--   * `INT IDENTITY(1,1) PRIMARY KEY` → `INTEGER PRIMARY KEY AUTOINCREMENT`
--     (одна колонка, авто-инкремент, монотонные ID без переиспользования);
--   * `NVARCHAR(N) / NVARCHAR(MAX)`        → `TEXT` (SQLite не enforce'ит длину);
--   * `DATE`                                → `TEXT` в ISO 8601 (EF6-провайдер сам
--     сериализует DateTime ↔ строку);
--   * `DECIMAL(4,1)`                        → `REAL` (CVSS 0.0..10.0,
--     IEEE 754, точность ≥10 знаков — для отображения и сортировки достаточно);
--   * `BIT`                                 → `INTEGER` (0/1);
--   * нет `GO`-разделителей;
--   * FK работают только при включённом `PRAGMA foreign_keys = ON;` — выставляется
--     EF6-провайдером и в SqliteBootstrap.cs на каждом подключении;
--   * `ON DELETE CASCADE` поддерживается.
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ----------------------------------------------------------------------------
-- DROP в обратном порядке (для повторного запуска / реинициализации)
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS vulnerability_testing_updates;
DROP TABLE IF EXISTS vulnerability_mitigations;
DROP TABLE IF EXISTS vulnerability_external_ids;
DROP TABLE IF EXISTS vulnerability_source_links;
DROP TABLE IF EXISTS vulnerability_cwes;
DROP TABLE IF EXISTS vulnerability_products;
DROP TABLE IF EXISTS product_product_types;
DROP TABLE IF EXISTS vulnerabilities;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS product_types;
DROP TABLE IF EXISTS vendors;
DROP TABLE IF EXISTS os_platforms;
DROP TABLE IF EXISTS vuln_classes;
DROP TABLE IF EXISTS severity_levels;
DROP TABLE IF EXISTS vuln_statuses;
DROP TABLE IF EXISTS exploit_availabilities;
DROP TABLE IF EXISTS exploitation_methods;
DROP TABLE IF EXISTS fix_methods;
DROP TABLE IF EXISTS vuln_states;
DROP TABLE IF EXISTS incident_relations;
DROP TABLE IF EXISTS cwes;

-- ============================================================================
-- СПРАВОЧНИКИ
-- ============================================================================

CREATE TABLE vendors (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_vendors_name UNIQUE (name)
);

CREATE TABLE product_types (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_product_types_name UNIQUE (name)
);

CREATE TABLE products (
    id        INTEGER PRIMARY KEY AUTOINCREMENT,
    name      TEXT    NOT NULL,
    vendor_id INTEGER NULL,
    CONSTRAINT FK_products_vendors      FOREIGN KEY (vendor_id) REFERENCES vendors(id),
    CONSTRAINT UQ_products_vendor_name  UNIQUE (vendor_id, name)
);
CREATE INDEX IX_products_name ON products(name);

CREATE TABLE product_product_types (
    product_id      INTEGER NOT NULL,
    product_type_id INTEGER NOT NULL,
    CONSTRAINT PK_product_product_types               PRIMARY KEY (product_id, product_type_id),
    CONSTRAINT FK_product_product_types_products      FOREIGN KEY (product_id)      REFERENCES products(id)      ON DELETE CASCADE,
    CONSTRAINT FK_product_product_types_product_types FOREIGN KEY (product_type_id) REFERENCES product_types(id) ON DELETE CASCADE
);

CREATE TABLE os_platforms (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_os_platforms_name UNIQUE (name)
);

CREATE TABLE vuln_classes (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_vuln_classes_name UNIQUE (name)
);

CREATE TABLE severity_levels (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_severity_levels_name UNIQUE (name)
);

CREATE TABLE vuln_statuses (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_vuln_statuses_name UNIQUE (name)
);

CREATE TABLE exploit_availabilities (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_exploit_availabilities_name UNIQUE (name)
);

CREATE TABLE exploitation_methods (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_exploitation_methods_name UNIQUE (name)
);

CREATE TABLE fix_methods (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_fix_methods_name UNIQUE (name)
);

CREATE TABLE vuln_states (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_vuln_states_name UNIQUE (name)
);

CREATE TABLE incident_relations (
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT UQ_incident_relations_name UNIQUE (name)
);

CREATE TABLE cwes (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    code        TEXT NOT NULL,
    description TEXT NULL,
    CONSTRAINT UQ_cwes_code UNIQUE (code)
);

-- ============================================================================
-- ОСНОВНАЯ ТАБЛИЦА
-- ============================================================================

CREATE TABLE vulnerabilities (
    id                          INTEGER PRIMARY KEY AUTOINCREMENT,
    bdu_code                    TEXT    NOT NULL,
    name                        TEXT    NULL,
    description                 TEXT    NULL,
    discovery_date              TEXT    NULL,
    publication_date            TEXT    NULL,
    last_update_date            TEXT    NULL,
    cvss_2_0_vector             TEXT    NULL,
    cvss_2_0_score              REAL    NULL,
    cvss_3_0_vector             TEXT    NULL,
    cvss_3_0_score              REAL    NULL,
    cvss_4_0_vector             TEXT    NULL,
    cvss_4_0_score              REAL    NULL,
    fix_info                    TEXT    NULL,
    other_info                  TEXT    NULL,
    exploitation_consequences   TEXT    NULL,
    vuln_class_id               INTEGER NULL,
    -- Полный текст уровня опасности из BDU. FK severity_level_id сохраняется
    -- для фильтров и сортировки на стороне UI/SQL.
    severity_text               TEXT    NULL,
    severity_level_id           INTEGER NULL,
    status_id                   INTEGER NULL,
    state_id                    INTEGER NULL,
    exploit_availability_id     INTEGER NULL,
    exploitation_method_id      INTEGER NULL,
    fix_method_id               INTEGER NULL,
    incident_relation_id        INTEGER NULL,
    cwe_id                      INTEGER NULL,
    CONSTRAINT UQ_vulnerabilities_bdu_code                 UNIQUE      (bdu_code),
    CONSTRAINT FK_vulnerabilities_vuln_classes             FOREIGN KEY (vuln_class_id)           REFERENCES vuln_classes(id),
    CONSTRAINT FK_vulnerabilities_severity_levels          FOREIGN KEY (severity_level_id)       REFERENCES severity_levels(id),
    CONSTRAINT FK_vulnerabilities_vuln_statuses            FOREIGN KEY (status_id)               REFERENCES vuln_statuses(id),
    CONSTRAINT FK_vulnerabilities_vuln_states              FOREIGN KEY (state_id)                REFERENCES vuln_states(id),
    CONSTRAINT FK_vulnerabilities_exploit_availabilities   FOREIGN KEY (exploit_availability_id) REFERENCES exploit_availabilities(id),
    CONSTRAINT FK_vulnerabilities_exploitation_methods     FOREIGN KEY (exploitation_method_id)  REFERENCES exploitation_methods(id),
    CONSTRAINT FK_vulnerabilities_fix_methods              FOREIGN KEY (fix_method_id)           REFERENCES fix_methods(id),
    CONSTRAINT FK_vulnerabilities_incident_relations       FOREIGN KEY (incident_relation_id)    REFERENCES incident_relations(id),
    CONSTRAINT FK_vulnerabilities_cwes                     FOREIGN KEY (cwe_id)                  REFERENCES cwes(id)
);
-- В SQLite индекс по убыванию полезен только для первого ключа сложного индекса
-- и для ORDER BY DESC при count rows. Создаём ASC — оптимизатор решает сам.
CREATE INDEX IX_vulnerabilities_publication_date    ON vulnerabilities(publication_date);
CREATE INDEX IX_vulnerabilities_discovery_date      ON vulnerabilities(discovery_date);
CREATE INDEX IX_vulnerabilities_severity_level_id   ON vulnerabilities(severity_level_id);
CREATE INDEX IX_vulnerabilities_status_id           ON vulnerabilities(status_id);
CREATE INDEX IX_vulnerabilities_class_id            ON vulnerabilities(vuln_class_id);
CREATE INDEX IX_vulnerabilities_cwe_id              ON vulnerabilities(cwe_id);
CREATE INDEX IX_vulnerabilities_cvss3_score         ON vulnerabilities(cvss_3_0_score);

-- ============================================================================
-- ПОДЧИНЁННЫЕ СУЩНОСТИ (1:N и M:N)
-- ============================================================================

CREATE TABLE vulnerability_products (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    vulnerability_id INTEGER NOT NULL,
    product_id       INTEGER NOT NULL,
    product_version  TEXT    NULL,
    os_platform_id   INTEGER NULL,
    CONSTRAINT FK_vulnerability_products_vulnerabilities FOREIGN KEY (vulnerability_id) REFERENCES vulnerabilities(id) ON DELETE CASCADE,
    CONSTRAINT FK_vulnerability_products_products        FOREIGN KEY (product_id)       REFERENCES products(id),
    CONSTRAINT FK_vulnerability_products_os_platforms    FOREIGN KEY (os_platform_id)   REFERENCES os_platforms(id)
);
CREATE INDEX IX_vulnerability_products_vulnerability_id ON vulnerability_products(vulnerability_id);
CREATE INDEX IX_vulnerability_products_product_id       ON vulnerability_products(product_id);
CREATE INDEX IX_vulnerability_products_os_platform_id   ON vulnerability_products(os_platform_id);
CREATE INDEX IX_vulnerability_products_version          ON vulnerability_products(product_version);

-- M:N: уязвимость <-> CWE.
CREATE TABLE vulnerability_cwes (
    vulnerability_id INTEGER NOT NULL,
    cwe_id           INTEGER NOT NULL,
    CONSTRAINT PK_vulnerability_cwes                 PRIMARY KEY (vulnerability_id, cwe_id),
    CONSTRAINT FK_vulnerability_cwes_vulnerabilities FOREIGN KEY (vulnerability_id) REFERENCES vulnerabilities(id) ON DELETE CASCADE,
    CONSTRAINT FK_vulnerability_cwes_cwes            FOREIGN KEY (cwe_id)           REFERENCES cwes(id)
);
CREATE INDEX IX_vulnerability_cwes_vulnerability_id ON vulnerability_cwes(vulnerability_id);
CREATE INDEX IX_vulnerability_cwes_cwe_id           ON vulnerability_cwes(cwe_id);

CREATE TABLE vulnerability_source_links (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    vulnerability_id INTEGER NOT NULL,
    url              TEXT    NOT NULL,
    CONSTRAINT FK_vulnerability_source_links_vulnerabilities FOREIGN KEY (vulnerability_id) REFERENCES vulnerabilities(id) ON DELETE CASCADE
);
CREATE INDEX IX_vulnerability_source_links_vulnerability_id ON vulnerability_source_links(vulnerability_id);

CREATE TABLE vulnerability_external_ids (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    vulnerability_id INTEGER NOT NULL,
    external_id      TEXT    NOT NULL,
    source           TEXT    NULL,
    CONSTRAINT FK_vulnerability_external_ids_vulnerabilities FOREIGN KEY (vulnerability_id) REFERENCES vulnerabilities(id) ON DELETE CASCADE
);
CREATE INDEX IX_vulnerability_external_ids_vulnerability_id ON vulnerability_external_ids(vulnerability_id);
CREATE INDEX IX_vulnerability_external_ids_external_id      ON vulnerability_external_ids(external_id);
CREATE INDEX IX_vulnerability_external_ids_source           ON vulnerability_external_ids(source);

CREATE TABLE vulnerability_mitigations (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    vulnerability_id INTEGER NOT NULL,
    measure          TEXT    NOT NULL,
    CONSTRAINT FK_vulnerability_mitigations_vulnerabilities FOREIGN KEY (vulnerability_id) REFERENCES vulnerabilities(id) ON DELETE CASCADE
);
CREATE INDEX IX_vulnerability_mitigations_vulnerability_id ON vulnerability_mitigations(vulnerability_id);

CREATE TABLE vulnerability_testing_updates (
    id                INTEGER PRIMARY KEY AUTOINCREMENT,
    vulnerability_id  INTEGER NOT NULL,
    update_identifier TEXT    NULL,
    update_name       TEXT    NULL,
    CONSTRAINT FK_vulnerability_testing_updates_vulnerabilities FOREIGN KEY (vulnerability_id) REFERENCES vulnerabilities(id) ON DELETE CASCADE
);
CREATE INDEX IX_vulnerability_testing_updates_vulnerability_id ON vulnerability_testing_updates(vulnerability_id);

-- ============================================================================
-- ПРЕДЗАПОЛНЕНИЕ СПРАВОЧНИКОВ
-- ВАЖНО: порядок INSERT'ов задаёт severity_level_id
--   1=Критический, 2=Высокий, 3=Средний, 4=Низкий, 5=Информационный.
--   Form1 опирается на эти ID при сортировке «по критичности» — не менять порядок.
-- ============================================================================

INSERT INTO severity_levels(name) VALUES
    ('Критический'), ('Высокий'), ('Средний'), ('Низкий');

INSERT INTO vuln_statuses(name) VALUES
    ('Подтверждена производителем'),
    ('Подтверждена в ходе исследований'),
    ('Потенциальная уязвимость');

INSERT INTO vuln_states(name) VALUES
    ('Опубликована'), ('Отклонена');

INSERT INTO exploit_availabilities(name) VALUES
    ('Существует'), ('Существует в открытом доступе'), ('Данные уточняются');

INSERT INTO incident_relations(name) VALUES
    ('Да'), ('Нет'), ('Данные уточняются');

INSERT INTO exploitation_methods(name) VALUES
    ('Анализ целевого объекта'),
    ('Вероятностные методы'),
    ('Данные уточняются'),
    ('Злоупотребление функционалом'),
    ('Инъекция'),
    ('Исчерпание ресурсов'),
    ('Манипулирование ресурсами'),
    ('Манипулирование структурами данных'),
    ('Манипулирование сроками и состоянием'),
    ('Несанкционированный доступ'),
    ('Применение специально созданных элементов'),
    ('Социальная инженерия'),
    ('Сбор информации');

INSERT INTO fix_methods(name) VALUES
    ('Обновление программного обеспечения'),
    ('Использование рекомендаций производителя'),
    ('Использование средств защиты'),
    ('Изменение настроек'),
    ('Замена программного обеспечения'),
    ('Данные уточняются');
