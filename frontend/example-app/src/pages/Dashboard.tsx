import { useEffect } from "react";
import { Link } from "react-router-dom";
import { useStore } from "../hooks/useStore";
import { useBalance } from "../hooks/useBalance";
import { Skeleton, SkeletonCards } from "../components/Skeleton";
import {
  IconWallet,
  IconTrendingUp,
  IconTrendingDown,
  IconActivity,
  IconStore,
  IconUsers,
  IconFile,
  IconRepeat,
  IconChevronRight,
  IconAlert,
} from "../components/Icon";

const brl = (v: number) =>
  v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

export function Dashboard() {
  const { store } = useStore();
  const { balance, loading, error, loadBalance } = useBalance();

  useEffect(() => {
    loadBalance();
  }, [loadBalance]);

  if (loading) {
    return (
      <div className="max-w-[1200px] mx-auto p-6 md:p-10 w-full">
        <Skeleton width="220px" height="28px" style={{ marginBottom: "10px" }} />
        <Skeleton width="320px" height="14px" style={{ marginBottom: "32px" }} />
        <SkeletonCards count={4} />
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-[1200px] mx-auto p-6 md:p-10 w-full">
        <div className="pp-alert pp-alert-danger" role="alert">
          <IconAlert size={18} />
          {error}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1200px] mx-auto p-6 md:p-10 w-full animate-fade-in-up">
      <header className="mb-8 flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="font-display text-3xl font-bold text-slate-900 mb-1.5">
            Visão geral
          </h1>
          <p className="text-sm text-slate-500">
            Indicadores em tempo real da sua conta ProxyPay.
          </p>
        </div>
        {store && (
          <div className="inline-flex items-center gap-3 px-4 py-2.5 bg-white border border-slate-200 rounded-md">
            <span className="pp-brand-mark sm" aria-hidden="true" />
            <div className="text-[13px]">
              <div className="text-slate-500 text-[11px] uppercase tracking-wider font-semibold">
                Loja ativa
              </div>
              <div className="font-semibold text-slate-900">
                {store.name} &middot;{" "}
                <code className="pp-code-inline text-[11px]">{store.clientId}</code>
              </div>
            </div>
          </div>
        )}
      </header>

      {!store && (
        <div className="pp-alert pp-alert-warning mb-6" role="alert">
          <IconAlert size={18} />
          <div>
            Você ainda não tem uma loja configurada.{" "}
            <Link to="/admin/store" className="text-amber-700 font-semibold underline">
              Criar loja
            </Link>
          </div>
        </div>
      )}

      {/* Hero metric + 3 complementary */}
      <div className="grid grid-cols-1 lg:grid-cols-[1.3fr_1fr_1fr_1fr] gap-5 mb-5">
        <div className="pp-metric pp-metric-hero">
          <div className="pp-metric-head">
            <span className="pp-metric-label">Saldo disponível</span>
            <span className="pp-metric-chip">
              <IconWallet size={16} />
            </span>
          </div>
          <span className="pp-metric-value pp-money">{brl(balance?.balance ?? 0)}</span>
          <div className="flex items-center gap-3">
            <button type="button" className="btn btn-sm" style={{ background: "rgba(255,255,255,0.16)", color: "#fff" }}>
              Solicitar saque
            </button>
            <span className="text-[12px] text-white/70">
              Atualizado agora
            </span>
          </div>
        </div>

        <MetricCard
          label="Créditos"
          icon={<IconTrendingUp size={16} />}
          value={brl(balance?.totalCredits ?? 0)}
          foot="Total recebido no período"
          valueClass="text-emerald-600"
          chipClass="bg-emerald-50 text-emerald-600"
        />
        <MetricCard
          label="Débitos"
          icon={<IconTrendingDown size={16} />}
          value={brl(balance?.totalDebits ?? 0)}
          foot="Saídas e saques"
          valueClass="text-red-600"
          chipClass="bg-red-50 text-red-600"
        />
        <MetricCard
          label="Transações"
          icon={<IconActivity size={16} />}
          value={String(balance?.transactionCount ?? 0)}
          foot="Operações concluídas"
          chipClass="bg-amber-50 text-amber-600"
        />
      </div>

      {/* Quick access */}
      <h2 className="font-display text-lg font-semibold text-slate-900 mb-4 mt-10">
        Acesso rápido
      </h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        <QuickLink
          to="/admin/store"
          icon={<IconStore size={18} />}
          title="Loja"
          desc="Configure dados e estratégia de cobrança"
        />
        <QuickLink
          to="/admin/customers"
          icon={<IconUsers size={18} />}
          title="Clientes"
          desc="Gerencie compradores e dados fiscais"
        />
        <QuickLink
          to="/admin/invoices"
          icon={<IconFile size={18} />}
          title="Faturas"
          desc="Acompanhe status e pagamentos"
        />
        <QuickLink
          to="/admin/billings"
          icon={<IconRepeat size={18} />}
          title="Recorrência"
          desc="Assinaturas e cobranças cíclicas"
        />
      </div>
    </div>
  );
}

function MetricCard({
  label,
  icon,
  value,
  foot,
  valueClass = "",
  chipClass = "bg-indigo-50 text-indigo-600",
}: {
  label: string;
  icon: React.ReactNode;
  value: string;
  foot?: string;
  valueClass?: string;
  chipClass?: string;
}) {
  return (
    <div className="pp-metric">
      <div className="pp-metric-head">
        <span className="pp-metric-label">{label}</span>
        <span className={`pp-metric-chip ${chipClass}`}>{icon}</span>
      </div>
      <span className={`pp-metric-value pp-money ${valueClass}`}>{value}</span>
      {foot && <span className="pp-metric-foot">{foot}</span>}
    </div>
  );
}

function QuickLink({
  to,
  icon,
  title,
  desc,
}: {
  to: string;
  icon: React.ReactNode;
  title: string;
  desc: string;
}) {
  return (
    <Link
      to={to}
      className="pp-card pp-card-interactive p-5 flex items-start gap-4 group no-underline"
    >
      <span className="w-10 h-10 rounded-md bg-indigo-50 text-indigo-600 grid place-items-center shrink-0 group-hover:bg-indigo-100 transition">
        {icon}
      </span>
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between">
          <strong className="font-display font-semibold text-slate-900 text-[15px]">
            {title}
          </strong>
          <IconChevronRight
            size={16}
            className="text-slate-400 group-hover:text-indigo-600 transition group-hover:translate-x-0.5"
          />
        </div>
        <p className="text-sm text-slate-500 mt-0.5">{desc}</p>
      </div>
    </Link>
  );
}
