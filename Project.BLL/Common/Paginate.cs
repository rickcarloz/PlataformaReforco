using Project.DTO.Common;

namespace Project.BLL.Common
{
    public static class Paginate
    {
        public static PaginateModel<T> List<T>(FilterBase<T> model, List<T> data) where T : new()
        {
            int CURRENTPAGE = model.Page;
            int TOTALCOUNT = data == null ? 0 : data.Count();
            int PAGESIZE = model.Size == 0 ? TOTALCOUNT : model.Size;

            int TotalPages = model.Size == 0 ? 1 : (int)Math.Ceiling(TOTALCOUNT / (double)PAGESIZE);
            var ITEMS = model.Size == 0 ? data : data?.Skip((CURRENTPAGE - 1) * PAGESIZE).Take(PAGESIZE).ToList();

            return new PaginateModel<T>()
            {
                CurrentPage = CURRENTPAGE,
                TotalCount = TOTALCOUNT,
                PageSize = ITEMS == null ? 0 : ITEMS.Count(),
                TotalPages = TotalPages,
                Data = ITEMS
            };
        }

    }
}
