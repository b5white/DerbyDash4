using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Layout {

    public partial class Breadcrumb: ComponentBase {
        [Inject]
        protected NavigationManager NavManager { get; set; } = null!;

        protected List<BreadcrumbItem> Breadcrumbs { get; set; } = new List<BreadcrumbItem>();

        protected override void OnInitialized() {
            base.OnInitialized();
            string uri = NavManager.ToBaseRelativePath(NavManager.Uri);
            string key = StripPath(uri);
            UpdateBreadcrumbs(key);
            StateHasChanged();
        }

        private string StripPath(string uri) {
            string result;
            int index = uri.IndexOf('/');
            if (index != -1) {
                result = uri.Substring(index + 1);
            } else {
                result = uri;
            }
            return result;
        }

        private void UpdateBreadcrumbs(string uri) {
            Breadcrumbs = new List<BreadcrumbItem>();
            BreadcrumbItem link = BreadcrumbLink(uri);
            link.IsActive = true;
            if (link.Title != null) {
                Breadcrumbs.Add(link);
                while (link.Parent != "") {
                    link = BreadcrumbLink(link.Parent);
                    Breadcrumbs.Add(link);
                }
            }
            Breadcrumbs.Reverse();
        }

        public BreadcrumbItem BreadcrumbLink(string key) {
            return new BreadcrumbItem(BreadcrumbLinks.GetBreadcrumbLink(key));
        }
    }
}