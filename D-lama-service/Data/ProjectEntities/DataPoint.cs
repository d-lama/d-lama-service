using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Data point entity.
    /// </summary>
    public abstract class DataPoint : Entity
    {
        [Required]
        public int Row { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreationTime { get; set; } // on create
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTime { get; set; } // on update
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Version { get; set; } // increment per update
    }
}