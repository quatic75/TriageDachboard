# Triage Dashboard API Reference

**Base URL:** `http://localhost:5010`  
**Content-Type:** `application/json`

## Overview

The Triage Dashboard API manages pipeline failure records with AI-assisted triage information, Jira ticket tracking, and automated retry capabilities.

---

## Data Model: PipelineFailure

### Core Fields
- **id** (guid, required): Unique identifier, auto-generated
- **runId** (string, required): Pipeline run identifier
- **pipelineName** (string, required): Name of the pipeline
- **activityName** (string, optional): Specific activity that failed
- **errorMessage** (string, required): Error message from the failure

### AI Triage Fields
- **classification** (string, optional): Error category (e.g., "CompilationError", "NetworkError", "SecurityVulnerability", "ResourceExhaustion", "ConfigurationError", "TimeoutError")
- **confidence** (number, optional): AI confidence score (0.0 to 1.0)
- **summary** (string, optional): Brief summary of the failure
- **rootCause** (string, optional): Identified root cause
- **suggestedFix** (string, optional): Recommended fix
- **sourceJson** (string, optional): Raw JSON source data used by AI for traceability

### Automation Status Fields
- **autoHandled** (boolean, required): Whether the failure was auto-handled (default: false)
- **jiraCreated** (boolean, required): Whether a Jira ticket was created (default: false)
- **jiraTicketId** (string, optional): Generated Jira ticket ID (e.g., "TRIAGE-1234")
- **jiraTicketUrl** (string, optional): Full URL to Jira ticket (e.g., "https://jira.example.com/browse/TRIAGE-1234")
- **retryAttempted** (boolean, required): Whether a retry was attempted (default: false)
- **status** (string, required): Current status - one of:
  - `"Open"`: Newly created, requires attention
  - `"Resolved"`: Fixed or auto-handled
  - `"NeedsIntervention"`: Requires manual intervention

### Timestamp Fields
- **createdAt** (datetime, required): Record creation timestamp (UTC, auto-generated)
- **updatedAt** (datetime, required): Last update timestamp (UTC, auto-updated)

---

## Endpoints

### 1. List All Failures

**GET** `/failures`

Returns all pipeline failures in the system.

**Response:** `200 OK`
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "runId": "run-001",
    "pipelineName": "Build Pipeline",
    "activityName": "Compile",
    "errorMessage": "Compilation failed: missing semicolon",
    "classification": "CompilationError",
    "confidence": 0.95,
    "summary": "Syntax error in source code",
    "rootCause": "Missing semicolon in Main.cs line 42",
    "suggestedFix": "Add semicolon at end of statement",
    "sourceJson": "{\"line\": 42, \"file\": \"Main.cs\"}",
    "autoHandled": false,
    "jiraCreated": true,
    "jiraTicketId": "TRIAGE-1001",
    "jiraTicketUrl": "https://jira.example.com/browse/TRIAGE-1001",
    "retryAttempted": false,
    "status": "Open",
    "createdAt": "2026-03-25T10:30:00Z",
    "updatedAt": "2026-03-25T10:35:00Z"
  }
]
```

---

### 2. Get Failure by ID

**GET** `/failures/{id}`

Retrieves a specific failure record by its GUID.

**Parameters:**
- `id` (path, guid, required): Failure ID

**Response:** `200 OK` (same schema as list item) or `404 Not Found`

---

### 3. Create New Failure

**POST** `/failures`

Creates a new pipeline failure record.

**Request Body:**
```json
{
  "runId": "run-123",
  "pipelineName": "Deployment Pipeline",
  "activityName": "Deploy to Staging",
  "errorMessage": "Connection timeout to database",
  "classification": "NetworkError",
  "confidence": 0.88,
  "summary": "Database connection failed",
  "rootCause": "Network latency exceeded threshold",
  "suggestedFix": "Increase connection timeout or check network",
  "sourceJson": "{\"timeout_ms\": 5000, \"host\": \"db.example.com\"}"
}
```

**Required Fields:** `runId`, `pipelineName`, `errorMessage`  
**Optional Fields:** All other fields

**Response:** `201 Created`
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "runId": "run-123",
  "pipelineName": "Deployment Pipeline",
  "activityName": "Deploy to Staging",
  "errorMessage": "Connection timeout to database",
  "classification": "NetworkError",
  "confidence": 0.88,
  "summary": "Database connection failed",
  "rootCause": "Network latency exceeded threshold",
  "suggestedFix": "Increase connection timeout or check network",
  "sourceJson": "{\"timeout_ms\": 5000, \"host\": \"db.example.com\"}",
  "autoHandled": false,
  "jiraCreated": false,
  "jiraTicketId": null,
  "jiraTicketUrl": null,
  "retryAttempted": false,
  "status": "Open",
  "createdAt": "2026-03-25T16:30:00Z",
  "updatedAt": "2026-03-25T16:30:00Z"
}
```

