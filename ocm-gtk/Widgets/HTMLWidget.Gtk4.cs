using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using JavaScriptCore;
using Gtk;
using WebKit;

namespace ocmgtk
{
    [SupportedOSPlatform("linux")]
    [GObject.Subclass<Gtk.Box>]
    public partial class HTMLWidget
    {
        private bool contentLoaded;

        // GirCore 0.8.1's generated Decision getter casts a generic GObject.Object
        // to PolicyDecision and throws; consume raw GValues and native functions instead.
        private static readonly PropertyInfo SignalValuesProperty =
            typeof(GObject.SignalArgs).GetProperty("Args", BindingFlags.Instance | BindingFlags.NonPublic);

        public WebView View { get; private set; }

        public event EventHandler ContentLoaded;
        public event EventHandler<HtmlInternalCommandEventArgs> InternalCommandRequested;
        public event EventHandler<HtmlExternalNavigationEventArgs> ExternalNavigationRequested;

        public static HTMLWidget New()
        {
            return NewWithProperties([]);
        }

        partial void Initialize()
        {
            View = WebView.New();
            Append(View);
            View.OnLoadChanged += HandleLoadChanged;
            View.OnDecidePolicy += HandleDecidePolicy;
        }

        public void SetHTML(string html, string baseURL)
        {
            contentLoaded = false;
            View.LoadHtml(html, baseURL);
        }

        public string HTML
        {
            set => SetHTML(value, "http://www.geocaching.com");
        }

        public Task<Value> EvaluateJavascriptAsync(string function)
        {
            return View.EvaluateJavascriptAsync(function);
        }

        public void ExecuteFunction(string function)
        {
            _ = ObserveJavascriptAsync(function);
        }

        private async Task ObserveJavascriptAsync(string function)
        {
            try
            {
                await EvaluateJavascriptAsync(function);
            }
            catch (System.Exception exception)
            {
                Console.Error.WriteLine("HTMLWidget GTK4 JavaScript failed: " + exception.Message);
            }
        }

        private void HandleLoadChanged(WebView view, WebView.LoadChangedSignalArgs args)
        {
            if (args.LoadEvent != LoadEvent.Finished)
                return;

            contentLoaded = true;
            ContentLoaded?.Invoke(this, EventArgs.Empty);
        }

        private static GObject.Value[] GetRawSignalValues(WebView.DecidePolicySignalArgs args)
        {
            if (SignalValuesProperty == null)
                throw new InvalidOperationException("GirCore did not expose raw signal values.");

            return (GObject.Value[])SignalValuesProperty.GetValue(args);
        }

        private bool HandleDecidePolicy(WebView view, WebView.DecidePolicySignalArgs args)
        {
            if (!contentLoaded || args.DecisionType != PolicyDecisionType.NavigationAction)
                return false;

            GObject.Value[] values = GetRawSignalValues(args);
            if (values == null || values.Length <= 1)
                return false;

            using GObject.Object rawDecision = values[1].GetObject();
            if (rawDecision == null)
                return false;

            IntPtr decisionPointer = rawDecision.Handle.DangerousGetHandle();
            WebKit.Internal.PolicyDecision.Ignore(decisionPointer);

            using WebKit.Internal.NavigationActionUnownedHandle navigationAction =
                WebKit.Internal.NavigationPolicyDecision.GetNavigationAction(decisionPointer);
            if (navigationAction == null)
                return true;

            IntPtr requestPointer = WebKit.Internal.NavigationAction.GetRequest(navigationAction);
            if (requestPointer == IntPtr.Zero)
                return true;

            using GLib.Internal.NonNullableUtf8StringUnownedHandle uriHandle =
                WebKit.Internal.URIRequest.GetUri(requestPointer);
            string uri = Marshal.PtrToStringUTF8(uriHandle.DangerousGetHandle());
            if (string.IsNullOrEmpty(uri))
                return true;

            if (uri.StartsWith("ocm://", StringComparison.Ordinal))
            {
                string[] request = uri.Substring(6).Split('/');
                if (request.Length >= 2 &&
                    (request[0] == "deleteLog" || request[0] == "editLog"))
                {
                    InternalCommandRequested?.Invoke(
                        this,
                        new HtmlInternalCommandEventArgs(request[0], request[1]));
                }
            }
            else
            {
                ExternalNavigationRequested?.Invoke(this, new HtmlExternalNavigationEventArgs(uri));
            }

            return true;
        }
    }

    public sealed class HtmlInternalCommandEventArgs : EventArgs
    {
        public string Command { get; }
        public string Key { get; }

        public HtmlInternalCommandEventArgs(string command, string key)
        {
            Command = command;
            Key = key;
        }
    }

    public sealed class HtmlExternalNavigationEventArgs : EventArgs
    {
        public string Uri { get; }

        public HtmlExternalNavigationEventArgs(string uri)
        {
            Uri = uri;
        }
    }
}