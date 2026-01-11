using FrameZone_WebApi.PhotographerBooking.DTOs;
using FrameZone_WebApi.PhotographerBooking.Repositories;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.Services
{
    public class PhotographerService : IPhotographerService
    {
        private readonly IPhotographerRepository _photographerRepository;
        private readonly IAvailableSlotRepository _availableSlotRepository;

        public PhotographerService(IPhotographerRepository photographerRepository, IAvailableSlotRepository availableSlotRepository)
        {
            _photographerRepository = photographerRepository;
            _availableSlotRepository = availableSlotRepository;
        }

        public async Task<List<PhotographerDto>> GetAllPhotographersAsync()
        {
            var photographers = await _photographerRepository.GetAllPhotographersAsync();
            return photographers.Select(p => MapToDto(p)).ToList();
        }

        public async Task<PhotographerDto> GetPhotographerByIdAsync(int id)
        {
            var photographer = await _photographerRepository.GetPhotographerByIdAsync(id);
            if (photographer == null) return null;
            return MapToDto(photographer);
        }

        public async Task<List<PhotographerDto>> SearchPhotographersAsync(PhotographerSearchDto searchDto)
        {
            var photographers = await _photographerRepository.SearchPhotographersAsync(
                searchDto.Keyword, 
                searchDto.Location, 
                searchDto.StudioType, 
                searchDto.Tag,
                searchDto.StartDate,
                searchDto.EndDate,
                searchDto.ServiceTypeId
            );

            return photographers.Select(p => MapToDto(p, searchDto.StartDate, searchDto.EndDate)).ToList();
        }

        public async Task<List<AvailableSlotDto>> GetPhotographerAvailableSlotsAsync(int photographerId, DateTime start, DateTime end)
        {
            var slots = await _availableSlotRepository.GetAvailableSlotsAsync(photographerId, start, end);
            var slotDtos = new List<AvailableSlotDto>();

            foreach (var slot in slots)
            {
                var isAvailable = await _availableSlotRepository.IsSlotAvailableAsync(slot.AvailableSlotId);
                if (isAvailable)
                {
                    slotDtos.Add(new AvailableSlotDto
                    {
                        AvailableSlotId = slot.AvailableSlotId,
                        StartDateTime = slot.StartDateTime,
                        EndDateTime = slot.EndDateTime,
                        IsAvailable = true
                    });
                }
            }
            return slotDtos;
        }

        private PhotographerDto MapToDto(Photographer p, DateTime? filterStart = null, DateTime? filterEnd = null)
        {
            var reviews = p.Bookings?.SelectMany(b => b.Reviews).ToList() ?? new List<Review>();
            var rating = reviews.Any() ? Math.Round(reviews.Average(r => (double)r.Rating), 1) : 0;
            var minPrice = p.PhotographerServices?.Any() == true ? p.PhotographerServices.Min(s => s.BasePrice) : 0;

            // Calculate SlotCount based on filter range or default (e.g., next 30 days or all future)
            // If range is provided, count slots in range.
            // Requirement: "If date range selected, show count in range."
            int slotCount = 0;
            if (p.AvailableSlots != null)
            {
                var slotsQuery = p.AvailableSlots.Where(s => s.BookingId == null);
                
                if (filterStart.HasValue && filterEnd.HasValue)
                {
                    slotsQuery = slotsQuery.Where(s => s.StartDateTime >= filterStart.Value && s.StartDateTime <= filterEnd.Value);
                }
                else
                {
                    // If no range, maybe count all future available slots? 
                    // Or just 0 if UI handles "Earliest available" differently.
                    // Let's count all future ones for now as a "total available" indicator.
                    slotsQuery = slotsQuery.Where(s => s.StartDateTime >= DateTime.Now);
                }
                slotCount = slotsQuery.Count();
            }

            return new PhotographerDto
            {
                PhotographerId = p.PhotographerId,
                DisplayName = p.DisplayName, // Direct mapping to avoid duplication with StudioName
                StudioName = p.StudioName,
                // Map StudioType to first ServiceType name if available, else fallback to StudioType string
                StudioType = p.PhotographerServices?.FirstOrDefault()?.ServiceType?.ServiceName ?? p.StudioType ?? "攝影師", 
                // Map to first ServiceArea City if available, else fallback to Address
                StudioAddress = p.ServiceAreas?.FirstOrDefault()?.City ?? p.StudioAddress, 
                Description = p.Description,
                AvatarUrl = p.AvatarUrl,
                PortfolioUrl = p.PortfolioUrl,
                PortfolioFile = p.PortfolioFile,
                ServiceCities = p.ServiceAreas?.Select(sa => sa.City).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList() ?? new List<string>(),
                YearsOfExperience = p.YearsOfExperience,
                Specialties = p.PhotographerSpecialties?.Select(s => s.SpecialtyTag?.SpecialtyName ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>(),
                Services = p.PhotographerServices?.Select(s => new ServiceDto
                {
                    PhotographerServiceId = s.PhotographerServiceId,
                    ServiceTypeId = s.ServiceTypeId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    BasePrice = s.BasePrice,
                    Duration = s.Duration,
                    MaxRevisions = s.MaxRevisions,
                    DeliveryDays = s.DeliveryDays,
                    IncludedPhotos = s.IncludedPhotos,
                    AdditionalServices = s.AdditionalServices
                }).ToList() ?? new List<ServiceDto>(),
                
                // Calculated fields
                Rating = (double)rating,
                ReviewCount = reviews.Count,
                MinPrice = minPrice,
                TotalBookings = p.Bookings?.Count ?? 0,
                SlotCount = slotCount,
                EarliestAvailableDate = p.AvailableSlots?
                    .Where(s => s.BookingId == null && s.StartDateTime >= DateTime.Today && s.StartDateTime <= DateTime.Today.AddDays(7))
                    .OrderBy(s => s.StartDateTime)
                    .Select(s => (DateTime?)s.StartDateTime)
                    .FirstOrDefault()
            };
        }
    }
}
