# Codex configuration for SASD-Crawler

Repository-wide instructions:

- `../AGENTS.md`
- `../RULES.md`

Project execpolicy:

- `rules/default.rules`

Codex scans rules at startup. Restart Codex after changing `.rules` files.
Project-level rules apply only when the project `.codex` layer is trusted.

## Expected policy

| Command | Decision |
|---|---|
| `git status/add/commit/push` | allow |
| `git reset/clean/rebase` | prompt |
| `git push --force ...` | prompt |
| `dotnet restore/build/test/publish` | allow |
| `dotnet ef database drop` | prompt |
| `gh pr create/view/checks` | allow |
| `gh run view/watch` | allow |
| `gh release view/create/upload` | allow |
| `gh repo delete/rename/transfer` | prompt |
| `gh release delete` | prompt |
| `gh secret ...` | prompt |
| `gh api ...` | prompt |
| read-only PowerShell commands listed in the rules | allow |
| `Remove-Item`, `Clear-Content`, `Stop-Process` | prompt |

## Optional policy checks

If your installed Codex supports `execpolicy check`:

```powershell
codex execpolicy check --pretty --rules .codex/rules/default.rules -- git status
codex execpolicy check --pretty --rules .codex/rules/default.rules -- git reset --hard HEAD
codex execpolicy check --pretty --rules .codex/rules/default.rules -- dotnet build
codex execpolicy check --pretty --rules .codex/rules/default.rules -- dotnet ef database drop
codex execpolicy check --pretty --rules .codex/rules/default.rules -- gh release create v0.0.1
codex execpolicy check --pretty --rules .codex/rules/default.rules -- gh release delete v0.0.1
```

Because `prefix_rule` is prefix-based, risky push flags must be written immediately after `git push`, for example:

```text
git push --force origin branch
git push --force-with-lease origin branch
git push --delete origin branch
```
