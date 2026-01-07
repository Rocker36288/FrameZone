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
            var photographers = await _photographerRepository.SearchPhotographersAsync(searchDto.Keyword, searchDto.Location, searchDto.StudioType);
            return photographers.Select(p => MapToDto(p)).ToList();
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

        private PhotographerDto MapToDto(Photographer p)
        {
            var reviews = p.Bookings?.SelectMany(b => b.Reviews).ToList() ?? new List<Review>();
            var rating = reviews.Any() ? Math.Round(reviews.Average(r => (double)r.Rating), 1) : 0;
            var minPrice = p.PhotographerServices?.Any() == true ? p.PhotographerServices.Min(s => s.BasePrice) : 0;

            // User Requirement Mapping:
            // Name: StudioName or DisplayName
            // Loc: ServiceArea.City
            // Type: ServiceTypes.ServiceTypeName (ServiceName)
            // Tags: SpecialtyTags.SpecialtyName

            return new PhotographerDto
            {
                PhotographerId = p.PhotographerId,
                DisplayName = !string.IsNullOrEmpty(p.StudioName) ? p.StudioName : p.DisplayName, // Prioritize StudioName if available, or keep DisplayName
                StudioName = p.StudioName,
                // Map StudioType to first ServiceType name if available, else fallback to StudioType string
                StudioType = p.PhotographerServices?.FirstOrDefault()?.ServiceType?.ServiceName ?? p.StudioType ?? "攝影師", 
                // Map to first ServiceArea City if available, else fallback to Address
                StudioAddress = p.ServiceAreas?.FirstOrDefault()?.City ?? p.StudioAddress, 
                Description = p.Description,
                AvatarUrl = p.AvatarUrl,
                PortfolioUrl = p.PortfolioUrl,
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
                //Rating = (double)rating,
                //ReviewCount = reviews.Count,
                //MinPrice = minPrice,
                //TotalBookings = p.Bookings?.Count ?? 0
            };
        }
    }
}
