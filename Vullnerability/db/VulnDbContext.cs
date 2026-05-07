using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Vullnerability.db
{
    // EF6 Code First контекст для SQLite-БД с уязвимостями.
    // Connection string и файл .sqlite готовит SqliteBootstrap.
    public class VulnDbContext : DbContext
    {
        public VulnDbContext() : base("name=VulnDbContext")
        {
            // БД уже создана скриптом 01_schema.sqlite.sql, EF её не трогает
            Database.SetInitializer<VulnDbContext>(null);
        }

        // на случай, если путь к файлу .sqlite приходит извне
        public VulnDbContext(System.Data.Common.DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
            Database.SetInitializer<VulnDbContext>(null);
        }

        public DbSet<Vulnerability> Vulnerabilities { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<OsPlatform> OsPlatforms { get; set; }
        public DbSet<VulnClass> VulnClasses { get; set; }
        public DbSet<SeverityLevel> SeverityLevels { get; set; }
        public DbSet<VulnStatus> VulnStatuses { get; set; }
        public DbSet<ExploitAvailability> ExploitAvailabilities { get; set; }
        public DbSet<ExploitationMethod> ExploitationMethods { get; set; }
        public DbSet<FixMethod> FixMethods { get; set; }
        public DbSet<VulnState> VulnStates { get; set; }
        public DbSet<IncidentRelation> IncidentRelations { get; set; }
        public DbSet<Cwe> Cwes { get; set; }
        public DbSet<VulnerabilityProduct> VulnerabilityProducts { get; set; }
        public DbSet<VulnerabilitySourceLink> VulnerabilitySourceLinks { get; set; }
        public DbSet<VulnerabilityExternalId> VulnerabilityExternalIds { get; set; }
        public DbSet<VulnerabilityMitigation> VulnerabilityMitigations { get; set; }
        public DbSet<VulnerabilityTestingUpdate> VulnerabilityTestingUpdates { get; set; }
        public DbSet<VulnerabilityCwe> VulnerabilityCwes { get; set; }

        protected override void OnModelCreating(DbModelBuilder mb)
        {
            mb.Conventions.Remove<PluralizingTableNameConvention>();

            // CVSS — одна цифра после точки
            mb.Entity<Vulnerability>().Property(v => v.Cvss2_0_Score).HasPrecision(4, 1);
            mb.Entity<Vulnerability>().Property(v => v.Cvss3_0_Score).HasPrecision(4, 1);
            mb.Entity<Vulnerability>().Property(v => v.Cvss4_0_Score).HasPrecision(4, 1);

            // продукт — типы продукта (многие ко многим)
            mb.Entity<Product>()
                .HasMany(p => p.ProductTypes)
                .WithMany(t => t.Products)
                .Map(m =>
                {
                    m.ToTable("product_product_types");
                    m.MapLeftKey("product_id");
                    m.MapRightKey("product_type_id");
                });

            // уязвимость — CWE (многие ко многим, через отдельную сущность)
            mb.Entity<VulnerabilityCwe>()
                .HasKey(vc => new { vc.VulnerabilityId, vc.CweId });
            mb.Entity<VulnerabilityCwe>()
                .HasRequired(vc => vc.Vulnerability)
                .WithMany(v => v.VulnerabilityCwes)
                .HasForeignKey(vc => vc.VulnerabilityId)
                .WillCascadeOnDelete(true);
            mb.Entity<VulnerabilityCwe>()
                .HasRequired(vc => vc.Cwe)
                .WithMany(c => c.VulnerabilityCwes)
                .HasForeignKey(vc => vc.CweId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(mb);
        }
    }

    // ---- Справочники ----

    [Table("vendors")]
    public class Vendor
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(255)] public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }

    [Table("product_types")]
    public class ProductType
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(255)] public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }

    [Table("products")]
    public class Product
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(500)] public string Name { get; set; }
        [Column("vendor_id")] public int? VendorId { get; set; }

        [ForeignKey(nameof(VendorId))] public virtual Vendor Vendor { get; set; }

        public virtual ICollection<ProductType> ProductTypes { get; set; } = new HashSet<ProductType>();
        public virtual ICollection<VulnerabilityProduct> VulnerabilityProducts { get; set; } = new HashSet<VulnerabilityProduct>();
    }

    [Table("os_platforms")]
    public class OsPlatform
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(500)] public string Name { get; set; }
    }

    [Table("vuln_classes")]
    public class VulnClass
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(255)] public string Name { get; set; }
    }

    [Table("severity_levels")]
    public class SeverityLevel
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(64)] public string Name { get; set; }
    }

    [Table("vuln_statuses")]
    public class VulnStatus
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(128)] public string Name { get; set; }
    }

    [Table("exploit_availabilities")]
    public class ExploitAvailability
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(128)] public string Name { get; set; }
    }

    [Table("exploitation_methods")]
    public class ExploitationMethod
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(255)] public string Name { get; set; }
    }

    [Table("fix_methods")]
    public class FixMethod
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(255)] public string Name { get; set; }
    }

    [Table("vuln_states")]
    public class VulnState
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(64)] public string Name { get; set; }
    }

    [Table("incident_relations")]
    public class IncidentRelation
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("name"), Required, StringLength(64)] public string Name { get; set; }
    }

    [Table("cwes")]
    public class Cwe
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("code"), Required, StringLength(32)] public string Code { get; set; }
        [Column("description"), StringLength(1000)] public string Description { get; set; }

        public virtual ICollection<VulnerabilityCwe> VulnerabilityCwes { get; set; } = new HashSet<VulnerabilityCwe>();
    }

    // ---- Основная таблица ----

    [Table("vulnerabilities")]
    public class Vulnerability
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("bdu_code"), Required, StringLength(32)] public string BduCode { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("description")] public string Description { get; set; }
        [Column("discovery_date")] public DateTime? DiscoveryDate { get; set; }
        [Column("publication_date")] public DateTime? PublicationDate { get; set; }
        [Column("last_update_date")] public DateTime? LastUpdateDate { get; set; }

        [Column("cvss_2_0_vector"), StringLength(255)] public string Cvss2_0_Vector { get; set; }
        [Column("cvss_2_0_score")] public decimal? Cvss2_0_Score { get; set; }
        [Column("cvss_3_0_vector"), StringLength(255)] public string Cvss3_0_Vector { get; set; }
        [Column("cvss_3_0_score")] public decimal? Cvss3_0_Score { get; set; }
        [Column("cvss_4_0_vector"), StringLength(255)] public string Cvss4_0_Vector { get; set; }
        [Column("cvss_4_0_score")] public decimal? Cvss4_0_Score { get; set; }

        [Column("fix_info")] public string FixInfo { get; set; }
        [Column("other_info")] public string OtherInfo { get; set; }
        [Column("exploitation_consequences")] public string ExploitationConsequences { get; set; }

        // развёрнутый текст уровня опасности (может содержать несколько строк по разным CVSS)
        [Column("severity_text")] public string SeverityText { get; set; }

        [Column("vuln_class_id")] public int? VulnClassId { get; set; }
        [Column("severity_level_id")] public int? SeverityLevelId { get; set; }
        [Column("status_id")] public int? StatusId { get; set; }
        [Column("state_id")] public int? StateId { get; set; }
        [Column("exploit_availability_id")] public int? ExploitAvailabilityId { get; set; }
        [Column("exploitation_method_id")] public int? ExploitationMethodId { get; set; }
        [Column("fix_method_id")] public int? FixMethodId { get; set; }
        [Column("incident_relation_id")] public int? IncidentRelationId { get; set; }
        [Column("cwe_id")] public int? CweId { get; set; }

        [ForeignKey(nameof(VulnClassId))] public virtual VulnClass VulnClass { get; set; }
        [ForeignKey(nameof(SeverityLevelId))] public virtual SeverityLevel SeverityLevel { get; set; }
        [ForeignKey(nameof(StatusId))] public virtual VulnStatus Status { get; set; }
        [ForeignKey(nameof(StateId))] public virtual VulnState State { get; set; }
        [ForeignKey(nameof(ExploitAvailabilityId))] public virtual ExploitAvailability ExploitAvailability { get; set; }
        [ForeignKey(nameof(ExploitationMethodId))] public virtual ExploitationMethod ExploitationMethod { get; set; }
        [ForeignKey(nameof(FixMethodId))] public virtual FixMethod FixMethod { get; set; }
        [ForeignKey(nameof(IncidentRelationId))] public virtual IncidentRelation IncidentRelation { get; set; }
        [ForeignKey(nameof(CweId))] public virtual Cwe Cwe { get; set; }

        public virtual ICollection<VulnerabilityProduct> Products { get; set; } = new HashSet<VulnerabilityProduct>();
        public virtual ICollection<VulnerabilitySourceLink> SourceLinks { get; set; } = new HashSet<VulnerabilitySourceLink>();
        public virtual ICollection<VulnerabilityExternalId> ExternalIds { get; set; } = new HashSet<VulnerabilityExternalId>();
        public virtual ICollection<VulnerabilityMitigation> Mitigations { get; set; } = new HashSet<VulnerabilityMitigation>();
        public virtual ICollection<VulnerabilityTestingUpdate> TestingUpdates { get; set; } = new HashSet<VulnerabilityTestingUpdate>();
        public virtual ICollection<VulnerabilityCwe> VulnerabilityCwes { get; set; } = new HashSet<VulnerabilityCwe>();
    }

    // ---- Подчинённые сущности ----

    [Table("vulnerability_products")]
    public class VulnerabilityProduct
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("vulnerability_id")] public int VulnerabilityId { get; set; }
        [Column("product_id")] public int ProductId { get; set; }
        [Column("product_version"), StringLength(255)] public string ProductVersion { get; set; }
        [Column("os_platform_id")] public int? OsPlatformId { get; set; }

        [ForeignKey(nameof(VulnerabilityId))] public virtual Vulnerability Vulnerability { get; set; }
        [ForeignKey(nameof(ProductId))] public virtual Product Product { get; set; }
        [ForeignKey(nameof(OsPlatformId))] public virtual OsPlatform OsPlatform { get; set; }
    }

    [Table("vulnerability_source_links")]
    public class VulnerabilitySourceLink
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("vulnerability_id")] public int VulnerabilityId { get; set; }
        [Column("url"), Required, StringLength(2000)] public string Url { get; set; }

        [ForeignKey(nameof(VulnerabilityId))] public virtual Vulnerability Vulnerability { get; set; }
    }

    [Table("vulnerability_external_ids")]
    public class VulnerabilityExternalId
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("vulnerability_id")] public int VulnerabilityId { get; set; }
        [Column("external_id"), Required, StringLength(128)] public string ExternalId { get; set; }
        [Column("source"), StringLength(32)] public string Source { get; set; }

        [ForeignKey(nameof(VulnerabilityId))] public virtual Vulnerability Vulnerability { get; set; }
    }

    [Table("vulnerability_mitigations")]
    public class VulnerabilityMitigation
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("vulnerability_id")] public int VulnerabilityId { get; set; }
        [Column("measure"), Required] public string Measure { get; set; }

        [ForeignKey(nameof(VulnerabilityId))] public virtual Vulnerability Vulnerability { get; set; }
    }

    [Table("vulnerability_testing_updates")]
    public class VulnerabilityTestingUpdate
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("vulnerability_id")] public int VulnerabilityId { get; set; }
        [Column("update_identifier"), StringLength(255)] public string UpdateIdentifier { get; set; }
        [Column("update_name"), StringLength(500)] public string UpdateName { get; set; }

        [ForeignKey(nameof(VulnerabilityId))] public virtual Vulnerability Vulnerability { get; set; }
    }

    // у одной уязвимости может быть несколько CWE
    [Table("vulnerability_cwes")]
    public class VulnerabilityCwe
    {
        [Column("vulnerability_id")] public int VulnerabilityId { get; set; }
        [Column("cwe_id")] public int CweId { get; set; }

        public virtual Vulnerability Vulnerability { get; set; }
        public virtual Cwe Cwe { get; set; }
    }
}
