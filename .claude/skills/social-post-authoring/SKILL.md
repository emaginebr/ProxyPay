---
name: social-post-authoring
description: Author a single social post per network with the correct format, aspect ratio, character limits, hashtags and plain-text caption. Use whenever writing or editing the copy/media of a post for YouTube, Instagram, TikTok, X.com or Threads. Domain-agnostic (works for any campaign).
allowed-tools: Read, Write, Edit, Glob, Grep
---

# Social Post Authoring

How to compose one social post per network so it is ready to copy-paste. This
skill is **platform mechanics only** — it knows nothing about the campaign's
domain (a band album, an online game, a product launch). It expects a campaign
project under `social-media/` with a `config.md` holding cross-cutting settings
(mandatory hashtags, X account tier, allowed media folders). Read `config.md`
before authoring.

> The literal on-disk tokens written into posts (status values, the `##` section
> headings, `FALTANTE`, field labels) are kept verbatim because the generated
> posts target a PT-BR audience. The documentation around them is in English.

## Post file format

One `.md` file **per network that receives the post on a given day**. Threads has
**no own file** — it is replicated from `instagram.md` (note "replicar no
Threads" in the Instagram file's `## Observações`). The `<network>` files are
`youtube.md`, `instagram.md`, `tiktok.md`, `x.md`.

```
# <Tema> — <Rede>
- Data: YYYY-MM-DD · Tipo: <tipo> · Item: <item|Geral>
- Formato: <vídeo long-form|reels|short|imagem|texto|enquete>
- Status: falta-conteudo | planejado | agendado | publicado | cancelado | aguardando
- Material: <path(s) or "FALTANTE — requisitar">

## Legenda
<final copy in plain text, with emojis + CTA>

## Hashtags (máx. 5)
<#... — at most 5, lowercase>

## Observações
<e.g. replicar no Threads>
```

The `Item:` field names the campaign item the post primarily promotes (a domain
pack may rename it — e.g. a music album pack uses `Faixa:`). The status values and
lifecycle are owned by the `social-media-scheduling` skill.

## Network × post-type matrix (hard rule)

| Network | Receives | Format |
|---|---|---|
| YouTube | **only** full long-form videos + excerpt shorts of the content itself (teaser/catalog) | long-form 16:9 / Short 9:16 (excerpt) — **no image posts; no info/announcement/backstage/countdown/concept/poll/eve shorts** |
| Instagram | short videos (Reels = **video only**) + image posts (feed 1:1 or carousel) | Reels 4:5 video / feed 1:1 image — **no long-form videos**, **no photo slideshow as Reels** |
| TikTok | long-form + short videos — **video only**, no photo carousel/slideshow; **no polls** | long-form 16:9 (same file as YouTube/X) / short 9:16 |
| X.com | long-form + short videos + image/text | video 16:9 / short 9:16 / image 16:9 / text |
| Threads | mirrors Instagram | replicate the Instagram content |

### Video delivery rule (same content, distinct deliverables per format)

| Content type | Format | Networks (where it goes) |
|---|---|---|
| Full long-form video | **16:9** | YouTube (full video) + X.com + TikTok |
| Short — content excerpt | **9:16** | YouTube Short + TikTok + X.com |
| Short — content excerpt | **4:5** (companion) | Instagram Reels |
| Short — announcement / info / backstage / countdown / concept / eve | **9:16** | TikTok + X.com (**NOT** YouTube — YT only takes excerpt/long-form content) |
| Short — announcement etc. | **4:5** (companion) | Instagram Reels |

Rules of thumb:
- **Full long-form videos are a single 16:9 deliverable** that fans out to
  **YouTube + X.com + TikTok** — the same file is reused across the three posts.
  **No separate 9:16 cut is produced**; TikTok accepts the 16:9 master directly.
  Instagram **never** receives long-form videos.
- **Shorts need TWO companion deliverables for the same content:** a **9:16**
  master that goes to YouTube Short + TikTok + X.com (3 posts share one file), and
  a **4:5** variant that goes to Instagram Reels (Reels does not accept 9:16 in
  our pipeline). File-naming convention: the 9:16 master uses the bare name (e.g.
  `<slug>-intro.mp4`) and the 4:5 companion uses the `-4x5` suffix on the same
  stem (`<slug>-intro-4x5.mp4`), stored side-by-side in the same folder. When you
  assign a short to a post, pick the file matching the network's required aspect —
  **never** assign the 9:16 file to an Instagram Reels slot.
- The 16:9 format is the **exception**, reserved for full long-form videos
  (YouTube + X + TikTok). Everything else short-form is 9:16 (YT Short / TikTok /
  X) and 4:5 (IG Reels).
- **Polls — X.com only (native poll). Instagram receives a sibling
  informational post, not a poll.** On a poll date:
  - **X.com**: native poll **with an image** (not a video) — 16:9 visual support
    (the item's art or a cover).
  - **Instagram**: a regular **feed post (1:1)** on the same theme, written as
    **informational** copy — present the question/concept and invite the audience
    to answer in the comments, without claiming there is a native poll. **Do not**
    use Story 9:16.
  - **Threads**: mirrors the Instagram informational post.
  - **YouTube** and **TikTok**: **excluded** on poll dates — do not create
    `tiktok.md` / `youtube.md`.
- **Instagram Reels and TikTok are video only — no photo slideshow.** If the post
  idea is built from static photos (backstage etc.):
  - **Instagram** → **feed post** (single image 1:1 or carousel). Mark `Formato:`
    as `post (carrossel)` or `post (imagem)`, not `reels`.
  - **TikTok** → a **`.mp4` produced from the photos** (montage / sequence edit).
    Mark the source photos as input and create a **FALTANTE** entry for the video.
  - **YouTube Short** → confirm case by case; by default, same as TikTok.
  - **X.com** → accepts video and image; can use the 16:9 still directly or the
    `.mp4` montage.

### Image aspect-ratio rule (hard)

| Network | Format | Aspect ratio |
|---|---|---|
| Instagram | image / feed post | **1:1** |
| Instagram | Story | **9:16** |
| X.com | image | **16:9** |
| Threads | mirrors Instagram | 1:1 (feed) / 9:16 (story) |

YouTube and TikTok generally don't take image-only posts. The same content may
need **one variant per ratio** (e.g. 1:1 for an IG feed post and 16:9 for X).
Aspect ratio is a fact about the file on disk: vertical ≈ 9:16, landscape ≈ 16:9,
square = 1:1 — judge by orientation, not exact pixels. If an asset is in the wrong
bucket for its slot, swap it for a compliant one; if none exists, mark `Material:`
as **FALTANTE** requesting the correct ratio. You cannot probe pixel dimensions —
when the ratio is not clear from the filename/inventory, request a probe rather
than assuming.

## Post types (generic)

Full item (long-form); item-release announcement; teasers; general info; per-
network release dates; "now on channel/platform X"; polls; and any other creative
angle that fits. **Domain packs may add their own types** (e.g. a music pack adds
"reflexão sobre a música").

## Hashtags — max 5, always lowercase (all networks)

Never more than **5 hashtags** per post; keep them relevant to the campaign.
**All hashtags lowercase on every network** — no CamelCase, even for proper nouns.

- **Instagram, Threads, TikTok and X.com — hashtags inline with the caption.**
  These networks have a single caption/tweet field; append the hashtag line to the
  bottom of the `## Legenda` section (after a blank line), not in an isolated
  section. Keep the `## Hashtags` heading only as a documentation pointer
  ("incluídas na legenda — veja acima").
- **YouTube — hashtags inline with the TITLE, not with the description.** YouTube
  splits a video into a **title** (own field, **100-character** cap) and a
  **description**. Hashtags live in the **title**. So `youtube.md` has a
  `## Título (máx. 100 caracteres)` section right above `## Legenda`, with the
  title text followed by a blank line and the hashtag line. The `## Legenda`
  (description) **must not** include hashtags.
- **Mandatory hashtags (always-on).** `config.md` holds a "Hashtags obrigatórias"
  list — these tags **must appear in every post**, on every network. Read it at
  the start of the session; they count toward the 5-tag cap; only the remaining
  slots are free. Never duplicate a mandatory tag with a synonym. Do not hardcode
  the list here — the user edits it in `config.md`.

## Character limits

- **X.com — 280.** The full post body on X (caption + line breaks + hashtags) must
  fit in **280 characters**. Count before saving; if it exceeds, shorten the
  caption (not the hashtags) and re-count. Add `## Contagem\n<n>/280 caracteres`
  to the bottom of every `x.md`. If the user confirms an X Premium account, the
  limit can be raised.
- **YouTube title — 100 characters.** The `## Título (máx. 100 caracteres)`
  section includes title + hashtag line (separated by a blank line) and the sum
  (counted as one block) must fit in 100. Count before saving; if it exceeds,
  shorten the title first. Add `## Contagem Título\n<n>/100 caracteres`.
  Convention: each printable character = 1, the `\n` between title and hashtags =
  1, emoji = 2. The `## Legenda` (description) has no cap at our scale.
- **YouTube description — `<` and `>` are forbidden.** YouTube does not accept `<`
  and `>` in the description; replace with the closest readable equivalent (`>` →
  `→`/`:`/"para"/"vai a"; `<` → `←`/"vem de"/"menor que"; `>>` → `→`/`—`). This
  rule applies to YouTube only.

## Caption is plain text — no Markdown

The `## Legenda` is literally pasted into the network, which does **not** render
Markdown. Write **plain text only**: no bold/italics/inline-code/headings/Markdown
links (write the bare URL)/Markdown bullets (use `•` or line breaks). **Emojis are
allowed and encouraged.** For emphasis use voice and punctuation (CAPS on a single
word, em-dash, ellipsis…) or emojis. The rule applies to the caption only — the
rest of the `.md` (frontmatter, `## Observações`, `## Contagem`) keeps Markdown as
internal documentation that is never published.

## Standard PT-BR — no gender-neutral language

Write captions in standard PT-BR with conventional masculine/feminine endings; **do
not use neutral forms**. Forbidden: neutral `-e` endings (`bem-vinde`, `todes`,
`amigues`), neutral pronouns/articles (`elu`, `ile`, `delu`), `@`/`x` inside words
(`tod@s`, `todxs`) and any invented neutral morphology. Use the masculine as the
generic for a mixed audience (`bem-vindo`, `todos`), unless the post addresses a
feminine subject. Applies to the caption; internal documentation follows the same
convention.

## Backstage privacy — no personal or location details

When the post is `bastidores` (process/making-of), the caption must **not** include
personal information or location details: no civil/full real name that differs from
the public name, no home address/neighborhood/city, no studio/venue/street name, no
schedule that exposes a recurring physical location. Keep the copy abstract about
*where* and *who in real life* — talk about the content, the process, the mood.

**Public/artistic names ARE allowed and encouraged.** The project/brand name and
each person's **public/artistic** name (persona) may be used freely in the caption
and as hashtags — these are public personas, not personal info. Only strip actual
personal details (real civil name if different, address, location, schedule) and
rewrite to preserve the public identity. Applies to every network's caption;
internal Observações may keep production notes, never copied into the caption.
