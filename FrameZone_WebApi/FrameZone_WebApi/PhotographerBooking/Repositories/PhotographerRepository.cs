using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.PhotographerBooking.Repositories
{
    public class PhotographerRepository : IPhotographerRepository
    {
        private readonly AAContext _context;
        private readonly ILogger<PhotographerRepository> _logger;

        public PhotographerRepository(AAContext context, ILogger<PhotographerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Photographer>> GetAllPhotographersAsync()
        {
            return await _context.Photographers
                .Include(p => p.User)
                .Include(p => p.ServiceAreas)
                .Include(p => p.PhotographerServices).ThenInclude(ps => ps.ServiceType)
                .Include(p => p.PhotographerSpecialties).ThenInclude(ps => ps.SpecialtyTag)
                .Include(p => p.Bookings).ThenInclude(b => b.Reviews).ThenInclude(r => r.ReviewPhotos)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Photographer> GetPhotographerByIdAsync(int id)
        {
            return await _context.Photographers
                .Include(p => p.User)
                .Include(p => p.ServiceAreas)
                .Include(p => p.PhotographerServices).ThenInclude(ps => ps.ServiceType)
                .Include(p => p.PhotographerSpecialties).ThenInclude(ps => ps.SpecialtyTag)
                .Include(p => p.Bookings).ThenInclude(b => b.Reviews).ThenInclude(r => r.ReviewPhotos)
                .FirstOrDefaultAsync(p => p.PhotographerId == id);
        }

        public async Task<List<Photographer>> SearchPhotographersAsync(string keyword, string location, string studioType, string tag, DateTime? startDate = null, DateTime? endDate = null, int? serviceTypeId = null)
        {
            var query = _context.Photographers.AsQueryable();

            // AND Condition 1: Date Range
            if (startDate.HasValue && endDate.HasValue)
            {
                // We want photographers who have ANY available slot in the valid range.
                // Using Any() is more efficient than getting IDs list first if the dataset is huge, 
                // but the previous approach (Get IDs) is sometimes faster if Photographer table is joined with huge slots table.
                // Standard EF Core way for 'AND' condition:
                query = query.Where(p => p.AvailableSlots.Any(s => 
                    s.StartDateTime >= startDate.Value && 
                    s.StartDateTime <= endDate.Value && 
                    s.BookingId == null));
            }
            //關鍵字邏輯
            // AND Condition 2: Keyword (Multi-term & Tags)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var terms = keyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var term in terms)
                {
                    bool isNumeric = decimal.TryParse(term, out decimal priceVal);

                    query = query.Where(p => 
                        p.DisplayName.Contains(term) || 
                        p.StudioName.Contains(term) || 
                        (p.Description != null && p.Description.Contains(term)) ||
                        p.PhotographerSpecialties.Any(ps => ps.SpecialtyTag.SpecialtyName.Contains(term)) ||
                        p.PhotographerServices.Any(s => 
                            s.ServiceName.Contains(term) || 
                            (s.Description != null && s.Description.Contains(term)) || 
                            (isNumeric && s.BasePrice == priceVal) ||
                            s.ServiceType.ServiceName.Contains(term)) ||
                        p.ServiceAreas.Any(sa => sa.City != null && sa.City.Contains(term))
                    );
                }
            }

            // AND Condition 3: Location
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(p => 
                    p.StudioAddress.Contains(location) || 
                    p.ServiceAreas.Any(sa => sa.City.Contains(location))
                );
            }

            // AND Condition 4: Studio Type (Legacy String)
            if (!string.IsNullOrWhiteSpace(studioType))
            {
                query = query.Where(p => p.StudioType == studioType);
            }

            // AND Condition 5: Service Type ID (New Strict)
            if (serviceTypeId.HasValue)
            {
                query = query.Where(p => p.PhotographerServices.Any(ps => ps.ServiceTypeId == serviceTypeId.Value));
            }

            // AND Condition 6: Tags
            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(p => 
                    p.PhotographerSpecialties.Any(ps => ps.SpecialtyTag.SpecialtyName.Contains(tag))
                );
            }

            return await query
                .Include(p => p.User)
                .Include(p => p.ServiceAreas)
                .Include(p => p.PhotographerServices).ThenInclude(ps => ps.ServiceType)
                .Include(p => p.PhotographerSpecialties).ThenInclude(ps => ps.SpecialtyTag)
                .Include(p => p.Bookings).ThenInclude(b => b.Reviews).ThenInclude(r => r.ReviewPhotos)
                // Filtered Include for performance: ONLY load available slots in the future (or range if needed for detailed view)
                // Note: EF Core supports Filtered Include. We only need future available slots to avoid loading history.
                .Include(p => p.AvailableSlots.Where(s => s.BookingId == null && s.StartDateTime >= DateTime.Today))
                .AsSplitQuery() // Optimization: Split queries to avoid data explosion with multiple Includes
                .ToListAsync();
        }

        public async Task<bool> CreatePhotographerAsync(Photographer photographer)
        {
            try
            {
                _context.Photographers.Add(photographer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating photographer");
                return false;
            }
        }

        public async Task<bool> UpdatePhotographerAsync(Photographer photographer)
        {
            try
            {
                _context.Photographers.Update(photographer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating photographer");
                return false;
            }
        }

        public async Task<bool> DeletePhotographerAsync(int id)
        {
            try
            {
                var photographer = await _context.Photographers.FindAsync(id);
                if (photographer == null) return false;

                _context.Photographers.Remove(photographer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photographer");
                return false;
            }
        }
    }
}
