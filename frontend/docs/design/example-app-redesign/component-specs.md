# ProxyPay example-app — Component Specs

Consumption index for `frontend-react-developer`. Every component below is described by **purpose → shadcn parts to compose → variants → states → tokens used**. No `.tsx` here — this is the design spec.

Stack target: React + Vite + Tailwind + shadcn/ui (light theme only).

All tokens reference `tokens.css` (semantic layer preferred). Tailwind mappings live in `tailwind.config.extend.js`.

---

## 0. Priority Ladder Applied (non-negotiable)

1. **Contrast** — all text ≥ 4.5:1 on its surface (verified: `slate-900` on `#fff`, `slate-600` on `#fff`, `indigo-700` on `indigo-50`).
2. **Touch targets** — buttons ≥ 40×40 px (primary/secondary default `height: 40`), icon buttons 38×38 with visible hit area, row action buttons 28×28 BUT always inside a row that's ≥ 56 px tall so the effective tap target exceeds 44 px.
3. **Focus rings** — every interactive element uses `--focus-ring` (`0 0 0 2px #fff, 0 0 0 4px var(--border-focus)`) on `:focus-visible`. Never `outline: none` alone.
4. **Reduced motion** — `tokens.css` nullifies durations when `prefers-reduced-motion: reduce`. Pulse/float animations are turned off.
5. **Color-not-alone** — every status badge includes a leading dot + UPPERCASE label so colorblind users rely on text, not hue.

---

## 1. App Shell

### 1.1 Layout (authenticated)

- **Purpose**: Top-level frame for `/admin/*` routes. Fixed sidebar + sticky topbar + main content.
- **Compose from shadcn**: `Sheet` (mobile sidebar) + custom layout (shadcn has no Sidebar primitive as of shadcn v1).
- **Grid**: `grid-cols-[264px_1fr]` on `lg+`, stacks (sheet trigger in topbar) below `md`.
- **Tokens**: `--sidebar-width`, `--topbar-height`, `--topbar-bg`, `--topbar-blur`, `--surface-page`.

### 1.2 Sidebar

- **Purpose**: Primary navigation + store switcher.
- **Sections**: brand, store switcher, `Geral`, `Operações`, `Playground`, footer help card.
- **Compose from shadcn**: `DropdownMenu` for store switcher; `Tooltip` for icon-only mobile state; `ScrollArea` for overflow.
- **Item states**: default / hover / active / focus-visible / disabled.
  - default: `--sidebar-item-fg` on transparent
  - hover: `--sidebar-item-bg-hover`
  - active: `--sidebar-item-bg-active` + left 3px gradient bar (`--sidebar-item-indicator`) + text `--sidebar-item-fg-active`
  - focus: `--focus-ring`
- **Icons**: Lucide React (`stroke-width: 2`, size 18). Never emoji.
- **Badge inside item**: `.sb-item-badge` — small pill for counts (accent-soft bg).

### 1.3 Topbar

- **Purpose**: Breadcrumb + global search + notifications + user avatar.
- **Compose from shadcn**: `Breadcrumb`, `Input` (search), `Command` + `Dialog` (⌘K palette), `DropdownMenu` (user menu), `Tooltip`.
- **Behavior**: sticky, glass effect via `backdrop-filter: blur(12px) saturate(1.4)`.
- **Tokens**: `--topbar-bg`, `--topbar-border`, `--topbar-height`, `--topbar-blur`.

---

## 2. Buttons

### Variants
| Variant | Bg | Fg | Border | Shadow | Hover |
|---|---|---|---|---|---|
| primary | `--btn-primary-bg` | `#fff` | none | `--btn-primary-shadow` | `--btn-primary-bg-hover` + `--glow-primary` lift -1 |
| secondary | `--btn-secondary-bg` | `--text-primary` | `--btn-secondary-border` | none | `--slate-50` + `--border-strong` |
| ghost | transparent | `--text-secondary` | none | none | `--slate-100` + `--text-primary` |
| danger | `--btn-danger-bg` | `#fff` | none | sm | `--red-700` |

### Sizes
- `sm` → height 32, padding-x 14, font-size 12
- `md` (default) → height 40, padding-x 20, font-size 13
- `lg` → height 48, padding-x 24, font-size 15

### States
- `:hover` — color shift + subtle lift (primary only: `translateY(-1px)`)
- `:focus-visible` — `--focus-ring`
- `:active` — darken 1 shade, remove lift
- `[disabled]` — opacity 0.5, `cursor: not-allowed`
- loading — disabled + inline spinner (reuse existing `FullscreenLoading` spinner at 14 px)

