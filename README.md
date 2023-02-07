# Use

Run all:
```powershell
sudo dotnet run -c RELEASE
```

## Commandline Arguments

### Any Categories
Run any tests that have any of the properties provided in the argument `anyCategories`
```
sudo dotnet run -c RELEASE -- --anyCategories Copy
```

### Filter

```powershell
sudo dotnet run -c RELEASE -- --filter="*SystemTextJsonDeserializationBasic*"
```
***