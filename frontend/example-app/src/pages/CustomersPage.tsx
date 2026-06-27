import { useState, useEffect } from "react";
import { useCustomer } from "../hooks/useCustomer";
import { SkeletonTable } from "../components/Skeleton";
import {
  IconUsers,
  IconAlert,
  IconChevronLeft,
  IconChevronRight,
} from "../components/Icon";

function initials(name: string) {
  const parts = name.trim().split(/\s+/);
  return ((parts[0]?.[0] ?? "?") + (parts[1]?.[0] ?? "")).toUpperCase();
}

function avatarTone(seed: string) {
  const pool = [
    "bg-indigo-100 text-indigo-700",
    "bg-emerald-100 text-emerald-700",
    "bg-amber-100 text-amber-700",
    "bg-sky-100 text-sky-700",
  ];
  const sum = seed.split("").reduce((a, c) => a + c.charCodeAt(0), 0);
  return pool[sum % pool.length];
}

export function CustomersPage() {
  const { customers, loading, error, loadCustomers } = useCustomer();
  const [page, setPage] = useState(0);
  const pageSize = 20;

  useEffect(() => {
    loadCustomers(page * pageSize, pageSize);
  }, [loadCustomers, page]);

  return (
    <div className="max-w-[1200px] mx-auto p-6 md:p-10 w-full animate-fade-in-up">
      <header className="flex items-start justify-between flex-wrap gap-4 mb-8">
        <div>
          <h1 className="font-display text-3xl font-bold text-slate-900 mb-1.5">
            Clientes
          </h1>
          <p className="text-sm text-slate-500">
            Compradores cadastrados a partir das faturas e cobranças emitidas.
          </p>
        </div>
      </header>

      {error && (
        <div className="pp-alert pp-alert-danger mb-5" role="alert">
          <IconAlert size={18} />
          {error}
        </div>
      )}

      {loading ? (
        <SkeletonTable rows={6} cols={6} />
      ) : customers.length === 0 ? (
        <div className="pp-card">
          <div className="pp-empty">
            <div className="pp-empty-icon">
              <IconUsers size={28} />
            </div>
            <div className="pp-empty-title">Nenhum cliente ainda.</div>
            <p className="pp-empty-desc">
              Seus clientes aparecerão aqui assim que você emitir a primeira fatura ou cobrança.
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
                  <th scope="col">Documento</th>
                  <th scope="col">Telefone</th>
                  <th scope="col">Criado em</th>
                </tr>
              </thead>
              <tbody>
                {customers.map((c) => (
                  <tr key={c.customerId}>
                    <td className="td-id">#{c.customerId}</td>
                    <td>
                      <div className="flex items-center gap-3">
                        <div
                          className={`w-8 h-8 rounded-full grid place-items-center text-[11px] font-bold font-display shrink-0 ${avatarTone(
                            c.name
                          )}`}
                          aria-hidden="true"
                        >
                          {initials(c.name)}
                        </div>
                        <div className="min-w-0">
                          <div className="font-semibold text-slate-900 text-[13.5px] leading-tight">
                            {c.name}
                          </div>
                          <div className="text-[11.5px] text-slate-500 truncate">
                            {c.email}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="td-mono">{c.documentId}</td>
                    <td className="td-mono">{c.cellphone}</td>
                    <td className="text-slate-600">
                      {new Date(c.createdAt).toLocaleDateString("pt-BR")}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination
            page={page}
            hasNext={customers.length >= pageSize}
            onPrev={() => setPage((p) => p - 1)}
            onNext={() => setPage((p) => p + 1)}
          />
        </div>
      )}
    </div>
  );
}

function Pagination({
  page,
  hasNext,
  onPrev,
  onNext,
}: {
  page: number;
  hasNext: boolean;
  onPrev: () => void;
  onNext: () => void;
}) {
  return (
    <div className="pp-pagination">
      <span className="pp-pagination-label">Página {page + 1}</span>
      <div className="pp-pagination-nav">
        <button
          type="button"
          className="pp-pagination-btn"
          onClick={onPrev}
          disabled={page === 0}
          aria-label="Página anterior"
        >
          <IconChevronLeft size={14} />
        </button>
        <button
          type="button"
          className="pp-pagination-btn"
          onClick={onNext}
          disabled={!hasNext}
          aria-label="Próxima página"
        >
          <IconChevronRight size={14} />
        </button>
      </div>
    </div>
  );
}
