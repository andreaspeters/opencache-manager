using System;
using System.Net;
using System.Runtime.Versioning;
using Mono.Unix;
using Gtk;
using ocmengine;

namespace ocmgtk
{
    [SupportedOSPlatform("linux")]
    [GObject.Subclass<Gtk.Box>]
    public partial class DescriptionWidget
    {
        public HTMLWidget HtmlWidget { get; private set; }
        public Button HintButton { get; private set; }
        public Label HintLabel { get; private set; }
        private Geocache currentCache;
        private Dialog hintDialog;
        public event EventHandler HintDialogOpened;

        public static DescriptionWidget New()
        {
            return NewWithProperties([]);
        }

        partial void Initialize()
        {
            HtmlWidget = HTMLWidget.New();
            HintButton = Button.NewWithLabel(Catalog.GetString("Show Hint"));
            HintLabel = Label.New(string.Empty);
            HintButton.Sensitive = false;
            HintButton.OnClicked += (_, _) => ShowHint();
            HtmlWidget.SetVexpand(true);
            Append(HtmlWidget);
            Append(HintButton);
        }

        public void SetCache(Geocache cache)
        {
            currentCache = cache;
            if (cache == null)
            {
                HtmlWidget.HTML = "<b>NO CACHE SELECTED</b>";
                HintLabel.Label_ = string.Empty;
                HintButton.Sensitive = false;
                return;
            }

            SetDescription(cache);
            HintLabel.Label_ = cache.Hint ?? string.Empty;
            HintButton.Sensitive = !string.IsNullOrWhiteSpace(cache.Hint);
        }

        private void ShowHint()
        {
            if (currentCache == null || string.IsNullOrWhiteSpace(currentCache.Hint))
                return;

            if (hintDialog == null)
            {
                hintDialog = Dialog.New();
                hintDialog.Title = Catalog.GetString("Cache Hint");
                hintDialog.Modal = true;
                hintDialog.SetDefaultSize(600, 400);
                if (GetRoot() is Window parent)
                    hintDialog.TransientFor = parent;
                ((Gtk.Box)hintDialog.GetContentArea()).Append(HintLabel);
                hintDialog.AddButton("_Close", (int)ResponseType.Close);
                hintDialog.OnResponse += (_, _) => hintDialog.Hide();
            }

            hintDialog.Present();
            HintDialogOpened?.Invoke(this, EventArgs.Empty);
        }

        public void SetDescription(Geocache cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            string baseUrl = cache.URL == null
                ? null
                : cache.URL.Scheme + "://" + cache.URL.Host;
            string placedBy = string.IsNullOrEmpty(cache.PlacedBy)
                ? string.Empty
                : WebUtility.HtmlEncode(Catalog.GetString(" by ") + cache.PlacedBy);
            string html = "<HTML><BODY><div style='font-family:sans-serif;font-size:10pt; background-color:#FFFFFF'>" +
                          "<b>" + WebUtility.HtmlEncode(cache.Name) + ":" +
                          WebUtility.HtmlEncode(cache.CacheName) + placedBy + "</b><br/>" +
                          WebUtility.HtmlEncode(Geocache.GetCTypeString(cache.TypeOfCache)) +
                          "<br/><br/>" + cache.ShortDesc + "<br/><br/>" + cache.LongDesc +
                          "</div></BODY></HTML>";
            HtmlWidget.SetHTML(html, baseUrl);
        }
    }
}
