##To Generate Migrations in CIRLib
Use the below command which takes the CIRLib.Test as the startup Project.

```
dotnet ef migrations add YourMigrationName  -p "CIRLib.csproj" -s "..\..\test\unit\CIRLib.Test\CIRLib.Test.csproj"
```
