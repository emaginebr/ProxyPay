import { useState, useEffect, useMemo } from "react";
import { useBilling } from "../hooks/useBilling";
import { SkeletonTable } from "../components/Skeleton";
import {
  IconRepeat,
  IconAlert,
  IconChevronLeft,
  IconChevronRight,
} from "../components/Icon";

const frequencyLabels: Record<number, string> = {
  1: "Mensal",
  2: "Trimestral",
  3: "Semestral",
  4: "Anual",
};

const paymentLabels: Record<number, string> = {
  1: "PIX",
  2: "Boleto",
  3: "Crédito",
  4: "Débito",
};

const paymentBadge: Record<number, string> = {
  1: "pp-badge-brand",
  2: "pp-badge-info",
  3: "pp-badge-neutral",
  4: "pp-badge-neutral",
};

const statusLabels: Record<number, string> = {
  1: "Ativa",
  2: "Pausada",
  3: "Cancelada",
  4: "Concluída",
};

const statusBadge: Record<number, string> = {
  1: "pp-badge-success",
  2: "pp-badge-warning",
  3: "pp-badge-danger",
  4: "pp-badge-neutral",
};

type FilterKey = "all" | "active" | "paused" | "ended";

const filterMap: Record<FilterKey, (s: number) => boolean> = {
  all: () => true,
  active: (s) => s === 1,
  paused: (s) => s === 2,
  ended: (s) => s === 3 || s === 4,
};

export function BillingsPage() {
  const { billings, loading, error, loadBillings } = useBilling();
  const [page, setPage] = useState(0);
  const [filter, setFilter] = useState<FilterKey>("all");
  const pageSize = 20;

  useEffect(() => {
    loadBillings(page * pageSize, pageSize);
  }, [loadBillings, page]);

  const counts = useMemo(
    () => ({
      all: billings.length,
      active: billings.filter((b) => filterMap.active(b.status)).length,
      paused: billings.filter((b) => filterMap.paused(b.status)).length,
      ended: billings.filter((b) => filterMap.ended(b.status)).length,
    }),
    [billings]
  );

  const filtered = billings.filter((b) => filterMap[filter](b.status));

  return (
    <div className="max-w-[1200px] mx-auto p-6 md:p-10 w-full animate-fade-in-up">
      <header className="flex items-start justify-between flex-wrap gap-4 mb-6">
        <div>
          <h1 className="font-display text-3xl font-bold text-slate-900 mb-1.5">
            Recorrências
          </h1>
          <p className="text-sm text-slate-500">
            Assinaturas e cobranças cíclicas com gestão automática.
          </p>
        </div>
      </header>

      <div className="flex flex-wrap gap-2 mb-5" role="toolbar" aria-label="Filtros de status">
        {(
          [
            ["all", "Todas"],
            ["active", "Ativas"],
            ["paused", "Pausadas"],
            ["ended", "Encerradas"],
          ] as Array<[FilterKey, string]>
        ).map(([key, label]) => (
          <button
            key={key}
            type="button"
            className="pp-chip"
            aria-pressed={filter === key}
            onClick={() => setFilter(key)}
          >
            {label}
            <span className="pp-chip-count">{counts[key]}</span>
          </button>
        ))}
      </div>

      {error && (
        <div className="pp-alert pp-alert-danger mb-5" role="alert">
          <IconAlert size={18} />
          {error}
        </div>
      )}

      {loading ? (
        <SkeletonTable rows={6} cols={7} />
      ) : filtered.length === 0 ? (
        <div className="pp-card">
          <div className="pp-empty">
            <div className="pp-empty-icon">
              <IconRepeat size={28} />
            </div>
            <div className="pp-empty-title">Sem cobranças recorrentes.</div>
            <p className="pp-empty-desc">
              Configure assinaturas via <code className="pp-code-inline">&lt;BillingPayment /&gt;</code>{" "}
              para começar a cobrar ciclicamente.
            </p>
          </div>
        </div>
      ) : (
        <div className="pp-table-wrap">
          <div className="overflow-x-auto">
            <table className="pp-table">
              <thead>
                <tr>
                  <th scope="col">ID</th>
                  <th scope="col">Cliente</th>
                  <th scope="col">Frequência</th>
                  <th scope="col">Método</th>
                  <th scope="col">Início</th>
                  <th scope="col">Status</th>
                  <th scope="col">Criado em</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((b) => (
                  <tr key={b.billingId}>
                    <td className="td-id">#{b.billingId}</td>
                    <td className="font-medium text-slate-900">
                      {b.customer?.name ?? <span className="text-slate-400">&mdash;</span>}
                    </td>
                    <td className="text-slate-600">
                      {frequencyLabels[b.frequency] ?? b.frequency}
                    </td>
                    <td>
                      <span
                        className={`pp-badge ${
                          paymentBadge[b.paymentMethod] ?? "pp-badge-neutral"
                        } no-dot`}
                      >
                        {paymentLabels[b.paymentMethod] ?? b.paymentMethod}
                      </span>
                    </td>
                    <td className="text-slate-600">
                      {new Date(b.billingStartDate).toLocaleDateString("pt-BR")}
                    </td>
                    <td>
                      <span className={`pp-badge ${statusBadge[b.status] ?? "pp-badge-neutral"}`}>
                        {statusLabels[b.status] ?? String(b.status)}
                      </span>
                    </td>
                    <td className="text-slate-600">
                      {new Date(b.createdAt).toLocaleDateString("pt-BR")}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="pp-pagination">
            <span className="pp-pagination-label">
              {filtered.length} recorrências &middot; Página {page + 1}
            </span>
            <div className="pp-pagination-nav">
              <button
                type="button"
                className="pp-pagination-btn"
                onClick={() => setPage((p) => p - 1)}
                disabled={page === 0}
                aria-label="Página anterior"
              >
                <IconChevronLeft size={14} />
              </button>
              <button
                type="button"
                className="pp-pagination-btn"
                onClick={() => setPage((p) => p + 1)}
                disabled={billings.length < pageSize}
                aria-label="Próxima página"
              >
                <IconChevronRight size={14} />
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
