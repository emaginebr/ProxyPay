---
name: band-music-reflection
description: Domain post type for band album campaigns — "reflexão sobre a música", a gap-filling post whose hook is derived from a track's lyrics and whose media is a vocal-only excerpt video. Use when filling an empty cadence slot in a band album campaign. Part of the band-album-campaign pack.
allowed-tools: Read, Write, Edit, Glob, Grep
---

# Band Music Reflection — "reflexão sobre a música"

Post type used as the **default gap-filler** in a band album campaign (an empty
cadence slot / `falta-conteudo` placeholder). It belongs to the
`band-album-campaign` pack and rides on the generic authoring/scheduling/assets
skills. Full rule text may also be mirrored in `social-media/config.md` → "Tipo de
post 'reflexão sobre a música'".

> The post-type name and on-disk tokens are kept verbatim (PT-BR audience); the
> documentation is in English.

## Rule (hard)

When filling a gap date, the default post type is **"reflexão sobre a música"**: a
discussion/provocation whose subject is **derived from the lyrics of the track that
owns that period** (the track naming the `<NN-slug>/posts/` folder where the gap
lives, per the chronological allocation rule in `band-album-campaign`).

- The band has a critical/provocative stance on faith, religion and dogma; the
  reflection pulls debate from a real line/theme of **that** track.
- **Never invent a stance the lyrics do not support** — ground the hook in what the
  lyric actually says (verify against `musicas/letras/<track>.md`).
- Each reflection belongs to ONE track and derives from THAT track's lyrics.
- **The subject is a PROPOSAL** and may be adjusted by the user at any time.

## Copy and artifacts

- **The reflection copy is the caption (`## Legenda`) only:** the band's
  provocative/reflective voice, pulling debate, **ending with an open question to
  the audience**; plain text, no Markdown (per `social-post-authoring`).
- **Do NOT** embed a `## Prompt de imagem` or `## Script de vídeo` block — those
  artifacts are no longer produced for this type.

## Media — vocal-only excerpt video

The deliverable is a **vocal-only video of the exact lyric excerpt** the reflection
is about — a short carrying the audio/vocal of the specific verse/excerpt (no
instrumental concept video, no image). Because it is an **excerpt short of the
content itself** (the song's own vocal), it qualifies under the "YouTube only takes
long-form + excerpt shorts" rule — so **YouTube is NO LONGER `cancelado` for
reflection**: it receives a YT Short of the vocal (`youtube.md` active).

Files in **`videos/vocals/<NN-slug>/`** (dedicated vocal-only folder — vocals were
moved out of `videos/off/` on 2026-06-05; `videos/off/` is now backstage only):
- 9:16 master `videos/vocals/<NN-slug>/reflexao-<gancho>.mp4` (or the real vocal-
  clip name, e.g. `videos/vocals/03-homem-de-barro/homem-de-barro-vocals-01.mp4`)
- 4:5 companion `-4x5.mp4` for IG Reels.

The post must state in its description **which verse/excerpt of the lyric the vocal
must contain**. Mark `Material:` FALTANTE (non-blocking while the date is distant;
⛔ if the date arrives without media) and log it in `materiais.md` → "A produzir /
requisitar" (via `social-media-assets`).

## Per-network distribution

Distributes as an excerpt short: **YouTube Short + TikTok + X** share the 9:16
master; **Instagram** uses the 4:5 companion (Reels); **Threads** mirrors IG.
