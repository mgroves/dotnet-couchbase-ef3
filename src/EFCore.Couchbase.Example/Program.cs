using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Couchbase.Example
{
    public class MyContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var clientConfiguration = new ClientConfiguration
            {
                Servers = new List<Uri> { new Uri("http://localhost:8091") }
            };
            var authenticator = new PasswordAuthenticator("Administrator", "password");

            optionsBuilder.UseCouchbase(clientConfiguration, authenticator, "eftest");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>()
                .Property(b => b.Url)
                .IsRequired();
        }
    }

    public class Blog
    {
        [Key]
        public string BlogId { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var context = new MyContext();

            context.Blogs.Add(new Blog
            {
                BlogId = Path.GetRandomFileName(),
                Url = "https://crosscuttingconcerns.com",
                Timestamp = DateTime.Now
            });

            context.SaveChanges();

            context.Dispose();
        }

    }
}
