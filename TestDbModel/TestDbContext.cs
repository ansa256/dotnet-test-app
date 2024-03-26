namespace TestDbModel;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

public class TestDbContext : DbContext
{
    public DbSet<Character> Characters { get; set; }

    public string DbPath { get; }

    public TestDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "rickandmorty.db");
    }

    // TODO: we need to parameterize the connection string to support remote databases
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class Character
{
    public Character(int id)
    {
        Id = id;
    }

    public int Id { get; set; }

    public string Name { get; set; } = new("unknown");

    public string Image { get; set; } = new("");

    public string Species { get; set; } = new("unknown");
}
