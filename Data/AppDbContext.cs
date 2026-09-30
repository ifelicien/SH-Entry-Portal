using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;
using SH_Entry_Portal.Models.Generated;

namespace SH_Entry_Portal.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Member> Members { get; set; }

    // Manually added: audit trail for member changes
    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    // Manually added: second table for tracking sisterhood events
    public virtual DbSet<Event> Events { get; set; }

    // Manually added: audit trail for event changes
    public virtual DbSet<EventAuditLog> EventAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicit null translator: without it, Npgsql snake_cases enum labels (e.g. "Member" -> "member"),
        // which don't match the actual Postgres enum values we defined
        modelBuilder
            .HasPostgresEnum<MemberRole>(nameTranslator: new NpgsqlNullNameTranslator())
            .HasPostgresEnum<MemberStatus>(nameTranslator: new NpgsqlNullNameTranslator());

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("members_pkey");

            entity.ToTable("members");

            entity.HasIndex(e => e.Email, "members_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.JoinedOn)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("joined_on");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("audit_log");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("now()").HasColumnName("changed_at");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("events");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Location).HasColumnName("location");
            // Explicit column type: EF Core's Npgsql provider defaults DateTime to timestamptz (UTC-only),
            // but this column stores plain local wall-clock time with no timezone conversion
            entity.Property(e => e.EventTime)
                .HasColumnName("event_time")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsPaid).HasColumnName("is_paid");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
        });

        modelBuilder.Entity<EventAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("event_audit_log");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("now()").HasColumnName("changed_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
