# Chore 6 — Build the container images and roll them out

### Background

Infrastructure is up but the container apps are still serving placeholder content. You're a **platform engineer, not a developer** — you don't know the .NET/React stack. Copilot reads [workload-app/](../workload-app/), produces two Dockerfiles, and writes the rollout script. You stay in your lane and review.

### Hints

- Backend: multi-stage build at `dockerfiles/backend/Dockerfile`, built against the .NET 10 minimal API source in [workload-app/backend/HotelBooking.Api/](../workload-app/backend/HotelBooking.Api/) (build context passed at build time, e.g. `az acr build --file dockerfiles/backend/Dockerfile workload-app/backend/HotelBooking.Api`).
- Frontend: multi-stage build at `dockerfiles/frontend/Dockerfile`, built against [workload-app/frontend/](../workload-app/frontend/), served by nginx. nginx config (also under `dockerfiles/frontend/`) does SPA fallback (`try_files $uri /index.html`) and reverse-proxies `/api/` to the backend container app's **internal** ingress FQDN, keeping the frontend the only public surface.
- **Nothing is written into `workload-app/`** — no `Dockerfile`, no `.dockerignore`, no nginx config. Every build asset lives under `dockerfiles/`.
- Every `FROM` line uses **`mcr.microsoft.com`** — per [.github/copilot-instructions.md](../.github/copilot-instructions.md) rule 5.
- `dockerfiles/Build-And-Deploy.ps1`:
  - Reads ACR login server and container app names from the previous chore's deployment outputs (or accepts them as parameters). Do **not** hard-code names.
  - `az acr login` (Entra-based, no admin user).
  - `az acr build` for both images (server-side build — no local Docker needed). Tag with `:latest` and `git rev-parse --short HEAD`.
  - `az containerapp update --image <acr>/<repo>:<sha>` triggers a new ACA revision. Wait for `Healthy` before moving on.
  - Print the public frontend FQDN at the end.
- Script must be **idempotent**.

### Outcome

- `curl https://<frontend-fqdn>/` returns the SPA's `index.html`.
- `curl https://<frontend-fqdn>/api/hotels` returns JSON.
- A booking POST works (backend reaches SQL via private endpoint with its UAMI).
- Application Insights shows a single distributed trace browser → frontend → backend → SQL.

### Note for the platform engineer

You are **not** expected to debug `.cs` or `.tsx` files. If the image fails to build, decide whether it's infra (base image tag, registry auth, network) or application (hand it back to the app team). **Do not edit anything under [workload-app/](../workload-app/)** — that rule still applies.
