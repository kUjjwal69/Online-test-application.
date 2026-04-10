using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Models.Entities;

namespace TestManagementApplication.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Apply any pending migrations automatically
            await context.Database.MigrateAsync();

            await SeedAdminUserAsync(context, logger);
            await SeedSampleDataAsync(context, logger);
        }

        private static async Task SeedAdminUserAsync(AppDbContext context, ILogger logger)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (existing != null)
            {
                // Ensure expected defaults without duplicating users
                var updated = false;
                if (!string.Equals(existing.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    existing.Role = "Admin";
                    updated = true;
                }
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    updated = true;
                }
                if (string.IsNullOrWhiteSpace(existing.Email))
                {
                    existing.Email = "admin@testplatform.com";
                    updated = true;
                }
                if (updated)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Existing 'admin' user updated to Admin role and activated.");
                }
                else
                {
                    logger.LogInformation("Default admin user already exists. Skipping admin seed.");
                }
                return;
            }

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@testplatform.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
            logger.LogInformation("Default admin user created. Username: admin | Password: Admin@123");
        }

        private static async Task SeedSampleDataAsync(AppDbContext context, ILogger logger)
        {
            // Skip if sample test already exists
            if (await context.Tests.AnyAsync())
            {
                logger.LogInformation("Sample data already exists. Skipping sample seed.");
                return;
            }

            var admin = await context.Users.FirstAsync(u => u.Role == "Admin");

            // ── Seed sample candidate ──────────────────────────────────────
            var candidate = new User
            {
                Id = Guid.NewGuid(),
                Username = "john_doe",
                Email = "john@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(candidate);

            // ── Seed sample test ───────────────────────────────────────────
            var test = new Test
            {
                Id = Guid.NewGuid(),
                Title = "C# Fundamentals Quiz",
                Description = "A basic quiz to test your C# programming knowledge.",
                DurationMinutes = 30,
                TotalMarks = 10,
                PassingMarks = 6,
                ViolationThreshold = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = admin.Id
            };
            context.Tests.Add(test);

            // ── Seed sample questions ──────────────────────────────────────
            var questions = new List<Question>
            {
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 1, Marks = 1, CorrectOption = "B",
                    QuestionText = "What is the default value of an integer in C#?",
                    OptionA = "null", OptionB = "0", OptionC = "-1", OptionD = "undefined" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 2, Marks = 1, CorrectOption = "C",
                    QuestionText = "Which keyword is used to define an interface in C#?",
                    OptionA = "abstract", OptionB = "class", OptionC = "interface", OptionD = "struct" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 3, Marks = 1, CorrectOption = "A",
                    QuestionText = "Which of the following is a value type in C#?",
                    OptionA = "int", OptionB = "string", OptionC = "object", OptionD = "Array" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 4, Marks = 1, CorrectOption = "D",
                    QuestionText = "What does 'async' keyword indicate in C#?",
                    OptionA = "Synchronous execution", OptionB = "Thread-safe code", OptionC = "Parallel loops", OptionD = "Asynchronous method" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 5, Marks = 1, CorrectOption = "B",
                    QuestionText = "Which collection type allows duplicate elements and maintains insertion order?",
                    OptionA = "HashSet<T>", OptionB = "List<T>", OptionC = "Dictionary<K,V>", OptionD = "SortedSet<T>" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 6, Marks = 1, CorrectOption = "A",
                    QuestionText = "What is the base class for all classes in C#?",
                    OptionA = "object", OptionB = "base", OptionC = "System", OptionD = "ValueType" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 7, Marks = 1, CorrectOption = "C",
                    QuestionText = "Which access modifier makes a member accessible only within the same class?",
                    OptionA = "protected", OptionB = "internal", OptionC = "private", OptionD = "public" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 8, Marks = 1, CorrectOption = "D",
                    QuestionText = "What is LINQ in C#?",
                    OptionA = "A database engine", OptionB = "A UI framework", OptionC = "A security library", OptionD = "Language Integrated Query" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 9, Marks = 1, CorrectOption = "A",
                    QuestionText = "Which method is the entry point of a C# application?",
                    OptionA = "Main()", OptionB = "Start()", OptionC = "Run()", OptionD = "Execute()" },
                new() { Id = Guid.NewGuid(), TestId = test.Id, OrderIndex = 10, Marks = 1, CorrectOption = "B",
                    QuestionText = "What does 'sealed' keyword prevent in C#?",
                    OptionA = "Overloading", OptionB = "Inheritance", OptionC = "Instantiation", OptionD = "Implementation" },
            };
            context.Questions.AddRange(questions);

            // ── Assign test to candidate ───────────────────────────────────
            var assignment = new TestAssignment
            {
                Id = Guid.NewGuid(),
                TestId = test.Id,
                UserId = candidate.Id,
                AssignedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            context.TestAssignments.Add(assignment);

            await context.SaveChangesAsync();
            logger.LogInformation("Sample data seeded: 1 test with 10 questions assigned to candidate 'john_doe' (password: User@123).");
        }
    }
}
