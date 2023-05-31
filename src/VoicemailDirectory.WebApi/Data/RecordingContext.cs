using Microsoft.EntityFrameworkCore;

namespace VoicemailDirectory.WebApi.Data;

public class RecordingContext : DbContext
{
    public DbSet<Recording>? Recordings { get; set; }

    public RecordingContext(DbContextOptions<RecordingContext> options) : base(options)
    {
    }
}