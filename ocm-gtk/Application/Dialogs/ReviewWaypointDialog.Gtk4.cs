using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Gtk;
using Mono.Unix;
using ocmengine;

namespace ocmgtk
{
    [SupportedOSPlatform("linux")]
    public sealed class ReviewWaypointDialog
    {
        private readonly Geocache cache;
        private readonly IList<Match> matches;
        private readonly ACacheStore cacheStore;
        private readonly Action completed;
        private int matchIndex;
        private readonly Label errorLabel;

        public Dialog Window { get; }
        public ReviewWaypointWidget ReviewWidget { get; }
        public event Action<Waypoint> WaypointAdded;

        public ReviewWaypointDialog(Window parent, Geocache cache, IList<Match> matches,
            ACacheStore cacheStore, bool ignoreWaypointPrefixes, Action completed = null)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (matches == null || matches.Count == 0)
                throw new ArgumentException("At least one waypoint match is required.", nameof(matches));
            this.cache = cache;
            this.matches = matches;
            this.cacheStore = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
            this.completed = completed;

            Window = Dialog.New();
            Window.Title = Catalog.GetString("Reviewing Waypoint 1 of ") + matches.Count;
            Window.TransientFor = parent;
            Window.Modal = true;
            Window.SetDefaultSize(800, 600);

            ReviewWidget = new ReviewWaypointWidget
            {
                CacheStore = cacheStore,
                IgnoreWaypointPrefixes = ignoreWaypointPrefixes
            };
            errorLabel = Label.New(string.Empty);
            Box content = (Box)Window.GetContentArea();
            content.Append(ReviewWidget.Widget);
            content.Append(errorLabel);

            Window.AddButton("_Cancel", (int)ResponseType.Cancel);
            Window.AddButton("_Skip", (int)ResponseType.No);
            Window.AddButton("_Add", (int)ResponseType.Ok);
            Window.OnResponse += HandleResponse;
            ShowCurrentMatch();
        }

        public void Present()
        {
            Window.Present();
        }

        private void HandleResponse(Dialog dialog, Dialog.ResponseSignalArgs response)
        {
            if (response.ResponseId == (int)ResponseType.Cancel)
            {
                Window.Hide();
                completed?.Invoke();
                return;
            }

            if (response.ResponseId == (int)ResponseType.No)
            {
                Advance();
                return;
            }

            if (response.ResponseId != (int)ResponseType.Ok)
                return;

            try
            {
                Waypoint point = ReviewWidget.GetPoint();
                cacheStore.AddWaypointOrCache(point, false, false);
                WaypointAdded?.Invoke(point);
                Advance();
            }
            catch (System.Exception exception)
            {
                errorLabel.Label_ = Catalog.GetString("Could not add waypoint: ") + exception.Message;
            }
        }

        private void Advance()
        {
            matchIndex++;
            if (matchIndex >= matches.Count)
            {
                Window.Hide();
                completed?.Invoke();
                return;
            }

            ShowCurrentMatch();
        }

        private void ShowCurrentMatch()
        {
            ReviewWidget.SetPreview(cache, matches[matchIndex]);
            Window.Title = string.Format(Catalog.GetString("Reviewing Waypoint {0} of {1}"),
                matchIndex + 1, matches.Count);
            errorLabel.Label_ = string.Empty;
        }
    }
}
