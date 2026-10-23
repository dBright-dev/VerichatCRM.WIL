using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Interceptors
{
    /// <summary>
    /// Writes an <see cref="AuditLog"/> row for every Added / Modified / Deleted
    /// entity on a tracked save. Password hashes are never written out, and the
    /// AuditLog set itself is skipped so the interceptor cannot recurse.
    /// </summary>
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(User.PasswordHash),
            nameof(User.PasswordResetToken),
            nameof(User.PasswordResetExpiry)
        };

        private readonly IHttpContextAccessor _http;

        public AuditSaveChangesInterceptor(IHttpContextAccessor http) => _http = http;

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (eventData is not null) WriteEntries(eventData.Context);
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (eventData is not null) await WriteEntriesAsync(eventData.Context, cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void WriteEntries(DbContext? context)
        {
            if (context == null) return;

            var entries = CollectEntries(context);
            if (entries.Count == 0) return;

            var (userId, userEmail, ip) = ReadCaller();

            foreach (var (entry, action) in entries)
                context.Set<AuditLog>().Add(BuildEntry(entry, action, userId, userEmail, ip));

            context.SaveChanges();
        }

        private async Task WriteEntriesAsync(DbContext? context, CancellationToken ct)
        {
            if (context == null) return;

            var entries = CollectEntries(context);
            if (entries.Count == 0) return;

            var (userId, userEmail, ip) = ReadCaller();

            foreach (var (entry, action) in entries)
                context.Set<AuditLog>().Add(BuildEntry(entry, action, userId, userEmail, ip));

            await context.SaveChangesAsync(ct);
        }

        private (string UserId, string? UserEmail, string? Ip) ReadCaller()
        {
            var principal = _http.HttpContext?.User;
            var userId = principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? principal?.FindFirst("sub")?.Value
                         ?? "system";
            var userEmail = principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                            ?? principal?.FindFirst("email")?.Value;
            var ip = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return (userId, userEmail, ip);
        }

        private static List<(EntityEntry Entry, string Action)> CollectEntries(DbContext context) =>
            context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                            && e.Entity is not AuditLog)
                .Select(e => (e, e.State switch
                {
                    EntityState.Added => "Added",
                    EntityState.Deleted => "Deleted",
                    _ => "Modified"
                }))
                .ToList();

        private static AuditLog BuildEntry(EntityEntry entry, string action, string userId, string? userEmail, string? ip) =>
            new()
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = entry.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .Select(p => p.CurrentValue?.ToString() ?? string.Empty)
                    .FirstOrDefault() ?? string.Empty,
                OldValues = entry.State == EntityState.Added ? null : Serialize(entry, original: true),
                NewValues = entry.State == EntityState.Deleted ? null : Serialize(entry, original: false),
                IpAddress = ip,
                Timestamp = DateTime.UtcNow
            };

        private static string? Serialize(EntityEntry entry, bool original)
        {
            var payload = entry.Properties
                .Where(p => !p.Metadata.IsPrimaryKey() && !SensitiveProperties.Contains(p.Metadata.Name))
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => (original ? p.OriginalValue : p.CurrentValue)?.ToString() ?? string.Empty);

            return payload.Count == 0 ? null : JsonSerializer.Serialize(payload);
        }
    }
}