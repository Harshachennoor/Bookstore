using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;


namespace Bookstore.Models
{
    public class BookstoreContext : IdentityDbContext<User>
    {
        public BookstoreContext(DbContextOptions<BookstoreContext> options)
            : base(options)
        { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<Genre> Genres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BookAuthor: set primary key 
            modelBuilder.Entity<BookAuthor>().HasKey(ba => new { ba.BookId, ba.AuthorId });

            // BookAuthor: set foreign keys 
            modelBuilder.Entity<BookAuthor>().HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);
            modelBuilder.Entity<BookAuthor>().HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            // Book: remove cascading delete with Genre
            modelBuilder.Entity<Book>().HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .OnDelete(DeleteBehavior.Restrict);

            // seed initial data
            modelBuilder.ApplyConfiguration(new SeedGenres());
            modelBuilder.ApplyConfiguration(new SeedBooks());
            modelBuilder.ApplyConfiguration(new SeedAuthors());
            modelBuilder.ApplyConfiguration(new SeedBookAuthors());
        }

        public static async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            UserManager<User> userManager =
                serviceProvider.GetRequiredService<UserManager<User>>();
            RoleManager<IdentityRole> roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            
            var userCreationList = new List<UserCreation>{
                // new UserCreation() {
                //     username = "manager",
                //     password = "Sesame",
                //     roleName = "Manager"
                // },
                new UserCreation() {
                    username = "manager",
                    password = "Sesame",
                    roleName = "Manager"
                },
                new UserCreation() {
                    username = "seller",
                    password = "Sesame",
                    roleName = "seller"
                },
            };

            foreach(var userCreation in userCreationList) {

                if (await roleManager.FindByNameAsync(userCreation.roleName) == null)
                    await roleManager.CreateAsync(new IdentityRole(userCreation.roleName));

                // if username doesn't exist, create it and add to role
                if (await userManager.FindByNameAsync(userCreation.username) == null)
                {
                    User user = new User { UserName = userCreation.username };
                    var result = await userManager.CreateAsync(user, userCreation.password);
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(user, userCreation.roleName);
                }
            }
        }
        
    }

    public class UserCreation {
            
        public string username = "";
        public string password = "";
        public string roleName = "";
    }
}