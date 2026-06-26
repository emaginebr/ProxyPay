import { useState, useEffect, useCallback, useRef } from "react";
import { createPortal } from "react-dom";
import { useProxyPay } from "../hooks/useProxyPay";
import type {
  PixPaymentProps,
  QRCodeResponse,
  QRCodeStatusResponse,
} from "../types/payment";

type ModalState =
  | { step: "closed" }
  | { step: "loading" }
  | { step: "qrcode"; data: QRCodeResponse }
  | { step: "error"; error: Error };

const DEFAULT_POLL_INTERVAL = 10000;

export function PixPayment({
  customer,
  items,
  onSuccess,
  onError,
  onStatusChange,
  pollInterval = DEFAULT_POLL_INTERVAL,
  modalTitle = "Pagamento PIX",
  children,
  overlayClassName,
  modalClassName,
}: PixPaymentProps) {
  const { createQRCode, checkQRCodeStatus } = useProxyPay();
  const [state, setState] = useState<ModalState>({ step: "closed" });
  const [copied, setCopied] = useState(false);
  const [timeLeft, setTimeLeft] = useState<string | null>(null);
  const pollingRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const expirationRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const stopTimers = useCallback(() => {
    if (pollingRef.current) {
      clearInterval(pollingRef.current);
      pollingRef.current = null;
    }
    if (expirationRef.current) {
      clearInterval(expirationRef.current);
      expirationRef.current = null;
    }
  }, []);

  const closeModal = useCallback(() => {
    stopTimers();
    setState({ step: "closed" });
    setCopied(false);
    setTimeLeft(null);
  }, [stopTimers]);

  const handleOpen = useCallback(async () => {
    setState({ step: "loading" });
    try {
      const data = await createQRCode(customer, items);
      setState({ step: "qrcode", data });
    } catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      setState({ step: "error", error });
      onError?.(error);
    }
  }, [createQRCode, customer, items, onError]);

  // Polling
  useEffect(() => {
    if (state.step !== "qrcode") return;

    const { invoiceId } = state.data;

    pollingRef.current = setInterval(async () => {
      try {
        const status = await checkQRCodeStatus(invoiceId);
        onStatusChange?.(status);

        if (status.paid) {
          stopTimers();
          onSuccess?.(status);
        } else if (
          status.statusText === "Expired" ||
          status.statusText === "Cancelled"
        ) {
          stopTimers();
          const error = new Error(
            `Pagamento ${status.statusText.toLowerCase()}`,
          );
          setState({ step: "error", error });
          onError?.(error);
        }
      } catch {
        // Retry silently on network errors
      }
    }, pollInterval);

    return stopTimers;
  }, [
    state.step,
    state.step === "qrcode" ? state.data.invoiceId : null,
    pollInterval,
    checkQRCodeStatus,
    onSuccess,
    onError,
    onStatusChange,
    stopTimers,
  ]);

  // Countdown
  useEffect(() => {
    if (state.step !== "qrcode" || !state.data.expiredAt) {
      setTimeLeft(null);
      return;
    }

    const expiry = new Date(state.data.expiredAt).getTime();

    const tick = () => {
      const diff = expiry - Date.now();
      if (diff <= 0) {
        setTimeLeft("Expirado");
        stopTimers();
        const error = new Error("QR Code expirado");
        setState({ step: "error", error });
        onError?.(error);
        return;
      }
      const mins = Math.floor(diff / 60000);
      const secs = Math.floor((diff % 60000) / 1000);
      setTimeLeft(`${mins}:${secs.toString().padStart(2, "0")}`);
    };

    tick();
    expirationRef.current = setInterval(tick, 1000);

    return () => {
      if (expirationRef.current) {
        clearInterval(expirationRef.current);
        expirationRef.current = null;
      }
    };
  }, [
    state.step,
    state.step === "qrcode" ? state.data.expiredAt : null,
    stopTimers,
    onError,
  ]);

  const handleCopy = useCallback(async (brCode: string) => {
    try {
      await navigator.clipboard.writeText(brCode);
    } catch {
      const textarea = document.createElement("textarea");
      textarea.value = brCode;
      textarea.style.position = "fixed";
      textarea.style.opacity = "0";
      document.body.appendChild(textarea);
      textarea.select();
      document.execCommand("copy");
      document.body.removeChild(textarea);
    }
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }, []);

  const handleOverlayClick = useCallback(
    (e: React.MouseEvent) => {
      if (e.target === e.currentTarget) closeModal();
    },
    [closeModal],
  );

  // Trigger button
  const trigger = (
    <div onClick={handleOpen} style={{ display: "inline-block" }}>
      {children}
    </div>
  );

  if (state.step === "closed") return trigger;

  // Modal
  const modal = createPortal(
    <div
      className={overlayClassName}
      onClick={handleOverlayClick}
      style={{
        position: "fixed",
        inset: 0,
        zIndex: 9999,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: "rgba(0, 0, 0, 0.5)",
        ...(!overlayClassName ? {} : undefined),
      }}
    >
      <div
        className={modalClassName}
        style={
          !modalClassName
            ? {
                background: "#fff",
                borderRadius: "16px",
                padding: "40px 32px",
                maxWidth: "440px",
                width: "90%",
                position: "relative",
                textAlign: "center",
                fontFamily: "system-ui, -apple-system, sans-serif",
                boxShadow: "0 25px 60px rgba(0, 0, 0, 0.15)",
              }
            : undefined
        }
      >
        {/* Close button */}
        <button
          onClick={closeModal}
          style={{
            position: "absolute",
            top: "16px",
            right: "16px",
            background: "#f1f5f9",
            border: "none",
            fontSize: "16px",
            cursor: "pointer",
            color: "#64748b",
            lineHeight: 1,
            width: "32px",
            height: "32px",
            borderRadius: "8px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
          aria-label="Fechar"
        >
          ✕
        </button>

        {/* Loading */}
        {state.step === "loading" && (
          <div style={{ padding: "48px 0" }}>
            <div style={{
              width: "40px",
              height: "40px",
              border: "3px solid #e2e8f0",
              borderTopColor: "#10b981",
              borderRadius: "50%",
              animation: "proxypay-spin 0.8s linear infinite",
              margin: "0 auto 16px",
            }} />
            <style>{`@keyframes proxypay-spin { to { transform: rotate(360deg) } }`}</style>
            <p style={{ color: "#64748b", fontSize: "15px", margin: 0 }}>
              Gerando QR Code...
            </p>
          </div>
        )}

        {/* Error */}
        {state.step === "error" && (
          <div style={{ padding: "16px 0" }}>
            <div style={{
              width: "48px",
              height: "48px",
              borderRadius: "50%",
              background: "#fef2f2",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              margin: "0 auto 16px",
              fontSize: "24px",
            }}>
              !
            </div>
            <p style={{ color: "#dc2626", marginBottom: "20px", fontSize: "15px" }}>
              {state.error.message}
            </p>
            <button
              onClick={handleOpen}
              style={{
                background: "#10b981",
                color: "#fff",
                border: "none",
                padding: "12px 28px",
                borderRadius: "10px",
                cursor: "pointer",
                fontSize: "14px",
                fontWeight: 600,
              }}
            >
              Tentar novamente
            </button>
          </div>
        )}

        {/* QR Code */}
        {state.step === "qrcode" && (
          <>
            <h2 style={{
              margin: "0 0 24px",
              fontSize: "20px",
              fontWeight: 700,
              color: "#0f172a",
            }}>
              {modalTitle}
            </h2>

            <div style={{
              display: "inline-block",
              padding: "16px",
              background: "#f8fafc",
              borderRadius: "16px",
              border: "1px solid #e2e8f0",
            }}>
              <img
                src={state.data.brCodeBase64}
                alt="QR Code PIX"
                style={{
                  width: "200px",
                  height: "200px",
                  display: "block",
                  borderRadius: "8px",
                }}
              />
            </div>

            {timeLeft && (
              <p style={{
                margin: "16px 0 0",
                fontSize: "14px",
                color: "#64748b",
              }}>
                Expira em:{" "}
                <strong style={{
                  color: timeLeft === "Expirado" ? "#dc2626" : "#10b981",
                  fontWeight: 700,
                }}>{timeLeft}</strong>
              </p>
            )}

            <p style={{
              margin: "20px 0 10px",
              fontSize: "13px",
              color: "#94a3b8",
              textTransform: "uppercase",
              letterSpacing: "0.05em",
              fontWeight: 600,
            }}>
              Codigo PIX copia e cola
            </p>

            <div style={{
              background: "#f8fafc",
              border: "1px solid #e2e8f0",
              borderRadius: "10px",
              padding: "12px",
              display: "flex",
              gap: "10px",
              alignItems: "center",
            }}>
              <div style={{
                flex: 1,
                minWidth: 0,
                padding: "8px 12px",
                background: "#fff",
                border: "1px solid #e2e8f0",
                borderRadius: "8px",
                fontSize: "12px",
                fontFamily: "monospace",
                color: "#334155",
                overflow: "hidden",
                textOverflow: "ellipsis",
                whiteSpace: "nowrap",
                cursor: "text",
                userSelect: "all",
                lineHeight: "1.5",
              }}
                title={state.data.brCode}
                onClick={(e) => {
                  const range = document.createRange();
                  range.selectNodeContents(e.currentTarget);
                  const sel = window.getSelection();
                  sel?.removeAllRanges();
                  sel?.addRange(range);
                }}
              >
                {state.data.brCode}
              </div>
              <button
                onClick={() => handleCopy(state.data.brCode)}
                style={{
                  background: copied ? "#10b981" : "#0f172a",
                  color: "#fff",
                  border: "none",
                  padding: "10px 20px",
                  borderRadius: "8px",
                  cursor: "pointer",
                  fontSize: "13px",
                  fontWeight: 600,
                  whiteSpace: "nowrap",
                  transition: "background 0.2s",
                  flexShrink: 0,
                }}
              >
                {copied ? "Copiado!" : "Copiar"}
              </button>
            </div>

            <div style={{
              margin: "24px 0 0",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: "8px",
              fontSize: "14px",
              color: "#64748b",
            }}>
              <div style={{
                width: "8px",
                height: "8px",
                borderRadius: "50%",
                background: "#10b981",
                animation: "proxypay-pulse 1.5s ease-in-out infinite",
              }} />
              <style>{`@keyframes proxypay-pulse { 0%, 100% { opacity: 1 } 50% { opacity: 0.3 } }`}</style>
              Aguardando pagamento...
            </div>
          </>
        )}
      </div>
    </div>,
    document.body,
  );

  return (
    <>
      {trigger}
      {modal}
    </>
  );
}
