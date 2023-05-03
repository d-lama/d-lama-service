using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ProjectEntities
{
    public class LabeledDataPoint : Entity
    {
        [Required]
        public int LabelId { get; set; }
        public Label Label { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public int DataPointId { get; set; }
        public DataPoint DataPoint { get; set; } = null!;
    }
}
