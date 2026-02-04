using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class HotelDbContext : DbContext, Icontext
    {

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<CategoryWord> CategoriesWords { get; set; }


        public async Task Save()
        {
            await SaveChangesAsync();
        }

        //כל זה קופילוט הציע מהה המורה אומרת???????????????????????????????


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=DESKTOP-1VUANBN;database=HotelApDB;trusted_connection=true;TrustServerCertificate=True");
        }
        //פעולה זו על בשביל SQL כי יש לי 2 מפתחו זרים 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Employee)
                .WithMany(e => e.Requests)
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoryWord>()
                .HasOne(cw => cw.Category)
                .WithMany(c => c.CategoryWords)
                .HasForeignKey(cw => cw.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoryWord>()
                .HasOne(cw => cw.Word)
                .WithMany(w => w.CategoryWords)
                .HasForeignKey(cw => cw.WordId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}