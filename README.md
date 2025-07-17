# Workflow Engine API

A minimal and extensible Workflow Engine built with **.NET Core**, allowing users to define workflows, states, transitions, and execute state transitions for workflow instances.

## 🚀 Features

- Define custom **states**, **transitions**, and **workflows**
- Create and manage **workflow instances**
- Perform valid transitions based on current state
- Track transition **history** for each instance

## 📁 Project Structure

All the logic is implemented in `Program.cs` using minimal APIs and in-memory data stores.

---

## 🔧 Setup Instructions

1. Clone the repository:

```bash
git clone https://github.com/impxharsh/Workflow-Engine.git
cd Workflow-Engine
```
## ▶️ Run the Project

```bash
dotnet run
```
-- to see live updates / changes,
USE:
```bash
dotnet watch run
```

## API ENDPOINTS (Using Postman)
- create states
  ```bash
  Method: POST
  URL: http://localhost:5000/states
  Body (JSON):
  {
  "id": "draft",
  "name": "Draft",
  "isInitial": true,
  "isFinal": false,
  "enabled": true
  }

- create workflow
- ```bash
  Method: POST
  URL: http://localhost:5000/workflows
  Body (JSON):
  {
  "id": "wf1",
  "name": "Document Approval",
  "stateIds": ["draft", "review", "approved"]
  }
- create transitions
  
  ```bash
  Method: POST
  URL: http://localhost:5000/transitions 
  Body (JSON):
    {
  "id": "to-review",
  "fromStateId": "draft",
  "toStateId": "review",
  "workflowId": "wf1"
  }
- create workflowinstances
  ```bash
  Method: POST
  URL: http://localhost:5000/workflowinstances
  Body (JSON):

  { "workflowId": "docwf" }
- Perform State Transition
  ```bash
  Method: POST
  URL: http://localhost:5000/workflowinstances/{id}/transition
  Body (JSON):
  {
  "toState": "Approved"
  }
Replace {id} with actual id you recieved earlier

- Get Workflow Instance
 ```bash
  Method: GET
  URL: http://localhost:5000/workflowinstances/{id}
```
Replace {id} with actual id you recieved earlier
  




