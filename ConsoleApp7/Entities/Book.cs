namespace ConsoleApp7.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int PublishYear { get; set; }
    public string? ISBN { get; set; }
    public int GenreId { get; set; }
    public int QuantityInStock { get; set; }

    public Author Author { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}
