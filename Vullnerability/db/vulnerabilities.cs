namespace Vullnerability.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vulnerabilities
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public vulnerabilities()
        {
            vulnerability_external_ids = new HashSet<vulnerability_external_ids>();
            vulnerability_mitigations = new HashSet<vulnerability_mitigations>();
            vulnerability_products = new HashSet<vulnerability_products>();
            vulnerability_source_links = new HashSet<vulnerability_source_links>();
            vulnerability_testing_updates = new HashSet<vulnerability_testing_updates>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(32)]
        public string bdu_code { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        [Column(TypeName = "date")]
        public DateTime? discovery_date { get; set; }

        [Column(TypeName = "date")]
        public DateTime? publication_date { get; set; }

        [Column(TypeName = "date")]
        public DateTime? last_update_date { get; set; }

        [StringLength(255)]
        public string cvss_2_0_vector { get; set; }

        public decimal? cvss_2_0_score { get; set; }

        [StringLength(255)]
        public string cvss_3_0_vector { get; set; }

        public decimal? cvss_3_0_score { get; set; }

        [StringLength(255)]
        public string cvss_4_0_vector { get; set; }

        public decimal? cvss_4_0_score { get; set; }

        public string fix_info { get; set; }

        public string other_info { get; set; }

        public string exploitation_consequences { get; set; }

        public int? vuln_class_id { get; set; }

        public int? severity_level_id { get; set; }

        public int? status_id { get; set; }

        public int? state_id { get; set; }

        public int? exploit_availability_id { get; set; }

        public int? exploitation_method_id { get; set; }

        public int? fix_method_id { get; set; }

        public int? incident_relation_id { get; set; }

        public int? cwe_id { get; set; }

        public virtual cwes cwes { get; set; }

        public virtual exploit_availabilities exploit_availabilities { get; set; }

        public virtual exploitation_methods exploitation_methods { get; set; }

        public virtual fix_methods fix_methods { get; set; }

        public virtual incident_relations incident_relations { get; set; }

        public virtual severity_levels severity_levels { get; set; }

        public virtual vuln_classes vuln_classes { get; set; }

        public virtual vuln_states vuln_states { get; set; }

        public virtual vuln_statuses vuln_statuses { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vulnerability_external_ids> vulnerability_external_ids { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vulnerability_mitigations> vulnerability_mitigations { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vulnerability_products> vulnerability_products { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vulnerability_source_links> vulnerability_source_links { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vulnerability_testing_updates> vulnerability_testing_updates { get; set; }
    }
}
