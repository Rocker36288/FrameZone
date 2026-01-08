using FrameZone_WebApi.Models;
using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Repositories;

namespace FrameZone_WebApi.Shopping.Services
{
    /// <summary>
    /// 收件地址服務實作
    /// </summary>
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<AddressService> _logger;

        public AddressService(
            IAddressRepository addressRepository,
            ILogger<AddressService> logger)
        {
            _addressRepository = addressRepository;
            _logger = logger;
        }

        /// <summary>
        /// 取得使用者的所有收件地址
        /// </summary>
        public async Task<List<ReceivingAddressDto>> GetUserAddressesAsync(long userId)
        {
            try
            {
                var addresses = await _addressRepository.GetByUserIdAsync(userId);

                return addresses.Select(a => new ReceivingAddressDto
                {
                    AddressId = a.AddressId,
                    RecipientName = a.RecipientName,
                    PhoneNumber = a.PhoneNumber,
                    FullAddress = a.FullAddress,
                    IsDefault = a.IsDefault,
                    CreatedAt = a.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者收件地址時發生錯誤，UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 取得使用者的預設收件地址
        /// </summary>
        public async Task<ReceivingAddressDto?> GetDefaultAddressAsync(long userId)
        {
            try
            {
                var addresses = await _addressRepository.GetByUserIdAsync(userId);
                var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault);

                if (defaultAddress == null)
                    return null;

                return new ReceivingAddressDto
                {
                    AddressId = defaultAddress.AddressId,
                    RecipientName = defaultAddress.RecipientName,
                    PhoneNumber = defaultAddress.PhoneNumber,
                    FullAddress = defaultAddress.FullAddress,
                    IsDefault = defaultAddress.IsDefault,
                    CreatedAt = defaultAddress.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者預設收件地址時發生錯誤，UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 建立新的收件地址
        /// </summary>
        public async Task<(bool Success, string Message, ReceivingAddressDto? Data)> CreateAddressAsync(long userId, CreateAddressDto dto)
        {
            try
            {
                // 驗證地址資料
                var validation = ValidateAddress(dto);
                if (!validation.IsValid)
                {
                    return (false, validation.ErrorMessage, null);
                }

                // 如果設為預設地址，先將其他地址的預設狀態取消
                if (dto.IsDefault)
                {
                    var existingAddresses = await _addressRepository.GetByUserIdAsync(userId);
                    if (existingAddresses.Any(a => a.IsDefault))
                    {
                        // 有其他預設地址，需要更新
                        // 這裡先建立新地址，之後再更新預設狀態
                    }
                }

                // 建立新地址實體
                var newAddress = new ReceivingAddress
                {
                    UserId = userId,
                    RecipientName = dto.RecipientName.Trim(),
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    FullAddress = dto.FullAddress.Trim(),
                    IsDefault = dto.IsDefault,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // 儲存到資料庫
                var createdAddress = await _addressRepository.CreateAsync(newAddress);

                // 如果設為預設，更新其他地址的預設狀態
                if (dto.IsDefault)
                {
                    await _addressRepository.UpdateDefaultStatusAsync(userId, createdAddress.AddressId);
                }

                _logger.LogInformation("使用者 {UserId} 成功建立新收件地址 {AddressId}", userId, createdAddress.AddressId);

                // 回傳建立的地址 DTO
                var resultDto = new ReceivingAddressDto
                {
                    AddressId = createdAddress.AddressId,
                    RecipientName = createdAddress.RecipientName,
                    PhoneNumber = createdAddress.PhoneNumber,
                    FullAddress = createdAddress.FullAddress,
                    IsDefault = createdAddress.IsDefault,
                    CreatedAt = createdAddress.CreatedAt
                };

                return (true, "收件地址建立成功", resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立收件地址時發生錯誤，UserId: {UserId}", userId);
                return (false, "建立收件地址時發生錯誤，請稍後再試", null);
            }
        }

        /// <summary>
        /// 驗證地址資料格式
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateAddress(CreateAddressDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RecipientName))
            {
                return (false, "收件人姓名不可為空");
            }

            if (dto.RecipientName.Length > 100)
            {
                return (false, "收件人姓名不可超過 100 個字元");
            }

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                return (false, "電話號碼不可為空");
            }

            if (dto.PhoneNumber.Length > 20)
            {
                return (false, "電話號碼不可超過 20 個字元");
            }

            if (string.IsNullOrWhiteSpace(dto.FullAddress))
            {
                return (false, "完整地址不可為空");
            }

            if (dto.FullAddress.Length > 500)
            {
                return (false, "地址不可超過 500 個字元");
            }

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Message)> UpdateAddressAsync(long userId, int addressId, CreateAddressDto dto)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.UserId != userId)
                return (false, "找不到地址或無權限修改");

            var validation = ValidateAddress(dto);
            if (!validation.IsValid) return (false, validation.ErrorMessage);

            address.RecipientName = dto.RecipientName.Trim();
            address.PhoneNumber = dto.PhoneNumber.Trim();
            address.FullAddress = dto.FullAddress.Trim();
            
            // 如果從非預設改為預設
            bool needUpdateDefaults = !address.IsDefault && dto.IsDefault;
            address.IsDefault = dto.IsDefault;

            await _addressRepository.UpdateAsync(address);

            if (needUpdateDefaults)
            {
                await _addressRepository.UpdateDefaultStatusAsync(userId, addressId);
            }

            return (true, "地址更新成功");
        }

        public async Task<(bool Success, string Message)> DeleteAddressAsync(long userId, int addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.UserId != userId)
                return (false, "找不到地址或無權限刪除");

            await _addressRepository.DeleteAsync(address);
            return (true, "地址已刪除");
        }

        public async Task<(bool Success, string Message)> SetDefaultAddressAsync(long userId, int addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.UserId != userId)
                return (false, "找不到地址或無權限操作");

            await _addressRepository.UpdateDefaultStatusAsync(userId, addressId);
            return (true, "預設地址已變更");
        }
    }
}
