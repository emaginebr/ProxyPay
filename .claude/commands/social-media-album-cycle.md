---
description: Stamp the fixed 28-post launch cycle of a band album track onto its posts folder, following the per-network distribution template and the mutability restriction (only planejado/falta-conteudo posts may change).
argument-hint: <album-slug> <NN-track-slug>
allowed-tools: Read, Write, Edit, Glob, Grep
---

# /social-media-album-cycle — Stamp a track's fixed 28-post launch cycle

Stamps the **fixed 28-post template** of a track's launch cycle onto
`social-media/albuns/<album-slug>/<NN-track-slug>/posts/`. Each launch cycle has
**exactly 28 days = 28 posts** (daily cadence from 2026-07-05; launches every 4
weeks, always Saturday). **1 cycle = 1 folder** `<NN-slug>/posts/` (chronological
allocation rule from `band-album-campaign`): folder `<NN>` covers track N's warm-up
and its launch.

> The 28-post template block and on-disk tokens (status values, `FALTANTE`, the
> `(faixa anterior)/(faixa atual)` markers) are kept **verbatim** because the
> generated posts target a PT-BR audience and the template must be preserved
> exactly. The surrounding documentation is in English.

## Input

`$ARGUMENTS` = `<album-slug> <NN-track-slug>` (e.g. `genesis 03-homem-de-barro`).

- Read the launch anchors from `albuns/<album-slug>/plano.md`: F2=2026-07-04,
  F3=2026-08-01, F4=2026-08-29, F5=2026-09-26, F6=2026-10-24, F7=2026-11-21,
  F8=2026-12-19, F9=2027-01-16, F10=2027-02-13.
- Determine track N (target) and track N-1 (previous).

## Cycle semantics (slots 1..28)

