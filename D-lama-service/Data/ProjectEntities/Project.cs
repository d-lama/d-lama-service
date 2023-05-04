﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Project entity.
    /// </summary>
    public class Project : Entity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreationDate { get; set; }

        [Required]
        public DateTime UpdateDate { get; set; }

        [Required]
        public bool IsReady { get; set; }

        // Required foreign key property
        public int OwnerId { get; set; }

        // Required reference navigation to principal
        [JsonIgnore]
        public User Owner { get; set; } = null!;

        // Collections navigation containing dependents
        public ICollection<DataPoint> DataPoints { get; } = new List<DataPoint>();

        public ICollection<Label> Labels { get; } = new List<Label>();

        public Project(string name, string description)
        {
            Name = name;
            Description = description;
            UpdateDate = DateTime.UtcNow;
            IsReady = false;
        }
    }
}
