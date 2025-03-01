using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

public static class DbSeeder
{
    public static void SeedAdmin(ApplicationDbContext context)
    {
        if (!context.Users.Any(u => u.UserType == UserType.Administrator))
        {
            context.Users.Add(new User
            {
                Password = "admin@123",
                FirstName = "School",
                LastName = "Admin",
                Sex = "Male", 
                DOB = DateTime.Parse("1980-01-01"),
                Email = "admin@school.com",
                UserType = UserType.Administrator
            });
            context.SaveChanges();
        }
    }
}