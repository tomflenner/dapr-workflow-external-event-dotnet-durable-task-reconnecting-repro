using Dapr.Workflow;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using WFExternalEventPotentialIssue;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging();
builder.Services.AddDaprWorkflow(
    options =>
    {
        options.RegisterWorkflow<DummyWorkflow>();
        options.RegisterActivity<DummyActivity>();
    });
var app = builder.Build();

app.MapPost("/schedule-workflow", async ([FromServices] DaprWorkflowClient workflowClient, [FromServices] ILogger<Program> logger) =>
{
    var createWorkflow = false;
    try
    {
        var throttle = await workflowClient.GetWorkflowStateAsync(nameof(DummyWorkflow), false);
        if (!throttle.Exists || !throttle.IsWorkflowRunning) createWorkflow = true;
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Unknown)
    {
        createWorkflow = true;
    }

    if (!createWorkflow)
    {
        logger.LogInformation(
            "'{WorkflowId}' workflow already exists and is running",
            nameof(DummyWorkflow)
        );
        return Results.Ok(new { Message = "Workflow already exists and is running." });
    }

    logger.LogWarning(
        "'{WorkflowId}' workflow does not exist, attempting to schedule it",
        nameof(DummyWorkflow)
    );

    try
    {
        await workflowClient.ScheduleNewWorkflowAsync(nameof(DummyWorkflow), nameof(DummyWorkflow));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to schedule workflow '{WorkflowId}'", nameof(DummyWorkflow));
        return Results.Problem("Failed to schedule workflow.");
    }
});

app.MapPost("/raise-event", async ([FromServices] DaprWorkflowClient workflowClient, [FromServices] ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Raising event for workflow...");
        await workflowClient.RaiseEventAsync(nameof(DummyWorkflow), "SIGNAL");
        return Results.Ok("Event raised successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to raise event for workflow");
        return Results.Problem("Failed to raise event for workflow.");
    }
});

await app.RunAsync();
