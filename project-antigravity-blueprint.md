# 🚀 Project FaUG: AI Gateway Blueprint

FauG is a high-performance, secure AI gateway designed to govern, monitor, and protect LLM interactions for autonomous agents (like Claude Code and Codex) and web applications.

## Core Architectural Pillars

- **Zero-Trust Governance**: Every request is authenticated via Virtual Keys with strict budget and policy enforcement.
- **The "Cursor" Routing Model**: A single endpoint that abstracts multiple providers (OpenAI, Anthropic, Azure, AWS Bedrock).
- **Multi-Layered Defense**: Sequential security filters for both input (prompts) and output (responses).
- **Observability First**: Real-time token tracking, cost attribution, and security risk scoring.

## 🔄 The Life Cycle of a Prompt (Internal Workflow)

The gateway processes every request through a Sequential Middleware Pipeline. Any stage can "short-circuit" the request if a violation is detected.

1. **Ingress**: Request hits the .NET 9 endpoint (e.g., POST /v1/chat/completions).

2. **Auth & Context Retrieval**:
   - Validate VirtualKey.
   - Retrieve User and Organisation metadata.
   - Load the associated Security Policy.

3. **Financial Gate (Hard Check)**:
   - Verify User.CurrentSpend < User.AllocatedBudget.
   - Verify Org.CurrentSpend < Org.TotalMonthlyBudget.
   - If failed: Return 402 Payment Required.

4. **Input Shielding**:
   - **Injection Scan**: Detect jailbreak patterns.
   - **PII Masking**: Redact emails, keys, and secrets from the prompt.
   - **Topic Filtering**: Block prohibited intents.

5. **Dynamic Routing**:
   - Select the ProviderAccount based on policy or requested model.soft 
   - Inject system instructions (Instruction Following Verification).

6. **Inference Execution**:
   - Proxy the request to the LLM (Streaming tokens supported via SSE).

7. **Output Shielding**:
   - **Quality Check**: Validate JSON/Structure.
   - **Leak Check**: Scan response for accidental credential leaks.
   - **Groundedness**: Optional check for RAG-based hallucinations.

8. **Egress & Audit**:
   - Stream/Deliver response to client.
   - **Async Logging**: Update usage logs and spend tables without delaying the user.

## Phased Implementation Roadmap

### Phase 1: Foundation (The Financial Engine)

- [ ] DB Setup: Physical implementation of the ER Diagram (PostgreSQL recommended).
- [ ] Proxy Core: Build the .NET 9 DelegatingHandler for basic request forwarding.
- [ ] Budget Controller: Implement logic to increment and enforce spend limits.
- [ ] Token Ledger: Build the async RequestLog service to track every call.

### Phase 2: Security Perimeter (The Firewall)

- [ ] Jailbreak Interceptor: Implement keyword and pattern-based injection detection.
- [ ] PII Redactor: Integrate a regex/NLP engine to scrub inputs.
- [ ] Model Registry: Create the routing table that maps "friendly names" to real API keys.

### Phase 3: Agent Orchestration (The Mission Control)

- [ ] CLI Support: Configure gateway to handle standard Anthropic/OpenAI CLI headers (Claude Code).
- [ ] Routing Logic: Implement automatic failover (e.g., if OpenAI is down, use Anthropic).
- [ ] Streaming Proxy: Ensure Server-Sent Events (SSE) work through the .NET middleware.

### Phase 4: Reliability & Quality (The Validator)

- [ ] Response Firewall: Build the post-inference scanner to catch model hallucinations or leaks.
- [ ] Validator Agent: Set up a "Shadow Model" check for high-risk code/structured outputs.
- [ ] Latency Tracking: Log Time-to-First-Token (TTFT) for performance monitoring.

### Phase 5: Visibility (The Dashboard)

- [ ] Next.js Admin Console: Create the UI for Org/User management.
- [ ] Live Spend Charts: Build the observability view for token usage and costs.
- [ ] Risk Scoring: Generate alerts for users attempting frequent injections.

## 🛠️ Technical Stack & Tools

| Component | Technology | Why? |
|-----------|-----------|------|
| Backend API | .NET 9 / YARP | High throughput and built-in reverse proxy tools. |
| Frontend UI | Next.js 15 + Tailwind | Modern, fast, and great for real-time dashboards. |
| Database | PostgreSQL | Handles complex relational schemas and JSON logs. |
| Caching | Redis | For ultra-fast budget and policy lookups. |
| DLP Engine | Microsoft Presidio | Enterprise-grade PII detection and redaction. |
| Observability | OpenTelemetry | Standardized tracing across agentic workflows. |

## 📜 Security Guardrails Checklist

- [ ] **Prompt Injection**: Blocking "ignore previous instructions" and "DAN" modes.
- [ ] **Tool Abuse**: Restricting function-calling based on user role.
- [ ] **Data Leakage**: Preventing internal API keys from appearing in responses.
- [ ] **Instruction Following**: Enforcing mandatory system prefixes on every call.
- [ ] **Budget Integrity**: Atomic spend updates to prevent "double-spending."
