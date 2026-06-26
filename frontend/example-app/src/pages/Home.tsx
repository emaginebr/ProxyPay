import { Link } from "react-router-dom";
import { CodeBlock } from "../components/CodeBlock";
import {
  IconCheck,
  IconZap,
  IconFile,
  IconRepeat,
  IconClock,
  IconCode,
  IconBox,
  IconPalette,
  IconTrendingUp,
} from "../components/Icon";

function SyntheticQR({ className = "" }: { className?: string }) {
  // Fixed positions that make the QR look believable. Decorative only.
  const cells = [
    [5, 7], [5, 8], [5, 10], [5, 12], [5, 13], [5, 15],
    [6, 8], [6, 9], [6, 11], [6, 13], [6, 14],
    [7, 7], [7, 10], [7, 12], [7, 15],
    [8, 8], [8, 9], [8, 11], [8, 13], [8, 14],
    [9, 7], [9, 10], [9, 12], [9, 13],
    [10, 7], [10, 8], [10, 10], [10, 14], [10, 15],
    [11, 9], [11, 11], [11, 12], [11, 13],
    [12, 7], [12, 8], [12, 10], [12, 14],
    [13, 9], [13, 11], [13, 12], [13, 15],
    [14, 7], [14, 8], [14, 13], [14, 14],
    [15, 10], [15, 12], [15, 14],
  ];
  return (
    <div className={`qr-synth ${className}`}>
      <span className="qr-corner-bl" />
      {cells.map(([r, c], i) => (
        <span key={i} className="qr-cell" style={{ gridArea: `${r} / ${c}` }} />
      ))}
    </div>
  );
}

function FloatCard({
  tone,
  icon,
  title,
  subtitle,
  className = "",
  style,
}: {
  tone: "ok" | "warn" | "brand";
  icon: React.ReactNode;
  title: string;
  subtitle: string;
  className?: string;
  style?: React.CSSProperties;
}) {
  const toneMap = {
    ok: "bg-emerald-50 text-emerald-600",
    warn: "bg-amber-50 text-amber-600",
    brand: "bg-indigo-50 text-indigo-600",
  } as const;
  return (
    <div
      className={`absolute bg-white border border-slate-200 rounded-lg px-4 py-3 shadow-lg flex items-center gap-3 ${className}`}
      style={style}
    >
      <div className={`w-9 h-9 rounded-md grid place-items-center flex-shrink-0 ${toneMap[tone]}`}>
        {icon}
      </div>
      <div className="min-w-0">
        <strong className="block text-slate-900 font-semibold text-[13px] leading-tight">
          {title}
        </strong>
        <span className="block text-[11px] text-slate-500 leading-tight mt-0.5">{subtitle}</span>
      </div>
    </div>
  );
}

