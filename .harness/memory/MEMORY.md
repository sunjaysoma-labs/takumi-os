# Takumi OS — Team Memory

## Project Decisions

| Date | Decision | Owner |
|---|---|---|
| 2026-06-05 | Neo4j Aura chosen for memory engine over pgvector — graph traversal and entity relationships are first-class requirements | bootstrap |

## Day-1 Priority

Memory Engine: see `.harness/docs/memory-engine-design.md`

## Tech Stack Summary

- Frontend: Next.js 15, TypeScript, Tailwind, Shadcn
- Backend: FastAPI, Python 3.11+, Poetry
- Databases: PostgreSQL (structured), Neo4j Aura (graph/memory)
- AI: Abstraction layer — MiniMax, OpenAI, Anthropic, Gemini via Azure AI Gateway
- Azure: Static Web Apps → API Management → Container Apps; Service Bus; Blob Storage