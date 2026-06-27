import { useNavigate, Link } from "react-router-dom";
import { RegisterForm } from "nauth-react";

export function Signup() {
  const navigate = useNavigate();

  return (
    <div className="pp-auth-shell relative">
      <div aria-hidden="true" className="pp-aurora" />
      <div className="pp-auth-card relative animate-fade-in-up">
        <div className="text-center mb-7">
          <div className="inline-flex items-center gap-2 mb-3">
            <span className="pp-brand-mark" aria-hidden="true" />
            <span className="font-display font-bold text-slate-900 text-lg tracking-tight">
              ProxyPay
            </span>
          </div>
          <h1 className="font-display text-2xl font-bold text-slate-900 mb-1.5">
            Criar conta
          </h1>
          <p className="text-sm text-slate-500">
            Comece a aceitar pagamentos em minutos
          </p>
        </div>
        <RegisterForm onSuccess={() => navigate("/admin/store")} />
        <p className="text-center mt-5 text-[13px] text-slate-500">
          Já tem conta?{" "}
          <Link to="/login" className="text-indigo-600 font-semibold hover:text-indigo-700">
            Entrar
          </Link>
        </p>
      </div>
    </div>
  );
}
