import { useSearchParams, Link } from "react-router-dom";
import { IconCheckCircle, IconXCircle } from "../components/Icon";

export function PaymentComplete() {
  const [params] = useSearchParams();
  const status = params.get("status");
  const from = params.get("from");

  const isSuccess = status === "success" || status === "paid";
  const backTo = from === "billing" ? "/demo/billing" : "/demo/invoice";
  const backLabel = from === "billing" ? "Demo Recorrência" : "Demo Fatura";

  return (
    <div className="pp-wrap max-w-xl mx-auto py-20 text-center animate-fade-in-up">
      <div
        className={`w-20 h-20 rounded-full grid place-items-center mx-auto mb-6 ${
          isSuccess ? "bg-emerald-50 text-emerald-600" : "bg-red-50 text-red-600"
        }`}
      >
        {isSuccess ? <IconCheckCircle size={40} /> : <IconXCircle size={40} />}
      </div>

      <h1 className="font-display text-3xl font-bold text-slate-900 mb-3">
        {isSuccess ? "Pagamento confirmado" : "Pagamento cancelado"}
      </h1>

      <p className="text-base text-slate-600 mb-8 leading-relaxed max-w-lg mx-auto">
        {isSuccess
          ? "Seu pagamento foi processado com sucesso. Você pode fechar esta página ou voltar para a demo."
          : "O pagamento foi cancelado ou não foi concluído. Você pode tentar novamente quando quiser."}
      </p>

      {status && (
        <span
          className={`pp-badge ${isSuccess ? "pp-badge-success" : "pp-badge-danger"} mb-8`}
        >
          Status: {status}
        </span>
      )}

      <div className="flex gap-3 justify-center flex-wrap">
        <Link to={backTo} className="btn btn-primary">
          Voltar para {backLabel}
        </Link>
        <Link to="/" className="btn btn-secondary">
          Início
        </Link>
      </div>
    </div>
  );
}
