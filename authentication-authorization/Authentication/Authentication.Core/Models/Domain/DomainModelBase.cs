using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Authentication.Core.Interfaces;

namespace Authentication.Core.Models.Domain
{
    public abstract class DomainModelBase : IIdentifiableEntity, ICreatableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}