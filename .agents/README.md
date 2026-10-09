# Agent rules — layout and usage
For humans: no tool loads this file.

| Path | Purpose |
|---|---|
| `AGENTS.md` | Core rules for every role and tool (opencode and Kilo Code load it automatically) |
| `.agents/roles/{planner,reviewer,actor}.md` | Per-role rules and templates |
| `.agents/project.md` | Repository facts and invariants (loaded through `opencode.json` / `kilo.jsonc`) |
| `.agents/chatbot.md` | Extra rules for chatbots without repo tools |
| `.agents/to-do.md`, `.agents/chat/*.md` | Pipeline state: roadmap, plan, audit, act report |
| `.opencode/agents/`, `.kilo/agents/` | Role pickers — stubs pointing at the role files (keep the folder your tool reads) |
| `.github/copilot-instructions.md` | Copilot stub |

**opencode / Kilo Code:** pick the `planner`, `reviewer` or `actor` agent and set each agent's model in the tool (order below).
**Chatbots (ChatGPT, Grok, AI Studio, Claude):** paste the output of `cat AGENTS.md .agents/chatbot.md .agents/roles/<role>.md .agents/project.md` (Git Bash on Windows: add `| clip`), then upload `.agents/to-do.md` and `.agents/chat/*.md`.
**New repo:** copy `AGENTS.md`, `.agents/` (keep `roles/` and `chatbot.md`, rewrite `project.md`, reset the state files), the agent stubs and the two JSON configs.

## Model order (moved from the old role files)
**Planner**
*Model, in order: Nemotron 3 Ultra (XHigh reasoning) → Gemini 3.8 Flash → DeepSeek V4.1 Flash → Inkling → Pixel Canary (set reasoning to high/xhigh — 262K context, no architecture-specific benchmark yet, so behind the proven reasoners) → Space Bunny Alpha (capable, but anonymous — use with a little more caution)*

**Reviewer**
*Model, in order: Gemini 3.8 Flash → DeepSeek V4.1 Flash → Nemotron 3 Super → Qwen3.8 27B — the lighter two are fine for execution checks even if you keep the heavier pair for plan review*

**Actor**
*Model, in order: MiMo-V2.6-Flash → Pixel Canary (stealth — ties GPT-6 Astra on coding/frontend benchmarks, good to try, but free only "for a limited time" so don't depend on it long-term) → Laguna S 2.1 → North Mini Code → Laguna XS 2.1 → Nemotron 3.5 Lightning*
*Reasoning effort: low by default. For MiMo-V2.6-Flash specifically, set it to off — Xiaomi's own guidance for Cline-style harnesses.*
