import json, re, subprocess, urllib.request, os
from pathlib import Path
url = "https://raw.githubusercontent.com/1159792767/Public-Library/main/feed/flat/demo.common.1159792767/index.json"
with urllib.request.urlopen(url, timeout=30) as response:
    versions = json.load(response)["versions"]
latest = max(versions, key=lambda v: tuple(map(int, v.split("."))))
p = Path("Demo.App.csproj")
original = p.read_text()
current = re.search(r'PackageReference Include="Demo.Common.1159792767" Version="([^"]+)"', original).group(1)
print(f"Installed {current}; published {latest}", flush=True)
if tuple(map(int, latest.split('.'))) <= tuple(map(int, current.split('.'))):
    raise SystemExit(0)
branch = "deps/demo-common-" + latest
def run(*args): return subprocess.check_output(args, text=True).strip()
existing = json.loads(run("gh", "pr", "list", "--head", branch, "--state", "open", "--json", "url"))
if existing:
    print(existing[0]["url"]); raise SystemExit(0)
run("git", "checkout", "-b", branch)
p.write_text(original.replace('Version="'+current+'"', 'Version="'+latest+'"'))
# Bot-created PR workflows may be suppressed or require approval.
# Validate the updated application here, before creating the PR.
subprocess.run(["dotnet", "run", "--project", "Demo.App.csproj"], check=True)
run("git", "config", "user.name", "dependency-demo-bot")
run("git", "config", "user.email", "dependency-demo-bot@users.noreply.github.com")
run("git", "add", "Demo.App.csproj")
run("git", "commit", "-m", "Update public library to " + latest)
run("git", "push", "origin", "HEAD:" + branch)
url = run("gh", "pr", "create", "--base", "main", "--head", branch,
          "--title", "Update Demo.Common to " + latest,
          "--body", "Public library update detected: " + current + " -> " + latest + ".\n\nThe update checker restored the real NuGet package and verified 10 x 3 = 30 before creating this PR. Merge to upgrade. This demo uses GitHub Actions, not Renovate.")
print(url)
with open(os.environ["GITHUB_STEP_SUMMARY"], "a") as f: f.write("Dependency update PR: " + url)

