using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Base
{

    /// <summary>
    /// for when we want to know we should create that class in DB
    /// </summary>
    public interface IEntity
    {


    }

    /// <summary>
    /// for when  ID type is not int
    /// </summary>
    public abstract class BaseEntity<TKey> : IEntity
    {
        [Key]
        public TKey Id { get; set; }
        
        /// <inheritdoc/>
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public DateTime? ModifiedDate { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public int? CreatedByUserId { get; set; } = -1;

        /// <inheritdoc/>
        public int? ModifiedByUserId { get; set; } = -1;

    }

    public abstract class BaseEntity : BaseEntity<int>
    {

    }
}
