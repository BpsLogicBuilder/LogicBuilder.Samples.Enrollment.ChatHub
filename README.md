# LogicBuilder.Samples.Enrollment.ChatHub

[![CI](https://github.com/BpsLogicBuilder/LogicBuilder.Samples.Enrollment.ChatHub/actions/workflows/ci.yml/badge.svg)](https://github.com/BpsLogicBuilder/LogicBuilder.Samples.Enrollment.ChatHub/actions/workflows/ci.yml)
[![CodeQL](https://github.com/BpsLogicBuilder/LogicBuilder.Samples.Enrollment.ChatHub/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/BpsLogicBuilder/LogicBuilder.Samples.Enrollment.ChatHub/actions/workflows/github-code-scanning/codeql)
[![codecov](https://codecov.io/gh/BpsLogicBuilder/LogicBuilder.Samples.Enrollment.ChatHub/graph/badge.svg?token=L4011F6T4X)](https://codecov.io/gh/BpsLogicBuilder/LogicBuilder.Samples.Enrollment.ChatHub)
[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=BpsLogicBuilder_LogicBuilder.Samples.Enrollment.ChatHub&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=BpsLogicBuilder_LogicBuilder.Samples.Enrollment.ChatHub)

A sample ASP.NET Core SignalR application demonstrating how to expose a chat endpoint that dynamically selects and runs AI agents driven by a **Logic Builder** rules-engine workflow.

## What this repository does

Chat clients connect to the `AgentAIChatHub` SignalR hub (mapped at `/agentChatHub`) and exchange messages with an AI agent. Rather than wiring the client to a single, statically configured agent, the agent to use is **selected dynamically at runtime** by a Logic Builder flow:

1. A client calls `SendMessageToAgent(threadId, userMessage, agentIdentifier)` on the hub.
2. If no `threadId` is supplied yet, the hub asks the `IAgentInitializer` to initialize an agent for the given `agentIdentifier`. The initializer runs a Logic Builder flow (via `IGetAgentFlowHelper.RunFlow`) that resolves the identifier to a configured `AIAgent`, and a new chat session/thread is created for it.
3. Subsequent messages on the same thread are forwarded to the resolved `AIAgent` through `IAgentHandler`, and streamed responses (chunks, completion, and errors) are sent back to the calling client over the SignalR connection.

## Key projects

- **Enrollment.ChatHub** – The ASP.NET Core host. Configures SignalR, dependency injection, and exposes `AgentAIChatHub` as the client-facing endpoint.
- **Enrollment.ChatHub.Flow** – The Logic Builder flow/ruleset project (`Rulesets/*.module`) responsible for the business logic that selects and builds the appropriate `AIAgent` for a given agent identifier, and for terminating/completing flows.
- **Enrollment.ChatHub.Test / Enrollment.ChatHub.Flow.Test** – Unit tests covering the hub, agent initializer, and flow behavior.

## Why Logic Builder?

Using a Logic Builder-driven flow to select the agent decouples the chat transport (SignalR) from agent selection/configuration logic, allowing agent behavior to be authored and changed as rules/flows rather than hard-coded application code.
