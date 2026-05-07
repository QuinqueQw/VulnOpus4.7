namespace Vullnerability.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class cwes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public cwes()
        {
            vulnerabilities = new HashSet<vulnerabilities>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(32)]
        public string code { get; set; }

        [StringLength(1000)]
        public string description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vulnerabilities> vulnerabilities { get; set; }
    }
}
