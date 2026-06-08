# AGENTS.md — Takumi OS

Organizational Intelligence Platform: memory atoms, graph relationships, and executive reasoning.
Stack: Next.js 15 + FastAPI monorepo, Neo4j + PostgreSQL, Azure deployment, MiniMax AI.

## Setup commands

- Install deps: `pnpm install` (workspace root)
- Start dev: `pnpm dev` (all apps), `pnpm dev --filter web` (frontend only), `pnpm dev --filter api` (backend only)
- Build: `pnpm build`
- Test: `pnpm test`
- Lint: `pnpm lint`
- Typecheck: `pnpm typecheck`

## Project layout

- `apps/web/` — Next.js 15 frontend (React Server Components, Tailwind, Shadcn)
- `apps/api/` — FastAPI modular monolith (Python 3.12+)
- `packages/memory/` — Memory Engine core: atom CRUD, graph traversal, event bus
- `packages/shared/` — Shared TypeScript types and Python schemas
- `packages/ai/` — AI Provider abstraction (MiniMax, OpenAI, Anthropic, Gemini)
- `docs/` — Engineering specs, UX specs, architecture docs

## Code style

- TypeScript: strict mode, no implicit any, prefer `unknown` over `any`
- Python: type hints everywhere, Pydantic v2 models, async-first
- Prettier: single quotes, 100-char width, no semicolons in TypeScript
- ESLint flat config, Ruff for Python
- Run `pnpm lint:fix` / `ruff check --fix` before committing

## Testing instructions

- Unit tests: Vitest (frontend), pytest (backend)
- E2E tests: Playwright (CX)
- All tests must pass before opening a PR
- Memory Engine has its own integration test suite (see `packages/memory/`)

## PR & commit conventions

- Branch from `main`; never push to it directly
- Commit message: conventional commits (`feat:` / `fix:` / `docs:` / `refactor:` / `test:`)
- Open PR via `gh pr create` once CI is green

## Security

- Never commit secrets — `.env` is in `.gitignore`, use Azure Key Vault for secrets at runtime
- API keys go in `.env.local` (frontend) and `.env` (backend) — both gitignored
- All AI Provider calls go through `packages/ai/` abstraction, never raw SDK in app code

## Core modules (in build order)

1. **Memory Engine** — Day 1. Memory atoms, Neo4j graph, event-driven ingestion.
2. **Dashboard** — Executive overview screen.
3. **Agents** — Agentic task execution layer.
4. **Risk Center** — Risk monitoring and alerting.
5. **Intelligence Hub** — AI-powered insights and reasoning.

## AI Provider

Primary: MiniMax via `packages/ai/`. Abstract interface supports OpenAI / Anthropic / Gemini as fallbacks.
Configure via `AI_PROVIDER` / `AI_API_KEY` env vars.