namespace WinSuperResolution.Models
{
    public enum ConnectionStatus
    {
        Unknown,
        Historical,
        Inactive,
        Active
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
        RequiresExactMatch,
        CurrentScaleUnavailable,
        NoVerifiedProfile,
        Available
    }
}
