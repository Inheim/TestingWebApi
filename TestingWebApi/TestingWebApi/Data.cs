using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestingWebApi
{
    public class CSVData
    {
        public DateTime date { get; set; }

        public int executiontime { get; set; }

        public double value { get; set; }
    }

    [Table("values")]
    public class Data
    {
        [Column("id")]
        public int id { get; set; }

        public DateTime date { get; set; }

        public int executiontime { get; set; }

        public double value { get; set; }
    }

    [Table("results")]
    public class Result
    {
        [Key]
        [Required]
        public required string name { get; set; }

        public int delta { get; set; }

        public DateTime date { get; set; }

        public double averageexecutiontime { get; set; }

        public double averagevalue { get; set; }

        public double medianvalue { get; set; }

        public double maxvalue { get; set; }

        public double minvalue { get; set; }
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Data> Data { get; set; } = null!;

        public DbSet<Result> Result { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=qwerty");
        }
    }
}
