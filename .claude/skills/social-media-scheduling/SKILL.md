---
name: social-media-scheduling
description: Manage the scheduling state of a social-media campaign — post status lifecycle, per-network pre-schedule windows and frontiers, and keeping the cronograma/agenda derived views in sync with the posts. Use when scheduling, re-scheduling, or reconciling post status. Domain-agnostic.
allowed-tools: Read, Write, Edit, Glob, Grep
---

# Social Media Scheduling

Owns the **time/state** layer of a campaign under `social-media/`: the post status
lifecycle, the per-network scheduling windows, and the synchronization between the
posts (single source of truth) and their **derived views** (`cronograma.md` /
`agenda.md`). It is platform mechanics only — the folder tree of a specific
campaign is defined by the domain pack (e.g. an album pack), but the status
semantics and the derived-view sync rules here apply to any campaign.

> The literal status tokens and artifact section labels are kept verbatim (the
> generated files target a PT-BR audience). The documentation is in English.

## Status lifecycle — 6 values

```
falta-conteudo → planejado → agendado → publicado     (happy path)
                 + cancelado (excluded by a rule)
                 + aguardando (posts with no date yet)
```

- `falta-conteudo` — the slot's date exists (slot reserved) but the post has **no
  theme/material/caption** defined yet. Frontmatter has the date and item, but
  `Tipo`, `Material`, `Legenda` and post-specific hashtags are "a definir". Counts
  as ⚠️ FALTANTE non-blocking in cronograma/agenda.
- `planejado` — ready in this repo (Material, copy, hashtags) but **not yet
  queued** in the network's composer.
- `agendado` — the user has queued the post in the network's composer; the network
  will publish on the scheduled date.
- `publicado` — the post is live.
- `cancelado` — excluded by some rule.
- `aguardando` — ready but waiting for a trigger external to the calendar (post
  with no fixed date). See the domain pack that defines dateless posts.

**Never publish.** After the user reports what was published or changed, update
each post's `Status` and the derived views.

## Per-network pre-schedule windows

Each network has its own rolling pre-queue limit. The calendar can plan posts far
into the future, but the actual queuing inside each platform's composer happens in
chunks as time progresses.

| Network | Pre-schedule window |
|---|---|
| Instagram (Meta Business Suite) | **29 days** |
| Threads (mirrors Instagram) | **29 days** (inherited) |
| TikTok | **30 days** |
| X.com | **no practical limit** — accepts > 30 days, track the frontier |
| YouTube | **no practical limit** — accepts long windows, track the frontier |

## Per-network scheduling frontier

- When the user reports "agendado no `<rede>` até `<data>`", record that
  **frontier** in `config.md` and in the master `cronograma.md`/`agenda.md`, **and**
  flip every active post on `<rede>` with date `≤ <data>` from `planejado` to
  `agendado`. The frontier is the date-level marker; `Status` is the per-post
  marker — they must stay in sync. Skip posts already `publicado`, `cancelado` or
  `aguardando`.
- When today's date moves forward and a frontier slips below the respective cutoff
  (today + window), surface a reminder so the user knows it's time to queue more
  posts on that network.
- For networks whose window is TBD, plan without a frontier until the user gives
  the constraint.

## Derived-view synchronization (hard rule)

The posts in `posts/` are the **single source of truth**. `cronograma.md` (the
**by-date** view) and `agenda.md` (the **by-network** view) are **derived views** —
they must never lag behind the post state. Whenever you create, edit, cancel or
re-allocate a post — or mark it `⚠️ FALTANTE não-bloqueante` — propagate the change
to:

- The "Pendência de mídia" column of that date's `cronograma.md`: the cell becomes
  `⚠️` if any post on the date is marked, `⛔` if any is blocking, `✅` only if
  **all** posts on the date are clean.
- The "Material" column of that post's `agenda.md` (✅ / ⚠️ / ⛔ summary).
- The aggregated counts and Pendência column of the master `cronograma.md`.

### `agenda.md` — content & maintenance

`agenda.md` is the index of posts **grouped by network**, each section listing the
posts chronologically with a clickable link to the post file (the by-network
scheduling view, used to queue platform by platform). Sections in order: YouTube,
Instagram, TikTok, X.com, Threads. Each section holds a chronological table with
columns: `Data`, `Tipo`, `Status` (planejado / agendado / publicado / cancelado /
aguardando), `Material` (short summary: ✅ ready / ⚠️ FALTANTE / ⛔ blocking), and
`Arquivo` (relative Markdown link to the `<rede>.md`). The Threads section lists
each date that has an `instagram.md` flagged "replicar no Threads", linking the
`instagram.md`. Cancelled dates go to a "Canceladas" sub-table at the end of the
network's section. Do not duplicate caption content — index rows only.

> The concrete folder layout (`albuns/<x>/<faixa>/...` vs another structure) and
> the master `cronograma.md` are defined by the campaign's **domain pack**. This
> skill only guarantees that, whatever the layout, the views stay in sync with the
> posts per the rules above.
