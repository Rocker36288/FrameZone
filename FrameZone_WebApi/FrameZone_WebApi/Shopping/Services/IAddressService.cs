using FrameZone_WebApi.Shopping.DTOs;

namespace FrameZone_WebApi.Shopping.Services
{
    /// <summary>
    /// 收件地址服務介面
    /// </summary>
    public interface IAddressService
    {
        /// <summary>
        /// 取得使用者的所有收件地址
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>收件地址列表</returns>
        Task<List<ReceivingAddressDto>> GetUserAddressesAsync(long userId);

        /// <summary>
        /// 取得使用者的預設收件地址
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>預設收件地址，若無則回傳 null</returns>
        Task<ReceivingAddressDto?> GetDefaultAddressAsync(long userId);

        /// <summary>
        /// 建立新的收件地址
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">建立地址 DTO</param>
        /// <returns>建立結果</returns>
        Task<(bool Success, string Message, ReceivingAddressDto? Data)> CreateAddressAsync(long userId, CreateAddressDto dto);

        /// <summary>
        /// 驗證地址資料格式
        /// </summary>
        /// <param name="dto">建立地址 DTO</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string ErrorMessage) ValidateAddress(CreateAddressDto dto);

        Task<(bool Success, string Message)> UpdateAddressAsync(long userId, int addressId, CreateAddressDto dto);
        Task<(bool Success, string Message)> DeleteAddressAsync(long userId, int addressId);
        Task<(bool Success, string Message)> SetDefaultAddressAsync(long userId, int addressId);
    }
}