### Compose from shadcn
- `Button` with custom `variant` and `size` props added to `button-variants`.

---

## 3. Card

### Variants
| Variant | Bg | Border | Shadow | Notes |
|---|---|---|---|---|
| surface (default) | `--card-bg` | `--card-border` | `--card-shadow` | standard content container |
| elevated | `--card-bg-elevated` | `--card-border` | `--shadow-md` | raised above siblings |
| glass | `--card-bg-glass` | `--indigo-100` | `--shadow-lg` + blur | hero floating cards only |
| dark-feature | `linear-gradient(135deg, indigo-950, indigo-700)` | none | `--shadow-lg` | bento PIX feature cell, metric hero |

- **Radius**: `--card-radius` (16 px).
- **Padding**: `--card-padding` (24 px).
- **Hover** (when interactive): border → `--card-border-hover`, shadow → `--card-shadow-hover`, `translateY(-2px)`.
- **Compose from shadcn**: `Card`, `CardHeader`, `CardTitle`, `CardDescription`, `CardContent`, `CardFooter`.

---

## 4. Metric Card (Dashboard)

- **Purpose**: Single-value KPI with optional delta and icon.
- **Variants**:
  - `default` — white surface, icon top-right, delta pill bottom-left.
  - `hero` — dark gradient, large value, primary CTA inside.
- **Structure**:
  - Header row: label (uppercase 12 px, tracking-wider) + icon chip (32×32, soft bg).
  - Value: `font-display`, 30 px (60 px for hero), tabular-nums, letter-spacing -0.03em.
  - Footer: delta pill (`up` = `emerald-50/600`, `down` = `red-50/600`) + caption.
- **Tokens**: `--metric-*` family.
- **Compose from shadcn**: `Card` + `Badge`.

---

## 5. Badge (Status Pill)

Invariant: **dot + UPPERCASE label** (color-not-alone rule).

| Variant | Bg | Fg | Use case |
|---|---|---|---|
| success | `--badge-success-bg` | `--badge-success-fg` | Pago |
| warning | `--badge-warning-bg` | `--badge-warning-fg` | Pendente |
| danger | `--badge-danger-bg` | `--badge-danger-fg` | Expirada / Falha |
| info | `--badge-info-bg` | `--badge-info-fg` | Boleto |
| brand | `--badge-brand-bg` | `--badge-brand-fg` | PIX |
| neutral | `--badge-neutral-bg` | `--badge-neutral-fg` | Recorrência, draft |

- Height 20 px, padding 2×12, radius full, font 11 px SemiBold, uppercase, tracking `0.04em`.
- `.badge-no-dot` modifier to remove the leading dot (e.g., method tags in table).
- **Compose from shadcn**: `Badge` with extended `variant` prop.

---

## 6. Input / Select / Form Field

- **Input**: height 40, padding 10×14, border `--input-border`, radius `--input-radius`, font `--fs-base`.
- **Focus**: border `--input-border-focus` + ring `--input-ring-focus`. Never remove focus outline.
- **Disabled**: bg `--input-bg-disabled`, cursor not-allowed, fg `--text-muted`.
- **Label**: 12.5 px medium, `--text-tertiary`, `display: block`, margin-bottom 6 px. Placeholder is never the sole label.
- **Error**: border `--color-danger`, helper text below in `--color-danger`, icon left of message.
- **Helper text**: 12 px `--text-tertiary`, always rendered with 4 px top margin (reserved space to prevent CLS).
- **Compose from shadcn**: `Form` + `FormField` + `FormLabel` + `FormControl` + `FormMessage` (react-hook-form + zod).

---

## 7. Data Table

- **Purpose**: Primary listing surface for customers, invoices, billings.
- **Header**: bg `--table-header-bg`, fg `--table-header-fg`, 11 px SemiBold uppercase, tracking-wider, `font-display`, border-bottom `--border-default`.
- **Row**: padding 16×20, border-bottom `--table-row-border`, hover `--table-row-bg-hover`, transition `--dur-fast`.
- **Cell types**:
  - `.td-id` — monospace 12 px `--text-tertiary`
  - `.td-amount` — monospace, tabular-nums, right-aligned, SemiBold
  - `.cell-user` — avatar (32×32 full-radius, initials in indigo-100/700 or custom) + name (13 px) + email (11 px tertiary)
