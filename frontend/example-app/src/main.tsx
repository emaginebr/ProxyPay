import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Routes, Route, Navigate, useLocation } from "react-router-dom";
import { NAuthProvider, useProtectedRoute, useNAuth } from "nauth-react";
import "nauth-react/styles";
import { StoreProvider } from "./contexts/StoreContext";
import { BalanceProvider } from "./contexts/BalanceContext";
import { CustomerProvider } from "./contexts/CustomerContext";
import { InvoiceProvider } from "./contexts/InvoiceContext";
import { BillingProvider } from "./contexts/BillingContext";
import { useStore } from "./hooks/useStore";
import { Layout } from "./Layout";
import { Home } from "./pages/Home";
import { Docs } from "./pages/Docs";
import { Login } from "./pages/Login";
import { Signup } from "./pages/Signup";
import { DemoPix } from "./pages/DemoPix";
import { DemoInvoice } from "./pages/DemoInvoice";
import { DemoBilling } from "./pages/DemoBilling";
import { PaymentComplete } from "./pages/PaymentComplete";
import { Dashboard } from "./pages/Dashboard";
import { StorePage } from "./pages/StorePage";
import { CustomersPage } from "./pages/CustomersPage";
import { InvoicesPage } from "./pages/InvoicesPage";
import { BillingsPage } from "./pages/BillingsPage";
import "./index.css";

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  useProtectedRoute({ redirectTo: "/login" });
  return <>{children}</>;
}

function RequireStore({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useNAuth();
  const { store, loading } = useStore();
  const location = useLocation();

  if (!isAuthenticated) return <>{children}</>;
  if (loading)
    return (
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="pp-spinner" aria-label="Carregando" />
      </div>
    );

  const isStoreRoute = location.pathname === "/admin/store";
  const isPublicRoute = ["/", "/docs", "/login", "/signup"].includes(location.pathname) || location.pathname.startsWith("/demo");

  if (!store && !isStoreRoute && !isPublicRoute) {
    return <Navigate to="/admin/store" replace />;
  }

  return <>{children}</>;
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BrowserRouter basename="/">
      <NAuthProvider
        config={{
          apiUrl: import.meta.env.VITE_NAUTH_API_URL || "http://localhost:5000",
          language: "pt",
          enableFingerprinting: true,
          headers: {
            "X-Tenant-Id": import.meta.env.VITE_TENANT_ID,
          },
        }}
      >
        <StoreProvider>
          <BalanceProvider>
            <CustomerProvider>
              <InvoiceProvider>
                <BillingProvider>
                  <RequireStore>
                    <Routes>
                      <Route element={<Layout />}>
                        <Route index element={<Home />} />
                        <Route path="docs" element={<Docs />} />
                        <Route path="login" element={<Login />} />
                        <Route path="signup" element={<Signup />} />
                        <Route path="admin/store" element={<ProtectedRoute><StorePage /></ProtectedRoute>} />
                        <Route path="demo/pix" element={<DemoPix />} />
                        <Route path="demo/invoice" element={<DemoInvoice />} />
                        <Route path="demo/billing" element={<DemoBilling />} />
                        <Route path="demo/complete" element={<PaymentComplete />} />
                        <Route path="admin/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
                        <Route path="admin/customers" element={<ProtectedRoute><CustomersPage /></ProtectedRoute>} />
                        <Route path="admin/invoices" element={<ProtectedRoute><InvoicesPage /></ProtectedRoute>} />
                        <Route path="admin/billings" element={<ProtectedRoute><BillingsPage /></ProtectedRoute>} />
                      </Route>
                    </Routes>
                  </RequireStore>
                </BillingProvider>
              </InvoiceProvider>
            </CustomerProvider>
          </BalanceProvider>
        </StoreProvider>
      </NAuthProvider>
    </BrowserRouter>
  </StrictMode>,
);
