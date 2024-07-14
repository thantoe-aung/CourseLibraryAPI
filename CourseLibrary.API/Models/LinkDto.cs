namespace CourseLibrary.API.Models
{
    public class LinkDto
    {
        public string? Href{ get; set; }

        public string? Method { get; set; }

        public string? Rel { get; set; }
        public LinkDto(string? href,string? rel,string? method)
        {
            Href = href;
            Method = method;
            Rel = rel;
        }
    }
}
