namespace DerbyDash.Components.Layout {
    public class BreadcrumbItem {
        public string Title { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string Parent { get; set; } = default!;
        public bool IsActive { get; set; } = false;

        public BreadcrumbItem(BreadcrumbLinksItem item) {
            Title = item.Title;
            Url = item.Url;
            Parent = item.Parent;
        }
    }
}
