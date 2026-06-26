/**
 * ProxyPay example-app — Tailwind config theme.extend
 * Drop-in block for example-app/tailwind.config.js
 *
 * Direction: Editorial Fintech + Bento Grid (landing)
 *            Informational Minimalism (dashboard)
 *
 * Usage in tailwind.config.js:
 *
 *   import { themeExtend } from './docs/design/example-app-redesign/tailwind.config.extend.js';
 *   export default {
 *     darkMode: ['class'],
 *     content: [...],
 *     theme: { extend: themeExtend },
 *     plugins: [require('tailwindcss-animate')],
 *   };
 */

export const themeExtend = {
  colors: {
    // Semantic aliases (use these in components, not primitives)
    border: 'hsl(var(--border-default-hsl, 214 32% 91%))', // fallback; prefer CSS vars
    background: 'var(--surface-page)',
    foreground: 'var(--text-primary)',
    muted: {
      DEFAULT: 'var(--surface-sunken)',
      foreground: 'var(--text-tertiary)',
    },
    card: {
      DEFAULT: 'var(--card-bg)',
      foreground: 'var(--text-primary)',
    },
    primary: {
      DEFAULT: 'var(--color-primary)',
      hover: 'var(--color-primary-hover)',
      soft: 'var(--color-primary-soft)',
      foreground: 'var(--color-primary-on)',
      50: 'var(--indigo-50)',
      100: 'var(--indigo-100)',
      200: 'var(--indigo-200)',
      300: 'var(--indigo-300)',
      400: 'var(--indigo-400)',
      500: 'var(--indigo-500)',
      600: 'var(--indigo-600)',
      700: 'var(--indigo-700)',
      800: 'var(--indigo-800)',
      900: 'var(--indigo-900)',
      950: 'var(--indigo-950)',
    },
    accent: {
      DEFAULT: 'var(--color-accent)',
      hover: 'var(--color-accent-hover)',
      soft: 'var(--color-accent-soft)',
      foreground: 'var(--color-accent-on)',
      50: 'var(--emerald-50)',
      100: 'var(--emerald-100)',
      200: 'var(--emerald-200)',
      300: 'var(--emerald-300)',
      400: 'var(--emerald-400)',
      500: 'var(--emerald-500)',
      600: 'var(--emerald-600)',
      700: 'var(--emerald-700)',
    },
    amber: {
      50: 'var(--amber-50)',
      100: 'var(--amber-100)',
      200: 'var(--amber-200)',
      300: 'var(--amber-300)',
      400: 'var(--amber-400)',
      500: 'var(--amber-500)',
      600: 'var(--amber-600)',
    },
    slate: {
      50: 'var(--slate-50)',
      100: 'var(--slate-100)',
      200: 'var(--slate-200)',
      300: 'var(--slate-300)',
      400: 'var(--slate-400)',
      500: 'var(--slate-500)',
      600: 'var(--slate-600)',
      700: 'var(--slate-700)',
      800: 'var(--slate-800)',
      900: 'var(--slate-900)',
      950: 'var(--slate-950)',
    },
    success: {
      DEFAULT: 'var(--color-success)',
      soft: 'var(--color-success-soft)',
    },
    warning: {
      DEFAULT: 'var(--color-warning)',
      soft: 'var(--color-warning-soft)',
    },
    destructive: {
      DEFAULT: 'var(--color-danger)',
      soft: 'var(--color-danger-soft)',
      foreground: '#FFFFFF',
    },
    info: {
      DEFAULT: 'var(--color-info)',
      soft: 'var(--color-info-soft)',
    },
  },

  fontFamily: {
    display: ['Space Grotesk', 'Outfit', 'system-ui', 'sans-serif'],
    sans: ['Inter', 'DM Sans', 'system-ui', '-apple-system', 'sans-serif'],
    mono: ['JetBrains Mono', 'Fira Code', 'ui-monospace', 'Consolas', 'monospace'],
  },

  fontSize: {
    xs:   ['0.75rem',   { lineHeight: '1.5' }],
    sm:   ['0.8125rem', { lineHeight: '1.55' }],
    base: ['0.9375rem', { lineHeight: '1.65' }],
    md:   ['1rem',      { lineHeight: '1.6' }],
    lg:   ['1.125rem',  { lineHeight: '1.55' }],
    xl:   ['1.25rem',   { lineHeight: '1.45' }],
    '2xl':['1.5rem',    { lineHeight: '1.3', letterSpacing: '-0.01em' }],
    '3xl':['1.875rem',  { lineHeight: '1.2', letterSpacing: '-0.02em' }],
    '4xl':['2.25rem',   { lineHeight: '1.15', letterSpacing: '-0.025em' }],
    '5xl':['3rem',      { lineHeight: '1.1',  letterSpacing: '-0.03em' }],
    '6xl':['3.75rem',   { lineHeight: '1.05', letterSpacing: '-0.035em' }],
    display: ['clamp(2.5rem, 6vw + 1rem, 4.5rem)', { lineHeight: '1.02', letterSpacing: '-0.04em' }],
  },

  spacing: {
    '0':  '0',
    '1':  '0.25rem',
    '2':  '0.5rem',
    '3':  '0.75rem',
    '4':  '1rem',
    '5':  '1.25rem',
    '6':  '1.5rem',
    '8':  '2rem',
    '10': '2.5rem',
    '12': '3rem',
    '16': '4rem',
    '20': '5rem',
    '24': '6rem',
  },

  borderRadius: {
    xs:  '4px',
    sm:  '8px',
    md:  '12px',
    lg:  '16px',
    xl:  '20px',
    '2xl': '28px',
    full: '9999px',
  },

  boxShadow: {
    xs:   '0 1px 2px rgba(15, 23, 42, 0.04)',
    sm:   '0 1px 3px rgba(15, 23, 42, 0.06), 0 1px 2px rgba(15, 23, 42, 0.04)',
    md:   '0 4px 12px rgba(15, 23, 42, 0.06), 0 2px 4px rgba(15, 23, 42, 0.04)',
    lg:   '0 12px 28px rgba(15, 23, 42, 0.08), 0 4px 8px rgba(15, 23, 42, 0.04)',
    xl:   '0 24px 48px rgba(15, 23, 42, 0.10), 0 8px 16px rgba(15, 23, 42, 0.04)',
    '2xl':'0 32px 80px rgba(15, 23, 42, 0.14)',
    glow:        '0 16px 40px -12px rgba(79, 70, 229, 0.35)',
    'glow-accent':'0 16px 40px -12px rgba(16, 185, 129, 0.35)',
    'glow-amber': '0 16px 40px -12px rgba(245, 158, 11, 0.30)',
  },

  backgroundImage: {
    'gradient-brand':   'linear-gradient(135deg, #4F46E5 0%, #6366F1 45%, #10B981 100%)',
    'gradient-accent':  'linear-gradient(135deg, #10B981, #34D399)',
    'gradient-aurora': [
      'radial-gradient(1200px 600px at 10% 0%, rgba(99,102,241,0.18), transparent 60%)',
      'radial-gradient(900px 500px at 90% 10%, rgba(16,185,129,0.14), transparent 60%)',
      'radial-gradient(800px 500px at 50% 100%, rgba(245,158,11,0.10), transparent 60%)',
    ].join(','),
    'grid-faint':
      'linear-gradient(var(--slate-100) 1px, transparent 1px), linear-gradient(90deg, var(--slate-100) 1px, transparent 1px)',
  },

  backgroundSize: {
    grid: '40px 40px',
  },

  transitionTimingFunction: {
    'out-expo':  'cubic-bezier(0.16, 1, 0.3, 1)',
    'in-out-circ':'cubic-bezier(0.65, 0, 0.35, 1)',
    spring:      'cubic-bezier(0.34, 1.56, 0.64, 1)',
  },

  transitionDuration: {
    fast: '150ms',
    base: '220ms',
    slow: '320ms',
  },

  keyframes: {
    'fade-in-up': {
      '0%': { opacity: '0', transform: 'translateY(8px)' },
      '100%': { opacity: '1', transform: 'translateY(0)' },
    },
    shimmer: {
      '0%': { backgroundPosition: '-200% 0' },
      '100%': { backgroundPosition: '200% 0' },
    },
    'pulse-ring': {
      '0%': { boxShadow: '0 0 0 0 rgba(16,185,129,0.4)' },
      '100%': { boxShadow: '0 0 0 14px rgba(16,185,129,0)' },
    },
    'float-slow': {
      '0%,100%': { transform: 'translateY(0)' },
      '50%':     { transform: 'translateY(-6px)' },
    },
  },

  animation: {
    'fade-in-up':  'fade-in-up 420ms cubic-bezier(0.16, 1, 0.3, 1) both',
    shimmer:       'shimmer 1.8s linear infinite',
    'pulse-ring':  'pulse-ring 1.6s ease-out infinite',
    'float-slow':  'float-slow 6s ease-in-out infinite',
  },

  zIndex: {
    base: '0',
    raised: '10',
    sticky: '20',
    dropdown: '40',
    overlay: '80',
    modal: '100',
    toast: '200',
    tooltip: '300',
  },

  screens: {
    xs: '480px',
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1440px',
  },
};

export default themeExtend;
