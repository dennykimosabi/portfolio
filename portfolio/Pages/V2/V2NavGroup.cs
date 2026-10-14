using System.Collections.Generic;

namespace Storefront.Client.Pages.V2;

/// <summary>One group in the v2 section sidebar.</summary>
public sealed record V2NavGroup(string Title, List<(string Href, string Label)> Items);
