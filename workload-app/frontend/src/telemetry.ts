import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { SimpleSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-proto';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { registerInstrumentations } from '@opentelemetry/instrumentation';

export function initTelemetry() {
  const otlpEndpoint = import.meta.env.VITE_OTEL_EXPORTER_OTLP_ENDPOINT || '/otel';

  const resource = resourceFromAttributes({
    [ATTR_SERVICE_NAME]: 'frontend',
  });

  const exporter = new OTLPTraceExporter({
    url: `${otlpEndpoint}/v1/traces`,
    headers: {},
  });

  const provider = new WebTracerProvider({
    resource,
    spanProcessors: [new SimpleSpanProcessor(exporter)],
  });

  provider.register({
    contextManager: new ZoneContextManager(),
  });

  registerInstrumentations({
    instrumentations: [
      new FetchInstrumentation({
        propagateTraceHeaderCorsUrls: [/.*/],
        ignoreUrls: [/\/otel\//],
      }),
      new DocumentLoadInstrumentation(),
    ],
  });
}
