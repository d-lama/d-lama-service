﻿using Data.ProjectEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// User entity.
    /// </summary>
    public class User : Entity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }

        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; }

        [Required]
        [JsonIgnore]
        public string PasswordSalt { get; set; } 

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        // Collection navigation containing dependents
        public ICollection<Project> Projects { get; } = new List<Project>();
        public ICollection<LabeledDataPoint> LabeledDataPoints { get; } = new List<LabeledDataPoint>();

        public User(string email, string firstName, string lastName, string passwordHash, string passwordSalt, DateTime birthDate, bool isAdmin)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            BirthDate = birthDate.Date;
            IsAdmin = isAdmin;
        }
    }
}
