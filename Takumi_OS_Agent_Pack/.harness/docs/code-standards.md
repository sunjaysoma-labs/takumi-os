# Takumi OS — Code Standards

## TypeScript (apps/web/, packages/)

- `strict: true` in `tsconfig.json` — no implicit any, no implicit null
- Prefer `interface` over `type` for object shapes; `type` for unions
- No `any` — use `unknown` and narrow with type guards
- Component files: PascalCase (`MemoryExplorer.tsx`), hook files: camelCase prefixed `use`
- Shadcn UI components: do not modify generated files directly; extend via wrapper components in `components/ui/`

## Python (apps/api/, packages/memory/)

- Type hints on all function signatures
- Pydantic v2 models: `BaseModel` + `model_validator`, no `orm_mode`
- Async: use `async def` for all FastAPI route handlers and Neo4j calls
- Run `ruff check --fix` and `mypy .` before committing

## Git workflow

- Branch: `feat/memory-engine`, `fix/atom-search`, etc.
- Commit: conventional commits (feat, fix, docs, test, refactor, chore)
- PR: describe the spec section addressed, link to the relevant doc
- Never force-push `main`

## Test policy

- New behavior = new test. No exceptions.
- Unit test next to source: `MemoryEngine.ts` → `MemoryEngine.test.ts`
- Integration test for each API route
- E2E for critical flows only (login, memory create, graph explore, dashboard load)