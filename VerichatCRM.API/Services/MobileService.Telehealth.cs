using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Services
{
    public partial class MobileService
    {
        public async Task<IEnumerable<TelehealthAppointmentResponse>> GetAppointmentsAsync(string userId) =>
            await _db.TelehealthAppointments
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.StartsAt)
                .Select(a => MapAppointment(a))
                .ToListAsync();

        public async Task<TelehealthAppointmentResponse> BookAppointmentAsync(
            string userId, string doctorName, string specialty, string facility, DateTime startsAt)
        {
            var appt = new TelehealthAppointment
            {
                UserId = userId,
                DoctorName = doctorName.Trim(),
                Specialty = specialty ?? string.Empty,
                Facility = facility ?? string.Empty,
                StartsAt = startsAt,
                Status = "pending"
            };
            _db.TelehealthAppointments.Add(appt);
            await _db.SaveChangesAsync();
            return MapAppointment(appt);
        }

        private static TelehealthAppointmentResponse MapAppointment(TelehealthAppointment a) => new()
        {
            Id = a.AppointmentId,
            DoctorName = a.DoctorName,
            Specialty = a.Specialty,
            Facility = a.Facility,
            StartsAt = a.StartsAt,
            Status = a.Status
        };

        public async Task<LabResultsMobileResponse> GetLabResultsAsync(string userId)
        {
            var results = await _db.LabResultRecords
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();
            return new LabResultsMobileResponse
            {
                LimsConnected = true,
                ConnectedLabs = results.Select(r => r.Laboratory).Distinct().ToList(),
                Results = results.Select(r => new LabResultMobileResponse
                {
                    Id = r.LabResultId,
                    Reference = r.Reference,
                    TestName = r.TestName,
                    Laboratory = r.Laboratory,
                    ReportedAt = r.ReportedAt,
                    Status = r.Status,
                    FileSizeKb = r.FileSizeKb,
                    IsNew = r.IsNew,
                    PdfUrl = r.PdfUrl
                }).ToList()
            };
        }

        public async Task<bool> MarkLabResultViewedAsync(string userId, string labResultId)
        {
            var result = await _db.LabResultRecords
                .FirstOrDefaultAsync(r => r.LabResultId == labResultId && r.UserId == userId);
            if (result == null) return false;
            result.IsNew = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MobilePrescriptionResponse>> GetPrescriptionsAsync(string userId) =>
            await _db.MobilePrescriptions
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.IssuedAt)
                .Select(p => new MobilePrescriptionResponse
                {
                    Id = p.PrescriptionId,
                    Reference = p.Reference,
                    Medication = p.Medication,
                    Instructions = p.Instructions,
                    Prescriber = p.Prescriber,
                    IssuedAt = p.IssuedAt,
                    Status = p.Status,
                    QrPayload = p.QrPayload
                }).ToListAsync();
    }
}
