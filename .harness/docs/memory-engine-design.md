# Memory Engine Design

> Day-1 technical specification. Owned by `memory-engineer`. Subject to revision as the schema evolves.

## Overview

The Memory Engine is the core of Takumi OS — a graph-native knowledge store built on Neo4j Aura. Every piece of information ingested by the platform is represented as a **memory atom** with typed relationships, enabling traversal, inference, and executive reasoning.

## Graph Schema

### Node Types

| Label | Description | Key Properties |
|---|---|---|
| `MemoryAtom` | Core unit of knowledge — a single fact, event, or insight | `id`, `content`, `source`, `created_at`, `confidence`, `tags[]` |
| `Person` | Human entity | `id`, `name`, `email`, `role`, `department` |
| `Organization` | Company or team entity | `id`, `name`, `type`, `industry` |
| `Event` | A calendar meeting, action item, or organizational event | `id`, `title`, `start_time`, `end_time`, `type` |
| `Document` | File or transcript (Blob Storage reference) | `id`, `title`, `blob_url`, `mime_type`, `ingested_at` |
| `Concept` | Abstract concept node for semantic clustering | `id`, `label`, `embedding_id` |

### Relationship Types

| Type | From | To | Properties |
|---|---|---|---|
| `KNOWS` | Person | Person | `context`, `strength` |
| `MEMBER_OF` | Person | Organization | `role`, `since` |
| `PART_OF` | Event | Organization | — |
| `OCCURRED_AT` | Event | Event | `temporal_relation` |
| `REFERENCES` | MemoryAtom | Document \| MemoryAtom | `relevance_score` |
| `RELATED_TO` | MemoryAtom | Concept | `weight` |
| `CREATED_BY` | MemoryAtom \| Document | Person | — |
| `INGESTED_FROM` | MemoryAtom | Document | `extracted_at` |

## Cypher Patterns

### Create a memory atom with entity linking

```cypher
MERGE (a:MemoryAtom {id: $atom_id})
  ON CREATE SET a.content = $content,
                a.source = $source,
                a.created_at = datetime(),
                a.confidence = $confidence
WITH a
UNWIND $tags AS tag
  MERGE (c:Concept {label: tag})
  MERGE (a)-[:RELATED_TO {weight: 1.0}]->(c)
RETURN a
```

### Traverse from person to related memory atoms

```cypher
MATCH (p:Person {id: $person_id})-[:CREATED_BY|REFERENCES*1..3]->(a:MemoryAtom)
WHERE a.created_at >= datetime() - duration('P30D')
RETURN a ORDER BY a.created_at DESC
LIMIT 50
```

### Temporal event chain

```cypher
MATCH (e1:Event)-[:OCCURRED_AT]->(e2:Event)
WHERE e1.start_time >= datetime() - duration('P7D')
RETURN e1, e2 ORDER BY e1.start_time
```

## API Surface (backend/api/memory.py)

| Method | Path | Description |
|---|---|---|
| POST | `/memory/atoms` | Ingest a new memory atom |
| GET | `/memory/atoms/{id}` | Retrieve atom with direct relationships |
| GET | `/memory/atoms?person_id=&since=&limit=` | List atoms filtered by entity/time |
| POST | `/memory/search` | Semantic + graph hybrid search |
| GET | `/memory/graph?root_id=&depth=` | Knowledge graph traversal |
| POST | `/memory/entities` | Link a memory atom to persons/concepts |

## Event-Driven Ingestion (Azure Service Bus)

1. Source event arrives on `takumi.memory.ingest` queue
2. FastAPI worker picks up message, extracts structured payload
3. Memory engine creates/merges nodes via Cypher
4. Optionally triggers embedding generation via AI Gateway
5. Related `Concept` nodes updated with embedding reference

## Indexes

```cypher
CREATE INDEX memory_atom_created_at IF NOT EXISTS FOR (a:MemoryAtom) ON (a.created_at);
CREATE INDEX memory_atom_source IF NOT EXISTS FOR (a:MemoryAtom) ON (a.source);
CREATE INDEX person_email IF NOT EXISTS FOR (p:Person) ON (p.email);
CREATE INDEX concept_label IF NOT EXISTS FOR (c:Concept) ON (c.label);
```

## Seed Data

Run `backend/memory/seed.py` against Neo4j Aura to populate sample:
- 3 `Organization` nodes (your company + 2 partners)
- 5 `Person` nodes (executives)
- 10 `MemoryAtom` nodes with varied relationships
- 3 `Event` nodes forming a temporal chain