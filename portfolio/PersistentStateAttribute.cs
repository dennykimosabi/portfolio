// Local shim for .NET 9: the PersistentStateAttribute ships in the framework
// starting with .NET 10. This keeps the Storefront.Client components compiling
// unchanged on net9.0. Delete this file if the project ever moves back to net10.0+.
namespace Microsoft.AspNetCore.Components;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PersistentStateAttribute : Attribute
{
}
