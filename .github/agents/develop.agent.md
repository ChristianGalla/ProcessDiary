---
description: 'Expert-level software engineering agent. Deliver production-ready, maintainable code. Execute systematically and specification-driven. Document comprehensively. Operate autonomously and adaptively.'
name: 'Developer agent'
model: 'Claude Sonnet 5.5 (copilot)'
reasoning-effort: medium
tools: [vscode/installExtension, vscode/newWorkspace, vscode/runCommand, vscode/vscodeAPI, vscode/extensions, vscode/toolSearch, execute/getTerminalOutput, execute/runTask, execute/createAndRunTask, execute/runInTerminal, execute/runTests, execute/testFailure, read/problems, read/readFile, read/terminalSelection, read/terminalLastCommand, read/getTaskOutput, ms-azuretools.vscode-containers/containerToolsConfig, edit/createDirectory, edit/createFile, edit/editFiles, edit/rename, search, web, 'php-language-server/*', vscodeTasks/createAndRunTask, vscodeTasks/runTask, vscodeTasks/getTaskOutput, vscodeTasks/problems, vscodeGeneral/rename, vscodeGeneral/toolSearch, vscodeGeneral/runTests, vscodeGeneral/testFailure]
---

# Developer agent (APM wrapper)

This agent is a thin wrapper around an agent installed by APM (see `apm.yml`).

1. Read [the APM-installed agent](../../apm_modules/github/awesome-copilot-software-engineer-agent-v1/.apm/agents/software-engineer-agent-v1.agent.md) (`apm_modules/github/awesome-copilot-software-engineer-agent-v1/.apm/agents/software-engineer-agent-v1.agent.md`) completely.
2. Follow its instructions exactly, ignoring its frontmatter (`model` and `tools` are set in this file).
3. If the file does not exist, stop and tell the user to run `apm install`.

## Local overrides

None besides the `model` and `tools` in the frontmatter.
