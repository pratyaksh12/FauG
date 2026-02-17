using System;

namespace FauG.Gateway.Core.Entities;

public class Organisation : BaseModel
{
    public string Name{get;set;} = string.Empty;
    public decimal TotalMontlyBudget{get; set;}
    public decimal TotalCurrentSpend{get; set;}

    // relationships
    public ICollection<User> Users{get; set;} = new List<User>();
    public ICollection<ProviderAccount> ProviderAccounts{get; set;} = new List<ProviderAccount>();

}
