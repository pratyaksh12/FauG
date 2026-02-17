using System;
using FauG.Gateway.Core.Entities;
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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // relationships
        modelBuilder.Entity<Organisation>().HasMany(org => org.ProviderAccounts).WithOne(account => account.Organisation).HasForeignKey(account => account.OrganisationId);
        modelBuilder.Entity<Organisation>().HasMany(org => org.Users).WithOne(user => user.Organisation).HasForeignKey(user => user.OrganisationId);
        modelBuilder.Entity<User>().HasMany(user => user.VirtualKeys).WithOne(key => key.User).HasForeignKey(key => key.UserId);
        modelBuilder.Entity<VirtualKey>().HasMany(key => key.RequestLogs).WithOne(request => request.VirtualKey).HasForeignKey(request => request.VirtualKeyId);
        modelBuilder.Entity<VirtualKey>().HasOne(key => key.Policy).WithOne(policy => policy.VirtualKey).HasForeignKey<Policy>(policy => policy.VirtualKeyId);
    }
}
