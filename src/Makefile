dbProjectPath=Cpb.Database
contextNamespace=Cpb.Database.DbCoffeePointContext
startupProjectPath=Cpb.Api

add-migration:
	dotnet ef migrations add $(name) --project "$(dbProjectPath)" --context "$(contextNamespace)" --startup-project "$(startupProjectPath)"
	
apply-migration:
	dotnet ef database update --project "$(dbProjectPath)" --context "$(contextNamespace)" --startup-project "$(startupProjectPath)"