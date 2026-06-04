# Chore 1 — Curate the Copilot toolbox

### Background

The community maintains a large catalogue of reusable Copilot customizations at [aka.ms/awesome-copilot](https://aka.ms/awesome-copilot) (the [`github/awesome-copilot`](https://github.com/github/awesome-copilot) repo) — **agents**, **instructions**, and **skills**. Picking the right ones up front means Copilot follows your conventions (CAF naming, AVM, Bicep best practices, draw.io diagram authoring, deployment preflight, container best practices) without you having to re-prompt them every chore.

### Hints

- Browse the [`agents/`](https://github.com/github/awesome-copilot/tree/main/agents), [`instructions/`](https://github.com/github/awesome-copilot/tree/main/instructions), and [`skills/`](https://github.com/github/awesome-copilot/tree/main/skills) folders — or use the [machine-readable index](https://awesome-copilot.github.com/llms.txt) — and let Copilot match items to the chores ahead.
- Install layout:
  - **Instructions** → `.github/instructions/*.instructions.md` (use `applyTo` glob front-matter).
  - **Skills** → `.github/skills/<skill-name>/SKILL.md` (plus any bundled assets).
  - **Agents** → `.github/agents/<agent-name>.agent.md`.
- After installing, **Developer: Reload Window** (`Ctrl+Shift+P`). New customizations are only discovered on window load.
- Smoke test: ask Copilot for a Bicep resource name (should cite naming instructions); ask for a Dockerfile (should pull from `mcr.microsoft.com`).

### Outcome

The repo has a curated set of agents/instructions/skills under `.github/`, and Copilot now follows the workshop's conventions without re-prompting. Every subsequent chore assumes these are loaded.

### Why this chore exists

The exact set drifts as the community publishes new items. Treat this chore as recurring — re-running it every few months keeps the repo's Copilot configuration current.
