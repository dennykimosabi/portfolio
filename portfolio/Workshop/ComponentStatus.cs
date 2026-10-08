namespace Storefront.Client.Workshop;

/// <summary>Lifecycle maturity of a component in the workshop catalog.</summary>
public enum ComponentStatus
{
    /// <summary>New or evolving. The API may still change — use with care.</summary>
    Experimental,
    /// <summary>Reviewed and dependable. Safe for production use.</summary>
    Stable,
    /// <summary>Superseded but still supported. Prefer the replacement.</summary>
    Legacy,
    /// <summary>Do not use in new work. Will be removed in a future pass.</summary>
    Deprecated
}
