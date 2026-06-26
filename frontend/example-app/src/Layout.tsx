import { useState, useRef, useEffect } from "react";
import { NavLink, Outlet, useNavigate, useLocation, Link } from "react-router-dom";
import { useAuth } from "nauth-react";
import {
  IconLayout,
  IconStore,
  IconUsers,
  IconFile,
  IconRepeat,
  IconZap,
  IconMenu,
  IconX,
  IconChevronDown,
  IconChevronRight,
  IconLogout,
  IconBook,
  IconHome,
  IconSparkle,
} from "./components/Icon";

const adminLinks = [
  { to: "/admin/dashboard", Icon: IconLayout, label: "Dashboard" },
  { to: "/admin/store",     Icon: IconStore,  label: "Loja" },
  { to: "/admin/customers", Icon: IconUsers,  label: "Clientes" },
  { to: "/admin/invoices",  Icon: IconFile,   label: "Faturas" },
  { to: "/admin/billings",  Icon: IconRepeat, label: "Recorrência" },
];

const demoLinks = [
  { to: "/demo/pix",     Icon: IconZap,  label: "PIX" },
  { to: "/demo/invoice", Icon: IconFile, label: "Fatura" },
  { to: "/demo/billing", Icon: IconRepeat, label: "Recorrente" },
];

function NavDropdown({ label, items }: { label: string; items: typeof demoLinks }) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const location = useLocation();
  const isActive = items.some((item) => location.pathname === item.to);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  return (
    <div className="relative" ref={ref}>
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className={`inline-flex items-center gap-1.5 px-3.5 py-2 rounded-sm text-sm font-medium transition ${
          isActive ? "text-primary" : "text-slate-600 hover:text-slate-900 hover:bg-slate-100"
        }`}
        aria-haspopup="menu"
        aria-expanded={open}
      >
        {label}
        <IconChevronDown size={14} />
      </button>
      {open && (
        <div
          role="menu"
          className="absolute top-full mt-2 right-0 min-w-[200px] bg-white border border-slate-200 rounded-md shadow-lg p-1.5 z-40 animate-fade-in-up"
        >
          {items.map(({ to, Icon, label: itemLabel }) => (
            <NavLink
              key={to}
              to={to}
              onClick={() => setOpen(false)}
              className={({ isActive: a }) =>
                `flex items-center gap-2.5 px-3 py-2 rounded-sm text-sm font-medium transition ${
                  a ? "bg-indigo-50 text-indigo-700" : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
                }`
              }
            >
              <Icon size={16} />
              {itemLabel}
            </NavLink>
          ))}
        </div>
      )}
    </div>
  );
}

function PublicTopbar() {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate("/login");
  };

  return (
    <header className="pp-topbar">
      <div className="pp-wrap pp-topbar-inner">
        <NavLink to="/" className="pp-brand">
          <span className="pp-brand-mark" aria-hidden="true" />
          ProxyPay
        </NavLink>

        <nav className="pp-nav-main hidden md:flex" aria-label="Navegação principal">
          <NavLink to="/" end>
            Produto
          </NavLink>
          <NavDropdown label="Demos" items={demoLinks} />
          <NavLink to="/docs">Documentação</NavLink>
          {isAuthenticated && <NavLink to="/admin/dashboard">Painel</NavLink>}
        </nav>

        <div className="flex items-center gap-2">
          {isAuthenticated && user ? (
            <>
              <span className="hidden sm:flex pp-user-chip">
                <span className="pp-user-avatar" aria-hidden="true">
                  {(user.name || user.email || "?").slice(0, 1).toUpperCase()}
                </span>
                <span className="font-medium text-slate-700">{user.name || user.email}</span>
              </span>
              <button
                type="button"
                onClick={handleLogout}
                className="btn btn-ghost btn-sm"
                aria-label="Sair"
              >
                <IconLogout size={14} />
                <span className="hidden sm:inline">Sair</span>
              </button>
            </>
          ) : (
            <>
              <NavLink to="/login" className="btn btn-ghost btn-sm">
                Entrar
              </NavLink>
              <NavLink to="/signup" className="btn btn-primary btn-sm">
                Começar grátis
              </NavLink>
            </>
          )}
        </div>
      </div>
    </header>
  );
}

