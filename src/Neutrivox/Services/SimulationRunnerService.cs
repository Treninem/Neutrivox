using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class SimulationRunnerService
{
    private readonly SimulationSessionService _sessions;
    private readonly LogicExecutionService _logic;

    public SimulationRunnerService(SimulationSessionService sessions, LogicExecutionService logic)
    {
        _sessions = sessions;
        _logic = logic;
    }

    public SimulationResult ExecuteCycle(AutomationProject project, SimulationSession session)
    {
        var step = _sessions.Step(session, project);
        if (!step.Success) return step;

        var logic = _logic.Execute(project, session);
        foreach (var error in logic.Errors)
            session.Events.Add(new(DateTime.UtcNow, "LogicError", error));

        var message = logic.Success
            ? $"Cycle {session.Cycle} completed. {logic.ExecutedInstructions} instructions executed."
            : $"Cycle {session.Cycle} completed with {logic.Errors.Count} logic errors.";
        return new(logic.Success, message, 1);
    }

    public SimulationResult ExecuteCycles(AutomationProject project, SimulationSession session, int count)
    {
        if (count < 1) return new(false, "Cycle count must be greater than zero.", 0);
        SimulationResult result = new(true, "No cycles executed.", 0);
        for (var i = 0; i < count; i++)
        {
            result = ExecuteCycle(project, session);
            if (!result.Success) return new(false, result.Message, i + 1);
        }
        return new(true, result.Message, count);
    }
}