export function Home() {
  return (
    <div className="relative">
      {/* ═══ HERO ═══ */}
      <section className="pp-wrap relative pt-16 md:pt-20 pb-12">
        <div className="grid lg:grid-cols-[1.15fr_1fr] gap-10 lg:gap-16 items-center">
          <div className="animate-fade-in-up">
            <div className="inline-flex items-center gap-2 pl-1.5 pr-3.5 py-1.5 rounded-full bg-indigo-50 border border-indigo-100 text-indigo-700 text-xs font-semibold mb-6">
              <span className="bg-indigo-600 text-white px-2 py-0.5 rounded-full text-[10px] font-bold tracking-wider uppercase">
                v0.2
              </span>
              React + TypeScript &middot; zero dependências
            </div>
            <h1 className="font-display font-bold text-slate-900 leading-[1.02] tracking-[-0.04em] mb-5 text-[clamp(2.25rem,5.5vw+1rem,4rem)]">
              Pagamentos brasileiros,
              <br />
              em{" "}
              <span className="relative inline-block text-gradient-brand">
                três componentes
                <span
                  aria-hidden="true"
                  className="absolute left-0 right-0 -bottom-1 h-[3px] rounded-sm opacity-25"
                  style={{ background: "var(--gradient-brand)" }}
                />
              </span>
              .
            </h1>
            <p className="text-lg text-slate-600 leading-relaxed max-w-[52ch] mb-8">
              PIX com QR Code, faturas com redirect e cobranças recorrentes — tipados, leves e
              prontos para plugar no seu React. Você escreve a regra de negócio, a gente cuida
              do fluxo.
            </p>
            <div className="flex flex-wrap gap-3 mb-8">
              <Link to="/demo/pix" className="btn btn-primary btn-lg btn-arrow">
                Abrir demo do PIX
              </Link>
              <Link to="/docs" className="btn btn-secondary btn-lg">
                Ler a documentação
              </Link>
            </div>
            <ul className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-slate-500">
              {["8 KB gzipped", "ESM + CJS + tipos", "Multi-tenant via header"].map((m) => (
                <li key={m} className="inline-flex items-center gap-2">
                  <IconCheck size={16} className="text-accent shrink-0" />
                  {m}
                </li>
              ))}
            </ul>
          </div>

          {/* Hero visual */}
          <div
            className="hidden lg:block relative h-[520px]"
            aria-hidden="true"
          >
            <div
              className="absolute"
              style={{ top: 200, left: 180, width: 180, height: 2, opacity: 0.5, background: "repeating-linear-gradient(90deg, var(--indigo-300) 0 6px, transparent 6px 12px)" }}
            />

            <FloatCard
              tone="brand"
              icon={<IconFile size={16} />}
              title="Checkout criado"
              subtitle="invoice #BR-4821"
              className="animate-float-slow"
              style={{ top: 30, left: 0 }}
            />

            <div
              className="absolute bg-white border border-indigo-100 rounded-xl p-5 shadow-xl"
              style={{ top: 40, right: 40, width: 280, transform: "rotate(-3deg)" }}
            >
              <div className="flex items-center justify-between mb-3.5">
                <span className="text-[11px] font-bold text-slate-500 uppercase tracking-wider">
                  PIX &middot; R$
                </span>
                <span className="font-display text-lg font-bold text-slate-900">249,90</span>
              </div>
              <SyntheticQR />
              <div className="mt-3.5 flex items-center gap-2 px-3 py-2.5 bg-emerald-50 border border-emerald-200 rounded-sm text-emerald-700 font-semibold text-[13px]">
                <span className="pp-pulse-dot" />
                Aguardando pagamento
              </div>
            </div>

            <FloatCard
              tone="ok"
              icon={<IconCheck size={16} />}
              title="Pago em 4.2s"
              subtitle="via PIX - R$ 249,90"
              className="animate-float-slow"
              style={{ top: 320, right: 0, transform: "rotate(2deg)", animationDuration: "7s", animationDirection: "reverse" }}
            />

            <FloatCard
              tone="warn"
              icon={<IconTrendingUp size={16} />}
              title="+R$ 12.480 hoje"
              subtitle="63 transações"
              className="animate-float-slow"
              style={{ bottom: 10, left: 70, animationDuration: "8s" }}
            />
          </div>
        </div>
      </section>

      {/* ═══ TRUST STRIP ═══ */}
      <section className="relative bg-white border-y border-slate-200">
        <div className="pp-wrap py-8 flex items-center justify-between gap-8 flex-wrap">
          <span className="text-[11px] font-bold text-slate-500 uppercase tracking-wider">
            Construído sobre métodos brasileiros
          </span>
          <div className="flex gap-10 items-center flex-wrap text-slate-400 font-display font-bold text-base">
            <span>PIX</span>
            <span>Boleto</span>
            <span>Recorrência</span>
            <span>Multi-tenant</span>
            <span>GraphQL</span>
          </div>
        </div>
      </section>

      {/* ═══ BENTO ═══ */}
      <section className="pp-wrap py-20 md:py-24 relative">
        <header className="max-w-3xl mx-auto text-center mb-12">
          <span className="inline-block text-xs font-bold text-indigo-600 uppercase tracking-wider mb-3">
            A biblioteca
          </span>
          <h2 className="font-display font-bold text-4xl md:text-5xl text-slate-900 mb-4">
            Tudo que você precisaria construir &mdash; já construído.
          </h2>
          <p className="text-lg text-slate-600">
            Componentes pequenos e compostos, cada um com responsabilidade clara. Plugue só o
            que usar, ignore o resto.
          </p>
        </header>

        <div className="grid grid-cols-1 md:grid-cols-4 lg:grid-cols-6 gap-5 auto-rows-[180px]">
          {/* Hero PIX cell */}
          <article
            className="relative overflow-hidden rounded-xl p-6 text-white md:col-span-4 lg:col-span-3 lg:row-span-2 flex flex-col justify-between pp-card-interactive"
            style={{ background: "linear-gradient(135deg, var(--indigo-950) 0%, var(--indigo-800) 100%)" }}
          >
            <div>
              <span className="text-[11px] font-bold tracking-wider uppercase text-emerald-300 inline-flex items-center gap-1.5">
                <IconZap size={12} /> principal
              </span>
              <h3 className="font-display text-xl font-bold text-white mt-2.5 mb-2">
                &lt;PixPayment /&gt; &mdash; QR em 250ms
              </h3>
              <p className="text-white/75 text-sm leading-relaxed max-w-[36ch]">
                Gera o QR Code, abre o modal, faz polling até confirmar. Você só precisa passar{" "}
                <code className="bg-white/10 text-emerald-300 px-1.5 py-0.5 rounded text-[12px]">
                  customer
                </code>{" "}
                e{" "}
                <code className="bg-white/10 text-emerald-300 px-1.5 py-0.5 rounded text-[12px]">
                  items
                </code>
                .
              </p>
            </div>
            <div
              className="absolute -right-8 -bottom-8 w-56 h-56 opacity-80 rounded-2xl"
              style={{
                background:
                  "radial-gradient(circle at 30% 30%, rgba(52,211,153,0.35), transparent 60%), repeating-conic-gradient(from 45deg, rgba(255,255,255,0.05) 0 10deg, transparent 10deg 20deg)",
              }}
              aria-hidden="true"
            />
          </article>

          <BentoCell
            tone="indigo"
            icon={<IconFile size={22} />}
            title={"<InvoicePayment />"}
            desc="Redirect para checkout de fatura única. Retorna o status via callback."
            className="md:col-span-2 lg:col-span-3"
          />

          <BentoCell
            tone="emerald"
            icon={<IconRepeat size={22} />}
            title={"<BillingPayment />"}
            desc="Cobranças recorrentes com gestão automática de ciclos e inadimplência."
            className="md:col-span-2 lg:col-span-3"
          />

          <BentoCell
            tone="emerald"
            icon={<IconClock size={22} />}
            title="Polling nativo"
            desc="Intervalo configurável. Detecta expiração e cancelamento."
            className="md:col-span-2 lg:col-span-2"
          />

          <BentoCell
            tone="indigo"
            icon={<IconCode size={22} />}
            title="100% tipado"
            desc="Interfaces exportadas para Customer, InvoiceItem e respostas."
            className="md:col-span-2 lg:col-span-2"
            background="bg-indigo-50 border-indigo-200"
          />

          <BentoCell
            tone="sky"
            icon={<IconBox size={22} />}
            title="Zero dependências"
            desc="Só React como peer. Sem lib de QR, sem lib de modal."
            className="md:col-span-2 lg:col-span-2"
          />

          <BentoCell
            tone="emerald"
            icon={<IconZap size={22} />}
            title="8 KB gzipped - ESM + CJS com tree-shaking"
            desc="Pronto para produção. Testado em React 18 e 19, Next.js, Vite, Remix."
            className="md:col-span-4 lg:col-span-4"
            background="border-emerald-200"
            backgroundStyle={{ background: "linear-gradient(135deg, #ECFDF5, #fff 70%)" }}
          />

          <BentoCell
            tone="amber"
            icon={<IconPalette size={22} />}
            title="Tema livre"
            desc="Passe suas classes CSS. Zero opinião de design."
            className="md:col-span-4 lg:col-span-2"
            background="border-amber-200"
            backgroundStyle={{ background: "linear-gradient(135deg, #FFFBEB, #fff)" }}
          />
        </div>
      </section>

      {/* ═══ INSTALL ═══ */}
      <section className="pp-wrap pb-24">
        <div
          className="relative rounded-2xl p-8 md:p-10 overflow-hidden"
          style={{ background: "var(--slate-900)", color: "#E2E8F0" }}
        >
          <div
            aria-hidden="true"
            className="absolute inset-0 pointer-events-none"
            style={{
              background:
                "radial-gradient(600px 300px at 10% 0%, rgba(99,102,241,0.22), transparent 60%), radial-gradient(500px 300px at 90% 100%, rgba(16,185,129,0.14), transparent 60%)",
            }}
          />
          <div className="relative grid lg:grid-cols-[1fr_1.2fr] gap-10 items-center">
            <div>
              <span className="text-xs font-bold text-emerald-300 uppercase tracking-wider mb-3 inline-block">
                5 linhas
              </span>
              <h2 className="font-display text-3xl md:text-[2.2rem] font-bold text-white mb-4 leading-[1.1] tracking-[-0.03em]">
                Do <code className="text-amber-300 font-mono text-[0.9em]">npm install</code> ao
                primeiro pagamento.
              </h2>
              <p className="text-white/70 text-base mb-6 max-w-[52ch]">
                Instale, envolva seu app com o Provider e chame o componente. A gente cuida do
                resto &mdash; gera o QR, escuta o webhook, devolve o status.
              </p>
              <Link to="/docs" className="btn btn-primary btn-lg btn-arrow">
                Ver exemplo completo
              </Link>
            </div>
            <CodeBlock
              filename="App.tsx"
              language="tsx"
              code={`// 1. Envolva seu app
import { ProxyPayProvider, PixPayment } from "proxypay-react";

<ProxyPayProvider config={{
  baseUrl: "https://api.proxypay.co.ao",
  clientId: "srv-123",
  tenantId: "my-store",
}}>
  <PixPayment
    customer={customer}
    items={items}
    onSuccess={(status) => console.log("Pago!", status)}
  >
    <button>Pagar com PIX</button>
  </PixPayment>
</ProxyPayProvider>`}
            />
          </div>
        </div>
      </section>

      {/* ═══ WALKTHROUGH ═══ */}
      <section className="pp-wrap pb-24">
        <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center py-10 border-t border-slate-200">
          <div>
            <span className="text-xs font-bold text-emerald-600 uppercase tracking-wider mb-3 inline-block">
              PIX fluxo
            </span>
            <h3 className="font-display text-3xl font-bold text-slate-900 mb-4 leading-[1.2]">
              Um modal. Três estados. Zero refactor.
            </h3>
            <p className="text-slate-600 mb-6">
              O componente <code className="pp-code-inline">&lt;PixPayment&gt;</code> abstrai
              todo o fluxo: gera o QR, exibe o modal, escuta o status e chama seu callback
              quando o cliente paga.
            </p>
            <ul className="space-y-3">
              {[
                "QR Code renderizado inline &mdash; sem lib extra de canvas",
                "Intervalo de polling configurável (padrão: 10s)",
                "Callbacks onSuccess, onError, onStatusChange",
                "Modal totalmente customizável via className",
              ].map((t, i) => (
                <li key={i} className="flex gap-3 items-start text-sm text-slate-600">
                  <span
                    className="w-5 h-5 rounded-md bg-emerald-100 text-emerald-700 grid place-items-center flex-shrink-0 mt-0.5"
                    aria-hidden="true"
                  >
                    <IconCheck size={12} />
                  </span>
                  <span dangerouslySetInnerHTML={{ __html: t }} />
                </li>
              ))}
            </ul>
          </div>
          <PreviewFrame>
            <div className="flex items-center justify-between mb-3">
              <strong className="font-display text-[15px] text-slate-900">Pagamento PIX</strong>
              <span className="pp-badge pp-badge-warning">Aguardando</span>
            </div>
            <div className="max-w-[220px] mx-auto mb-3">
              <SyntheticQR />
            </div>
            <div className="text-center font-mono text-[11px] text-slate-500 p-3 bg-slate-50 rounded-sm border border-slate-100 break-all">
              00020126580014BR.GOV.BCB.PIX01...
            </div>
          </PreviewFrame>
        </div>

        <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center py-10 border-t border-slate-200">
          <PreviewFrame className="order-last lg:order-first">
            <div className="grid grid-cols-3 gap-2.5 mb-3.5">
              {([
                { label: "Saldo", val: "R$ 48.230", tone: "" },
                { label: "Créditos", val: "+12.480", tone: "text-emerald-600" },
                { label: "Trans.", val: "1.204", tone: "" },
              ] as const).map((m) => (
                <div key={m.label} className="bg-slate-50 rounded-md p-3 border border-slate-100">
                  <div className="text-[10px] font-bold text-slate-500 uppercase tracking-wider">
                    {m.label}
                  </div>
                  <div
                    className={`font-display text-xl font-bold text-slate-900 tabular mt-1 ${m.tone}`}
                  >
                    {m.val}
                  </div>
                </div>
              ))}
            </div>
            <div
              className="h-[120px] rounded-md border border-slate-100 relative overflow-hidden"
              style={{
                background: "linear-gradient(180deg, var(--indigo-50), #fff)",
              }}
            >
              <svg viewBox="0 0 300 120" preserveAspectRatio="none" className="absolute inset-0 w-full h-full">
                <defs>
                  <linearGradient id="gg" x1="0" x2="0" y1="0" y2="1">
                    <stop offset="0%" stopColor="#4F46E5" />
                    <stop offset="100%" stopColor="#4F46E5" stopOpacity="0" />
                  </linearGradient>
                </defs>
                <path
                  d="M 0 90 L 30 80 L 60 85 L 90 60 L 120 70 L 150 50 L 180 55 L 210 35 L 240 40 L 270 25 L 300 20 L 300 120 L 0 120 Z"
                  fill="url(#gg)"
                  opacity="0.22"
                />
                <path
                  d="M 0 90 L 30 80 L 60 85 L 90 60 L 120 70 L 150 50 L 180 55 L 210 35 L 240 40 L 270 25 L 300 20"
                  stroke="#4F46E5"
                  strokeWidth="2.5"
                  fill="none"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </div>
            <table className="w-full text-xs border-collapse mt-3.5">
              <thead>
                <tr className="text-[10px] text-slate-500 uppercase tracking-wider">
                  <th className="text-left py-2">Cliente</th>
                  <th className="text-left py-2">Valor</th>
                  <th className="text-left py-2">Status</th>
                </tr>
              </thead>
              <tbody>
                <tr className="border-t border-slate-100">
                  <td className="py-2.5 text-slate-900">Maria Santos</td>
                  <td className="py-2.5 text-slate-900 tabular">R$ 249,90</td>
                  <td className="py-2.5">
                    <span className="pp-badge pp-badge-success">Pago</span>
                  </td>
                </tr>
                <tr className="border-t border-slate-100">
                  <td className="py-2.5 text-slate-900">João Oliveira</td>
                  <td className="py-2.5 text-slate-900 tabular">R$ 89,00</td>
                  <td className="py-2.5">
                    <span className="pp-badge pp-badge-warning">Pendente</span>
                  </td>
                </tr>
                <tr className="border-t border-slate-100">
                  <td className="py-2.5 text-slate-900">Camila Rocha</td>
                  <td className="py-2.5 text-slate-900 tabular">R$ 1.280,00</td>
                  <td className="py-2.5">
                    <span className="pp-badge pp-badge-success">Pago</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </PreviewFrame>
          <div>
            <span className="text-xs font-bold text-emerald-600 uppercase tracking-wider mb-3 inline-block">
              Painel
            </span>
            <h3 className="font-display text-3xl font-bold text-slate-900 mb-4 leading-[1.2]">
              Painel admin para o merchant &mdash; incluso.
            </h3>
            <p className="text-slate-600 mb-6">
              O example-app vem com um painel completo: configuração de loja, clientes,
              faturas, cobranças recorrentes e saldo. Use como referência, como base, ou copie
              inteiro.
            </p>
            <ul className="space-y-3">
              {[
                "Autenticação via nauth-react",
                "GraphQL para leituras, REST para mutações",
                "Arquitetura em camadas: types &rarr; service &rarr; context &rarr; hook &rarr; page",
                "Tudo tipado, documentado, pronto para copiar",
              ].map((t, i) => (
                <li key={i} className="flex gap-3 items-start text-sm text-slate-600">
                  <span
                    className="w-5 h-5 rounded-md bg-emerald-100 text-emerald-700 grid place-items-center flex-shrink-0 mt-0.5"
                    aria-hidden="true"
                  >
                    <IconCheck size={12} />
                  </span>
                  <span dangerouslySetInnerHTML={{ __html: t }} />
                </li>
              ))}
            </ul>
          </div>
        </div>
      </section>

      {/* ═══ STATS ═══ */}
      <section className="pp-wrap pb-24">
        <div className="pp-card grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
          {[
            { value: "8", unit: "KB", label: "gzipped no bundle final" },
            { value: "3", unit: "", label: "componentes, uma API" },
            { value: "250", unit: "ms", label: "média para abrir o QR" },
            { value: "0", unit: "", label: "runtime dependencies" },
          ].map((s, i, arr) => (
            <div
              key={s.label}
              className={`p-8 ${i < arr.length - 1 ? "lg:border-r border-slate-200" : ""} ${
                i === 0 || i === 1 ? "sm:border-r border-slate-200" : ""
              } ${i < 2 ? "border-b sm:border-b-0 border-slate-200" : ""}`}
            >
              <div className="font-display font-bold text-[2.4rem] text-slate-900 leading-none tracking-[-0.03em] tabular">
                {s.value}
                {s.unit && <span className="text-xl text-slate-400 font-semibold ml-0.5">{s.unit}</span>}
              </div>
              <div className="text-xs text-slate-500 mt-2 font-medium">{s.label}</div>
            </div>
          ))}
        </div>
      </section>

      {/* ═══ CTA ═══ */}
      <section className="pp-wrap pb-24">
        <div
          className="relative rounded-2xl p-10 md:p-16 text-center overflow-hidden text-white"
          style={{ background: "var(--gradient-brand)" }}
        >
          <div
            aria-hidden="true"
            className="absolute inset-0 pointer-events-none"
            style={{
              background:
                "radial-gradient(600px 300px at 80% 20%, rgba(255,255,255,0.22), transparent 60%), radial-gradient(500px 300px at 20% 100%, rgba(16,185,129,0.3), transparent 60%)",
            }}
          />
          <div className="relative">
            <h2 className="font-display font-bold text-3xl md:text-[3rem] leading-[1.1] tracking-[-0.035em] text-white max-w-3xl mx-auto mb-4">
              Pronto para aceitar pagamentos em 5 minutos?
            </h2>
            <p className="text-white/85 text-lg max-w-xl mx-auto mb-8">
              Clone o example-app, configure suas credenciais e rode{" "}
              <code className="bg-white/20 text-white px-2 py-0.5 rounded-sm font-mono text-[0.9em]">
                npm run dev
              </code>
              . Produção ready em um commit.
            </p>
            <div className="flex gap-3 justify-center flex-wrap">
              <Link
                to="/signup"
                className="btn btn-lg btn-arrow"
                style={{ background: "#fff", color: "var(--indigo-700)", boxShadow: "0 8px 24px rgba(0,0,0,0.12)" }}
              >
                Começar agora
              </Link>
              <a
                href="https://github.com/emaginebr/proxypay-react"
                target="_blank"
                rel="noopener noreferrer"
                className="btn btn-lg"
                style={{ background: "rgba(255,255,255,0.12)", color: "#fff", borderColor: "rgba(255,255,255,0.25)", borderWidth: 1, borderStyle: "solid" }}
              >
                Ver no GitHub
              </a>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}

function BentoCell({
  tone,
  icon,
  title,
  desc,
  className = "",
  background = "",
  backgroundStyle,
}: {
  tone: "indigo" | "emerald" | "amber" | "sky";
  icon: React.ReactNode;
  title: string;
  desc: string;
  className?: string;
  background?: string;
  backgroundStyle?: React.CSSProperties;
}) {
  const iconTone = {
    indigo: "bg-indigo-100 text-indigo-700",
    emerald: "bg-emerald-50 text-emerald-600",
    amber: "bg-amber-100 text-amber-700",
    sky: "bg-sky-50 text-sky-600",
  }[tone];

  return (
    <article
      className={`pp-card pp-card-interactive rounded-xl p-6 flex flex-col justify-between ${background} ${className}`}
      style={backgroundStyle}
    >
      <div className={`w-11 h-11 rounded-md grid place-items-center mb-4 ${iconTone}`}>
        {icon}
      </div>
      <div>
        <h3 className="font-display text-[20px] font-bold text-slate-900 tracking-[-0.02em] mb-2">
          {title}
        </h3>
        <p className="text-slate-600 text-sm leading-[1.6]">{desc}</p>
      </div>
    </article>
  );
}

function PreviewFrame({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      className={`bg-white border border-slate-200 rounded-xl shadow-xl overflow-hidden ${className}`}
    >
      <div className="flex gap-1.5 px-4 py-2.5 bg-slate-50 border-b border-slate-200">
        <span className="w-2.5 h-2.5 rounded-full bg-slate-300" />
        <span className="w-2.5 h-2.5 rounded-full bg-slate-300" />
        <span className="w-2.5 h-2.5 rounded-full bg-slate-300" />
      </div>
      <div className="p-5">{children}</div>
    </div>
  );
}
