using System;

namespace FauG.Gateway.Core.Entities;

public class ModelCost : BaseModel
{
    public required string ModelName{get; set;}
    public required string Provider{get; set;}

    public decimal InputCostPer1k{get; set;}
    public decimal OutputCostPer1k{get; set;}
}
