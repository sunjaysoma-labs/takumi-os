# AGENTS.md

Takumi OS — Organizational Intelligence Platform. Memory-first, event-driven, graph-centric executive reasoning engine. Next.js 15 frontend + FastAPI backend + Neo4j + PostgreSQL.

## Setup commands

- Install frontend deps: `pnpm install` (from `frontend/`)
- Install backend deps: `poetry install` (from `backend/`)
- Start dev (full stack): `pnpm dev` (frontend) + `uvicorn backend.main:app --reload` (backend)
- Build frontend: `pnpm build` (from `frontend/`)
- Build backend: `poetry run uvicorn backend.main:app --host 0.0.0.0 --port 8000`
- Test: `pnpm test` (frontend) + `poetry run pytest` (backend)
- Lint: `pnpm lint` (frontend) + `poetry run ruff check` (backend)
- Typecheck: `pnpm typecheck` (frontend) + `poetry run mypy backend`

## Project layout

- `frontend/` — Next.js 15 app (App Router, TypeScript, Tailwind, Shadcn)
- `backend/` — FastAPI modular monolith
- `packages/` — shared TypeScript types / Python models
- `docs/` — architecture and design docs
- `scripts/` — dev tooling and migrations

## Code style

- TypeScript strict mode (`tsconfig.json: strict: true`), single quotes, 100-char width, Prettier formatting
- Python: `ruff` for linting + formatting, `mypy` for type checking
- Run `pnpm lint:fix` / `poetry run ruff check --fix` before committing
- Conventional commits (`feat:` / `fix:` / `docs:` / `refactor:`)

## Testing instructions

- Frontend: Vitest (`pnpm test`) + Playwright E2E (`pnpm test:e2e`)
- Backend: pytest (`poetry run pytest`) — unit + integration
- All tests must pass before opening a PR

## PR & commit conventions

- Branch from `main`; never push to it directly
- Commit message: conventional commits
- Open PR once CI is green; assign to at least one reviewer

## Security

- Never commit secrets — `.env` files are in `.gitignore`
- All API keys via environment variables or Azure Key Vault
- Rate limiting and auth on all FastAPI endpoints