- **Sortable header**: click toggles `asc/desc`, visual chevron at right, `aria-sort` on `<th>`.
- **Row actions**: 28×28 ghost icon buttons, right-aligned, visible on hover OR always-visible (decision: always-visible for discoverability on touch).
- **Empty state** (when 0 rows): see §9.
- **Loading state**: 5 rows of skeleton matching the column shape.
- **Compose from shadcn**: `Table`, `TableHeader`, `TableRow`, `TableHead`, `TableBody`, `TableCell` + `DropdownMenu` for row actions + `Checkbox` for bulk select (future).

### Pagination
- Shows `1–5 de 1.204 faturas` label.
- Numbered pills (active = primary filled), prev/next, ellipsis, last-page jump.
- Height 32, square radius-sm. Disabled state on prev/next when at edges.
- **Compose from shadcn**: `Pagination`, `PaginationContent`, `PaginationItem`, `PaginationLink`.

### Filter chips (above table)
- Pill buttons with count suffix. Active chip = `--color-primary-soft` bg, `--color-primary` fg, `--indigo-200` border.
- **Compose from shadcn**: `ToggleGroup` (single).

---

## 8. Modal / Dialog (Payment flows)

- **Used by**: `<PixPayment>` (library), demo flows (InvoicePayment, BillingPayment redirect confirmations).
- **Compose from shadcn**: `Dialog`, `DialogTrigger`, `DialogContent`, `DialogHeader`, `DialogTitle`, `DialogDescription`, `DialogFooter`.
- **Structure**:
  - Max-width 440 px (content-first, not over-wide for QR flow).
  - Radius `--radius-xl`, shadow `--shadow-2xl`, surface `--surface-default`.
  - Scrim: `rgba(15,23,42,0.48)` with `backdrop-filter: blur(4px)` — strong enough to isolate foreground per HIG.
  - Close button: top-right, 32×32, ghost.
  - Title: `font-display` 18 px SemiBold.
  - PIX content:
    - Amount (top right, display 18 px)
    - QR frame: `--qr-frame-*` tokens, 220×220 centered with emerald pulse dot below
    - PIX "copia e cola" code in monospace in `--slate-50` box with copy button
    - Status footer: soft emerald badge `Aguardando pagamento` with pulse ring animation (reduced-motion respected)
- **Accessibility**: `role="dialog"`, `aria-labelledby`, focus trap, ESC to dismiss, focus returns to trigger on close.

---

## 9. Empty State

- **Purpose**: Shown when a list/table is empty but loading is complete.
- **Structure**:
  - Icon (64×64, radius-xl, `--slate-100` bg, `--text-tertiary` fg) centered.
  - Title (`font-display` 18 px SemiBold, `--text-primary`).
  - Message (14 px `--text-secondary`, max 380 px wide, line-height 1.55).
  - Primary CTA (e.g., "Criar cliente", "Nova fatura").
- **Specific copies** (PT-BR):
  - Customers: "Nenhum cliente ainda." / "Seus clientes aparecerão aqui assim que você criar sua primeira fatura."
  - Invoices: "Nenhuma fatura emitida." / CTA "Nova fatura".
  - Billings: "Sem cobranças recorrentes." / CTA "Criar recorrência".

---

## 10. Alert / Inline Message

- **Success**: bg `--color-success-soft`, border `--emerald-200`, fg `--emerald-700`, icon check-circle.
- **Warning**: amber variant.
- **Danger**: red variant.
- **Info**: sky variant.
- Padding 14×18, radius `--radius-md`, icon left (20 px), optional close button right.
- **Compose from shadcn**: `Alert`, `AlertTitle`, `AlertDescription`.

---

## 11. Tabs (Landing install section + dashboard mini-tabs)

### Mini-tabs (dashboard, segmented)
- Container: `--slate-100` bg, 3 px padding, `--radius-sm`.
- Active tab: white bg + `--shadow-xs`, `--text-primary`.
- Inactive: transparent + `--text-tertiary`.
- **Compose from shadcn**: `Tabs`, `TabsList`, `TabsTrigger`, `TabsContent`.

### Code tabs (landing, dark install panel)
- Dark `rgba(255,255,255,0.04)` bg on `--slate-900` surface.
- Monospace body with syntax token classes: `.tk-key` (indigo-200), `.tk-str` (emerald-300), `.tk-fn` (amber-400), `.tk-com` (white 35% italic), `.tk-punct` (white 55%).
- Copy button top-right of tabs body.

---

