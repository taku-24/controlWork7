namespace ControlWork7.Models;

public class BookListViewModel
{
    public List<Book> Books { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    
    public string? TitleFilter { get; set; }
    public string? AuthorFilter { get; set; }
    public string StatusFilter { get; set; } = "all";
    public int? CategoryId { get; set; }

    
    public string Sort { get; set; } = "created_desc";

    
    public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();
}