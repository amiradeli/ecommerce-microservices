#### Migration Scripts

```bash
dotnet ef migrations add InitialIdentityServerMigration -o Share\Infrastructure\Data\Migrations
dotnet ef database update
```