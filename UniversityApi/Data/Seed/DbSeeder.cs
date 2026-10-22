using Microsoft.EntityFrameworkCore;
using UniversityApi.Models;

namespace UniversityApi.Data.Seed;

/// <summary>
/// Заполняет пустую базу тестовыми данными при первом запуске.
/// Если в базе уже есть хотя бы один студент или преподаватель, ничего не делает.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Students.AnyAsync() || await db.Teachers.AnyAsync())
        {
            logger.LogInformation("Seed skipped: database already contains data");
            return;
        }

        var teachers = new List<Teacher>
        {
            new() { FirstName = "Yerlan",  LastName = "Abenov",     Email = "y.abenov@university.kz",     Department = "Software Engineering" },
            new() { FirstName = "Gulnara", LastName = "Sadykova",   Email = "g.sadykova@university.kz",   Department = "Software Engineering" },
            new() { FirstName = "Marat",   LastName = "Zhaksylykov", Email = "m.zhaksylykov@university.kz", Department = "Computer Science" },
            new() { FirstName = "Aliya",   LastName = "Nurlanova",  Email = "a.nurlanova@university.kz",  Department = "Mathematics" },
            new() { FirstName = "Daniyar", LastName = "Kassymov",   Email = "d.kassymov@university.kz",   Department = "Cybersecurity" }
        };

        var courses = new List<Course>
        {
            new() { Name = "Web Services Development", Description = "REST API on ASP.NET Core, EF Core, Swagger", Credits = 5, Teacher = teachers[0] },
            new() { Name = "Databases",                Description = "Relational model, SQL, normalization, transactions", Credits = 5, Teacher = teachers[1] },
            new() { Name = "Computer Vision",          Description = "Image processing with OpenCV and Python", Credits = 6, Teacher = teachers[2] },
            new() { Name = "Algorithms and Data Structures", Description = "Sorting, graphs, dynamic programming", Credits = 6, Teacher = teachers[2] },
            new() { Name = "Linear Algebra",           Description = "Matrices, vector spaces, eigenvalues", Credits = 4, Teacher = teachers[3] },
            new() { Name = "Discrete Mathematics",     Description = "Logic, sets, combinatorics, graph theory", Credits = 3, Teacher = teachers[3] },
            new() { Name = "Network Security",         Description = "Cryptography basics, TLS, firewalls", Credits = 4, Teacher = teachers[4] },
            new() { Name = "Software Testing",         Description = "Unit, integration and API testing", Credits = 3, Teacher = teachers[0] }
        };

        var students = new List<Student>
        {
            new() { FirstName = "Aruzhan",  LastName = "Bekova",     Email = "aruzhan.bekova@student.university.kz",   BirthDate = new DateOnly(2004, 3, 14) },
            new() { FirstName = "Nursultan", LastName = "Omarov",    Email = "nursultan.omarov@student.university.kz", BirthDate = new DateOnly(2003, 11, 2) },
            new() { FirstName = "Dana",     LastName = "Akhmetova",  Email = "dana.akhmetova@student.university.kz",   BirthDate = new DateOnly(2005, 1, 27) },
            new() { FirstName = "Timur",    LastName = "Iskakov",    Email = "timur.iskakov@student.university.kz",    BirthDate = new DateOnly(2004, 7, 9) },
            new() { FirstName = "Kamila",   LastName = "Serikova",   Email = "kamila.serikova@student.university.kz",  BirthDate = new DateOnly(2004, 5, 30) },
            new() { FirstName = "Alikhan",  LastName = "Tulegenov",  Email = "alikhan.tulegenov@student.university.kz", BirthDate = new DateOnly(2003, 9, 18) },
            new() { FirstName = "Madina",   LastName = "Zhunusova",  Email = "madina.zhunusova@student.university.kz", BirthDate = new DateOnly(2005, 2, 11) },
            new() { FirstName = "Arman",    LastName = "Kenzhebayev", Email = "arman.kenzhebayev@student.university.kz", BirthDate = new DateOnly(2004, 12, 5) }
        };

        // Примеры записей на курсы: часть с оценками, часть без (оценка ещё не выставлена)
        var enrollments = new List<Enrollment>
        {
            new() { Student = students[0], Course = courses[0], Grade = 92 },
            new() { Student = students[0], Course = courses[1], Grade = 85 },
            new() { Student = students[0], Course = courses[4] },
            new() { Student = students[1], Course = courses[0], Grade = 78 },
            new() { Student = students[1], Course = courses[3] },
            new() { Student = students[2], Course = courses[2], Grade = 95 },
            new() { Student = students[2], Course = courses[5], Grade = 88 },
            new() { Student = students[3], Course = courses[0] },
            new() { Student = students[3], Course = courses[6], Grade = 70 },
            new() { Student = students[4], Course = courses[1], Grade = 90 },
            new() { Student = students[4], Course = courses[7] },
            new() { Student = students[5], Course = courses[3], Grade = 67 },
            new() { Student = students[6], Course = courses[2] },
            new() { Student = students[6], Course = courses[4], Grade = 81 }
        };

        db.Teachers.AddRange(teachers);
        db.Courses.AddRange(courses);
        db.Students.AddRange(students);
        db.Enrollments.AddRange(enrollments);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Seed completed: {Teachers} teachers, {Courses} courses, {Students} students, {Enrollments} enrollments",
            teachers.Count, courses.Count, students.Count, enrollments.Count);
    }
}
