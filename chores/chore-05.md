# Chore 5 — Build the container images and roll them out

Infrastructure is up but the apps still serve placeholder content. Containerise the workload
and roll the real images out. You are acting as the platform team: you author the build assets
and the rollout, but you do **not** touch application source.

## Requirements

- **Two Dockerfiles**, each next to the service it builds:
  - the .NET 10 API under `workload-app/backend/HotelBooking.Api/`.
  - the Vite/React SPA under `workload-app/frontend/`, served by nginx with SPA fallback and
    `/api/` reverse-proxied to the backend's **internal** ingress. The nginx config is a real
    file under `workload-app/frontend/nginx/` that is `COPY`-ed into the image (no inline
    heredocs).
- Every `FROM` line uses **`mcr.microsoft.com`** (see the repo's container-image rule).
- A **PowerShell rollout script** reads the infrastructure deployment outputs (no hard-coded
  names), builds both images server-side, tags each with `:latest` and the short Git SHA, and
  updates each container app to the new image. The script is **idempotent**.
- The **only** files added or changed inside `workload-app/` are container build assets
  (`Dockerfile`, `.dockerignore`, nginx config, entrypoint scripts). Application source stays
  untouched.

## Success criteria

**Done when**
- Both images are built and each container app runs its real image.

**Verify**
- The frontend FQDN returns the SPA's `index.html`.
- `/api/hotels` returns JSON.
- A booking POST succeeds end-to-end (backend reaches SQL over the private endpoint with its
  managed identity).

**Enough to move on**
- The app works end-to-end through the public frontend, and no application source was edited.

---
Background, build-context hints, and the platform-engineer scope note: [details-05.md](details-05.md).
