using System;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gtk;
using JavaScriptCore;
using Mono.Unix;
using ocmengine;

namespace ocmgtk
{
    [SupportedOSPlatform("linux")]
    public sealed class ReviewWaypointWidget
    {
        private Geocache parent;
        private DegreeMinutes latitude;
        private DegreeMinutes longitude;
        private bool jumpPending;
        private readonly Entry nameEntry;
        private readonly ComboBoxText flagEntry;
        private readonly TextView descriptionText;
        private readonly Label coordinateLabel;

        public Box Widget { get; }
        public HTMLWidget SourceText { get; }
        public ComboBoxText TypeChoice => flagEntry;
        public ACacheStore CacheStore { get; set; }
        public bool IgnoreWaypointPrefixes { get; set; }
        public event EventHandler PreviewJumpRequested;

        public Geocache ParentCache
        {
            set => parent = value;
        }

        public Match WaypointMatch
        {
            set => SetPreview(parent, value);
        }

        public ReviewWaypointWidget()
        {
            Widget = Box.New(Orientation.Vertical, 6);
            coordinateLabel = Label.New(string.Empty);
            nameEntry = Entry.New();
            flagEntry = ComboBoxText.New();
            flagEntry.AppendText(Catalog.GetString("Final Location"));
            flagEntry.AppendText(Catalog.GetString("Parking Area"));
            flagEntry.AppendText(Catalog.GetString("Question to Answer"));
            flagEntry.AppendText(Catalog.GetString("Reference Point"));
            flagEntry.AppendText(Catalog.GetString("Stages of a Multicache"));
            flagEntry.AppendText(Catalog.GetString("Trailhead"));
            flagEntry.AppendText(Catalog.GetString("Other"));
            flagEntry.Active = 0;
            descriptionText = TextView.New();
            SourceText = HTMLWidget.New();

            Widget.Append(coordinateLabel);
            Widget.Append(Label.New(Catalog.GetString("Waypoint type")));
            Widget.Append(flagEntry);
            Widget.Append(Label.New(Catalog.GetString("Waypoint name")));
            Widget.Append(nameEntry);
            Widget.Append(Label.New(Catalog.GetString("Description")));
            Widget.Append(descriptionText);
            Widget.Append(SourceText);

            flagEntry.OnChanged += (_, _) => UpdateWaypointName();
            SourceText.ContentLoaded += (_, _) =>
            {
                if (!jumpPending)
                    return;

                jumpPending = false;
                _ = VerifyPreviewJumpAsync();
            };
        }

        private async Task VerifyPreviewJumpAsync()
        {
            try
            {
                Value result = await SourceText.EvaluateJavascriptAsync("gotoSpot(); location.hash");
                string hash = result.ToString();
                if (hash.Contains("#spot", StringComparison.Ordinal))
                    PreviewJumpRequested?.Invoke(this, EventArgs.Empty);
                else
                    Console.Error.WriteLine("GTK4_WAYPOINT_GOTO_UNVERIFIED=" + hash);
            }
            catch (System.Exception exception)
            {
                Console.Error.WriteLine("GTK4_WAYPOINT_GOTO_ERROR=" + exception.Message);
            }
        }

        public void SetPreview(Geocache cache, Match match)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (match == null || !match.Success)
                throw new ArgumentException("A matched waypoint coordinate is required.", nameof(match));

            parent = cache;
            string description = (cache.ShortDesc ?? string.Empty) + (cache.LongDesc ?? string.Empty);
            if (match.Index < 0 || match.Index + match.Length > description.Length ||
                description.Substring(match.Index, match.Length) != match.Value)
                throw new ArgumentException("The match does not belong to this cache description.", nameof(match));

            DegreeMinutes[] coordinates = Utilities.ParseCoordString(match.Captures[0].Value);
            latitude = coordinates[0];
            longitude = coordinates[1];
            coordinateLabel.Label_ = Utilities.getCoordString(latitude, longitude);
            descriptionText.Buffer.Text = Catalog.GetString("Grabbed Waypoint");
            UpdateWaypointName();

            StringBuilder html = new StringBuilder(description);
            html.Insert(match.Index + match.Length, "</span></a>");
            html.Insert(match.Index, "<a name='spot' id='spot'><span style='background-color:green;color:white;'>");
            html.Insert(0, "<HTML><HEAD><TITLE>ignore</TITLE>" +
                           "<SCRIPT>function gotoSpot(){ window.location.href='#spot';}</SCRIPT>" +
                           "</HEAD><BODY onLoad='gotoSpot()'>");
            html.Append("</BODY></HTML>");

            jumpPending = true;
            SourceText.SetHTML(html.ToString(), cache.URL?.AbsoluteUri);
        }

        public Waypoint GetPoint()
        {
            if (parent == null || latitude == null || longitude == null)
                throw new InvalidOperationException("A waypoint coordinate has not been selected.");

            return new Waypoint
            {
                Name = nameEntry.Text_,
                Lat = latitude.GetDecimalDegrees(),
                Lon = longitude.GetDecimalDegrees(),
                Parent = parent.Name,
                Symbol = GetPointType(flagEntry.Active),
                Desc = descriptionText.Buffer.Text
            };
        }

        private void UpdateWaypointName()
        {
            if (parent == null)
                return;

            string name = IgnoreWaypointPrefixes
                ? parent.Name
                : GetPrefix(flagEntry.Active) + (parent.Name.Length > 2 ? parent.Name.Substring(2) : parent.Name);
            nameEntry.Text_ = CacheStore == null ? name : CacheStore.GetUniqueName(name);
        }

        private static string GetPrefix(int type)
        {
            switch (type)
            {
                case 0: return "FL";
                case 1: return "PK";
                case 2: return "QA";
                case 3: return "RP";
                case 4: return "SM";
                case 5: return "TR";
                default: return "OT";
            }
        }

        private static string GetPointType(int type)
        {
            switch (type)
            {
                case 0: return "Final Location";
                case 1: return "Parking Area";
                case 2: return "Question to Answer";
                case 3: return "Reference Point";
                case 4: return "Stages of a Multicache";
                case 5: return "Trailhead";
                default: return "Other";
            }
        }
    }
}
