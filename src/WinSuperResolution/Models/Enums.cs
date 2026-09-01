namespace WinSuperResolution.Models
{
    public enum ConnectionStatus
    {
        Unknown,
        Historical,
        Inactive,
        Active,
        Conflicted
    }

    public enum MatchStatus
    {
        Unmatched,
        Candidate,
        Exact,
        Ambiguous
    }

    public enum CalculationBasis
    {
        Unavailable,
        ActiveSize,
        PrimSurfSize
    }

    public enum ValidationStatus
    {
        Ready,
        Warning,
        Blocked,
        Error
    }

    public enum ScaleAvailabilityStatus
    {
        NoSelection,
        RequiresActiveDisplay,
        CurrentScaleUnavailable,
        NoCompatibleSettingsTarget,
        Available
    }
}
