using FrameZone_WebApi.Models;
using FrameZone_WebApi.Shopping.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Shopping.Services
{
    public interface IShoppingStoreService
    {
        Task<List<PickupStoreDto>> GetUserStoresAsync(long userId);
        Task<PickupStoreDto?> GetDefaultStoreAsync(long userId);
        Task<(bool Success, string Message, PickupStoreDto? Data)> CreateStoreAsync(long userId, CreatePickupStoreDto dto);
        Task<(bool Success, string Message)> UpdateStoreAsync(long userId, int storeId, CreatePickupStoreDto dto);
        Task<(bool Success, string Message)> DeleteStoreAsync(long userId, int storeId);
        Task<(bool Success, string Message)> SetDefaultStoreAsync(long userId, int storeId);
    }

    public class ShoppingStoreService : IShoppingStoreService
    {
        private readonly AAContext _context;
        private readonly ILogger<ShoppingStoreService> _logger;

        public ShoppingStoreService(AAContext context, ILogger<ShoppingStoreService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PickupStoreDto>> GetUserStoresAsync(long userId)
        {
            try
            {
                return await _context.PickupConvenienceStores
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.IsDefault)
                    .ThenByDescending(s => s.CreatedAt)
                    .Select(s => new PickupStoreDto
                    {
                        ConvenienceStoreId = s.ConvenienceStoreId,
                        RecipientName = s.RecipientName,
                        PhoneNumber = s.PhoneNumber,
                        ConvenienceStoreCode = s.ConvenienceStoreCode,
                        ConvenienceStoreName = s.ConvenienceStoreName,
                        IsDefault = s.IsDefault,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得門市列表失敗, UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<PickupStoreDto?> GetDefaultStoreAsync(long userId)
        {
            try
            {
                var store = await _context.PickupConvenienceStores
                    .Where(s => s.UserId == userId && s.IsDefault)
                    .Select(s => new PickupStoreDto
                    {
                        ConvenienceStoreId = s.ConvenienceStoreId,
                        RecipientName = s.RecipientName,
                        PhoneNumber = s.PhoneNumber,
                        ConvenienceStoreCode = s.ConvenienceStoreCode,
                        ConvenienceStoreName = s.ConvenienceStoreName,
                        IsDefault = s.IsDefault,
                        CreatedAt = s.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                return store;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得預設門市失敗, UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<(bool Success, string Message, PickupStoreDto? Data)> CreateStoreAsync(long userId, CreatePickupStoreDto dto)
        {
            try
            {
                // 如果設為預設地址，先將其他地址的預設狀態取消
                if (dto.IsDefault)
                {
                    var existingDefault = await _context.PickupConvenienceStores
                        .Where(s => s.UserId == userId && s.IsDefault)
                        .ToListAsync();

                    foreach (var s in existingDefault)
                    {
                        s.IsDefault = false;
                        s.UpdatedAt = DateTime.Now;
                    }
                }

                var newStore = new PickupConvenienceStore
                {
                    UserId = userId,
                    RecipientName = dto.RecipientName,
                    PhoneNumber = dto.PhoneNumber,
                    ConvenienceStoreCode = dto.ConvenienceStoreCode,
                    ConvenienceStoreName = dto.ConvenienceStoreName,
                    IsDefault = dto.IsDefault,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.PickupConvenienceStores.Add(newStore);
                await _context.SaveChangesAsync();

                var resultDto = new PickupStoreDto
                {
                    ConvenienceStoreId = newStore.ConvenienceStoreId,
                    RecipientName = newStore.RecipientName,
                    PhoneNumber = newStore.PhoneNumber,
                    ConvenienceStoreCode = newStore.ConvenienceStoreCode,
                    ConvenienceStoreName = newStore.ConvenienceStoreName,
                    IsDefault = newStore.IsDefault,
                    CreatedAt = newStore.CreatedAt
                };

                return (true, "門市建立成功", resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立門市失敗, UserId: {UserId}", userId);
                return (false, "建立門市失敗，請稍後再試", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateStoreAsync(long userId, int storeId, CreatePickupStoreDto dto)
        {
            var store = await _context.PickupConvenienceStores.FindAsync(storeId);
            if (store == null || store.UserId != userId) return (false, "找不到門市或無權限修改");

            store.RecipientName = dto.RecipientName;
            store.PhoneNumber = dto.PhoneNumber;
            store.ConvenienceStoreCode = dto.ConvenienceStoreCode;
            store.ConvenienceStoreName = dto.ConvenienceStoreName;

            bool needUpdateDefaults = !store.IsDefault && dto.IsDefault;
            store.IsDefault = dto.IsDefault;
            store.UpdatedAt = DateTime.Now;

            if (needUpdateDefaults)
            {
                var existing = await _context.PickupConvenienceStores
                    .Where(s => s.UserId == userId && s.IsDefault && s.ConvenienceStoreId != storeId)
                    .ToListAsync();
                foreach (var s in existing) s.IsDefault = false;
            }

            await _context.SaveChangesAsync();
            return (true, "門市更新成功");
        }

        public async Task<(bool Success, string Message)> DeleteStoreAsync(long userId, int storeId)
        {
            var store = await _context.PickupConvenienceStores.FindAsync(storeId);
            if (store == null || store.UserId != userId) return (false, "找不到門市或無權限刪除");

            _context.PickupConvenienceStores.Remove(store);
            await _context.SaveChangesAsync();
            return (true, "門市已刪除");
        }

        public async Task<(bool Success, string Message)> SetDefaultStoreAsync(long userId, int storeId)
        {
            var store = await _context.PickupConvenienceStores.FindAsync(storeId);
            if (store == null || store.UserId != userId) return (false, "找不到門市或無權限操作");

            var all = await _context.PickupConvenienceStores.Where(s => s.UserId == userId).ToListAsync();
            foreach (var s in all) s.IsDefault = (s.ConvenienceStoreId == storeId);

            await _context.SaveChangesAsync();
            return (true, "預設門市已變更");
        }
    }
}
