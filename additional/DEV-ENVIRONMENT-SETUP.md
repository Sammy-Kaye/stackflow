# StackFlow — Dev Environment & MCP Setup
**Session date:** 28 March 2026  
**Status:** Complete  
**Author:** Samuel

---

## What Was Accomplished

This session covered the full local development environment setup and wiring of Priority 1 and Priority 2 MCP servers for Claude Code. Google Stitch design session was also completed (26 screens across all screen groups).

---

## 1. GitHub MCP

### PAT Created
- **Token name:** `stackflow-claude-code`
- **Type:** Fine-grained, repo-scoped
- **Expiration:** 90 days (expires 26 June 2026) — set a calendar reminder to rotate
- **Resource owner:** Sammy-Kaye
- **Repository access:** Only `stackflow` repo
- **Permissions granted:**
  - Contents: Read and write
  - Issues: Read and write
  - Pull requests: Read and write
  - Metadata: Read-only (mandatory)
  - Workflows: Read-only

### MCP Install Command Used
```bash
claude mcp add github --transport stdio -e GITHUB_PERSONAL_ACCESS_TOKEN=<token> -- npx "-y" "@modelcontextprotocol/server-github"
```
Run from: `/mnt/d/My Projects/Stackflow`

### Verified
Claude Code successfully queried the `Sammy-Kaye/stackflow` repository and returned repo details via the GitHub MCP.

---

## 2. Local Files Pushed to GitHub

Git was not previously initialised on the local Stackflow folder. The following was done:

```bash
git init
git config --global user.email "samuelkaye92@gmail.com"
git config --global user.name "Sammy-Kaye"
git add .
git commit -m "Initial commit - StackFlow project scaffold"
git branch -M main
git remote add origin https://github.com/Sammy-Kaye/stackflow.git
git push -u origin main --force
```

**Note:** Remote URL was updated to embed the PAT for HTTPS authentication (GitHub no longer accepts passwords):
```bash
git remote set-url origin https://Sammy-Kaye:<token>@github.com/Sammy-Kaye/stackflow.git
```

### Files pushed in initial commit (16 files)
- `CLAUDE.md`
- `.claude/agents/` — all 6 sub-agent files
- `.claude/skills/` — all 8 skill files
- `.claude/settings.local.json`

---

## 3. Node.js

Already installed. No action needed.

- **Version:** v24.14.0

---

## 4. Docker Desktop

- **Installed:** Windows installer, WSL 2 backend selected during install
- **Docker version confirmed in WSL:** 29.3.1

### WSL Permission Fix
After install, Docker socket permissions needed to be set manually. Run this each time WSL is restarted if Docker gives permission errors:

```bash
sudo chmod 666 /var/run/docker.sock
```

> **Note:** This resets on reboot. A permanent fix can be set up later via a udev rule or a startup script.

---

## 5. PostgreSQL (Docker Container)

### Container Created
```bash
docker run -d \
  --name stackflow-postgres \
  -e POSTGRES_USER=stackflow \
  -e POSTGRES_PASSWORD=stackflow_dev \
  -e POSTGRES_DB=stackflow \
  -p 5432:5432 \
  postgres:16
```

- **Container name:** `stackflow-postgres`
- **Image:** `postgres:16`
- **Port:** `5432`
- **Credentials:** `stackflow` / `stackflow_dev`
- **Database:** `stackflow`

### ⚠️ Important — After Every Reboot
The container does not auto-start. Run these two commands when returning to work:

```bash
sudo chmod 666 /var/run/docker.sock
docker start stackflow-postgres
```

---

## 6. DBeaver

- **Installed:** Community Edition (Windows)
- **Connection configured:**
  - Host: `localhost`
  - Port: `5432`
  - Database: `stackflow`
  - Username: `stackflow`
  - Password: `stackflow_dev`
- **Status:** Connected successfully

---

## 7. PostgreSQL MCP

### MCP Install Command Used
```bash
claude mcp add postgres --transport stdio -- npx "-y" "@modelcontextprotocol/server-postgres" "postgresql://stackflow:stackflow_dev@localhost:5432/stackflow"
```
Run from: `/mnt/d/My Projects/Stackflow`

> **Note:** The connection string is passed as a direct argument, not an env var — this is what works with the current MCP server package.

### Verified
Claude Code successfully queried the `stackflow` database via the PostgreSQL MCP and confirmed the database is empty (no tables yet — migrations not run).

---

## 8. Stitch MCP

### Background
The original planned URL (`https://mcp.stitch.withgoogle.com/mcp`) no longer works. The correct approach requires:
- A **Stitch API key** (not OAuth)
- The **HTTP transport** pointing at `stitch.googleapis.com`

### API Key
Generated from: **stitch.withgoogle.com** → Profile picture → Stitch Settings → API key → Create key

### MCP Install Command Used
```bash
claude mcp add stitch --transport http https://stitch.googleapis.com/mcp --header "X-Goog-Api-Key: YOUR_API_KEY"
```
Run from: `/mnt/d/My Projects/Stackflow`

> **Note:** Previous attempts using `stitch-mcp` and `@_davideast/stitch-mcp` npm packages failed. The correct method is HTTP transport with the API key passed as a header.

### Verified
Claude Code queried the Stitch MCP and returned the StackFlow UI project:
- **Project:** StackFlow UI
- **ID:** `projects/1153536039782936752`
- **Screens:** 26 screen instances
- **Theme:** Dark, Manrope/Inter fonts, Teal (#1D9E75)
- **Device:** Desktop

---

## 9. Final MCP Status

```bash
claude mcp list
```

| MCP | Status |
|---|---|
| github | ✅ Connected |
| postgres | ✅ Connected |
| stitch | ✅ Connected |
| claude.ai Gmail | ⚠️ Needs authentication (claude.ai connector — ignore) |
| claude.ai Google Calendar | ⚠️ Needs authentication (claude.ai connector — ignore) |

---

## 10. Monorepo Decision

Discussed whether to use monorepo vs polyrepo (separate frontend/backend repos as used at work). **Decision: monorepo for now.**

- Solo developer — no cross-team coordination needed
- Claude Code works best with full codebase in one session
- Feature branches cover both frontend and backend changes in one PR
- Can split to polyrepo later when team grows

---

## Remaining MCPs (Not Yet Set Up)

| MCP | When to set up |
|---|---|
| Sentry | Before Phase 2 |
| Playwright | Before Real Testing phase |

---

## Full Environment Status

| Tool | Version / Status |
|---|---|
| Node.js | v24.14.0 |
| Docker Desktop | 29.3.1 |
| PostgreSQL container | Running (postgres:16) |
| DBeaver | Installed, connected |
| GitHub MCP | ✅ Connected |
| PostgreSQL MCP | ✅ Connected |
| Stitch MCP | ✅ Connected |
| Google Stitch Design | ✅ 26 screens complete |

---

## Next Session
Phase 1 development — project scaffold, domain entities, EF Core setup.
