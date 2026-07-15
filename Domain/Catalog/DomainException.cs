namespace CatalogAPI.Domain.Catalog;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