Folder `<NN>`'s cycle starts **on the day after track N-1's launch** (slot 1) and
ends **on track N's launch** (slot 28 = track N's anchor date):
- **"faixa atual" = track N** (names the folder, launched on slot 28).
- **"faixa anterior" = track N-1** (launched the day before slot 1).
- **slot K = the K-th day of the cycle** (slot 1 = day after the previous launch;
  slot 28 = track N's launch day).

## Execution

For each slot 1..28, materialize the `<network>.md` files in
`posts/<slot-date>/` per the template and the distribution notes below, delegating
post authoring to `social-post-authoring` and the domain post types to
`band-music-reflection` (reflection) and `band-album-campaign`. Sync
cronograma/agenda via `social-media-scheduling`.

### THE FIXED TEMPLATE (28 slots) — verbatim, preserving (faixa anterior)/(faixa atual)

```
01. Vídeo do refrão da música que acabou de ser lançada reforçando o lançamento no dia anterior (faixa anterior)
02. Reflexão da música anterior, vídeo apenas vocal com texto e texto de reflexão (faixa anterior)
03. Bastidores, imagens e vídeos da vocalista (faixa anterior)
04. Aviso de qual será a nova faixa, no IG e X capa da faixa e no YT Shorts e TikTok vídeo com capa e um pequeno trecho do refrão (faixa atual)
05. Vídeo com trecho de alguma faixa anterior (faixa anterior)
06. Bastidores, imagens e vídeos da vocalista (faixa atual)
07. Primeiro vídeo da faixa atual com uma introdução da faixa - trecho inicial da faixa (faixa atual)
08. Reflexão da música anterior, vídeo apenas vocal com texto e texto de reflexão (faixa anterior)
09. Enquete referente a faixa atual - apenas no IG e X, use algum material já existente (faixa atual)
10. Vídeo com trecho de alguma faixa anterior (faixa anterior)
11. Segundo vídeo da faixa atual com refrão da faixa (faixa atual)
12. Bastidores, imagens e vídeos da vocalista (faixa atual)
13. Vídeo com trecho de alguma faixa anterior (faixa anterior)
14. Reflexão da música atual, vídeo apenas vocal com texto e texto de reflexão (faixa atual)
15. Enquete referente a faixa atual - apenas no IG e X, use algum material já existente (faixa atual)
16. Bastidores, imagens e vídeos da vocalista (faixa atual)
17. Reflexão da música atual, vídeo apenas vocal com texto e texto de reflexão (faixa atual)
18. Vídeo com trecho de alguma faixa anterior (faixa anterior)
19. Repetir vídeo da faixa atual com intro da faixa - mude a legenda (faixa atual)
20. Terceiro vídeo da faixa atual com uma parte final da faixa (faixa atual)
21. Bastidores, imagens e vídeos da vocalista (faixa atual)
22. Reflexão da música atual, vídeo apenas vocal com texto e texto de reflexão (faixa atual)
23. Vídeo com trecho de alguma faixa anterior (faixa anterior)
24. Enquete referente a faixa atual - apenas no IG e X, use algum material já existente (faixa atual)
25. Reflexão da música anterior, vídeo apenas vocal com texto e texto de reflexão (faixa anterior)
26. Reflexão da música atual, vídeo apenas vocal com texto e texto de reflexão (faixa atual)
27. Aviso de lançamento de faixa amanhã, com vídeo do refrão (faixa atual)
28. Lançamento da Faixa — vídeo completo (clipe 16:9) apenas no YouTube; vídeo de capa no Instagram, TikTok e X (faixa atual)
```

### Per-network distribution notes (embedded in the template)

- **Slot 4 (new-track announcement):** IG and X = the current track's **cover**
  image; YouTube Short and TikTok = video with the cover + a short chorus excerpt.
- **Slots 9, 15, 24 (current-track poll):** **IG and X only** (X = native poll;
  IG = informational feed post per the poll rule in `social-post-authoring`).
  TikTok and YouTube stay **out** (do not create `tiktok.md`/`youtube.md`). **Reuse
  existing material** — do not requisition new media for a poll.
- **Reflection slots (2, 8, 14, 17, 22, 25, 26):** the "reflexão sobre a música"
  type (skill `band-music-reflection`): vocal-only excerpt video + caption ending
  in an open question; YT Short + TikTok + X share the 9:16 master + IG Reels 4:5 +
  Threads mirrors.
- **Backstage slots (3, 6, 12, 16, 21):** backstage type (vocalist images/videos),
  respecting privacy (no personal/location details — `social-post-authoring`) and
  the reuse cap of `videos/off`/`imagens/off` (1 use per network).
- **Excerpt slots (1, 5, 7, 10, 11, 13, 18, 19, 20, 23):** music excerpt shorts
  (intro/chorus/ending/catalog), 9:16 master → YT Short + TikTok + X + IG Reels 4:5
  companion. Slots 1 and 27 = chorus video; slot 7 = intro excerpt; slot 11 =
  chorus; slot 20 = ending; slot 19 = repeats the intro video (slot 7) with a
  different caption; slots 5/10/13/18/23 = an excerpt from a **previous** track
  (catalog).
- **Slot 28 (launch):** **full video (official 16:9 clip) on YouTube ONLY**; **cover
  video on Instagram, TikTok and X** (the current track).
  ⚠️ **Known divergence vs. the general rule:** the general full-track rule
  (`social-post-authoring`) sends the 16:9 clip to **YouTube + X + TikTok** (1 file
  → 3 networks). In this template, on the launch slot the full clip goes **only to
  YouTube**, while **Instagram, TikTok and X receive a cover video**. This
  distribution is specific to the template (slot 28) and **does NOT change the
  general rule** — it only documents the exception here. Do not edit `config.md`
  because of this, unless a config pointer explicitly mentions slot 28 and becomes
  inconsistent.

### Flexibility — reflection ↔ excerpt (explicit user permission)

Reflection posts **may be replaced by a music excerpt**, no problem. When a track
has **spare excerpt videos** (beyond the 3 standard teasers), those extra excerpts
**may occupy reflection slots** to avoid unnecessary FALTANTE media. User example:
"Homem de Barro has 5 excerpt videos (2 extra) and can replace reflection videos."
This is a **permission**, not an obligation.

## Application restriction (hard)

- **Only posts with status `planejado` or `falta-conteudo` may be changed.** Posts
  `publicado` or `agendado` are **untouchable** — skip them.
- The template **takes effect from the first cycle whose window is mutable** (after
  each network's scheduling frontier — `social-media-scheduling`).
- This application **runs only under explicit user instruction** for the named
  track — do not apply it to existing posts automatically.

## Reporting

At the end, print a concise summary:
- Album and target track, previous track, and each slot's date (1..28).
- Count of `<network>.md` files created vs. skipped (because `publicado`/`agendado`).
- List of slots marked `Material: FALTANTE` for requisition.
- Confirmation that the track's cronograma/agenda were synced.

## Boundaries

- **Never publishes** and **never creates media** — it only stamps the posts and
  requisitions missing media (via `social-media-assets`).
- Does not touch `publicado`/`agendado` posts.
- Specific to band album campaigns — depends on `band-album-campaign`.
