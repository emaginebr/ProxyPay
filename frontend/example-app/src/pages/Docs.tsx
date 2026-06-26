import { CodeBlock } from "../components/CodeBlock";
import { IconBook } from "../components/Icon";

export function Docs() {
  return (
    <div className="pp-wrap max-w-[860px] mx-auto py-12 md:py-16 animate-fade-in-up">
      <header className="mb-10 flex items-start gap-4">
        <span className="w-12 h-12 rounded-lg bg-indigo-50 text-indigo-600 grid place-items-center shrink-0">
          <IconBook size={22} />
        </span>
        <div>
          <h1 className="font-display text-4xl font-bold text-slate-900 mb-2">
            Documentação
          </h1>
          <p className="text-base text-slate-600 leading-relaxed max-w-[62ch]">
            Guia completo para integrar <strong>proxypay-react</strong> no seu projeto em
            poucos minutos.
          </p>
        </div>
      </header>

      <Section id="installation" title="1. Instalação">
        <CodeBlock language="bash" code="npm install proxypay-react" />
        <Para>
          O pacote requer <Inline>react &gt;= 18</Inline> e{" "}
          <Inline>react-dom &gt;= 18</Inline> como peer dependencies.
        </Para>
      </Section>

      <Section id="provider" title="2. ProxyPayProvider">
        <Para>
          Envolva sua aplicação (ou a parte que usa pagamentos) com o provider. Ele entrega a
          configuração para todos os componentes filhos.
        </Para>
        <CodeBlock
          filename="App.tsx"
          code={`import { ProxyPayProvider } from "proxypay-react";

function App() {
  return (
    <ProxyPayProvider
      config={{
        baseUrl: "https://api.sandbox.proxypay.co.ao",
        clientId: "50169143aa0e46b593dcf43adec0464e",
        tenantId: "minha-empresa",
      }}
    >
      {/* seus componentes aqui */}
    </ProxyPayProvider>
  );
}`}
        />

        <SubHeading>ProxyPayConfig</SubHeading>
        <PropsTable
          rows={[
            ["baseUrl", "string", "URL base da API ProxyPay"],
            ["clientId", "string", "Client ID (hex de 32 caracteres)"],
            ["tenantId", "string", "Identificador do tenant (header X-Tenant-Id)"],
          ]}
        />
      </Section>

      <Section id="pix-payment" title="3. PixPayment">
        <Para>
          Componente principal para pagamentos via PIX. Renderiza um botão (via{" "}
          <Inline>children</Inline>) e, ao clicar, abre um modal com o QR Code, campo para
          copiar o código e timer de expiração. Polling de status automático.
        </Para>
        <CodeBlock
          filename="Checkout.tsx"
          code={`import {
  PixPayment,
  type CustomerInfo,
  type InvoiceItem,
} from "proxypay-react";

const customer: CustomerInfo = {
  name: "Maria Santos",
  documentId: "98765432100",
  cellphone: "21988887777",
  email: "maria@email.com",
};

const items: InvoiceItem[] = [
  {
    id: "COURSE-001",
    description: "Curso React avançado",
    quantity: 1,
    unitPrice: 297.00,
    discount: 0,
  },
];

function Checkout() {
  return (
    <PixPayment
      customer={customer}
      items={items}
      pollInterval={10000}
      modalTitle="Complete o pagamento"
      onSuccess={(status) => {
        window.location.href = "/obrigado";
      }}
      onError={(error) => alert("Erro: " + error.message)}
      onStatusChange={(status) => {
        console.log("Status:", status.statusText);
      }}
    >
      <button>Comprar por R$ 297,00</button>
    </PixPayment>
  );
}`}
        />

        <SubHeading>Props</SubHeading>
        <PropsTable
          withDefault
          rows={[
            ["customer", "CustomerInfo", "\u2014", "Dados do cliente (obrigat\u00f3rio)"],
            ["items", "InvoiceItem[]", "\u2014", "Itens do pagamento (obrigat\u00f3rio)"],
            ["pollInterval", "number", "10000", "Intervalo de polling em ms"],
            ["modalTitle", "string", "\"Pagamento PIX\"", "T\u00edtulo do modal"],
            ["onSuccess", "(status) => void", "\u2014", "Callback quando o pagamento \u00e9 confirmado"],
            ["onError", "(error) => void", "\u2014", "Callback em caso de erro"],
            ["onStatusChange", "(status) => void", "\u2014", "Callback a cada checagem"],
            ["children", "ReactNode", "\u2014", "Bot\u00e3o que dispara o pagamento"],
            ["overlayClassName", "string", "\u2014", "Classe CSS do overlay"],
            ["modalClassName", "string", "\u2014", "Classe CSS do modal"],
          ]}
        />
      </Section>

      <Section id="use-proxy-pay" title="4. Hook useProxyPay">
        <Para>
          Para cenários avançados, use o hook diretamente para acessar os métodos da API.
        </Para>
        <CodeBlock
          filename="CustomPayment.tsx"
          code={`import { useProxyPay } from "proxypay-react";

function CustomPayment() {
  const { createQRCode, checkQRCodeStatus } = useProxyPay();

  async function handlePay() {
    const qr = await createQRCode(customer, items);
    console.log(qr.brCode);         // código PIX
    console.log(qr.brCodeBase64);   // imagem base64

    const status = await checkQRCodeStatus(qr.invoiceId);
    console.log(status.paid);       // true/false
    console.log(status.statusText); // "Pending", "Paid", etc.
  }

  return <button onClick={handlePay}>Pagar</button>;
}`}
        />
      </Section>

      <Section id="interfaces" title="5. Interfaces">
        <SubHeading>CustomerInfo</SubHeading>
        <CodeBlock
          language="typescript"
          code={`interface CustomerInfo {
  name: string;        // Nome completo
  documentId: string;  // CPF (11 dígitos, sem formatação)
  cellphone: string;   // Telefone
  email: string;       // E-mail
}`}
        />

        <SubHeading>InvoiceItem</SubHeading>
        <CodeBlock
          language="typescript"
          code={`interface InvoiceItem {
  id: string;          // Identificador do produto
  description: string; // Descrição
  quantity: number;    // Quantidade (> 0)
  unitPrice: number;   // Preço unitário em BRL
  discount: number;    // Desconto (>= 0)
}`}
        />

        <SubHeading>QRCodeResponse</SubHeading>
        <CodeBlock
          language="typescript"
          code={`interface QRCodeResponse {
  invoiceId: number;
  invoiceNumber: string;
  brCode: string;         // código PIX copia-e-cola
  brCodeBase64: string;   // imagem do QR como PNG base64
  expiredAt?: string;
}`}
        />

        <SubHeading>QRCodeStatusResponse</SubHeading>
        <CodeBlock
          language="typescript"
          code={`interface QRCodeStatusResponse {
  invoiceId: number;
  invoiceNumber: string;
  paid: boolean;
  status: number;               // 1-6 (enum)
  statusText: InvoiceStatus;    // "Pending" | "Paid" | ...
  expiresAt?: string;
}

type InvoiceStatus =
  | "Pending" | "Sent" | "Paid"
  | "Overdue" | "Cancelled" | "Expired";`}
        />
      </Section>
    </div>
  );
}

