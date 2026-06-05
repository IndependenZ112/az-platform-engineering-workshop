# Chore 5 — Build the container images and roll them out

- Two Dockerfiles, each next to the service it builds:
  - [workload-app/backend/HotelBooking.Api/Dockerfile](../workload-app/backend/HotelBooking.Api/Dockerfile) for the .NET 10 API.
  - [workload-app/frontend/Dockerfile](../workload-app/frontend/Dockerfile) for the Vite/React SPA, served by nginx with SPA fallback and `/api/` reverse-proxied to the backend's internal ingress. The nginx config lives in `workload-app/frontend/nginx/` and is `COPY`-ed into the image (no inline heredocs).
- Every `FROM` line uses **`mcr.microsoft.com`**.
- A PowerShell script (e.g. at the repo root or under a platform-team scripts folder) does the full rollout: reads outputs from the workload infrastructure deployment, logs in to ACR, builds with **`az acr build`** (build context = the matching `workload-app/<service>/` folder), tags with `:latest` and the short Git SHA, and updates each container app to the new image.
- The script is **idempotent**.
- After rollout, the frontend FQDN serves the SPA, `/api/hotels` returns JSON, and a booking POST works end-to-end.
- **You do not edit application source under `workload-app/`.** Container build assets (`Dockerfile`, `.dockerignore`, nginx config) are the only files you may add or change inside that folder.

Stuck or want to check your work? See [details-05.md](details-05.md).
