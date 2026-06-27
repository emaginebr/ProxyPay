import { useState, useEffect, useMemo } from "react";
import { useInvoice } from "../hooks/useInvoice";
import { SkeletonTable } from "../components/Skeleton";
import {
  IconFile,
  IconAlert,
  IconChevronLeft,
  IconChevronRight,
} from "../components/Icon";

const statusLabels: Record<number, string> = {
  1: "Pendente",
  2: "Enviada",
  3: "Paga",
  4: "Vencida",
  5: "Cancelada",
  6: "Expirada",
};

const statusBadge: Record<number, string> = {
  1: "pp-badge-warning",
  2: "pp-badge-info",
  3: "pp-badge-success",
  4: "pp-badge-danger",
  5: "pp-badge-neutral",
  6: "pp-badge-neutral",
};

type FilterKey = "all" | "paid" | "pending" | "expired";

const filterMap: Record<FilterKey, (s: number) => boolean> = {
  all: () => true,
  paid: (s) => s === 3,
  pending: (s) => s === 1 || s === 2,
  expired: (s) => s === 4 || s === 6,
};

export function InvoicesPage() {
  const { invoices, loading, error, loadInvoices } = useInvoice();
  const [page, setPage] = useState(0);
  const [filter, setFilter] = useState<FilterKey>("all");
  const pageSize = 20;

  useEffect(() => {
    loadInvoices(page * pageSize, pageSize);
  }, [loadInvoices, page]);

  const counts = useMemo(() => {
    return {
      all: invoices.length,
      paid: invoices.filter((i) => filterMap.paid(i.status)).length,
      pending: invoices.filter((i) => filterMap.pending(i.status)).length,
      expired: invoices.filter((i) => filterMap.expired(i.status)).length,
    };
  }, [invoices]);

  const filtered = invoices.filter((i) => filterMap[filter](i.status));

  return (
    <div className="max-w-[1200px] mx-auto p-6 md:p-10 w-full animate-fade-in-up">
      <header className="flex items-start justify-between flex-wrap gap-4 mb-6">
        <div>
          <h1 className="font-display text-3xl font-bold text-slate-900 mb-1.5">Faturas</h1>
          <p className="text-sm text-slate-500">
            Acompanhe pagamentos únicos emitidos pela sua loja.
          </p>
        </div>
      </header>

      <div className="flex flex-wrap gap-2 mb-5" role="toolbar" aria-label="Filtros de status">
        {(
          [
            ["all", "Todas"],
            ["paid", "Pagas"],
            ["pending", "Pendentes"],
            ["expired", "Expiradas"],
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
        <SkeletonTable rows={6} cols={6} />
      ) : filtered.length === 0 ? (
        <div className="pp-card">
          <div className="pp-empty">
            <div className="pp-empty-icon">
              <IconFile size={28} />
            </div>
            <div className="pp-empty-title">Nenhuma fatura emitida.</div>
            <p className="pp-empty-desc">
              Use o <code className="pp-code-inline">&lt;InvoicePayment /&gt;</code> ou a API
              para criar sua primeira fatura.
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
                  <th scope="col">Número</th>
                  <th scope="col">Cliente</th>
                  <th scope="col">Status</th>
                  <th scope="col">Vencimento</th>
                  <th scope="col">Pago em</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((inv) => (
                  <tr key={inv.invoiceId}>
                    <td className="td-id">#{inv.invoiceId}</td>
                    <td className="td-mono">{inv.invoiceNumber}</td>
                    <td className="text-slate-900 font-medium">
                      {inv.customer?.name ?? <span className="text-slate-400">&mdash;</span>}
                    </td>
                    <td>
                      <span className={`pp-badge ${statusBadge[inv.status] ?? "pp-badge-neutral"}`}>
                        {statusLabels[inv.status] ?? String(inv.status)}
                      </span>
                    </td>
                    <td className="text-slate-600">
                      {new Date(inv.dueDate).toLocaleDateString("pt-BR")}
                    </td>
                    <td className="text-slate-600">
                      {inv.paidAt ? (
                        new Date(inv.paidAt).toLocaleDateString("pt-BR")
                      ) : (
                        <span className="text-slate-400">&mdash;</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="pp-pagination">
            <span className="pp-pagination-label">
              {filtered.length} faturas &middot; Página {page + 1}
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
                disabled={invoices.length < pageSize}
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
