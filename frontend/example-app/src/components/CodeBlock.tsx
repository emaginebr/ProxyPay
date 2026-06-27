import { useState } from "react";
import { Highlight, themes } from "prism-react-renderer";
import { IconCopy, IconCheck } from "./Icon";

interface CodeBlockProps {
  code: string;
  language?: string;
  filename?: string;
}

export function CodeBlock({ code, language = "tsx", filename }: CodeBlockProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(code.trim());
      setCopied(true);
      setTimeout(() => setCopied(false), 1800);
    } catch {
      /* noop */
    }
  };

  return (
    <div className="relative group">
      {filename && (
        <div className="flex items-center justify-between px-4 py-2 bg-slate-800 border border-slate-700 border-b-0 rounded-t-md">
          <span className="text-mono text-xs text-slate-400">{filename}</span>
          <span className="text-xs uppercase tracking-wider text-slate-500 font-semibold">
            {language}
          </span>
        </div>
      )}
      <Highlight theme={themes.nightOwl} code={code.trim()} language={language}>
        {({ style, tokens, getLineProps, getTokenProps }) => (
          <pre
            className={`pp-code ${filename ? "rounded-t-none" : ""}`}
            style={{ ...style, margin: 0, background: "#0F172A" }}
          >
            <code>
              {tokens.map((line, i) => (
                <div key={i} {...getLineProps({ line })}>
                  {line.map((token, key) => (
                    <span key={key} {...getTokenProps({ token })} />
                  ))}
                </div>
              ))}
            </code>
          </pre>
        )}
      </Highlight>
      <button
        type="button"
        onClick={handleCopy}
        className="absolute top-3 right-3 inline-flex items-center gap-1.5 px-2.5 h-7 rounded-md bg-slate-800/80 border border-slate-700 text-slate-300 text-xs font-medium hover:bg-slate-700 hover:text-white transition opacity-0 group-hover:opacity-100 focus-visible:opacity-100"
        aria-label={copied ? "Copiado" : "Copiar código"}
      >
        {copied ? <IconCheck size={12} /> : <IconCopy size={12} />}
        {copied ? "Copiado" : "Copiar"}
      </button>
    </div>
  );
}
