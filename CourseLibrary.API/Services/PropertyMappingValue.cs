namespace CourseLibrary.API.Services
{
    public class PropertyMappingValue
    {
        public bool Revert { get;private set; }

        public IEnumerable<string> DestinationProperties { get; private set; }

        public PropertyMappingValue(IEnumerable<string> destinationPropertie,bool revert = false)
        {
            DestinationProperties = destinationPropertie ?? throw new ArgumentNullException(nameof(destinationPropertie));
            Revert = revert;
        }
    }
}
