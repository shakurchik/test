namespace WebApplication1.Models;

public class BooksDTO
{
    public int Id { get; set; }
    public String Title { get; set; } = null!;
    public List<PublishingHouseDTO> houses { get; set; } = null!;
}

public class AuthorDTO
{
    public int Id { get; set; }
    public String Title { get; set; }
}
public class GenreDTO
{
    public int Id { get; set; }
    public String Title { get; set; }
}
public class PublishingHouseDTO
{
    public int Id { get; set; }
    public String name { get; set; }
    public String owner_first { get; set; }
    public String owner_last { get; set; }
}