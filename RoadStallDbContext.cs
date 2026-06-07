using Microsoft.EntityFrameworkCore;
using RoadStallAPI.Models;

namespace RoadStallAPI;


public partial class RoadStallDbContext : DbContext
{
    public RoadStallDbContext(DbContextOptions<RoadStallDbContext> options)
       : base(options) { }

    public DbSet<Stock> Stock { get; set; }

    public DbSet<RoadStallAPI.Models.StockChange> StockChange { get; set; } = default!;

    public DbSet<RoadStallAPI.Models.StockTake> StockTake { get; set; } = default!;

    public DbSet<RoadStallAPI.Models.User> User { get; set; } = default!;

    public DbSet<RoadStallAPI.Models.Sale> Sale { get; set; } = default!;

    public DbSet<RoadStallAPI.Models.Role> Role { get; set; } = default!;

    public DbSet<RoadStallAPI.Models.StockTakeHistory> StockTakeHistory { get; set; } = default!;

    public DbSet<RoadStallAPI.Models.PushSubscriptions> PushSubscriptions { get; set; }

}

