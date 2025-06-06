# Dapr Workflow External Event Repro

This repository demonstrates a minimal setup to reproduce an issue with **Dapr Workflow** involving **external events** and **application restarts**.

## 🧪 Repro Scenario

The behavior occurs under the following steps:

1. **Schedule** a workflow that waits for an external event.
2. **Restart** the app (while the workflow is waiting).
3. Observe that `Dapr.DurableTask` logs show disconnecting/reconnecting behavior, potentially affecting event delivery or workflow reliability.

When you do the following steps :
1. **Schedule** a workflow that waits for an external event.
2. Observe that `Dapr.DurableTask` do not show logs disconnecting/reconnecting behavior.


## 🚀 Getting Started

### 1. Build and Start

From the root of the project:

```bash
docker compose build
docker compose up -d
```
ℹ️ This will start the app and Dapr sidecar in detached mode.

### 2. Schedule the Workflow
   Use curl to start a new workflow:

```bash
curl -X POST http://localhost:65000/schedule-workflow
```
This will start a workflow that waits for an external event.

#### 3. Simulate App Restart
Restart the application container:

```bash
docker compose restart wfexternaleventpotentialissue wfexternaleventpotentialissue-dapr
```
After restart, monitor logs for messages from `Dapr.DurableTask` about disconnects or replays.

### 4. Raise the External Event
Trigger the awaited event:

```bash
curl -X POST http://localhost:65000/raise-event
```


## 🐛 Expected Behavior vs. Actual Behavior

| Step             | Expected Behavior                                | Actual Behavior (Repro)                                                |
|------------------|--------------------------------------------------|------------------------------------------------------------------------|
| Schedule Workflow | Workflow starts and waits for external event     | ✅ Works as expected                                                    |
| Restart App       | Workflow should still be able to resume normally | ⚠️ `Dapr.DurableTask` shows disconnect/reconnect logs                  |
| Raise Event       | Workflow resumes and ContinueAsNew               | 🔄 Make the disconnect/reconnect logs disappear and exit from the loop |