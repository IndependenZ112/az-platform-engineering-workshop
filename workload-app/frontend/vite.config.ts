import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

// Parse OTEL_EXPORTER_OTLP_HEADERS (e.g. "x-otlp-api-key=abc123") into header object
function parseOtlpHeaders(): Record<string, string> {
  const raw = process.env.OTEL_EXPORTER_OTLP_HEADERS;
  if (!raw) return {};
  const headers: Record<string, string> = {};
  for (const pair of raw.split(',')) {
    const [key, ...rest] = pair.split('=');
    if (key) headers[key.trim()] = rest.join('=').trim();
  }
  return headers;
}

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': process.env.services__api__https__0 || process.env.services__api__http__0 || 'http://localhost:5000',
      '/agent-chat': process.env.services__api__https__0 || process.env.services__api__http__0 || 'http://localhost:5000',
      '/otel': {
        target: process.env.DOTNET_DASHBOARD_OTLP_HTTP_ENDPOINT_URL || 'http://localhost:19171',
        changeOrigin: true,
        secure: false,
        rewrite: (path: string) => path.replace(/^\/otel/, ''),
        headers: parseOtlpHeaders(),
      },
    },
  },
});
