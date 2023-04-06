using System.ComponentModel.DataAnnotations;

namespace Data
{
    /// <summary>
    /// Example class... Will be deleted....
    /// </summary>
    public class Example : Entity
    {
        [Required]
        public string Name { get; set; }
        public int Number { get; set; }
        public string Description { get; set; }

        public Example(string name, int number, string description) 
        {
            Name = name;
            Number = number;
            Description = description;
        }
    }
}
