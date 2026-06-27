---
name: social-media-assets
description: Catalog, track usage, and request media for a social-media campaign — inventory in materiais.md, usage derived from posts, reuse caps from config, allowed source folders, and missing-media requisition. Use when assigning media to a post or requesting media that does not exist. Domain-agnostic.
allowed-tools: Read, Write, Edit, Glob, Grep
---

# Social Media Assets

Owns the **media** layer of a campaign under `social-media/`: what exists, what is
missing, where media may come from, how often a piece may be reused, and how to
request what is missing. The agent **never creates media** — the user produces all
media; this skill only catalogs, tracks usage, and requisitions.

> Literal artifact tokens (`materiais.md`, the `Material:` field, `FALTANTE`, the
> "A produzir / requisitar" section label) are kept verbatim; the generated files
> target a PT-BR audience. The documentation is in English.

## Inventory and usage derived from posts

- `materiais.md` is the inventory of what is **available** or **missing** per item
  — it does **not** store usage.
- **Usage is derived from the posts** (single source, no duplication): before
  assigning media to a new post, Grep the `Material:` lines across
  `**/posts/**/*.md` to see which clips/images were already used and where.
  **Never persist a usage log** — recompute it from the posts.
- Every post records the path of the media it uses in its `Material:` field.

## Allowed media sources (hard rule)

You may locate, assign, and reference media **only** from the folders listed under
"Pastas de mídia permitidas (fontes)" in `config.md` (each folder **including all
its subfolders**). That table is the single source of truth — **never hardcode**
the folder list here. When you need media, run Glob/Grep scoped to those paths. A
file outside the allowed folders **must not** appear in a `Material:` field; if a
post can only be satisfied by media outside them, treat it as **FALTANTE** and
requisition it. To change the sources, the user edits the table in `config.md`.

## Reuse policy (hard) — caps come from config

Each source folder has a **reuse cap per network**, defined in the cap table in
`config.md` (the domain pack populates the values and the concrete folders). This
skill applies the generic **mechanism** without hardcoding the values:

- **Cap "1 use" / forbidden beyond the cap (e.g. backstage, sensitive material).**
  Hard cap — there is no `⚠️ FALTANTE` fallback; reuse simply isn't allowed. If a
  2nd assignment on the same network is needed, swap it for a different file or
  mark the post as pending fresh material.
- **Cap "N uses" with fallback (e.g. shorts/excerpts).** Allows up to N distinct
  dates (same network) to share the material before any marker. From the (N+1)-th
  use onward, mark the post `⚠️ FALTANTE não-bloqueante` (material reused, fresh
  would be ideal) and propagate to cronograma/agenda (`social-media-scheduling`).
- **Structural material (e.g. art/cover).** Use as needed, but mark
  `⚠️ FALTANTE não-bloqueante` when reuse forces same-material on consecutive posts
  on the same network.

Cross-cutting rules:
- **Never repeat the same material in two consecutive dated posts on the same
  network** — even within the cap.
- The rule is **per network**: the same short can legitimately go to 4 networks on
  the same day (one content distribution, not reuse). "Reuse" = the same
  `Material:` path appearing on the **same network** on **different dates**.

## Missing-media requisition

When a post needs media that does not exist, record it in `materiais.md` under
"A produzir / requisitar" and **tell the user directly** what to produce (aspect
ratio, content, destination folder). The user creates all media. **Never** route
the request to `ux-designer` or any other agent, and never suggest doing so. Never
fabricate or assume media exists. Mark the post `Material: FALTANTE` (non-blocking
while the date is distant; ⛔ if the date arrives without media) and propagate to
cronograma/agenda.
