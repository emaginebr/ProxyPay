---
name: band-streaming-launch
description: Domain convention for band album campaigns — undated streaming-launch announcement posts (Spotify/Deezer/Apple Music) under posts/pendente-streaming/, with Status aguardando, promoted to a dated slot when the streaming goes live. Use for streaming-launch announcements in a band album campaign. Part of the band-album-campaign pack.
allowed-tools: Read, Write, Edit, Glob, Grep
---

# Band Streaming Launch — dateless posts

Convention for the subset of band album posts that **cannot be tied to a fixed
publication date** — most notably the **streaming-launch announcement** ("já
estamos no Spotify/Deezer/Apple Music"). Streaming platforms (Spotify, Deezer,
Apple Music) require a **2–7 day review window** and may release at any moment
within it, so the calendar must not commit to a single launch date. Part of the
`band-album-campaign` pack; uses the generic `social-media-scheduling` status
`aguardando`.

> On-disk tokens and section labels are kept verbatim (PT-BR audience); the
> documentation is in English.

## Convention

- The dateless post lives at `<NN-track-slug>/posts/pendente-streaming/<rede>.md`
  (one file per network; Threads still mirrors `instagram.md`).
- `Data:` = `pendente — aguardando publicação no streaming` (no concrete date).
- `Status: aguardando` (a status distinct from planejado/publicado/cancelado:
  "ready but waiting for a trigger external to the calendar").
- Each track's `pendente-streaming/` stays in that track's folder (it is about that
  track's streaming milestone, analogous to its launch).

## Where they appear in the derived views

- In `cronograma.md` (per-track and master), dateless posts go in a dedicated
  **"Pendente — sem data"** section, never inside the dated table or the Gantt.
- In `agenda.md` (per-track), dateless posts appear in a dedicated **"Pendente —
  sem data"** sub-section at the top of each network section (before the dated
  active table), so the user spots them when scheduling.

## Promotion to a dated slot

When the user reports the streaming actually went live, **promote** the file to a
dated slot: move `pendente-streaming/<rede>.md` to `<actual-date>/<rede>.md`, update
`Data:` and `Status: publicado`, and reconcile cronogramas/agenda (via
`social-media-scheduling`).

## Reusing the convention

The same convention can be reused for other genuinely dateless post types if they
emerge (e.g. "post for when we hit X views") — keep them under `pendente-<motivo>/`.
