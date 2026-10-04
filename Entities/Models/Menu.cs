using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Menu
    {
        public Menu()
        {
            RolePermissions = new HashSet<RolePermission>();
        }

        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string MenuCode { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Icon { get; set; }
        public string Link { get; set; }
        public int? OrderNo { get; set; }
        public string FullParent { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
