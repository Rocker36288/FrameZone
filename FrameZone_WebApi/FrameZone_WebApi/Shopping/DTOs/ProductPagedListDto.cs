using System.Collections.Generic;

namespace FrameZone_WebApi.Shopping.DTOs
{
    public class ProductPagedListDto
    {
        public List<ProductListDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
