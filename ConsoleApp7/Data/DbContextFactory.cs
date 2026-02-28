using Microsoft.EntityFrameworkCore;

namespace ConsoleApp7.Data;

public static class DbContextFactory
{
    private const string ConnectionString = "Data Source=library.db";

    public static LibraryDbContext Create()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        var context = new LibraryDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
