using System.ComponentModel.DataAnnotations;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Data point set entity.
    /// </summary>
    public class DataPointSet : Entity
    {
        [Required]
        public string DataPointSetName { get; set; }

        public DataPointSet(string dataPointSetName)
        {
            DataPointSetName = dataPointSetName;
        }

    }
}
