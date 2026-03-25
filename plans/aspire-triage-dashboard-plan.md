## Plan: Aspire-Based Triage Dashboard with PostgreSQL and Next.js

Building a complete .NET Aspire orchestrated system with PostgreSQL database, .NET Minimal API, and Next.js dashboard for pipeline failure triage. The solution follows TDD principles and will run locally via Aspire with easy Azure deployment path.

**Phases: 8**

1. **Phase 1: Scaffold Aspire Solution Structure**
   - **Objective:** Create the Aspire AppHost, API service, and worker service projects with proper references
   - **Files/Functions to Create:**
     - `TriageDashboard.sln`
     - `AppHost/AppHost.csproj` and `AppHost/Program.cs`
     - `ApiService/ApiService.csproj` and `ApiService/Program.cs`
     - `WorkerService/WorkerService.csproj` and `WorkerService/Worker.cs`
     - `ApiService.Tests/ApiService.Tests.csproj`
   - **Tests to Write:**
     - `ApiService.Tests/ProjectStructureTests.cs` - verify projects load and dependencies exist
   - **Steps:**
     1. Write test to verify solution structure and project references
     2. Run test to see it fail
     3. Create solution file and add Aspire AppHost project
     4. Add ApiService project (Minimal API template)
     5. Add WorkerService project (Worker template)
     6. Add ApiService.Tests project (xUnit)
     7. Add necessary Aspire NuGet packages
     8. Run test to verify structure is correct

2. **Phase 2: Implement Data Model and DbContext**
   - **Objective:** Create PipelineFailure entity, Status enum, and ApplicationDbContext with proper configuration
   - **Files/Functions to Create:**
     - `ApiService/Models/PipelineFailure.cs` - entity model with all fields
     - `ApiService/Models/FailureStatus.cs` - enum (Open, Resolved, NeedsIntervention)
     - `ApiService/Data/ApplicationDbContext.cs` - EF Core context
     - `ApiService.Tests/Models/PipelineFailureTests.cs`
     - `ApiService.Tests/Data/DbContextTests.cs`
   - **Tests to Write:**
     - `PipelineFailureTests.ValidateRequiredFields` - ensure required fields validated
     - `PipelineFailureTests.DefaultValues` - verify default values (AutoHandled=false, etc.)
     - `DbContextTests.ConfigureEntity` - verify PipelineFailure is configured
     - `DbContextTests.CanCreateInMemoryContext` - test context instantiation
   - **Steps:**
     1. Write tests for FailureStatus enum values
     2. Create FailureStatus enum
     3. Write tests for PipelineFailure entity properties and constraints
     4. Create PipelineFailure entity with data annotations
     5. Write tests for ApplicationDbContext configuration
     6. Create ApplicationDbContext with DbSet<PipelineFailure>
     7. Add Npgsql.EntityFrameworkCore.PostgreSQL package
     8. Run all tests to verify models and context

3. **Phase 3: Implement CRUD API Endpoints**
   - **Objective:** Create GET /failures, GET /failures/{id}, POST /failures, PATCH /failures/{id} endpoints with validation
   - **Files/Functions to Create:**
     - `ApiService/Endpoints/FailureEndpoints.cs` - MapFailureEndpoints extension
     - `ApiService/DTOs/CreateFailureDto.cs`
     - `ApiService/DTOs/UpdateFailureDto.cs`
     - `ApiService/DTOs/FailureDto.cs`
     - `ApiService.Tests/Endpoints/FailureEndpointsTests.cs`
   - **Tests to Write:**
     - `FailureEndpointsTests.GetFailures_ReturnsAllFailures`
     - `FailureEndpointsTests.GetFailureById_ReturnsFailure_WhenExists`
     - `FailureEndpointsTests.GetFailureById_Returns404_WhenNotFound`
     - `FailureEndpointsTests.PostFailure_CreatesFailure_ReturnsCreated`
     - `FailureEndpointsTests.PatchFailure_UpdatesFields_Returns200`
     - `FailureEndpointsTests.PatchFailure_Returns404_WhenNotFound`
   - **Steps:**
     1. Write tests for GET /failures endpoint using WebApplicationFactory
     2. Run tests to see them fail
     3. Create FailureDto and DTOs for create/update
     4. Implement GET /failures endpoint in FailureEndpoints
     5. Run test to verify it passes
     6. Write tests for GET /failures/{id}
     7. Implement GET /failures/{id} with 404 handling
     8. Write tests for POST /failures
     9. Implement POST /failures with validation
     10. Write tests for PATCH /failures/{id}
     11. Implement PATCH /failures/{id} with partial updates
     12. Register endpoints in Program.cs
     13. Run all tests to verify CRUD operations

