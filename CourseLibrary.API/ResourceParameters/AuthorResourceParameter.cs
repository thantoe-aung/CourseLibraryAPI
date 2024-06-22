namespace CourseLibrary.API.ResourceParameters
{
    public class AuthorResourceParameter
    {
        const int maxPageSize = 20;

        public string? mainCategory { get; set; }

        public string? searchQuery { get; set; }

        private int _pageSize = 10;
        public int PageSize
        {

            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public int PageNumber { get; set; } = 1;

        public string OrderBy { get; set; } = "Name";
    }
}