**Location Header:** `/failures/{id}`

---

### 4. Update Failure (Partial Update)

**PATCH** `/failures/{id}`

Updates specific fields of a failure record. Only provided fields are updated.

**Parameters:**
- `id` (path, guid, required): Failure ID

**Request Body (all fields optional):**
```json
{
  "classification": "ConfigurationError",
  "confidence": 0.92,
  "summary": "Updated summary after analysis",
  "rootCause": "Incorrect configuration value",
  "suggestedFix": "Update config.json with correct value",
  "status": "Resolved"
}
```

**Response:** `200 OK` (returns updated full record) or `404 Not Found`

---

### 5. Create Jira Ticket

**POST** `/failures/{id}/jira`

Creates a mock Jira ticket for the failure. Generates a ticket ID and URL, sets `jiraCreated` to true.

**Parameters:**
- `id` (path, guid, required): Failure ID

**Request Body:** None

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "runId": "run-001",
  "jiraCreated": true,
  "jiraTicketId": "TRIAGE-2780",
  "jiraTicketUrl": "https://jira.example.com/browse/TRIAGE-2780",
  "updatedAt": "2026-03-25T16:35:00Z"
  // ... other fields
}
```

**Error:** `404 Not Found` if failure doesn't exist

**Business Logic:**
- Generates unique ticket ID in format: `TRIAGE-{4-digit-random-number}`
- Generates URL: `https://jira.example.com/browse/{ticketId}`
- Sets `jiraCreated = true`
- Updates `updatedAt` timestamp

**Idempotency:** Can be called multiple times; generates new ticket ID each time

---

### 6. Retry Pipeline

**POST** `/failures/{id}/retry`

Attempts to retry the failed pipeline. Implements escalation logic.

**Parameters:**
- `id` (path, guid, required): Failure ID

**Request Body:** None

**Response:** `200 OK` (returns updated record)

**Business Logic:**
1. **First retry:** Sets `retryAttempted = true`
2. **Second retry:** Sets `status = "NeedsIntervention"` (escalation)
3. Updates `updatedAt` timestamp

**Error:** `404 Not Found` if failure doesn't exist

**Example Workflow:**
```
Initial State:     retryAttempted=false, status="Open"
After 1st retry:   retryAttempted=true,  status="Open"
After 2nd retry:   retryAttempted=true,  status="NeedsIntervention"
```

---

## Common Patterns

### Creating and Triaging a Failure

```json
// 1. Create failure
POST /failures
{
  "runId": "run-456",
  "pipelineName": "CI Pipeline",
  "errorMessage": "Test failure in unit tests",
  "classification": "TestFailure",
  "confidence": 0.87,
  "summary": "3 unit tests failed",
  "sourceJson": "{\"failed_tests\": [\"Test1\", \"Test2\", \"Test3\"]}"
}

// 2. Update triage information
PATCH /failures/{id}
{
  "rootCause": "Recent code change broke dependencies",
  "suggestedFix": "Revert commit abc123 or update test mocks"
}

// 3. Create Jira ticket for tracking
POST /failures/{id}/jira

// 4. If auto-fix attempted, mark as resolved
PATCH /failures/{id}
{
  "status": "Resolved",
  "autoHandled": true
}
```