4. **Phase 4: Implement Business Logic Endpoints (Jira and Retry)**
   - **Objective:** Create POST /failures/{id}/jira and POST /failures/{id}/retry with proper state management
   - **Files/Functions to Create:**
     - `ApiService/Endpoints/FailureActionsEndpoints.cs`
     - `ApiService.Tests/Endpoints/FailureActionsTests.cs`
   - **Tests to Write:**
     - `FailureActionsTests.CreateJira_SetsJiraCreatedTrue`
     - `FailureActionsTests.CreateJira_Returns404_WhenNotFound`
     - `FailureActionsTests.Retry_FirstAttempt_SetsRetryAttempted`
     - `FailureActionsTests.Retry_SecondAttempt_SetsNeedsIntervention`
     - `FailureActionsTests.Retry_Returns404_WhenNotFound`
   - **Steps:**
     1. Write test for POST /failures/{id}/jira success case
     2. Run test to see it fail
     3. Implement POST /failures/{id}/jira endpoint
     4. Run test to verify it passes
     5. Write test for retry logic (first attempt)
     6. Write test for retry logic (second attempt → NeedsIntervention)
     7. Implement POST /failures/{id}/retry with retry tracking
     8. Run all tests to verify business logic
     9. Register action endpoints in Program.cs

5. **Phase 5: Configure PostgreSQL Integration and Aspire AppHost**
   - **Objective:** Wire up PostgreSQL in Aspire AppHost and configure API to use it with proper service discovery
   - **Files/Functions to Modify:**
     - `AppHost/Program.cs` - add PostgreSQL and service references
     - `ApiService/Program.cs` - configure DbContext with injected connection string
     - `ApiService/appsettings.json`
   - **Tests to Write:**
     - `AppHost.Tests/AppHostTests.cs` - verify Aspire model configuration
   - **Steps:**
     1. Write test to verify Aspire resources are configured
     2. In AppHost/Program.cs, add PostgreSQL using AddPostgres("postgres")
     3. Add database "triage_db" using AddDatabase
     4. Add ApiService project reference with WithReference to postgres
     5. In ApiService/Program.cs, configure DbContext with Aspire connection string
     6. Add Aspire.Npgsql.EntityFrameworkCore.PostgreSQL package
     7. Run AppHost to verify services start and database connects

6. **Phase 6: Create EF Migrations and Seed Data**
   - **Objective:** Generate initial migration and seed database with test data for development
   - **Files/Functions to Create:**
     - `ApiService/Migrations/*.cs` - EF Core migration files
     - `ApiService/Data/DbSeeder.cs` - seed data for testing
   - **Tests to Write:**
     - `DbSeederTests.Seed_CreatesTestData`
   - **Steps:**
     1. Write test to verify seeder creates expected test data
     2. Run test to see it fail
     3. Create DbSeeder class with sample PipelineFailure records
     4. Run dotnet ef migrations add InitialCreate
     5. Verify migration files created
     6. Add migration execution on startup (EnsureCreated or Migrate)
     7. Call DbSeeder in Program.cs startup
     8. Run AppHost and verify database created with seed data
     9. Run test to verify seeder works

7. **Phase 7: Integrate Next.js Dashboard**
   - **Objective:** Add Next.js dashboard to Aspire orchestration and configure API connectivity
   - **Files/Functions to Create:**
     - `Dashboard/package.json`
     - `Dashboard/app/page.tsx` - main failures list view
     - `Dashboard/app/api/failures/route.ts` - API proxy (optional)
     - `Dashboard/.env.local` - API URL configuration
   - **Tests to Write:**
     - Not applicable (manual verification)
   - **Steps:**
     1. Create Next.js app in Dashboard folder using npx create-next-app@latest
     2. Add AddNpmApp to AppHost/Program.cs for Dashboard
     3. Configure environment variable for API URL
     4. Create basic UI to display failures table
     5. Add API calls to GET /failures
     6. Add detail panel with action buttons
     7. Implement PATCH, Jira, and Retry actions
     8. Run dotnet run --project AppHost and verify dashboard loads
     9. Test full E2E flow: view failures, update, retry, create Jira

8. **Phase 8: Add Health Checks and Logging (Bonus)**
   - **Objective:** Implement health checks for API and database, add structured logging for operations
   - **Files/Functions to Create:**
     - `ApiService/HealthChecks/DatabaseHealthCheck.cs`
     - `ApiService.Tests/HealthChecks/HealthCheckTests.cs`
   - **Tests to Write:**
     - `HealthCheckTests.DatabaseHealthCheck_Healthy_WhenConnected`
     - `HealthCheckTests.ApiHealthCheck_Returns200`
   - **Steps:**
     1. Write tests for health check endpoints
     2. Add health checks to API services
     3. Register DbContext health check
     4. Add health check endpoint /health
     5. Add structured logging for retry attempts
     6. Add structured logging for Jira creation
     7. Verify health checks in Aspire dashboard
     8. Run all tests to verify health checks work

**Decisions Made:**
- Next.js App Router (modern, default)
- 15 sample failures for seed data
- Worker stub with basic logging loop
- Retry tracked on entity field (simpler)
- Tailwind CSS (comes with Next.js)
