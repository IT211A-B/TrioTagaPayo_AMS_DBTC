namespace Attendance_Management_System.Helpers
{
    public static class PaginationHelper
    {
        public static object Paginate<T>(IEnumerable<T> source, int page, int pageSize)
        {
            pageSize = Math.Min(pageSize, 100);
            var list = source.ToList();
            var totalCount = list.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var data = list.Skip((page - 1) * pageSize).Take(pageSize);
            return new { data, page, pageSize, totalCount, totalPages };
        }
    }
}