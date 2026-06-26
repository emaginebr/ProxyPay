# ProxyPay example-app — Redesign delivery index

Single consumption point for `frontend-react-developer` and any collaborator. All artifacts produced for the light-theme redesign of `example-app/`.

## Design directions (named)

| Surface | Direction | Rationale |
|---|---|---|
| Landing (`/`, `/docs`, `/demo/*`) | **Editorial Fintech + Bento Grid** | Asymmetric hero, expressive display type, bento arranges features with visual hierarchy, subtle glass + radial-aurora gradients convey trust + movement. Avoids generic "hero + 3 features + CTA". |
| Dashboard (`/admin/*`) | **Informational Minimalism** | White surfaces, subtle borders, high-contrast text, tabular-nums for money, color used sparingly only to carry status. Hero metric card reintroduces the brand gradient as the room's focal point. |
| Auth (`/login`, `/signup`) | **Centered content card on aurora** | Narrow 400 px card with brand mark, inherits the same tokens. |

## Palette

- **Primary** — Indigo 600 (`#4F46E5`) — trust, authority, financial seriousness.
- **Accent** — Emerald 500 (`#10B981`) — money, success, "paid" state.
- **Highlight** — Amber 400/500 (`#FBBF24` / `#F59E0B`) — attention, pending, warmth.
- **Neutrals** — Slate 50→950 — information hierarchy.
- **Feedback** — red-600 (danger), amber-600 (warning), emerald-600 (success), sky-600 (info).

All pairs verified to meet WCAG AA (text ≥ 4.5:1 on its designated surface).

## Typography pairing

- **Display** — Space Grotesk (500/600/700) — geometric, modern, editorial without being cold. Matches fintech tone.
- **Body / UI** — Inter (400/500/600/700) — battle-tested legibility at all sizes, excellent for data-dense tables.
- **Mono** — JetBrains Mono — code snippets, IDs, amounts in tables.

Scale: `12 / 13 / 15 / 16 / 18 / 20 / 24 / 30 / 36 / 48 / 60`, display uses `clamp(2.5rem, 6vw + 1rem, 4.5rem)` for the hero.

## Priority ladder applied

1. **Accessibility** — contrast, focus rings, color-not-alone (dotted badges), reduced-motion media query in tokens.
2. **Touch** — buttons ≥ 40 px, spacing ≥ 8 px between targets.
3. **Performance** — no layout-shift on async (reserved space for metric delta rows, skeleton for tables/chart).
4. **Style** — consistent tokens, Lucide SVG icons only (no emojis).
5. **Layout** — mobile-first breakpoints at 640/768/1024/1280.
6. **Typography/Color** — semantic tokens only in components.
7. **Animation** — 150–320 ms, `ease-out`, spring on press feedback.
8. **Forms** — inline validation, visible labels, helper text, focus management.
9. **Navigation** — sidebar + breadcrumb + predictable back, ⌘K command palette planned.
10. **Charts** — tooltip, legend, axis labels, accessible line color, empty/loading states defined.

## Artifacts

| Path (relative to this README) | Purpose | Skill of origin |
|---|---|---|
| `README.md` | This index | ui-ux-pro-max |
| `tokens.css` | Full 3-layer token system (CSS variables) | design-system |
| `tailwind.config.extend.js` | Drop-in `theme.extend` block (colors, fontFamily, fontSize, radius, shadow, keyframes, screens) | design-system |
| `landing.html` | Full landing page mockup (Hero + Trust + Bento + Install + Walkthrough + Stats + CTA + Footer) | ui-ux-pro-max + ui-styling |
| `dashboard.html` | Dashboard shell + home page mockup (sidebar + topbar + metrics + chart panel + activity + table) | ui-ux-pro-max + ui-styling |
| `component-specs.md` | Component-by-component specs: purpose, shadcn parts, variants, states, tokens, a11y. The source the React developer reads. | ui-styling + ui-ux-pro-max |

## How to consume (for `frontend-react-developer`)

1. **Install tokens** — copy `tokens.css` contents into `example-app/src/index.css` (replace the current `:root` block). Import `Space Grotesk`, `Inter`, `JetBrains Mono` via `<link>` in `index.html` or `@import` at the top of `index.css`.
2. **Extend Tailwind** — merge the `themeExtend` export from `tailwind.config.extend.js` into `example-app/tailwind.config.js` under `theme.extend`.
3. **Install shadcn/ui** — `npx shadcn@latest init` at `example-app/`, pick `Inter` as font, `Neutral` as base, `primary` with Indigo (we override via CSS vars anyway). Then add the components listed in `component-specs.md` §per surface:
   - Global: `button`, `card`, `badge`, `input`, `label`, `form`, `tabs`, `dialog`, `sheet`, `dropdown-menu`, `tooltip`, `toast` (sonner), `separator`, `skeleton`, `avatar`, `breadcrumb`, `scroll-area`, `command`.
   - Table pages: `table`, `pagination`, `toggle-group`, `select`, `popover`, `checkbox`.
   - Forms (Store, Login, Signup): `form`, `input`, `select`, `alert`.
4. **Map each existing page** following the table in `component-specs.md` §13. Preserve the existing types → services → contexts → hooks → pages architecture; only rewrite the JSX/className layer.
5. **Verify** against `component-specs.md` §15 accessibility audit before merging.

## Stack discrepancy notice

The current `example-app` already uses **Vite + React 19 + Tailwind 3.4 + tailwindcss-animate**, which matches the Vite-first default of this agent. No adapter required.

However, shadcn/ui is not yet installed. You have two options:

- **(A) Install shadcn/ui** — adds Radix UI primitives + the component generator. Adds ~40 KB to the dep tree but gives us accessible primitives out of the box. **Recommended.**
- **(B) Hand-roll** — keep zero deps, build on Radix directly or pure HTML. More work, more risk on a11y regressions.

Go with (A) unless the maintainer explicitly wants zero new dependencies in `example-app/`.

## Out of scope (deferred to siblings)

- Writing `.tsx` → handed off to `frontend-react-developer`.
- Modifying `src/` of the library itself (the redesign is exclusively for `example-app/`).
- Translating copy from PT-BR to EN (login page is already in PT; other pages use EN — unify in a follow-up with `react-i18n`).
- Backend/API changes → `dotnet-senior-developer`.

## Next (recommended)

1. Open `landing.html` and `dashboard.html` in a browser to sanity-check the visual direction before coding.
2. Approve or request adjustments (palette hue shift, bento layout permutations, dashboard hero-card density).
3. Hand off to `frontend-react-developer` with this folder as the single source of truth.
