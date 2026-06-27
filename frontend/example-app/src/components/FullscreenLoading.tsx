export function FullscreenLoading({ message = "Processando pagamento..." }: { message?: string }) {
  return (
    <div className="pp-fullscreen-loading" role="status" aria-live="polite">
      <div className="pp-fullscreen-loading-inner">
        <div className="pp-spinner" aria-hidden="true" />
        <p className="text-sm font-medium text-slate-700">{message}</p>
      </div>
    </div>
  );
}
