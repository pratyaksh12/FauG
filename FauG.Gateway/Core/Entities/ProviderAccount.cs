using System;

namespace FauG.Gateway.Core.Entities;

public class ProviderAccount : BaseModel
{
    public required string ProviderName{get; set;}
    public required string EncryptedApiKey{get; set;}

    // relationships
    public Guid OrganisationId{get; set;}
    public Organisation Organisation{get; set;} = null!;
}