function PublicFooter() {
  return (
    <footer className="relative z-10 mt-20 border-t border-slate-200 bg-white">
      <div className="pp-wrap py-16">
        <div className="grid grid-cols-1 md:grid-cols-[1.4fr_repeat(3,1fr)] gap-8 mb-12">
          <div>
            <Link to="/" className="pp-brand">
              <span className="pp-brand-mark" aria-hidden="true" />
              ProxyPay
            </Link>
            <p className="mt-3 text-sm text-slate-600 max-w-[32ch]">
              Biblioteca React para pagamentos brasileiros. PIX, boleto, recorrência — tudo em
              três componentes.
            </p>
          </div>
          <FooterCol
            title="Produto"
            links={[
              { label: "PIX", href: "/demo/pix" },
              { label: "Fatura", href: "/demo/invoice" },
              { label: "Recorrência", href: "/demo/billing" },
              { label: "Painel", href: "/admin/dashboard" },
            ]}
          />
          <FooterCol
            title="Developers"
            links={[
              { label: "Documentação", href: "/docs" },
              { label: "GitHub", href: "https://github.com/emaginebr/proxypay-react", external: true },
              { label: "Changelog", href: "https://github.com/emaginebr/proxypay-react/releases", external: true },
            ]}
          />
          <FooterCol
            title="Empresa"
            links={[
              { label: "Emagine", href: "https://emagine.com.br", external: true },
              { label: "Contato", href: "mailto:hello@emagine.com.br", external: true },
            ]}
          />
        </div>
        <div className="flex items-center justify-between pt-6 border-t border-slate-200 text-xs text-slate-500 flex-wrap gap-3">
          <span>&copy; 2026 Emagine &middot; MIT License</span>
          <span className="inline-flex items-center gap-1.5">
            Feito com <IconSparkle size={12} className="text-accent" /> em São Paulo
          </span>
        </div>
      </div>
    </footer>
  );
}

