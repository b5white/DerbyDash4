namespace DerbyDash.Components.Layout {
    public static partial class BreadcrumbLinks {
        public static BreadcrumbLinksItem GetBreadcrumbLink(string key) {
            key = key.ToLower();
            return BreadcrumbLinksDictionary.TryGetValue(key, out var item) ? item : new BreadcrumbLinksItem();
        }
    }
}
