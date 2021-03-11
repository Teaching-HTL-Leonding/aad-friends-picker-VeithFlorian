using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class Data
    {
        public class User
        {
            public int Id { get; set; }
            
            public string UserId { get; set; }
            
            public string FriendId { get; set; }
        }
    }
    
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options)
        { }

        public DbSet<Data.User> Users { get; set; }
    }
}