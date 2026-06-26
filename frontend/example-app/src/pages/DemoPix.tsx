import { useState } from "react";
import {
  ProxyPayProvider,
  PixPayment,
  type CustomerInfo,
  type InvoiceItem,
  type QRCodeStatusResponse,
} from "proxypay-react";
import { IconZap, IconActivity, IconAlert } from "../components/Icon";
import { generateRandomCpf } from "../utils/cpf";

const config = {
  baseUrl: import.meta.env.VITE_API_BASE_URL,
  clientId: import.meta.env.VITE_CLIENT_ID,
  tenantId: import.meta.env.VITE_TENANT_ID,
};

const brl = (v: number) =>
  v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

export function DemoPix() {
  const [customer, setCustomer] = useState<CustomerInfo>({
    name: "João da Silva",
    documentId: "12345678900",
    cellphone: "11999999999",
    email: "joao@email.com",
  });

  const [items, setItems] = useState<InvoiceItem[]>([
    {
      id: "PROD-001",
      description: "Camiseta ProxyPay",
      quantity: 1,
      unitPrice: 49.9,
      discount: 0,
    },
  ]);

  const [pollInterval, setPollInterval] = useState(10000);
  const [lastStatus, setLastStatus] = useState<QRCodeStatusResponse | null>(null);
  const [log, setLog] = useState<string[]>([]);

  const addLog = (msg: string) => {
    const time = new Date().toLocaleTimeString("pt-BR");
    setLog((prev) => [`[${time}] ${msg}`, ...prev].slice(0, 20));
  };

  const total = items.reduce(
    (sum, item) => sum + item.quantity * item.unitPrice - item.discount,
    0
  );

  const updateItem = (field: keyof InvoiceItem, value: string) => {
    setItems((prev) => {
      const item = { ...prev[0] };
      if (field === "quantity" || field === "unitPrice" || field === "discount") {
        (item as Record<string, unknown>)[field] = parseFloat(value) || 0;
      } else {
        (item as Record<string, unknown>)[field] = value;
      }
      return [item];
    });
  };

  return (
    <ProxyPayProvider config={config}>
      <div className="pp-wrap py-10 md:py-14 animate-fade-in-up">
        <header className="flex items-start gap-4 mb-4">
          <span className="w-12 h-12 rounded-lg bg-indigo-50 text-indigo-600 grid place-items-center shrink-0">
            <IconZap size={22} />
          </span>
          <div>
            <h1 className="font-display text-3xl font-bold text-slate-900 mb-1.5">
              Demo PIX
            </h1>
            <p className="text-sm text-slate-500 max-w-[62ch]">
              Configure os dados abaixo e clique em pagar para testar o componente{" "}
              <code className="pp-code-inline">&lt;PixPayment /&gt;</code> em tempo real.
            </p>
          </div>
        </header>

        <div className="pp-alert pp-alert-info mb-6" role="status">
          <IconAlert size={18} />
          <div>
            Ambiente de sandbox. Nenhum valor real será cobrado. O QR Code é gerado pela API
            ProxyPay.
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Form */}
          <div className="pp-card p-6 md:p-8">
            <FieldSection title="Dados do cliente">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <Field label="Nome">
                  <input
                    type="text"
                    value={customer.name}
                    onChange={(e) => setCustomer((c) => ({ ...c, name: e.target.value }))}
                    className="pp-input"
                  />
                </Field>
                <div className="pp-field">
                  <div className="flex items-center justify-between gap-2 mb-1.5">
                    <label className="pp-label !mb-0">CPF</label>
                    <button
                      type="button"
                      onClick={() =>
                        setCustomer((c) => ({ ...c, documentId: generateRandomCpf() }))
                      }
                      className="text-[11px] font-semibold text-indigo-600 hover:text-indigo-700"
                    >
                      Gerar aleatório
                    </button>
                  </div>
                  <input
                    type="text"
                    value={customer.documentId}
                    onChange={(e) =>
                      setCustomer((c) => ({ ...c, documentId: e.target.value }))
                    }
                    maxLength={11}
                    className="pp-input"
                  />
                </div>
                <Field label="Telefone">
                  <input
                    type="text"
                    value={customer.cellphone}
                    onChange={(e) =>
                      setCustomer((c) => ({ ...c, cellphone: e.target.value }))
                    }
                    className="pp-input"
                  />
                </Field>
                <Field label="E-mail">
                  <input
                    type="email"
                    value={customer.email}
                    onChange={(e) => setCustomer((c) => ({ ...c, email: e.target.value }))}
                    className="pp-input"
                  />
                </Field>
              </div>
            </FieldSection>

            <FieldSection title="Item">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <Field label="Descrição">
                  <input
                    type="text"
                    value={items[0].description}
                    onChange={(e) => updateItem("description", e.target.value)}
                    className="pp-input"
                  />
                </Field>
                <Field label="Preço (R$)">
                  <input
                    type="number"
                    step="0.01"
                    value={items[0].unitPrice}
                    onChange={(e) => updateItem("unitPrice", e.target.value)}
                    className="pp-input"
                  />
                </Field>
                <Field label="Quantidade">
                  <input
                    type="number"
                    min="1"
                    value={items[0].quantity}
                    onChange={(e) => updateItem("quantity", e.target.value)}
                    className="pp-input"
                  />
                </Field>
                <Field label="Desconto (R$)">
                  <input
                    type="number"
                    step="0.01"
                    value={items[0].discount}
                    onChange={(e) => updateItem("discount", e.target.value)}
                    className="pp-input"
                  />
                </Field>
              </div>
            </FieldSection>

            <FieldSection title="Configuração">
              <Field label="Intervalo de polling (ms)" help="Tempo entre checagens de status.">
                <input
                  type="number"
                  step="1000"
                  min="3000"
                  value={pollInterval}
                  onChange={(e) => setPollInterval(parseInt(e.target.value) || 10000)}
                  className="pp-input"
                />
              </Field>
            </FieldSection>

            <div className="mt-6 flex items-center justify-between gap-4 flex-wrap pb-5 border-b border-slate-200">
              <span className="text-sm text-slate-500 font-medium">Total</span>
              <span className="font-display text-2xl font-bold text-slate-900 tabular">
                {brl(total)}
              </span>
            </div>

            <PixPayment
              customer={customer}
              items={items}
              pollInterval={pollInterval}
              modalTitle="Pagamento PIX - Demo"
              onSuccess={(status) => {
                setLastStatus(status);
                addLog(`Pago! Fatura: ${status.invoiceNumber}`);
              }}
              onError={(err) => addLog(`Erro: ${err.message}`)}
              onStatusChange={(status) => addLog(`Status: ${status.statusText}`)}
            >
              <button type="button" className="btn btn-primary btn-lg w-full mt-5">
                Pagar {brl(total)} com PIX
              </button>
            </PixPayment>
          </div>

          {/* Log */}
          <div className="pp-card p-6 md:p-8 flex flex-col">
            <div className="flex items-center justify-between mb-5">
              <h2 className="font-display text-lg font-semibold text-slate-900 inline-flex items-center gap-2">
                <IconActivity size={18} className="text-indigo-600" />
                Eventos
              </h2>
              {lastStatus && (
                <span className="pp-badge pp-badge-success">Pago</span>
              )}
            </div>

            {lastStatus && (
              <div className="pp-alert pp-alert-success mb-4">
                <strong>Último status:</strong>&nbsp;{lastStatus.statusText}
              </div>
            )}

            <div className="flex flex-col gap-1.5 max-h-[440px] overflow-y-auto">
              {log.length === 0 && (
                <p className="text-sm text-slate-400 italic">
                  Clique em pagar para ver eventos aqui.
                </p>
              )}
              {log.map((entry, i) => (
                <div
                  key={i}
                  className="font-mono text-[11.5px] px-3 py-2 bg-slate-50 rounded-md text-slate-700 border border-slate-100 leading-snug"
                >
                  {entry}
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </ProxyPayProvider>
  );
}

function FieldSection({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <section className="mb-6 last:mb-0">
      <h2 className="font-display text-[11px] font-bold text-slate-500 uppercase tracking-wider mb-3">
        {title}
      </h2>
      {children}
    </section>
  );
}

function Field({
  label,
  help,
  children,
}: {
  label: string;
  help?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="pp-field">
      <label className="pp-label">{label}</label>
      {children}
      {help && <span className="pp-field-help">{help}</span>}
    </div>
  );
}
