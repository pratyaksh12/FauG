using System;

namespace FauG.Gateway.Core.Entities;

public class User : BaseModel
{
    public decimal AllocatedBudget{get; set;}
    public decimal CurrentSpend{get; set;}
    public bool Access{get; set;} = false;

    // relationships
    public ICollection<VirtualKey> VirtualKeys{get; set;} = new List<VirtualKey>();
    public Guid OrganisationId{get; set;}
    public Organisation Organisation{get; set;} = null!;
}
