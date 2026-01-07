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

        public async Task<List<Photographer>> SearchPhotographersAsync(string keyword, string location, string studioType)
        {
            var query = _context.Photographers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.DisplayName.Contains(keyword) || p.StudioName.Contains(keyword) || p.Description.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(p => p.StudioAddress.Contains(location));
            }

            if (!string.IsNullOrWhiteSpace(studioType))
            {
                query = query.Where(p => p.StudioType == studioType);
            }

            return await query
                .Include(p => p.User)
                .Include(p => p.ServiceAreas)
                .Include(p => p.PhotographerServices).ThenInclude(ps => ps.ServiceType)
                .Include(p => p.PhotographerSpecialties).ThenInclude(ps => ps.SpecialtyTag)
                .Include(p => p.Bookings).ThenInclude(b => b.Reviews).ThenInclude(r => r.ReviewPhotos)
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
