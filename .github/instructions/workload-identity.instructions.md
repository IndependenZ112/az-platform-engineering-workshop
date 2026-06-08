---
applyTo: '**/*.bicep,**/*.bicepparam,**/infra/**,**/infrastructure/**'
description: 'Passwordless identity wiring for the workshop workload — managed identities, ACR pull, SQL Entra admin, and the no-secrets contract. Use when designing or implementing any Bicep that touches identity, Azure SQL, Container Apps, or the container registry.'
---

# Workload identity & passwordless access

These rules make the workshop workload run **without a single secret** in parameters,
outputs, or app configuration. They apply to every Bicep file, parameter file, and design
doc for the workload spoke. They complement — not replace —
[azure-verified-modules-bicep](azure-verified-modules-bicep.instructions.md) and
[workload-network-exposure](workload-network-exposure.instructions.md).

The non-negotiable outcome: **identity is the only credential**. If you find yourself adding
a SQL password, an ACR admin username/password, a connection-string secret, or a Key Vault
round-trip just to authenticate a first-party Azure service to another, stop — you have taken
a wrong turn.

## Managed identities

- Use **user-assigned managed identities (UAMI)**. Do not rely on system-assigned identities
  for cross-resource grants, because the role assignment must exist before the consuming
  resource is created and a system-assigned principal id isn't known until then.
- Keep **runtime** and **deploy/CI** identities separate. A runtime identity on a container
  app must not be able to redeploy infrastructure; a CI identity must not have the app's
  data-plane rights. (CI identity scoping lives in
  [github-oidc-federation](github-oidc-federation.instructions.md).)

## Pulling images from ACR with a managed identity

The admin user on the registry stays **disabled**. Each container app pulls with its own
managed identity, which requires **two** things wired together — missing either one is the
most common cause of an `UNAUTHORIZED` pull at rollout time:

1. The managed identity holds **`AcrPull`** (role definition id
   `7f951dda-4ed3-4680-a7ca-43fe172d538d`) granted **on the registry scope**, from the Bicep
   that provisions the identity — not a post-deploy script.
2. The container app's **`registries[]`** entry references that same managed identity as its
   `identity`. Without this the placeholder MCR image still runs (MCR is anonymous) but the
   first pull of `*.azurecr.io/...` fails.

There is **one** workshop ACR — it is **not** stamped out per environment by the workload
template. The workload Bicep takes the registry **resource id** as a parameter and grants
each environment's runtime managed identity `AcrPull` on that single registry, so test and
prod container apps both reference the same `loginServer` in `registries[]`. Provisioning a
second ACR for prod (or any per-env duplicate) breaks
[build-once-promote-everywhere](build-once-promote-everywhere.instructions.md) — fix the
topology, do not work around it with `az acr import`.

## Azure SQL: managed identity is the Entra admin

- The **backend container app's managed identity is the SQL server's Microsoft Entra admin**,
  set **declaratively** on the AVM `sql/server` `administrator` parameter. No
  `az sql server ad-admin create`, no `deploymentScripts`, no jumpbox. Schema/seed creation
  runs on first app startup as that admin.
- **Microsoft Entra-only authentication is enabled** (`azureADOnlyAuthentication: true`).
- `publicNetworkAccess` is `Disabled`; SQL is reachable only through its private endpoint.

The three failure modes to get right in the `administrator` block (all silent until deploy or
runtime):

| Property | Correct value | Wrong value that looks right |
| --- | --- | --- |
| `principalType` | `Application` (required for a user-assigned MI) | `User` |
| `sid` | the MI's **`principalId`** | the MI's `clientId` or `resourceId` |
| `azureADOnlyAuthentication` | `true` | omitted (leaves SQL auth enabled) |

The deploying principal is **not** the data-plane admin and does not need data-plane access —
SQL is private and the MI owns the schema.

## Passwordless connection string

- Build the connection string **from resource properties at deploy time** and authenticate
  with the managed identity — e.g. `Authentication=Active Directory Default`, `Encrypt=True`,
  server FQDN from the SQL module output. No password, no secret reference.
- When several identities could be in scope, pass the intended UAMI's **`clientId`** as an
  environment variable (e.g. `AZURE_CLIENT_ID`) so the runtime credential picks the right one.

## No secrets — hard rule

- No secrets in `param`/`output`/`var`, no secrets in container app `env`, no admin
  credentials, no SQL logins. If a value is sensitive, the design is wrong — switch to an
  identity-based path.
- A production landing zone would typically make an Entra **group** the SQL admin and grant
  the app MI a contained DB user with least privilege. Collapsing both into one MI is a
  documented **workshop simplification** — note the trade-off in the design; don't change the
  workshop path.
