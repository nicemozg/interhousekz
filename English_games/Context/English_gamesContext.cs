using English_games.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace English_games.Context
{
    public class English_gamesContext : IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<MyTheme> MyThemes { get; set; }
        public DbSet<LinkForGame> LinkForGames { get; set; }
        public DbSet<LinkForBook> LinkForBooks { get; set; }
        public DbSet<Mobizon> Mobizons { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<MainContent> MainContents { get; set; }
        public DbSet<MainContetntHeader> MainContetntHeaders { get; set; }

        public English_gamesContext(DbContextOptions<English_gamesContext> options) : base(options)
        {
        }
    }
}