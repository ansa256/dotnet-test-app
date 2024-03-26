using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TestDbModel;

namespace TestDbViewerApp.Pages;

public class CharactersModel : PageModel
{
    private readonly TestDbContext _context;
    private readonly ILogger<CharactersModel> _logger;

    public IList<Character> Characters { get; set; } = Array.Empty<Character>();

    public CharactersModel(TestDbContext context, ILogger<CharactersModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        _logger.LogInformation("CharactersModel OnGetAsync.");
        Characters = await _context.Characters.ToListAsync();
    }
}
