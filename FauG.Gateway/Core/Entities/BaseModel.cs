using System;

namespace FauG.Gateway.Core.Entities;

public class BaseModel
{
    public Guid Id{get; set;} = Guid.NewGuid();
    public DateTime CreatedAt{get; set;} = DateTime.UtcNow;
}
