using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Central session state for a single open project. Keeps editing, simulation and change history together.</summary>
public sealed class ProjectSessionService
{
    public AutomationProject? Project { get; private set; }
    public SimulationSession Simulation { get; } = new();
    public ProjectChangeTracker Changes { get; } = new();
    public bool HasProject => Project is not null;

    public void Open(AutomationProject project)
    {
        Project = project;
        Simulation.ChannelValues.Clear();
        Simulation.TagValues.Clear();
        Changes.Clear();
        Changes.Record("Project", $"Opened project '{project.Name}'.");
    }

    public AutomationProject Create(string name)
    {
        var project = new AutomationProject { Name = name };
        Open(project);
        Changes.Record("Project", "Created a new project.");
        return project;
    }

    public void RecordChange(string area, string description) => Changes.Record(area, description);
}
