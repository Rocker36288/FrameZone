using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Shopping.Repositories
{
    /// <summary>
    /// 收件地址 Repository 介面
    /// </summary>
    public interface IAddressRepository
    {
        /// <summary>
        /// 根據使用者 ID 取得所有收件地址
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>收件地址列表</returns>
        Task<List<ReceivingAddress>> GetByUserIdAsync(long userId);

        /// <summary>
        /// 新增收件地址
        /// </summary>
        /// <param name="address">收件地址實體</param>
        /// <returns>新增後的收件地址</returns>
        Task<ReceivingAddress> CreateAsync(ReceivingAddress address);

        /// <summary>
        /// 更新預設地址狀態
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="newDefaultAddressId">新的預設地址 ID</param>
        Task UpdateDefaultStatusAsync(long userId, int newDefaultAddressId);
    }
}
