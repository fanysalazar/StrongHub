# StrongHub

Gym workout routine management system built with C# and Windows Forms.

## Description

StrongHub lets gym users register, manage their workout routines, log visits and
measurements, earn badges, receive notifications and view statistics. Trainers
manage routines and receive alerts about expiring routines and upcoming measurements.

## Technologies

- C# and Windows Forms (.NET 10)
- MVC architecture, OOP, SOLID and Clean Code
- Design patterns: Repository, Strategy and Observer
- JSON files for persistence
- xUnit for unit tests
- Jira for project tracking, GitHub for version control, SonarQube Cloud for code analysis

## Author

- Student: Stephany Salazar Vargas
- Course: Técnicas de Programación
- Professor: Luis Felipe Mora Umaña
- University: Universidad Politécnica Internacional

## Project structure

- `src/` source code (Models, Data, Controllers, Views)
- `tests/` unit tests
- `data/` JSON data files
- `docs/` documentation
- `tools/` data generation scripts

## How to run

Requirements: Windows and the .NET 10 SDK.

```powershell
dotnet build
dotnet test
dotnet run --project src\StrongHub.Views
```

Test users: `user1` to `user50` (members) and `trainer1` to `trainer45` (trainers).