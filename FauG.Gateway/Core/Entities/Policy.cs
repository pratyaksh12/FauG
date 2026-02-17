using System;
using FauG.Gateway.Core.Entities;

namespace FauG.Gateway.Core;

public class Policy : BaseModel
{   
    public decimal MaxTokenSpend{get; set;}
    public int RequestsPerMinute{get; set;}
    public string[] AllowedModels{get; set;} = [];

    // relationships
    public Guid VirtualKeyId{get; set;}
    public VirtualKey VirtualKey{get; set;} = null!;
}