function FooterCol({
  title,
  links,
}: {
  title: string;
  links: { label: string; href: string; external?: boolean }[];
}) {
  return (
    <div>
      <h4 className="font-display text-[13px] font-bold text-slate-900 mb-4">{title}</h4>
      <ul className="space-y-2">
        {links.map((l) => (
          <li key={l.label}>
            {l.external ? (
              <a
                href={l.href}
                target="_blank"
                rel="noopener noreferrer"
                className="text-sm text-slate-600 hover:text-primary"
              >
                {l.label}
              </a>
            ) : (
              <Link to={l.href} className="text-sm text-slate-600 hover:text-primary">
                {l.label}
              </Link>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}

function AdminShell() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleLogout = async () => {
    await logout();
    navigate("/login");
  };

  const currentLink = adminLinks.find((l) => location.pathname.startsWith(l.to));

  return (
    <div className="flex flex-1 relative">
      {/* Desktop sidebar */}
      <aside className="hidden lg:flex pp-sidebar" aria-label="Navegação do painel">
        <Link to="/" className="pp-brand px-2 mb-4">
          <span className="pp-brand-mark" aria-hidden="true" />
          ProxyPay
        </Link>

        <span className="pp-sidebar-section">Geral</span>
        {adminLinks.map(({ to, Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) => (isActive ? "active" : "")}
          >
            <span className="pp-sidebar-ico">
              <Icon size={18} />
            </span>
            {label}
          </NavLink>
        ))}

        <span className="pp-sidebar-section">Playground</span>
        {demoLinks.map(({ to, Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) => (isActive ? "active" : "")}
          >
            <span className="pp-sidebar-ico">
              <Icon size={18} />
            </span>
            {label}
          </NavLink>
        ))}

        <div className="pp-sidebar-footer-card">
          <div className="flex items-center gap-2 text-xs font-semibold text-indigo-700 uppercase tracking-wider mb-2">
            <IconBook size={12} />
            Documentação
          </div>
          <p className="text-xs text-slate-600 leading-relaxed mb-3">
            Aprenda como integrar ProxyPay no seu projeto em 5 minutos.
          </p>
          <Link to="/docs" className="btn btn-secondary btn-sm w-full">
            Ver guia
          </Link>
        </div>
      </aside>

      {/* Mobile drawer */}
      {mobileOpen && (
        <div className="lg:hidden fixed inset-0 z-50 flex" role="dialog" aria-modal="true">
          <div
            className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm"
            onClick={() => setMobileOpen(false)}
          />
          <aside className="relative flex flex-col bg-white w-72 max-w-[85vw] h-full p-4 shadow-xl animate-fade-in-up overflow-y-auto">
            <div className="flex items-center justify-between mb-4 px-2">
              <Link to="/" className="pp-brand">
                <span className="pp-brand-mark" aria-hidden="true" />
                ProxyPay
              </Link>
              <button
                type="button"
                onClick={() => setMobileOpen(false)}
                className="btn btn-ghost btn-sm"
                aria-label="Fechar menu"
              >
                <IconX size={18} />
              </button>
            </div>
            <div className="pp-sidebar !static !w-full !border-none !p-0 !h-auto">
              <span className="pp-sidebar-section">Geral</span>
              {adminLinks.map(({ to, Icon, label }) => (
                <NavLink
                  key={to}
                  to={to}
                  onClick={() => setMobileOpen(false)}
                  className={({ isActive }) => (isActive ? "active" : "")}
                >
                  <span className="pp-sidebar-ico">
                    <Icon size={18} />
                  </span>
                  {label}
                </NavLink>
              ))}
              <span className="pp-sidebar-section">Playground</span>
              {demoLinks.map(({ to, Icon, label }) => (
                <NavLink
                  key={to}
                  to={to}
                  onClick={() => setMobileOpen(false)}
                  className={({ isActive }) => (isActive ? "active" : "")}
                >
                  <span className="pp-sidebar-ico">
                    <Icon size={18} />
                  </span>
                  {label}
                </NavLink>
              ))}
            </div>
          </aside>
        </div>
      )}

      {/* Content area */}
      <div className="flex-1 min-w-0 flex flex-col">
        <div className="pp-admin-topbar">
          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => setMobileOpen(true)}
              className="btn btn-ghost btn-sm lg:hidden"
              aria-label="Abrir menu"
            >
              <IconMenu size={18} />
            </button>
            <nav aria-label="Breadcrumb" className="pp-crumb">
              <Link to="/admin/dashboard" className="hover:text-slate-900">
                <IconHome size={14} />
              </Link>
              <IconChevronRight size={12} className="text-slate-400" />
              <span className="pp-crumb-current">{currentLink?.label ?? "Painel"}</span>
            </nav>
          </div>
          <div className="flex items-center gap-2">
            {user && (
              <span className="pp-user-chip">
                <span className="pp-user-avatar" aria-hidden="true">
                  {(user.name || user.email || "?").slice(0, 1).toUpperCase()}
                </span>
                <span className="hidden sm:inline text-slate-700 font-medium">
                  {user.name || user.email}
                </span>
              </span>
            )}
            <button
              type="button"
              onClick={handleLogout}
              className="btn btn-ghost btn-sm"
              aria-label="Sair"
            >
              <IconLogout size={14} />
            </button>
          </div>
        </div>
        <main className="flex-1">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export function Layout() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  const isAdminRoute = location.pathname.startsWith("/admin");
  const isAuthRoute = location.pathname === "/login" || location.pathname === "/signup";
  const isLanding = location.pathname === "/";

  if (isAdminRoute && isAuthenticated) {
    return (
      <div className="flex flex-col min-h-screen bg-slate-50">
        <AdminShell />
      </div>
    );
  }

  return (
    <div className="flex flex-col min-h-screen relative">
      {isLanding && (
        <>
          <div className="pp-aurora" aria-hidden="true" />
          <div className="pp-grid-bg" aria-hidden="true" />
        </>
      )}
      <PublicTopbar />
      <main className="flex-1 relative z-10">
        <Outlet />
      </main>
      {!isAuthRoute && <PublicFooter />}
    </div>
  );
}
