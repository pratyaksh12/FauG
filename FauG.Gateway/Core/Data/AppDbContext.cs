using System;
using FauG.Gateway.Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace FauG.Gateway.Core.Data;

public class AppDbContext(DbContextOptions<AppDbContext>options) : DbContext(options)
{
    public DbSet<Organisation> Orgatisations{get; set;}
    public DbSet<User> Users{get; set;}
    public DbSet<Policy> Policies{get; set;}
    public DbSet<ProviderAccount> ProviderAccounts{get; set;}
    public DbSet<RequestLog> RequestLogs{get; set;}
    public DbSet<VirtualKey> VirtualKeys{get; set;}
    public DbSet<ModelCost> ModelCost{get; set;}


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // relationships
        modelBuilder.Entity<Organisation>().HasMany(org => org.ProviderAccounts).WithOne(account => account.Organisation).HasForeignKey(account => account.OrganisationId);
        modelBuilder.Entity<Organisation>().HasMany(org => org.Users).WithOne(user => user.Organisation).HasForeignKey(user => user.OrganisationId);
        modelBuilder.Entity<User>().HasMany(user => user.VirtualKeys).WithOne(key => key.User).HasForeignKey(key => key.UserId);
        modelBuilder.Entity<VirtualKey>().HasMany(key => key.RequestLogs).WithOne(request => request.VirtualKey).HasForeignKey(request => request.VirtualKeyId);
        modelBuilder.Entity<VirtualKey>().HasOne(key => key.Policy).WithOne(policy => policy.VirtualKey).HasForeignKey<Policy>(policy => policy.VirtualKeyId);

        modelBuilder.Entity<ModelCost>().HasData(
            new ModelCost{Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ModelName = "gpt-4o", Provider="OpenAI", InputCostPer1k=0.002m, OutputCostPer1k=0.00125m},
            new ModelCost{Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ModelName = "gpt-3.5-turbo", Provider="OpenAI", InputCostPer1k=0.0005m, OutputCostPer1k=0.0015m},
            new ModelCost{Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ModelName="llama3-70b-8192", Provider="Groq", InputCostPer1k=0.00059m, OutputCostPer1k=0.00079m}
        );
    }
}
