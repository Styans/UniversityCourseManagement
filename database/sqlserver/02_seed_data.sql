-- Тестовые данные University Course Management API для MS SQL Server
-- Запускать ПОСЛЕ 01_schema.sql в базе UniversityCourseDb.
-- Даты хранятся в UTC.
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

SET IDENTITY_INSERT [Teachers] ON;
INSERT INTO [Teachers] ([Id], [FirstName], [LastName], [Email], [Department]) VALUES (1, N'Yerlan', N'Abenov', N'y.abenov@university.kz', N'Software Engineering');
INSERT INTO [Teachers] ([Id], [FirstName], [LastName], [Email], [Department]) VALUES (2, N'Gulnara', N'Sadykova', N'g.sadykova@university.kz', N'Software Engineering');
INSERT INTO [Teachers] ([Id], [FirstName], [LastName], [Email], [Department]) VALUES (3, N'Marat', N'Zhaksylykov', N'm.zhaksylykov@university.kz', N'Computer Science');
INSERT INTO [Teachers] ([Id], [FirstName], [LastName], [Email], [Department]) VALUES (4, N'Aliya', N'Nurlanova', N'a.nurlanova@university.kz', N'Mathematics');
INSERT INTO [Teachers] ([Id], [FirstName], [LastName], [Email], [Department]) VALUES (5, N'Daniyar', N'Kassymov', N'd.kassymov@university.kz', N'Cybersecurity');
SET IDENTITY_INSERT [Teachers] OFF;
GO

SET IDENTITY_INSERT [Courses] ON;
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (1, N'Web Services Development', N'REST API on ASP.NET Core, EF Core, Swagger', 5, 1, N'2026-10-22 05:33:53.243857');
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (2, N'Databases', N'Relational model, SQL, normalization, transactions', 5, 2, N'2026-10-22 05:33:53.243857');
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (3, N'Computer Vision', N'Image processing with OpenCV and Python', 6, 3, N'2026-10-22 05:33:53.243857');
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (4, N'Algorithms and Data Structures', N'Sorting, graphs, dynamic programming', 6, 3, N'2026-10-22 05:33:53.243857');
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (5, N'Linear Algebra', N'Matrices, vector spaces, eigenvalues', 4, 4, N'2026-10-22 05:33:53.243857');
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (6, N'Discrete Mathematics', N'Logic, sets, combinatorics, graph theory', 3, 4, N'2026-10-22 05:33:53.243857');
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (7, N'Network Security', N'Cryptography basics, TLS, firewalls', 4, 5, N'2026-10-22 05:33:53.243857');
INSERT INTO [Courses] ([Id], [Name], [Description], [Credits], [TeacherId], [CreatedAt]) VALUES (8, N'Software Testing', N'Unit, integration and API testing', 3, 1, N'2026-10-22 05:33:53.243857');
SET IDENTITY_INSERT [Courses] OFF;
GO

SET IDENTITY_INSERT [Students] ON;
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (1, N'Aruzhan', N'Bekova', N'aruzhan.bekova@student.university.kz', N'2004-03-14', N'2026-10-22 05:33:53.243857');
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (2, N'Nursultan', N'Omarov', N'nursultan.omarov@student.university.kz', N'2003-11-02', N'2026-10-22 05:33:53.243857');
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (3, N'Dana', N'Akhmetova', N'dana.akhmetova@student.university.kz', N'2005-01-27', N'2026-10-22 05:33:53.243857');
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (4, N'Timur', N'Iskakov', N'timur.iskakov@student.university.kz', N'2004-07-09', N'2026-10-22 05:33:53.243857');
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (5, N'Kamila', N'Serikova', N'kamila.serikova@student.university.kz', N'2004-05-30', N'2026-10-22 05:33:53.243857');
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (6, N'Alikhan', N'Tulegenov', N'alikhan.tulegenov@student.university.kz', N'2003-09-18', N'2026-10-22 05:33:53.243857');
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (7, N'Madina', N'Zhunusova', N'madina.zhunusova@student.university.kz', N'2005-02-11', N'2026-10-22 05:33:53.243857');
INSERT INTO [Students] ([Id], [FirstName], [LastName], [Email], [BirthDate], [CreatedAt]) VALUES (8, N'Arman', N'Kenzhebayev', N'arman.kenzhebayev@student.university.kz', N'2004-12-05', N'2026-10-22 05:33:53.243857');
SET IDENTITY_INSERT [Students] OFF;
GO

SET IDENTITY_INSERT [Enrollments] ON;
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (1, 1, 1, N'2026-10-22 05:33:53.243857', 92.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (2, 1, 2, N'2026-10-22 05:33:53.243857', 85.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (3, 1, 5, N'2026-10-22 05:33:53.243857', NULL);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (4, 2, 1, N'2026-10-22 05:33:53.243857', 78.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (5, 2, 4, N'2026-10-22 05:33:53.243857', NULL);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (6, 3, 3, N'2026-10-22 05:33:53.243857', 95.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (7, 3, 6, N'2026-10-22 05:33:53.243857', 88.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (8, 4, 1, N'2026-10-22 05:33:53.243857', NULL);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (9, 4, 7, N'2026-10-22 05:33:53.243857', 70.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (10, 5, 2, N'2026-10-22 05:33:53.243857', 90.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (11, 5, 8, N'2026-10-22 05:33:53.243857', NULL);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (12, 6, 4, N'2026-10-22 05:33:53.243857', 67.0);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (13, 7, 3, N'2026-10-22 05:33:53.243857', NULL);
INSERT INTO [Enrollments] ([Id], [StudentId], [CourseId], [EnrollmentDate], [Grade]) VALUES (14, 7, 5, N'2026-10-22 05:33:53.243857', 81.0);
SET IDENTITY_INSERT [Enrollments] OFF;
GO