function Section({
  id,
  title,
  children,
}: {
  id: string;
  title: string;
  children: React.ReactNode;
}) {
  return (
    <section id={id} className="mb-14">
      <h2 className="font-display text-2xl font-bold text-slate-900 pb-3 mb-5 border-b border-slate-200 relative">
        {title}
        <span
          aria-hidden="true"
          className="absolute -bottom-[1px] left-0 w-12 h-0.5 rounded-sm"
          style={{ background: "var(--gradient-brand)" }}
        />
      </h2>
      <div className="flex flex-col gap-4 text-slate-600 leading-relaxed">{children}</div>
    </section>
  );
}

function SubHeading({ children }: { children: React.ReactNode }) {
  return (
    <h3 className="font-display text-base font-bold text-slate-900 mt-7 mb-2">
      {children}
    </h3>
  );
}

function Para({ children }: { children: React.ReactNode }) {
  return <p className="text-slate-600 leading-relaxed">{children}</p>;
}

function Inline({ children }: { children: React.ReactNode }) {
  return <code className="pp-code-inline">{children}</code>;
}

function PropsTable({
  rows,
  withDefault = false,
}: {
  rows: string[][];
  withDefault?: boolean;
}) {
  return (
    <div className="pp-table-wrap my-3">
      <table className="pp-table">
        <thead>
          <tr>
            <th scope="col">Prop</th>
            <th scope="col">Tipo</th>
            {withDefault && <th scope="col">Padrão</th>}
            <th scope="col">Descrição</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r, i) => (
            <tr key={i}>
              <td>
                <code className="pp-code-inline">{r[0]}</code>
              </td>
              <td className="td-mono">{r[1]}</td>
              {withDefault && <td className="td-mono">{r[2]}</td>}
              <td className="text-slate-600">{withDefault ? r[3] : r[2]}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
