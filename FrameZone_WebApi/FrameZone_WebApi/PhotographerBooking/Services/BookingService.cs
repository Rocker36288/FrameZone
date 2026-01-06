using FrameZone_WebApi.PhotographerBooking.DTOs;
using FrameZone_WebApi.PhotographerBooking.Repositories;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IAvailableSlotRepository _availableSlotRepository;
        private readonly IPhotographerRepository _photographerRepository;

        public BookingService(IBookingRepository bookingRepository, IAvailableSlotRepository availableSlotRepository, IPhotographerRepository photographerRepository)
        {
            _bookingRepository = bookingRepository;
            _availableSlotRepository = availableSlotRepository;
            _photographerRepository = photographerRepository;
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto)
        {
            // Validate slot availability
            bool isAvailable = await _availableSlotRepository.IsSlotAvailableAsync(createDto.AvailableSlotId);
            if (!isAvailable)
            {
                throw new Exception("Slot is not available");
            }

            var slot = await _availableSlotRepository.GetSlotByIdAsync(createDto.AvailableSlotId);
            if (slot == null) throw new Exception("Slot not found");

            // Create Booking entity
            var booking = new Booking
            {
                PhotographerId = createDto.PhotographerId,
                AvailableSlotId = createDto.AvailableSlotId,
                UserId = createDto.UserId,
                Location = createDto.Location,
                PaymentMethodId = createDto.PaymentMethodId,
                BookingStartDatetime = slot.StartDateTime,
                BookingEndDatetime = slot.EndDateTime,
                BookingStatus = "已確認", // Assuming immediate confirmation or pending logic
                PaymentStatus = "Unpaid", // Modify as needed
                BookingNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(), // Simple number generation
                ServicePrice = 0 // Needs to be calculated based on service selected, but DTO doesn't have service selection yet. Assuming 0 for now or fetch from photographer service?
                                 // Ideally CreateBookingDto should include ServiceId
            };

            // Assuming we need to set price. For now let's keep it simple.
            // In a real scenario, we would select a service.

            var result = await _bookingRepository.CreateBookingAsync(booking);
            if (!result) return null;

            return await GetBookingByIdAsync(booking.BookingId);
        }

        public async Task<BookingDto> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(id);
            if (booking == null) return null;
            return MapToDto(booking);
        }

        public async Task<List<BookingDto>> GetUserBookingsAsync(long userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            return bookings.Select(b => MapToDto(b)).ToList();
        }

        public async Task<List<BookingDto>> GetPhotographerBookingsAsync(int photographerId)
        {
            var bookings = await _bookingRepository.GetBookingsByPhotographerIdAsync(photographerId);
            return bookings.Select(b => MapToDto(b)).ToList();
        }

        private BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                BookingId = b.BookingId,
                PhotographerId = b.PhotographerId,
                PhotographerName = b.Photographer?.DisplayName,
                UserId = b.UserId,
                UserName = b.User?.UserProfile?.DisplayName ?? "Unknown",
                BookingStartDatetime = b.BookingStartDatetime,
                BookingEndDatetime = b.BookingEndDatetime,
                BookingStatus = b.BookingStatus,
                ServicePrice = b.ServicePrice,
                Location = b.Location
            };
        }
    }
}
