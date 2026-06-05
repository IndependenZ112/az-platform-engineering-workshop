# Chore 5 — Build the container images and roll them out

### Background

Infrastructure is up but the container apps are still serving placeholder content. You're a **platform engineer, not a developer** — you don't know the .NET/React stack. Copilot reads [workload-app/](../workload-app/), produces two Dockerfiles, and writes the rollout script. You stay in your lane and review.

### Hints

- Backend: multi-stage build at [workload-app/backend/HotelBooking.Api/Dockerfile](../workload-app/backend/HotelBooking.Api/Dockerfile), built against the .NET 10 minimal API source in the same folder (build context = that folder, e.g. `az acr build --file workload-app/backend/HotelBooking.Api/Dockerfile workload-app/backend/HotelBooking.Api`). Add a `.dockerignore` next to it. The backend image serves the API only — **do not bundle the SPA into it**; the frontend image (nginx) is what ships `index.html` and the static assets.
- Frontend: multi-stage build at [workload-app/frontend/Dockerfile](../workload-app/frontend/Dockerfile), built against [workload-app/frontend/](../workload-app/frontend/), served by nginx. The nginx config lives at `workload-app/frontend/nginx/default.conf` (or `nginx.conf`) as a real file that the Dockerfile `COPY`s into the image — no heredocs, no `RUN echo` inlining. It does SPA fallback (`try_files $uri /index.html`) and reverse-proxies `/api/` to the backend container app's **internal** ingress FQDN, keeping the frontend the only public surface.
- The **only** files written into `workload-app/` are container build assets (`Dockerfile`, `.dockerignore`, nginx config, entrypoint scripts). Application source (`.cs`, `.tsx`, `.ts`, `package.json`, `appsettings.json`, `index.html`, build configs) is still off-limits.
- Every `FROM` line uses **`mcr.microsoft.com`** with the **pinned tags from [.github/copilot-instructions.md](../.github/copilot-instructions.md) rule 5** — `mcr.microsoft.com/dotnet/sdk:10.0` and `mcr.microsoft.com/dotnet/aspnet:10.0` for the backend, `mcr.microsoft.com/azurelinux/base/nodejs:24` and `mcr.microsoft.com/azurelinux/base/nginx:1.28` for the frontend. Don't substitute `:latest`, and don't pick a different Node major — Vite 7 needs Node ≥ 20.19 and Azure Linux only publishes 20.14 and 24 today, so 24 is the working choice.
- A PowerShell rollout script (e.g. `Build-And-Deploy.ps1` at the repo root or under a platform-team scripts folder):
  - Reads ACR login server and container app names from the previous chore's deployment outputs (or accepts them as parameters). Do **not** hard-code names.
  - `az acr login` (Entra-based, no admin user).
  - `az acr build` for both images (server-side build — no local Docker needed), each pointing at its own `workload-app/<service>/` build context. Tag with `:latest` and `git rev-parse --short HEAD`.
  - `az containerapp update --image <acr>/<repo>:<sha>` triggers a new ACA revision. Wait for `Healthy` before moving on.
  - Print the public frontend FQDN at the end.
- Script must be **idempotent**.

### Outcome

- `curl https://<frontend-fqdn>/` returns the SPA's `index.html`.
- `curl https://<frontend-fqdn>/api/hotels` returns JSON.
- A booking POST works (backend reaches SQL via private endpoint with its UAMI).
- Application Insights shows a single distributed trace browser → frontend → backend → SQL.

### Note for the platform engineer

You are **not** expected to debug `.cs` or `.tsx` files. If the image fails to build, decide whether it's infra (base image tag, registry auth, network) or application (hand it back to the app team). **Do not edit application source under [workload-app/](../workload-app/)** — only the container build assets listed above (`Dockerfile`, `.dockerignore`, nginx config, entrypoint scripts) are platform-team property.