### Retry with Escalation

```json
// 1. First retry attempt
POST /failures/{id}/retry
// Response: retryAttempted=true, status="Open"

// 2. Second retry attempt (escalates)
POST /failures/{id}/retry
// Response: retryAttempted=true, status="NeedsIntervention"

// 3. Create Jira ticket for human intervention
POST /failures/{id}/jira
```

---

## Status Transitions

Valid status transitions:
- `Open` → `Resolved` (issue fixed)
- `Open` → `NeedsIntervention` (via retry escalation)
- `NeedsIntervention` → `Resolved` (manual fix applied)
- Any status can be manually set via PATCH

---

## Error Responses

**404 Not Found:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

**400 Bad Request:** (validation errors)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "runId": ["The runId field is required."]
  }
}
```

---

## Example Use Cases

### Use Case 1: AI Agent Creates Failure After Pipeline Analysis
```bash
curl -X POST http://localhost:5010/failures \
  -H "Content-Type: application/json" \
  -d '{
    "runId": "run-789",
    "pipelineName": "Docker Build",
    "errorMessage": "Docker image build failed",
    "classification": "ResourceExhaustion",
    "confidence": 0.91,
    "summary": "Disk space exceeded during build",
    "rootCause": "Build artifacts not cleaned up",
    "suggestedFix": "Run cleanup script before build",
    "sourceJson": "{\"disk_usage_gb\": 95, \"threshold_gb\": 90}"
  }'
```

### Use Case 2: Query Open Failures Needing Attention
```bash
# Get all failures
curl http://localhost:5010/failures | jq '.[] | select(.status == "Open")'

# Get failures without Jira tickets
curl http://localhost:5010/failures | jq '.[] | select(.jiraCreated == false)'

# Get failures needing intervention
curl http://localhost:5010/failures | jq '.[] | select(.status == "NeedsIntervention")'
```

### Use Case 3: Automated Triage Workflow
```bash
FAILURE_ID="550e8400-e29b-41d4-a716-446655440000"

# Update with AI analysis
curl -X PATCH http://localhost:5010/failures/$FAILURE_ID \
  -H "Content-Type: application/json" \
  -d '{"rootCause": "API endpoint deprecated", "suggestedFix": "Update to v2 endpoint"}'

# Create Jira ticket
curl -X POST http://localhost:5010/failures/$FAILURE_ID/jira

# Mark as resolved if auto-fixed
curl -X PATCH http://localhost:5010/failures/$FAILURE_ID \
  -H "Content-Type: application/json" \
  -d '{"status": "Resolved", "autoHandled": true}'
```

---

## Response Headers

All successful responses include:
- `Content-Type: application/json; charset=utf-8`
- `Date`: Response timestamp

POST responses include:
- `Location: /failures/{id}`: URL of created resource

---

## Notes for AI Agents

1. **IDs are GUIDs:** Always use full GUID format (36 characters with hyphens)
2. **Timestamps are UTC:** All datetime values are in ISO 8601 format (UTC)
3. **Confidence is 0-1:** Represent AI confidence as decimal between 0.0 and 1.0
4. **Status is Enum:** Only use: "Open", "Resolved", or "NeedsIntervention"
5. **PATCH is Partial:** Only send fields you want to update, others remain unchanged
6. **Jira Tickets are Mock:** POST /jira generates mock data, doesn't connect to real Jira
7. **Retry Escalates:** Second retry automatically changes status to "NeedsIntervention"
8. **SourceJson for Traceability:** Store raw source data as JSON string for debugging/audit

---

## Database Seeding

The API includes 15 pre-seeded failure records on first run:
- 6 with status "Open"
- 4 with status "Resolved"
- 5 with status "NeedsIntervention"
- 7 with Jira tickets already created

Use these for testing queries and workflows without creating data first.
