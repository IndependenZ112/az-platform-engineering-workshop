---
applyTo: '**/.github/workflows/*.yml,**/.github/workflows/*.yaml'
description: 'Build-once, promote-everywhere release rule for the workshop app pipeline — build images once, pin deploys to the immutable digest, never rebuild for prod. Use when authoring or reviewing the application deploy workflow.'
---

# Build once, promote everywhere

The contract for the application pipeline: **build each image exactly once, then deploy that
same immutable artifact to every environment.** Prod must never run bytes that no environment
has tested. Applies to the app deploy workflow. Complements
[github-actions-ci-cd-best-practices](github-actions-ci-cd-best-practices.instructions.md) and
[github-oidc-federation](github-oidc-federation.instructions.md).

## Rules

- **One container registry for the whole workshop.** Test and prod pull from the **same
  ACR** — there is no per-environment registry, no `az acr import` between registries, no
  re-tag, no copy. Pinning a digest only proves identity if both environments resolve
  `<registry>/<repo>@sha256:...` to the same `<registry>`. Two registries means two
  independent artifacts that happen to share a digest string; that is **not** build-once.
- **Build runs once per workflow run.** A single build job builds both images into that one
  ACR and emits their **digests** (`@sha256:...`) as job outputs.
- **Capture the digest from the build output itself** — not by re-querying the registry or
  inspecting a tag afterwards, which can race with another build. The deployable reference is
  `<registry>/<repo>@sha256:...`.
- **Every deploy job pins to the digest**, not a moving tag. Test and prod deploy the *same*
  digest from the *same* registry, so "it passed in test" is evidence about the exact bytes
  prod will run.
- **No build step in the prod stage.** No `az acr build`, no re-tag, no rebuild, no
  cross-registry copy. If the prod job builds or imports anything, the contract is broken.
- **Gate prod** behind the `prod` GitHub Environment's required reviewer; chain jobs with
  `needs:` (build → deploy-test → smoke-test → deploy-prod).
- Allow a **`workflow_dispatch` with an explicit image reference** to skip the build and
  redeploy a known-good digest (rollback / promote-an-older-image).
- Each deploy job writes the digest it deployed to `$GITHUB_STEP_SUMMARY` so the reviewer can
  confirm test and prod match.

## Where the single ACR lives

The shared ACR is **not** stamped out by the per-environment workload template (which would
produce one registry per env and break this contract). It is provisioned **once**, outside
the `test` / `prod` parameter sweep, and referenced by every environment by resource id:

- Each environment's runtime managed identities hold **`AcrPull` on that one registry**,
  granted from the workload Bicep that creates the identity. Both `test` and `prod`
  container apps reference the same `<registry>.azurecr.io/...@sha256:...` image and the
  same `loginServer` in `registries[]`.
- The CI build identity holds **`AcrPush` on that one registry**. See
  [github-oidc-federation](github-oidc-federation.instructions.md) for the role list — there
  is no "AcrPull on the *other* registry" entry because there is no other registry.

## The proof

`az containerapp show` for test and prod returns **identical** `image` fields — same
`<registry>.azurecr.io`, same repo, same `@sha256:` digest. If the registry hostname differs,
two ACRs exist and the contract is already broken; fix the topology, don't paper over it with
`az acr import`.
