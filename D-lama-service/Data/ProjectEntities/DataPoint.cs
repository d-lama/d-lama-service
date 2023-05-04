using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Data point entity.
    /// </summary>
    public abstract class DataPoint : Entity
    {
        [Required]
        public int DataPointIndex { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreationDate { get; set; } // on create

        [Required]
        public DateTime UpdateDate { get; set; } // on update

        [Required]
        public int Version { get; set; } // increment per update

        public int ProjectId { get; set; }
        
        [JsonIgnore]
        public Project Project { get; set; } = null!;

        public ICollection<LabeledDataPoint> LabeledDataPoints { get; } = new List<LabeledDataPoint>();

    }
}