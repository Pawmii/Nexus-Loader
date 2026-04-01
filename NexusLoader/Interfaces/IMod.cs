namespace NexusLoader.Interfaces;

public interface IMod
{
    string Name { get; }
    string Author { get; }
    string? Description { get; }
    string Version { get; }
    string? RequiredNexusVersion { get; }

    void Enable();
    void Disable();

    void OnEnabling();
    void OnDisabling();
}