namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Preview-only shim for the .NET 10 <c>[PersistentState]</c> attribute so the client
/// components compile on the .NET 9 SDK. It does nothing: the preview runs InteractiveServer
/// without prerender, so there's no SSR → interactive hand-off to persist across.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PersistentStateAttribute : Attribute
{
}
