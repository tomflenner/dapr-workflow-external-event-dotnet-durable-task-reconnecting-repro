using Dapr.Workflow;

namespace WFExternalEventPotentialIssue;

public class DummyWorkflow : Workflow<object?, object?>
{
    public override async Task<object?> RunAsync(WorkflowContext context, object? input)
    {
        var logger = context.CreateReplaySafeLogger<DummyWorkflow>();
        logger.LogInformation("Waiting for SIGNAL event to start the workflow");
        
        await context.WaitForExternalEventAsync<object?>("SIGNAL");
        logger.LogInformation("SIGNAL event received");
        
        await context.CallActivityAsync<object?>(nameof(DummyActivity));
        logger.LogInformation("DummyActivity completed, restarting workflow");
        
        context.ContinueAsNew(null, false);

        // Will never be reached due to ContinueAsNew
        return null;
    }
}