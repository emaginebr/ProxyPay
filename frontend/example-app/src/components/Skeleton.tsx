interface SkeletonProps {
  width?: string;
  height?: string;
  borderRadius?: string;
  style?: React.CSSProperties;
  className?: string;
}

export function Skeleton({
  width = "100%",
  height = "16px",
  borderRadius = "6px",
  style,
  className = "",
}: SkeletonProps) {
  return (
    <div
      className={`pp-skel ${className}`.trim()}
      style={{ width, height, borderRadius, ...style }}
    />
  );
}

export function SkeletonTable({ rows = 5, cols = 4 }: { rows?: number; cols?: number }) {
  return (
    <div className="pp-table-wrap">
      <table className="pp-table">
        <thead>
          <tr>
            {Array.from({ length: cols }).map((_, i) => (
              <th key={i}>
                <Skeleton width="70%" height="12px" />
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {Array.from({ length: rows }).map((_, r) => (
            <tr key={r}>
              {Array.from({ length: cols }).map((_, c) => (
                <td key={c}>
                  <Skeleton width={c === 0 ? "48px" : "80%"} height="14px" />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function SkeletonCards({ count = 4 }: { count?: number }) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="pp-metric">
          <Skeleton width="48%" height="11px" />
          <Skeleton width="72%" height="30px" />
          <Skeleton width="30%" height="10px" />
        </div>
      ))}
    </div>
  );
}

export function SkeletonForm() {
  return (
    <div className="flex flex-col gap-5 max-w-[480px]">
      {Array.from({ length: 3 }).map((_, i) => (
        <div key={i} className="pp-field">
          <Skeleton width="110px" height="12px" style={{ marginBottom: "8px" }} />
          <Skeleton width="100%" height="40px" borderRadius="12px" />
        </div>
      ))}
      <div className="flex gap-3 mt-2">
        <Skeleton width="140px" height="40px" borderRadius="12px" />
      </div>
    </div>
  );
}
