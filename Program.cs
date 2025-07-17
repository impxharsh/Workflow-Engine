var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var states = new List<State>();
var workflows = new List<Workflow>();
var transitions = new List<Transition>();

app.MapGet("/", () => "Hello world");


// states...
app.MapPost("/states", (State state) =>
{
  states.Add(state);
  return Results.Created($"/states/{state.Id}", state);
});

app.MapGet("/states", () => states);


// workflows...
app.MapPost("/workflows", (Workflow workflow) =>
{
    if (workflow.StateIds.Distinct().Count() != workflow.StateIds.Count)
        return Results.BadRequest("Duplicate state IDs in workflow.");

    var allStatesExist = workflow.StateIds.All(id => states.Any(s => s.Id == id));
    if (!allStatesExist)
        return Results.BadRequest("Some states in the workflow do not exist.");

    var initialStates = states.Where(s => workflow.StateIds.Contains(s.Id) && s.IsInitial).ToList();
    if (initialStates.Count != 1)
        return Results.BadRequest("Workflow must contain exactly one initial state.");

    var hasDisabledStates = states.Any(s => workflow.StateIds.Contains(s.Id) && !s.Enabled);
    if (hasDisabledStates)
        return Results.BadRequest("Workflow cannot include disabled states.");

    workflows.Add(workflow);
    return Results.Created($"/workflows/{workflow.Id}", workflow);
});


app.MapGet("/workflows", () =>
{
    return Results.Ok(workflows);
});

// transitions...

app.MapPost("/transitions", (Transition transition) =>
{
  var workflow = workflows.FirstOrDefault(w => w.Id == transition.WorkflowId);
  if (workflow is null)
  {
    return Results.BadRequest($"Workflow ID '{transition.WorkflowId}' not found.");
  }
  // agar from and to state ids exist nhi krti tb not found
  if (!workflow.StateIds.Contains(transition.FromStateId) || !workflow.StateIds.Contains(transition.ToStateId))
  {
    return Results.BadRequest("FromStateId or ToStateId not found in the workflow states.");
  }

  transitions.Add(transition);
  return Results.Created($"/transitions/{transition.Id}", transition);
});

app.MapGet("/transitions", () =>
{
  return Results.Ok(transitions);
});


// workflow ki ids...
app.MapGet("/workflows/{id}", (string id) =>
{
  var workflow = workflows.FirstOrDefault(w => w.Id == id);
  if (workflow is null)
  {
    return Results.NotFound($"Workflow with ID '{id}' not found.");
  }

  return Results.Ok(workflow);
});


// instancesss..

var workflowInstances = new List<WorkflowInstance>();

app.MapPost("/workflowinstances", (CreateWorkflowInstanceRequest request) =>
{
    var workflow = workflows.FirstOrDefault(w => w.Id == request.WorkflowId);
    if (workflow is null)
    {
        return Results.BadRequest($"Workflow ID '{request.WorkflowId}' not found.");
    }

    var initialStateId = states
        .FirstOrDefault(s => s.IsInitial && workflow.StateIds.Contains(s.Id))?.Id;

    if (initialStateId is null)
    {
        return Results.BadRequest("No initial state defined for this workflow.");
    }

    var instance = new WorkflowInstance(Guid.NewGuid().ToString(), request.WorkflowId, initialStateId);
    workflowInstances.Add(instance);

    return Results.Created($"/workflowinstances/{instance.Id}", instance);
});

app.MapGet("/workflowinstances", () =>
{
  return Results.Ok(workflowInstances);
});

app.MapGet("/workflowinstances/{id}", (string id) =>
{
    var instance = workflowInstances.FirstOrDefault(i => i.Id == id);
    if (instance is null)
    {
        return Results.NotFound($"Workflow instance with ID '{id}' not found.");
    }

    return Results.Ok(instance);
});

app.MapPost("/workflowinstances/{instanceId}/transitions/{transitionId}", (string instanceId, string transitionId) =>
{
    var instance = workflowInstances.FirstOrDefault(i => i.Id == instanceId);
    if (instance is null)
        return Results.NotFound($"Workflow instance '{instanceId}' not found.");

    var transition = transitions.FirstOrDefault(t => t.Id == transitionId);
    if (transition is null)
        return Results.NotFound($"Transition '{transitionId}' not found.");

    if (instance.WorkflowId != transition.WorkflowId)
        return Results.BadRequest("Transition does not belong to the same workflow as the instance.");

    if (transition.FromStateId != instance.CurrentStateId)
        return Results.BadRequest("Transition not valid from current state.");

   instance.CurrentStateId = transition.ToStateId;
   instance.History.Add(new WorkflowInstanceHistoryEntry(transition.Id));


    return Results.Ok(instance);
});



app.Run();

record State(string Id, string Name, bool IsInitial, bool IsFinal, bool Enabled);
record Workflow(string Id, string Name, List<string> StateIds);
record Transition(string Id, string FromStateId, string ToStateId, string WorkflowId);
public class WorkflowInstance
{
    public string Id { get; set; }
    public string WorkflowId { get; set; }
    public string CurrentStateId { get; set; }
    public List<WorkflowInstanceHistoryEntry> History { get; set; } = new();

    public WorkflowInstance(string id, string workflowId, string currentStateId)
    {
        Id = id;
        WorkflowId = workflowId;
        CurrentStateId = currentStateId;
    }
}


public class WorkflowInstanceHistoryEntry
{
    public string TransitionId { get; set; }
    public DateTime Timestamp { get; set; }

    public WorkflowInstanceHistoryEntry(string transitionId)
    {
        TransitionId = transitionId;
        Timestamp = DateTime.UtcNow;
    }
}

record CreateWorkflowInstanceRequest(string WorkflowId);


