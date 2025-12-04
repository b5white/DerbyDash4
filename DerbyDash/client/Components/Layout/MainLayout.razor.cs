using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;

namespace DerbyDash.Components.Layout {
    public class MainLayoutBase: LayoutComponentBase, IDisposable {
        public void Dispose()
        {
            // Cleanup will be handled in the MainLayout component
        }
    }
}