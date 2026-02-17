using System;

namespace FauG.Gateway.Core.Entities;

public class VirtualKey : BaseModel
{
    public string KeyHash{get; set;} = string.Empty;
    public DateTime LastUsedAt{get; set;}
    public bool IsRevoked{get; set;} = false;

    // relationships
    public User User{get; set;} = null!;
    public Policy Policy{get; set;} = null!;
    public ICollection<RequestLog> RequestLogs{get; set;} = new List<RequestLog>();

}
