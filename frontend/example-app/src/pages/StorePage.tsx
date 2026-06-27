import { useState, useEffect } from "react";
import { useStore } from "../hooks/useStore";
import { Skeleton, SkeletonForm } from "../components/Skeleton";
import {
  IconStore,
  IconCheckCircle,
  IconAlert,
  IconCopy,
  IconCheck,
} from "../components/Icon";

export function StorePage() {
  const { store, loading, error, createStore, updateStore, deleteStore } = useStore();
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState("");
  const [localError, setLocalError] = useState("");
  const [copied, setCopied] = useState(false);

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [billingStrategy, setBillingStrategy] = useState(1);

  useEffect(() => {
    if (store) {
      setName(store.name);
      setEmail(store.email);
      setBillingStrategy(store.billingStrategy);
    }
  }, [store]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError("");
    setSuccess("");
    setSaving(true);
    try {
      if (store) {
        await updateStore({
          storeId: store.storeId,
          name,
          email,
          billingStrategy,
        });
        setSuccess("Loja atualizada com sucesso.");
      } else {
        const result = await createStore({ name, email, billingStrategy });
        setSuccess(`Loja criada. Client ID: ${result.clientId}`);
      }
    } catch (err) {
      setLocalError(err instanceof Error ? err.message : "Erro ao salvar");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!store) return;
    if (!confirm("Tem certeza que deseja excluir esta loja? Esta ação é irreversível.")) return;
    setLocalError("");
    setSaving(true);
    try {
      await deleteStore(store.storeId);
      setName("");
      setEmail("");
      setBillingStrategy(1);
      setSuccess("Loja excluída.");
    } catch (err) {
      setLocalError(err instanceof Error ? err.message : "Erro ao excluir");
    } finally {
      setSaving(false);
    }
  };

  const copyClientId = async () => {
    if (!store) return;
    await navigator.clipboard.writeText(store.clientId);
    setCopied(true);
    setTimeout(() => setCopied(false), 1800);
  };

  if (loading) {
    return (
      <div className="max-w-[1100px] mx-auto p-6 md:p-10 w-full">
        <Skeleton width="220px" height="28px" style={{ marginBottom: "24px" }} />
        <SkeletonForm />
      </div>
    );
  }

  const displayError = localError || error;

  return (
    <div className="max-w-[1100px] mx-auto p-6 md:p-10 w-full animate-fade-in-up">
      <header className="flex items-start gap-4 mb-8">
        <span className="w-12 h-12 rounded-lg bg-indigo-50 text-indigo-600 grid place-items-center shrink-0">
          <IconStore size={22} />
        </span>
        <div>
          <h1 className="font-display text-3xl font-bold text-slate-900 mb-1.5">
            {store ? "Configurar loja" : "Criar loja"}
          </h1>
          <p className="text-sm text-slate-500 max-w-[56ch]">
            Defina as informações da sua loja e a estratégia de cobrança usada pelas
            integrações.
          </p>
        </div>
      </header>

      {displayError && (
        <div className="pp-alert pp-alert-danger mb-5" role="alert">
          <IconAlert size={18} />
          {displayError}
        </div>
      )}
      {success && (
        <div className="pp-alert pp-alert-success mb-5" role="status">
          <IconCheckCircle size={18} />
          {success}
        </div>
      )}

      {store && (
        <div className="pp-card p-5 mb-6 flex items-center justify-between gap-4 flex-wrap">
          <div>
            <div className="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1">
              Client ID
            </div>
            <code className="font-mono text-sm text-slate-900">{store.clientId}</code>
          </div>
          <button
            type="button"
            onClick={copyClientId}
            className="btn btn-secondary btn-sm"
            aria-label="Copiar Client ID"
          >
            {copied ? <IconCheck size={14} /> : <IconCopy size={14} />}
            {copied ? "Copiado" : "Copiar"}
          </button>
        </div>
      )}

      <form onSubmit={handleSubmit} className="pp-card p-6 md:p-8">
        <h2 className="font-display text-lg font-semibold text-slate-900 mb-5">
          Informações da loja
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          <div className="pp-field">
            <label htmlFor="store-name" className="pp-label">
              Nome da loja
            </label>
            <input
              id="store-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              placeholder="Minha loja"
              className="pp-input"
            />
          </div>
          <div className="pp-field">
            <label htmlFor="store-email" className="pp-label">
              E-mail
            </label>
            <input
              id="store-email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="loja@email.com"
              className="pp-input"
            />
          </div>
          <div className="pp-field md:col-span-2">
            <label htmlFor="store-strategy" className="pp-label">
              Estratégia de cobrança
            </label>
            <select
              id="store-strategy"
              value={billingStrategy}
              onChange={(e) => setBillingStrategy(Number(e.target.value))}
              className="pp-input"
            >
              <option value={1}>Imediata</option>
              <option value={2}>Primeiro dia do mês</option>
            </select>
            <span className="pp-field-help">
              Define quando o ciclo de cobrança de uma recorrência é iniciado.
            </span>
          </div>
        </div>

        <div className="flex gap-3 mt-7 flex-wrap">
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? "Salvando..." : store ? "Atualizar loja" : "Criar loja"}
          </button>
          {store && (
            <button
              type="button"
              className="btn btn-danger"
              onClick={handleDelete}
              disabled={saving}
            >
              Excluir loja
            </button>
          )}
        </div>
      </form>
    </div>
  );
}
