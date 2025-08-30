using System.ComponentModel.DataAnnotations;

namespace ControlWork7.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, StringLength(100)]
    public string LastName { get; set; } = null!;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = null!;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = null!;
    
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
}