namespace WorkstationState
{
    internal interface IDiagnostic
    {
        void StartAndForget(CancellationToken cancellationToken = default);
    }
}