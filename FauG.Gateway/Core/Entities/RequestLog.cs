using System;

namespace FauG.Gateway.Core.Entities;

public class RequestLog : BaseModel
{
    public required string ModelName{get; set;}
    public int PromptTokens{get; set;}
    public int CompletionTokens{get; set;}
    public int TotalTokens{get; set;}
    public decimal EstimatedCost{get; set;}
    public int StatusCode{get; set;}
    public long DurationMs{get; set;}

    // relationship
    public Guid VirtualKeyId{get; set;}
    public VirtualKey VirtualKey{get; set;} = null!;
    public Guid ProviderAccountId{get; set;}
    public ProviderAccount ProviderAccount{get; set;} = null!;
}
