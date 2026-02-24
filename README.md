FauG, which is derived from the Hindi word for "Soldier," stands for a resolute guardian. Faugi is the front-line defender in the quickly changing field of autonomous AI agents, making sure that every prompt is secure, every response is validated, and every dollar of the budget is tracked.

## 1. Core Architectural Pillars
- **Zero-Trust Governance**: Every request is authenticated via Virtual Keys with strict budget and policy enforcement before inference.
- **The "Cursor" Routing Model**: A single unified endpoint abstracting multiple providers (OpenAI, Groq).
- **Multi-Layered Defense**: Sequential security filters for input prompts leveraging local Machine Learning models.
- **Real-Time Observability**: Synchronous Redis tracking combined with asynchronous PostgreSQL persistence for zero-latency auditing.

## 2. High-Level System Architecture
The following diagram illustrates the deployment topology and network flow between the client, the API Gateway, and the external dependencies.

```mermaid
graph TD
    Client[Client Application] -->|HTTP POST| Gateway(FauG API Gateway)
    
    subgraph "FauG Infrastructure (.NET 9)"
        Gateway --> Auth[Auth Middleware]
        Auth --> RateLimit[Rate Limit Middleware]
        RateLimit --> Budget[Budget Middleware]
        Budget --> Security[Security Middleware]
        Security --> Proxy[YARP Proxy Engine]
        Proxy --> AsyncLog[Async Request Logger]
    end

    subgraph "Data Persistence"
        Redis[(Redis Cache)]
        Postgres[(PostgreSQL)]
    end

    subgraph "External Providers"
        OpenAI(OpenAI API)
        Groq(Groq API)
    end

    Auth <-->|Verify Key| Redis
    RateLimit <-->|Sliding Window| Redis
    Budget <-->|Atomic Decrement| Redis
    
    Proxy -->|Forward Stream| OpenAI
    Proxy -->|Forward Stream| Groq
    
    AsyncLog -.->|Persist Logs| Postgres
```

## 3. The Life Cycle of a Prompt
The gateway processes every request through a sequential middleware pipeline. Any stage can short-circuit the request if a violation is detected, preventing the payload from reaching the upstream provider.

```mermaid
sequenceDiagram
    participant Client
    participant Gateway as FauG Gateway
    participant Redis
    participant ML as Sentinel Model (Local)
    participant Provider as External LLM
    participant DB as PostgreSQL

    Client->>Gateway: POST /v1/chat/completions
    
    Note over Gateway: 1. Authentication
    Gateway->>Redis: Validate Virtual Key & Cache
    Redis-->>Gateway: Return Session Data
    
    Note over Gateway: 2. Resilience
    Gateway->>Redis: Increment Request Window (Rate Limit)
    Redis-->>Gateway: HTTP 429 if Exceeded
    
    Note over Gateway: 3. Financial Check
    Gateway->>Redis: Check Current Spend vs Budget
    Redis-->>Gateway: HTTP 402 if Exhausted
    
    Note over Gateway: 4. Security Perimeter
    Gateway->>Gateway: Apply Regex for PII Leakage
    Gateway->>ML: Evaluate Prompt for Jailbreak
    ML-->>Gateway: Injection Score (Drop if Malicious)
    
    Note over Gateway: 5. Proxy & Inference
    Gateway->>Provider: Forward Sanitized Request
    Provider-->>Gateway: Stream Response Chunks
    
    Note over Gateway: 6. Response Interception
    Gateway->>Gateway: Parse Completion Tokens from Buffer
    Gateway-->>Client: Flush Stream to Client
    
    Note over Gateway: 7. Audit & Persistence
    Gateway-)Redis: Deduct Budget (Fast-Path)
    Gateway-)DB: Write Cost to RequestLogs (Slow-Path)
```

## 4. Optimization Strategy 
To ensure minimal latency overhead against provider streaming, the Gateway utilizes aggressive caching mechanisms:

### A. Virtual Key Validation (Auth)
*   **Cache Strategy**: `Key:{Hash}` resolves to `{UserId, OrgId, Scopes}`.
*   **TTL**: 5 minutes (Sliding expiration).
*   **Result**: Eliminates relational database lookups on high-frequency token generation requests.

### B. Real-Time Budgeting (The High-Speed Counter)
*   **Cache Strategy**: Use Redis `DECR` (Atomic Decrement) for real-time budget enforcement.
*   **Persistence**: Asynchronous background workers flush usage from Redis to PostgreSQL.
*   **Result**: Prevents "double-spend" financial attacks and database locking during high concurrency.

## 5. Middleware Pipeline Order
The `.NET` middleware sequence is designed to maximize performance by dropping invalid traffic as early as possible.
1. `AuthMiddleware`: Validates identity.
2. `RateLimitingMiddleware`: Protects system health (mathematically limits RPM).
3. `BudgetMiddleware`: Protects project finances (limits total monetary spend).
4. `SecurityMiddleware`: Protects data (executes the heavy ONNX ML scoring).
5. `YARP Reverse Proxy`: Manages network egress and chunk buffering.
