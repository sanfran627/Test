using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Test
{
  public partial class MyDbContext : DbContext
  {
    public virtual DbSet<UserModel> User { get; set; }
    public virtual DbSet<ContactModel> Contact { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    //manually mapping entities.. Can't stand generated code
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      var userBuilder = modelBuilder.Entity<UserModel>(entity =>
      {
        entity.Property(e => e.UserId).HasColumnName("userId").HasColumnType("uniqueidentifier").IsRequired(); ;
        entity.Property(e => e.Username).HasColumnName("username").HasColumnType("nvarchar(255)").IsRequired(); ;
        entity.Property(e => e.Password).HasColumnName("password").HasColumnType("nvarchar(255)").IsRequired();
        entity.Property(e => e.FirstName).HasColumnName("firstName").HasColumnType("nvarchar(50)").IsRequired();
        entity.Property(e => e.LastName).HasColumnName("lastName").HasColumnType("nvarchar(50)").IsRequired();
        entity.Property(e => e.Title).HasColumnName("title").HasColumnType("nvarchar(50)").IsRequired().HasDefaultValue<string>(string.Empty);
        entity.HasKey(c => c.UserId);
        // unique key required for username
        entity.HasIndex(c => c.Username).IsUnique();
      });

      var contactBuilder = modelBuilder.Entity<ContactModel>(entity =>
      {
        entity.Ignore(e => e.Addresses);
        entity.Property(e => e.UserId).HasColumnName("userId").HasColumnType("uniqueidentifier").IsRequired(); ;
        entity.Property(e => e.UserId).HasColumnName("userId").HasColumnType("uniqueidentifier").IsRequired(); ;
        entity.Property(e => e.FirstName).HasColumnName("firstName").HasColumnType("nvarchar(50)").IsRequired();
        entity.Property(e => e.LastName).HasColumnName("lastName").HasColumnType("nvarchar(50)").IsRequired();
        entity.Property(e => e.DOB).HasColumnName("dob").HasColumnType("date").IsRequired();
        entity.Property(e => e.AddressesString).HasColumnName("addresses").HasColumnType("nvarchar(max)").IsRequired().HasDefaultValue<string>(string.Empty);
        entity.HasKey(c => new { c.ContactId, c.UserId });
      });
    }

    public async Task Initialize( IPasswordService pwd )
    {
      // create 20,000 users, the first user being the one that matches the signin credentials
      if ((await this.User.CountAsync()) == 0)
      {
        this.User.Add(new UserModel()
        {
          Username = "me@user.co",
          FirstName = "Test",
          LastName = "User",
          Password = await pwd.CreatePasswordHash( "ThisIsAV3ryC0mpl3XPassword!" ),
          Title = string.Empty,
          Type = UserType.Standard,
          UserId = Guid.NewGuid()
        });

        await this.SaveChangesAsync();
      }
    }

    //Example of SP support in EF.. No SP exists right now, but we get the point..
    public async Task<(bool found, UserModel user)> TryGetUserByIdUsingSP(Guid userId)
    {
      var param = new SqlParameter("@userId", SqlDbType.UniqueIdentifier) { Value = userId };
      var users = this.User.FromSql($"GetUser @userId", param);
      var user = await users.FirstOrDefaultAsync();
      return (user != null, user);
    }
  }
}
