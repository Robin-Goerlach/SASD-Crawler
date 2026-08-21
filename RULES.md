# RULES.md – Autonomous Codex command and safety rules

These rules are intentionally permissive for **safe repository-local development** and restrictive for destructive/system-wide actions.

## 1. General principle

Codex may proceed without asking for routine safe work inside this repository.

The agent may:

- inspect;
- edit;
- create;
- build;
- test;
- format;
- benchmark;
- stage;
- commit;
- push the current working branch;
- open/update a normal pull request.

The agent must preserve user data, secrets, machine configuration and repository history.

---

# 2. PowerShell – allowed without confirmation

## Read/inspect

Allowed examples:

```powershell
Get-ChildItem
Get-Item
Get-Content
Select-String
Test-Path
Resolve-Path
Get-FileHash
Get-Location
Get-Command
Get-Process
Get-CimInstance
```

Read-only inspection must avoid dumping secrets or unrelated private user files.

## Repository-local file creation/editing

Allowed inside the repository:

```powershell
New-Item
Set-Content
Add-Content
Copy-Item
Move-Item
Rename-Item
```

Use these only for files that belong to the project/task.

## Controlled cleanup

`Remove-Item` is allowed autonomously **only** for clearly generated disposable content inside the repository, for example:

```text
bin/
obj/
TestResults/
artifacts/
.tmp/
temporary benchmark output
```

Before removing anything else, prefer Git-aware editing/deletion of specifically intended project files and verify the path.

Never use broad patterns such as:

```powershell
Remove-Item -Recurse -Force *
```

or equivalent.

## .NET

Allowed:

```powershell
dotnet --info
dotnet --version
dotnet restore
dotnet build
dotnet test
dotnet format
dotnet clean
dotnet new
dotnet sln
dotnet add package
dotnet remove package
dotnet list package
dotnet pack
dotnet publish
```

Package changes must remain justified by the architecture and must not introduce obviously incompatible licensing/security risks.

## Safe public HTTP reads

Read-only requests to public official documentation are allowed when useful:

```powershell
Invoke-WebRequest -Method Get
Invoke-RestMethod -Method Get
```

Never pipe downloaded internet content directly into:

```powershell
Invoke-Expression
iex
powershell
pwsh
cmd
```

Never fetch or expose credentials from private endpoints.

---

# 3. PowerShell – not allowed autonomously

Do not perform system-wide or destructive actions such as:

```text
Format-Volume
Clear-Disk
Initialize-Disk
Remove-Partition
Set-Partition
diskpart
bcdedit
reg delete
Set-ExecutionPolicy
Disable-WindowsOptionalFeature
Stop-Service / Remove-Service on unrelated services
Set-NetFirewall*
Disable-NetAdapter
Remove-SmbShare
schtasks /Delete
Remove-Item outside the project/data sandbox
Start-Process -Verb RunAs
```

Also prohibited without explicit user direction:

- editing Windows Registry;
- changing firewall/network adapter configuration;
- installing/removing system software with `winget`, MSI, Chocolatey, package managers;
- modifying global environment variables;
- changing PowerShell execution policy;
- changing Windows services or scheduled tasks outside a dedicated project test environment;
- reading browser/password-manager/credential-store secrets;
- changing system time, boot configuration, disks or partitions.

---

# 4. Git – allowed without confirmation

Read-only:

```bash
git status
git diff
git diff --staged
git log
git show
git branch
git remote -v
git tag
git ls-files
git grep
git blame
git rev-parse
```

Normal safe development writes:

```bash
git switch -c <branch>
git checkout -b <branch>
git add <specific paths>
git add -A
git restore --staged <paths>
git commit -m "..."
git fetch
git pull --ff-only
git merge --ff-only <branch>
git push
git push -u origin <current-branch>
```

`git add -A` is allowed only after reviewing `git status`/`git diff` so secrets or unrelated generated files are not staged.

Normal `git push` is allowed for the current non-protected working branch.

---

# 5. Git – not allowed autonomously

Do not use:

