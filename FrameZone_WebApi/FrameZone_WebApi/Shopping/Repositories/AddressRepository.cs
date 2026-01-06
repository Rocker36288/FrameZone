using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Shopping.Repositories
{
    /// <summary>
    /// 收件地址 Repository 實作
    /// </summary>
    public class AddressRepository : IAddressRepository
    {
        private readonly AAContext _context;

        public AddressRepository(AAContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根據使用者 ID 取得所有收件地址，依預設狀態和建立時間排序
        /// </summary>
        public async Task<List<ReceivingAddress>> GetByUserIdAsync(long userId)
        {
            return await _context.ReceivingAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)  // 預設地址優先
                .ThenByDescending(a => a.CreatedAt)   // 再依建立時間排序
                .ToListAsync();
        }

        /// <summary>
        /// 新增收件地址
        /// </summary>
        public async Task<ReceivingAddress> CreateAsync(ReceivingAddress address)
        {
            _context.ReceivingAddresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        /// <summary>
        /// 更新預設地址狀態
        /// 將使用者的所有地址設為非預設，然後將指定地址設為預設
        /// </summary>
        public async Task UpdateDefaultStatusAsync(long userId, int newDefaultAddressId)
        {
            // 將該使用者的所有地址設為非預設
            var userAddresses = await _context.ReceivingAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            foreach (var addr in userAddresses)
            {
                addr.IsDefault = false;
            }

            // 將指定地址設為預設
            var newDefaultAddress = userAddresses.FirstOrDefault(a => a.AddressId == newDefaultAddressId);
            if (newDefaultAddress != null)
            {
                newDefaultAddress.IsDefault = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
