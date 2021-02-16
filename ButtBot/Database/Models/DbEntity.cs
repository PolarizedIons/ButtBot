using System;
using System.ComponentModel.DataAnnotations;

namespace ButtBot.Database.Models
{
    public class DbEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
