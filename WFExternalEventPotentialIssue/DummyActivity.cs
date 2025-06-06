using Dapr.Workflow;

namespace WFExternalEventPotentialIssue;

public class DummyActivity(ILogger<DummyActivity> logger) : WorkflowActivity<object?, object?>
{
    public override async Task<object?> RunAsync(WorkflowActivityContext context, object? input)
    {
        logger.LogInformation("Starting DummyActivity...");
        // Simulate some work
        await Task.Delay(2000);
        logger.LogInformation("DummyActivity completed.");
        return null;
    }
}