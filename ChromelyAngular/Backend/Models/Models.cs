using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChromelyAngular.Backend.Models
{
    public interface IAuditableObject
    {
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }

    public abstract class BaseObject : IAuditableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int ExternalId { get; set; }
        public bool Deleted { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }

    [Table("Persons")]
    public class Person : BaseObject
    {
        public string FullName { get; set; }
        public string Company { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Zone { get; set; }
        public string Status { get; set; }
        public string BlockReason { get; set; }
        public bool HasDeclaration { get; set; }
        public bool HasPcr { get; set; }
        public string Photo { get; set; }

    }

    [Table("Documents")]
    public class PcrDocument : BaseObject
    {
        public Person Person { get; set; }
        public int? PersonId { get; set; }
    }

    [Table("Requests")]
    public class FileRequest : BaseObject
    {
        public Event Event { get; set; }
        public int? EventId { get; set; }
    }

    [Table("Events")]
    public class Event : BaseObject
    {
        public DateTime Date { get; set; }
    }
}
