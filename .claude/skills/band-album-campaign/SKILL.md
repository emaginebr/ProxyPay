---
name: band-album-campaign
description: Domain pack for music/band album launches — album-as-project organized by track, chronological track-folder allocation, waterfall release cadence with anchor dates, teasers per track, music-specific reuse caps, and the Faixa post field. Use when the social-media campaign is a band album launch. Built on social-post-authoring + social-media-scheduling + social-media-assets.
allowed-tools: Read, Write, Edit, Glob, Grep
---

# Band Album Campaign

Domain pack that turns the generic social-media skills into a **band album launch**
campaign. It defines the **folder layout**, the **release cadence**, and the
**music-specific overrides** of the generic mechanics. The generic skills still
own per-post authoring (`social-post-authoring`), scheduling/state
(`social-media-scheduling`) and media/reuse mechanics (`social-media-assets`) —
this pack supplies the values and structure they expect.

> Literal artifact tokens written into files (the `Faixa:` field, folder/section
> labels) are kept verbatim because the generated posts target a PT-BR audience.
> The documentation is in English.

## Album = project, organized by track

Each album lives in `social-media/albuns/<album-slug>/`. At the **album root** sit
`plano.md`, `materiais.md` and a **thin** `cronograma.md` master/index. **Each
track of the album gets its own directory** at
`albuns/<album-slug>/<NN-track-slug>/` (e.g. `albuns/genesis/02-prefiro-valhalla/`)
containing:

- `cronograma.md` — the dated table for that track, pendency list, embedded
  Mermaid Gantt of that track only (the **by-date** view).
- `agenda.md` — index of the track's posts **grouped by network** (the by-network
  view; content/columns defined by `social-media-scheduling`).
- `timeline.mmd` — the Gantt source of that track (render with `mermaid-chart`).
- `posts/AAAA-MM-DD/<network>.md` — every post that primarily promotes that track,
  organized by date and network.

```
social-media/albuns/genesis/
├── plano.md
├── materiais.md
├── cronograma.md                  (master/index)
├── 01-dragao-na-garagem/
│   ├── cronograma.md
│   ├── agenda.md
│   ├── timeline.mmd
│   └── posts/AAAA-MM-DD/<network>.md
└── 02-prefiro-valhalla/
    ├── cronograma.md
    ├── agenda.md
    ├── timeline.mmd
    └── posts/AAAA-MM-DD/<network>.md
```

The master `cronograma.md` is **thin**: cross-track waterfall overview, links to
each per-track `cronograma.md`, consolidated blocker list, and cross-track context.
**Do not** duplicate per-track tables/pendency-lists/Gantts in the master.

### Post-format override

In the generic post format from `social-post-authoring`, the `Item:` field becomes
**`Faixa:`** (`Faixa: <faixa|Geral>`). `Faixa:` describes the **thematic content**
(which track the post promotes) and does **not** determine the folder — placement
is purely temporal (below).

## Chronological allocation rule (by next-to-launch track)

A post lives in the directory of the **next track to launch as of the post's date**
(the track being warmed up). The folder `<NN-track-slug>/posts/` contains
everything in that track's warm-up window, **including** its own launch-day post
(the **last** post in the folder) and any catalog-reinforcement posts of the
**previous** track in the same window.

Given launch dates `L1 < L2 < L3 < ...` (from `plano.md`):
- Posts dated `L(N-1) < d ≤ LN` → `<NN-track-slug>/posts/<d>/`.
- The very first track (no previous launch): only its own launch and
  pendente-streaming live in `<01-track-1-slug>/posts/`.

Intuition: when a track launches on YouTube, the folder "becomes" the next track's
folder going forward — so the next track's slug names the directory that holds its
warm-up campaign.

## Waterfall release — cadence and anchors

A track is released every **4 weeks (28 days)** on **Saturdays only** — this applies
to launches **at and after 2026-08-01 (Faixa 3)**. Faixa 1 launched before this
calendar; Faixa 2 launched **2026-07-04** under the older 6-week cadence. From
Faixa 3 onward, launches are always Saturdays every 28 days:

| Track | Launch anchor |
|---|---|
| F2 | 2026-07-04 |
| F3 | 2026-08-01 |
| F4 | 2026-08-29 |
| F5 | 2026-09-26 |
| F6 | 2026-10-24 |
| F7 | 2026-11-21 |
| F8 | 2026-12-19 |
| F9 | 2027-01-16 |
| F10 | 2027-02-13 |

Record dates relative to an **anchor supplied by the user** (in `plano.md`); do not
invent anchors. If the user changes the cadence or the launch day, update this
table.

### Post cadence — transition at 2026-07-04

Produce **1 post every 2 days before 2026-07-04** (inclusive of 07-04), then
**daily from 2026-07-05 onward**. Same subject across all networks, adapted per
network via the matrix in `social-post-authoring`.

### Teasers

**3 teasers per track**, published before that track's release.

## Music reuse caps (for `social-media-assets`)

The reuse policy is applied by `social-media-assets`; this pack supplies the values
and the concrete folders (mirror them in the cap table in `config.md`):

| Source folder | Cap per network | Beyond cap |
|---|---|---|
| `videos/off/*` (backstage — vocals moved out 2026-06-05) | **1 use** | **Forbidden** — do not reuse; swap for a different material |
| `videos/vocals/*` (vocal-only excerpt/reflection videos) | **1 use** (default — user-adjustable) | **Forbidden** by default (same treatment as `videos/off/*`) |
| `videos/shorts/*` (excerpt shorts: teasers/catalog) | **2 uses** | **3rd use onward** → `⚠️ FALTANTE não-bloqueante` |
| `imagens/*` (album/track art, photos) | as needed (art is structural) | mark `⚠️ FALTANTE não-bloqueante` when reuse forces same-material consecutively on the same network |
| `imagens/off/*` | **1 use** | same as `videos/off/*` (forbidden beyond cap) |

## Band facts

Read identity/tracklist/links/handles from the facts document named in `config.md`
(e.g. `docs/README.md`). Track lyrics in `musicas/letras/<track>.md` (used by the
`band-music-reflection` skill).

## Sibling skills/command of this pack

- `band-music-reflection` — the "reflexão sobre a música" post type (gap-filling
  default).
- `band-streaming-launch` — dateless streaming-launch posts.
- `/social-media-album-cycle` — command that stamps the fixed 28-post launch cycle.