```bash
git reset --hard
git clean -fd
git clean -fdx
git checkout -- .
git restore .
git branch -D
git rebase --onto ...   # when it rewrites shared history
git filter-branch
git filter-repo
git gc --prune=now
git push --force
git push --force-with-lease
git push --mirror
git tag -d <published-tag>
```

Do not:

- rewrite shared/published history;
- delete remote branches unless the task explicitly requires it and it is clearly safe;
- overwrite protected branches;
- modify Git credentials or SSH keys;
- change remote URLs to unexpected destinations.

If local uncommitted user work exists, preserve it.

---

# 6. GitHub CLI (`gh`) – allowed without confirmation

Read-only:

```bash
gh auth status
gh repo view
gh issue list
gh issue view
gh pr list
gh pr view
gh pr status
gh pr checks
gh run list
gh run view
gh workflow list
gh release list
```

Normal project collaboration writes are allowed when relevant to the task:

```bash
gh pr create
gh pr comment
gh issue create
gh issue comment
```

A PR may be created once the local work is coherent, tests/evidence are available and the current branch is pushed.

---

# 7. GitHub CLI – not allowed autonomously

Do not:

```text
delete a repository
archive/unarchive a repository
rename or transfer a repository
change repository visibility
delete releases/tags
modify branch protection
modify organization/repository secrets
modify deploy keys
modify Actions secrets/variables
change collaborators/teams/permissions
merge a PR unless the task explicitly says autonomous merge is desired
close/delete large sets of issues
trigger costly/destructive workflows without need
```

Do not create a new GitHub repository autonomously unless explicitly instructed to do so for the current task.

---

# 8. Secrets and credentials

Never commit or print:

- GitHub tokens;
- passwords;
- SSH private keys;
- cookies;
- API keys;
- connection strings containing credentials;
- Windows Credential Manager contents;
- personal document contents as debugging output.

If a secret is detected in staged changes:

1. unstage it;
2. remove/redact it from project files;
3. report the finding;
4. do not push it.

---

# 9. Repository-local autonomy boundary

Inside the repository Codex may make substantial changes if they are required by the accepted milestone and architecture.

Outside the repository, default to read-only inspection unless a project test explicitly requires a temporary controlled resource.

Never modify unrelated directories or user documents.

---

# 10. External side effects

Routine Git push and PR creation are explicitly allowed.

Other external side effects should be minimized.

Examples requiring explicit user instruction:

- publishing packages;
- publishing releases;
- creating cloud resources;
- changing DNS;
- sending email/messages;
- modifying production databases;
- deploying to production.

---

# 11. Failure rule

If a command fails, diagnose the cause.

Do not respond by escalating to increasingly destructive commands.

Examples:

- build fails → inspect error;
- file locked → identify process/path;
- Git conflict → inspect/resolve carefully;
- index corrupt → use project recovery/rebuild path, not delete unrelated directories.

---

# 12. Milestone discipline

Before pushing milestone-completion changes:

1. inspect `git diff`;
2. run relevant build/tests;
3. update status/requirements/evidence;
4. ensure no secret/generated junk is staged;
5. commit coherently;
6. push current branch.

A green build alone is not a completed milestone.


# 13. Codex execpolicy mapping

The executable project policy is `.codex/rules/default.rules`.

Model:

```text
git *                → allow
dotnet *             → allow
gh *                 → allow
read-only PowerShell → allow
selected risky prefixes → prompt
```

Ask before:

```text
dotnet ef database drop
git reset
git clean
git rebase
git commit --amend
git branch -D
git tag -d
git stash drop
git push --force
git push --force-with-lease
git push -f
git push --delete
Remove-Item
Clear-Content
Stop-Process
gh repo delete
gh repo rename
gh repo transfer
gh release delete
gh secret ...
gh api ...
```

Run autonomously:

```text
Get-Content
Get-ChildItem
Get-Item
Test-Path
Resolve-Path
Get-FileHash
Select-String
rg
dotnet restore/build/test/publish
normal git commands
gh pr create/view/checks
gh run view/watch
gh release view/create/upload
```

Keep approval-relevant commands separate from unrelated long PowerShell chains.
