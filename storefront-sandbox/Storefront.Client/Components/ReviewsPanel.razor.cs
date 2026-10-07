using Microsoft.AspNetCore.Components;
using SimplePart.Shared.Models.Interfaces;
using SimplePart.Shared.Models.Interfaces.Storefront;
using SimplePart.Shared.Models.Storefront;

namespace Storefront.Client.Components;

public partial class ReviewsPanel : ComponentBase
{
    [Inject] public IReviewsService ReviewsService { get; set; } = default!;
    [Inject] public IWebsiteContextProvider WebsiteContext { get; set; } = default!;

    [Parameter] public int NumReviews { get; set; } = 3;
    [Parameter] public bool ReturnAll { get; set; }

    [PersistentState] public List<Review>? _testimonials { get; set; }
    [PersistentState] public AverageReviewsResult? _average { get; set; }

    private bool _loaded;

    protected override async Task OnInitializedAsync()
    {
        if (_testimonials is not null)
        {
            _loaded = true;
            return;
        }

        var testimonialsTask = ReviewsService.GetTestimonialsAsync(new GetTestimonialsRequest
        {
            UkeyWebsite = WebsiteContext.UkeyWebsite,
            ReturnAll = ReturnAll,
            NumReviews = ReturnAll ? 0 : NumReviews
        });
        var averageTask = ReviewsService.GetAverageReviewsAsync(WebsiteContext.UkeyWebsite);

        await Task.WhenAll(testimonialsTask, averageTask);

        _testimonials = testimonialsTask.Result.ToList();
        _average = averageTask.Result;
        _loaded = true;
    }

    // Legacy uses "rating" column like "4/5" or "4.5/5". Strip "/5", parse, round to nearest int.
    private static int ParseStarCount(string rating)
    {
        if (string.IsNullOrWhiteSpace(rating)) return 0;
        var clean = rating.Replace("/5", "").Trim();
        if (!decimal.TryParse(clean, out var value)) return 0;
        var rounded = (int)Math.Round(value, MidpointRounding.AwayFromZero);
        return Math.Clamp(rounded, 0, 5);
    }
}
