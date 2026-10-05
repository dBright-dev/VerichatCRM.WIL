using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    public class InquiryService : IInquiryService
    {
        /// <summary>Hours within which an enquiry should receive a first response.</summary>
        private const int ResponseSlaHours = 24;

        private readonly AppDbContext _context;

        public InquiryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InquiryResponse>> GetInquiriesAsync(string? status, string? source, int? minScore)
        {
            var query = _context.Inquiries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                query = query.Where(i => i.Status == status);

            if (!string.IsNullOrWhiteSpace(source) && source != "All")
                query = query.Where(i => i.Source == source);

            if (minScore.HasValue)
                query = query.Where(i => i.Score >= minScore.Value);

            // Highest scored first: the queue is worked top-down.
            var items = await query
                .OrderByDescending(i => i.Score)
                .ThenByDescending(i => i.CreatedAt)
                .ToListAsync();

            return items.Select(Map).ToList();
        }

        public async Task<InquiryResponse?> GetByIdAsync(string inquiryId)
        {
            var inquiry = await _context.Inquiries
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.InquiryId == inquiryId);

            return inquiry is null ? null : Map(inquiry);
        }

        public async Task<InquiryResponse> CreateAsync(CreateInquiryRequest request)
        {
            var source = InquirySources.All.Contains(request.Source ?? string.Empty)
                ? request.Source!
                : InquirySources.Website;

            var inquiry = new Inquiry
            {
                Subject = request.Subject.Trim(),
                ContactName = request.ContactName.Trim(),
                ContactEmail = Blank(request.ContactEmail),
                ContactPhone = Blank(request.ContactPhone),
                Company = Blank(request.Company),
                Description = Blank(request.Description),
                ClientId = Blank(request.ClientId),
                Source = source,
                Score = Math.Clamp(request.Score, 0, 100),
                Status = InquiryStatuses.New,
                ResponseDueAt = DateTime.UtcNow.AddHours(ResponseSlaHours),
                CreatedAt = DateTime.UtcNow,
            };

            // Only keep the client link if that client actually exists, otherwise
            // the insert would fail on the foreign key.
            if (inquiry.ClientId != null &&
                !await _context.Clients.AnyAsync(c => c.ClientId == inquiry.ClientId))
            {
                inquiry.ClientId = null;
            }

            _context.Inquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            return Map(inquiry);
        }

        public async Task<InquiryResponse?> UpdateAsync(string inquiryId, UpdateInquiryRequest request)
        {
            var inquiry = await _context.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == inquiryId);
            if (inquiry is null) return null;

            if (!string.IsNullOrWhiteSpace(request.Subject))
                inquiry.Subject = request.Subject.Trim();
            if (!string.IsNullOrWhiteSpace(request.ContactName))
                inquiry.ContactName = request.ContactName.Trim();
            if (request.ContactEmail != null) inquiry.ContactEmail = Blank(request.ContactEmail);
            if (request.ContactPhone != null) inquiry.ContactPhone = Blank(request.ContactPhone);
            if (request.Company != null) inquiry.Company = Blank(request.Company);
            if (request.Notes != null) inquiry.Notes = Blank(request.Notes);
            if (request.OwnerUserId != null) inquiry.OwnerUserId = Blank(request.OwnerUserId);

            if (request.Score.HasValue)
                inquiry.Score = Math.Clamp(request.Score.Value, 0, 100);

            if (request.ClientId != null)
            {
                var clientId = Blank(request.ClientId);
                inquiry.ClientId = clientId != null &&
                                   await _context.Clients.AnyAsync(c => c.ClientId == clientId)
                    ? clientId
                    : null;
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!InquiryStatuses.All.Contains(request.Status))
                    throw new ArgumentException($"'{request.Status}' is not a valid inquiry status.");

                inquiry.Status = request.Status;

                // Moving off "New" means the first response has happened, which
                // closes the SLA clock.
                if (inquiry.RespondedAt is null && inquiry.Status != InquiryStatuses.New)
                    inquiry.RespondedAt = DateTime.UtcNow;
            }

            inquiry.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Map(inquiry);
        }

        public async Task<bool> DeleteAsync(string inquiryId)
        {
            var inquiry = await _context.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == inquiryId);
            if (inquiry is null) return false;

            _context.Inquiries.Remove(inquiry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InquirySummary> GetSummaryAsync()
        {
            var now = DateTime.UtcNow;

            var total = await _context.Inquiries.CountAsync();
            var open = await _context.Inquiries.CountAsync(i => InquiryStatuses.Open.Contains(i.Status));
            var fresh = await _context.Inquiries.CountAsync(i => i.Status == InquiryStatuses.New);
            var won = await _context.Inquiries.CountAsync(i => i.Status == InquiryStatuses.Won);

            var overdue = await _context.Inquiries.CountAsync(i =>
                InquiryStatuses.Open.Contains(i.Status) &&
                i.ResponseDueAt != null &&
                i.ResponseDueAt < now);

            var avgScore = await _context.Inquiries
                .Select(i => (decimal?)i.Score)
                .AverageAsync() ?? 0m;

            return new InquirySummary
            {
                Total = total,
                Open = open,
                New = fresh,
                Overdue = overdue,
                Won = won,
                AverageScore = Math.Round(avgScore, 1),
            };
        }

        private static string? Blank(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static InquiryResponse Map(Inquiry i) => new()
        {
            InquiryId = i.InquiryId,
            ClientId = i.ClientId,
            ClientName = i.Client?.ClientName,
            ContactName = i.ContactName,
            ContactEmail = i.ContactEmail,
            ContactPhone = i.ContactPhone,
            Company = i.Company,
            Subject = i.Subject,
            Description = i.Description,
            Source = i.Source,
            Status = i.Status,
            Score = i.Score,
            OwnerUserId = i.OwnerUserId,
            Notes = i.Notes,
            ResponseDueAt = i.ResponseDueAt,
            RespondedAt = i.RespondedAt,
            IsOverdue = InquiryStatuses.Open.Contains(i.Status)
                        && i.ResponseDueAt.HasValue
                        && i.ResponseDueAt.Value < DateTime.UtcNow,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt,
        };
    }
}