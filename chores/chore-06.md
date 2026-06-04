# Chore 6 — Build the container images and roll them out

- Two Dockerfiles live under `dockerfiles/`:
  - `Dockerfile.backend` for [workload-app/backend/HotelBooking.Api/](../workload-app/backend/HotelBooking.Api/).
  - `Dockerfile.frontend` for [workload-app/frontend/](../workload-app/frontend/), served by nginx with SPA fallback and `/api/` reverse-proxied to the backend's internal ingress.
- Every `FROM` line uses **`mcr.microsoft.com`**.
- A PowerShell script `dockerfiles/Build-And-Deploy.ps1` does the full rollout: reads outputs from the workload infrastructure deployment, logs in to ACR, builds with **`az acr build`**, tags with `:latest` and the short Git SHA, and updates each container app to the new image.
- The script is **idempotent**.
- After rollout, the frontend FQDN serves the SPA, `/api/hotels` returns JSON, and a booking POST works end-to-end.
- **You do not edit `workload-app/`.**

Stuck or want to check your work? See [details-06.md](details-06.md).