## 12. Landing-only components

### 12.1 Hero eyebrow pill
- Left-side tag chip (bg `--color-primary`, fg white, uppercase 10 px) + description text on `--color-primary-soft` rounded-full container.

### 12.2 Bento cell
- See `landing.html` `.bento-cell` + sizing classes. 8 cells total, 6-column grid on desktop, collapses to 4/1 cols at breakpoints.
- Hero cell (PIX) spans 3×2 with dark gradient background + subtle conic pattern.
- Hover: lift -2 px, border → `--card-border-hover`, shadow → `--shadow-md`.

### 12.3 Floating "float-card"
- Hero visual decoration. 3 cards around QR preview with synthetic animation `float-slow` (6–8 s, reduced-motion disables it).
- Each has a colored icon chip (ok/warn/brand) + two-line text.

### 12.4 Install section (dark slab)
- `--slate-900` surface inside `--radius-2xl` with aurora radial gradient background. Houses the code tabs and a primary CTA.

### 12.5 CTA block
- Full-width gradient-brand block with centered title, subtitle, two buttons (inverted primary = white on indigo-700).

---

## 13. Page-specific pattern mapping

| Existing page | Pattern to apply | Component set |
|---|---|---|
| `Home.tsx` | Landing HTML layout | Hero + Trust strip + Bento + Install + Walkthrough + Stats + CTA + Footer |
| `Login.tsx`, `Signup.tsx` | Auth card (centered, max 400 px) | `Card` + `Form` fields + brand mark + footer link |
| `Dashboard.tsx` | Dashboard shell | Metric cards (1 hero + 3 regular + 4 regular row) + chart panel + activity panel + recent-invoices table |
| `StorePage.tsx` | CRUD form page | `Card` container + `Form` + `admin-store-badge` (keep `clientId` display prominent) + action buttons (update + danger delete) |
| `CustomersPage.tsx` | Table page | Table + pagination + filter chips + empty state |
| `InvoicesPage.tsx` | Table page w/ status chips | Table + chip filters (Pagas/Pendentes/Expiradas) + pagination |
| `BillingsPage.tsx` | Table page w/ recurrence-aware columns | Table (adds "Próximo ciclo" column) + chip filters |
| `Docs.tsx` | Reading surface | Max-width 760 px, prose style using `--text-secondary`, headings `font-display`, props tables styled like data-table but borderless |
| `DemoPix.tsx`, `DemoInvoice.tsx`, `DemoBilling.tsx` | Playground 2-col | Form `Card` (left) + Log `Card` (right) + info `Alert` box on top |
| `PaymentComplete.tsx` | Success surface | Centered success card with large check icon, transaction summary, return CTA |

---

## 14. Specific state inventory for PIX demo

The PIX demo page is the most stateful surface. Each state must have a matching visual.

| State | Visual |
|---|---|
| idle (no click yet) | form visible, right column has illustrative empty state: "Clique em Pagar para ver eventos aqui." |
| modal open / loading QR | Dialog open, skeleton QR frame (shimmer background) + "Gerando QR…" |
| modal open / awaiting payment | QR visible + pulse dot + "Aguardando pagamento" badge + countdown "expira em 14:32" |
| modal open / paid | Swap content: green check large (80 px in emerald-500 circle) + "Pago!" + auto-close timer (5 s) |
| modal open / expired | Amber warning icon + "QR expirado" + "Gerar novo" primary button |
| error | Red danger icon + error message + "Tentar novamente" |

All transitions use `--dur-base` ease-out, fade + 8 px translate. `prefers-reduced-motion` reduces to fade-only.

---

## 15. Accessibility audit (must-pass)

- [ ] Every icon-only button has `aria-label` (topbar notifications, help, row actions).
- [ ] Tables use `<th scope="col">` and sortable headers have `aria-sort`.
- [ ] Sidebar is a `<nav aria-label="Primary">`.
- [ ] Breadcrumb uses `<nav aria-label="Breadcrumb">` + `<ol>` semantics.
- [ ] Modals: focus-trap + ESC + visible close affordance.
- [ ] Form errors announce via `role="alert"` or `aria-live="polite"`.
- [ ] Color contrast verified for every token combination (all semantic pairs in tokens.css pass AA).
- [ ] Focus ring visible on ALL interactive elements (no `outline: none` without replacement).
- [ ] `prefers-reduced-motion` honored (tokens.css media query).
- [ ] `prefers-color-scheme: dark` intentionally ignored — light theme is the product decision.
