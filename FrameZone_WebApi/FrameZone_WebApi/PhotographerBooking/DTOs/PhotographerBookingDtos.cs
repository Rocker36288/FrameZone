using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.DTOs
{
    public class PhotographerDto
    {
        public int PhotographerId { get; set; }
        public string DisplayName { get; set; }
        public string StudioName { get; set; }
        public string StudioType { get; set; }
        public string StudioAddress { get; set; }
        public string Description { get; set; }
        public string AvatarUrl { get; set; }
        public string PortfolioUrl { get; set; }
        public int? YearsOfExperience { get; set; }
        public List<string> Specialties { get; set; } = new List<string>();
        public List<ServiceDto> Services { get; set; } = new List<ServiceDto>();
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public decimal MinPrice { get; set; }
        public int TotalBookings { get; set; }
        public List<string> ServiceCities { get; set; } = new List<string>();
        public string PortfolioFile { get; set; }
        public int SlotCount { get; set; }
    }

    public class ServiceDto
    {
        public int PhotographerServiceId { get; set; }
        public int ServiceTypeId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public int Duration { get; set; }
        public int? MaxRevisions { get; set; }
        public int? DeliveryDays { get; set; }
        public int? IncludedPhotos { get; set; }
        public string AdditionalServices { get; set; }
    }

    public class PhotographerSearchDto
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? StudioType { get; set; }
        public int? ServiceTypeId { get; set; }
        public string? Tag { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class BookingDto
    {
        public int BookingId { get; set; }
        public int PhotographerId { get; set; }
        public string PhotographerName { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public DateTime BookingStartDatetime { get; set; }
        public DateTime BookingEndDatetime { get; set; }
        public string BookingStatus { get; set; }
        public decimal ServicePrice { get; set; }
        public string Location { get; set; }
    }

    public class CreateBookingDto
    {
        public int PhotographerId { get; set; }
        public int AvailableSlotId { get; set; }
        public long UserId { get; set; }
        public string Location { get; set; }
        public int PaymentMethodId { get; set; }
    }

    public class AvailableSlotDto
    {
        public int AvailableSlotId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class ServiceTypeDto
    {
        public int ServiceTypeId { get; set; }
        public string ServiceName { get; set; }
        public string IconUrl { get; set; }
    }
}
