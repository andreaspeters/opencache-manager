using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using Gtk;
using ocmengine;

namespace ocmgtk
{
    [SupportedOSPlatform("linux")]
    [GObject.Subclass<Gtk.Box>]
    public partial class LogViewerWidget
    {
        private List<CacheLog> logs = new List<CacheLog>();

        public HTMLWidget HtmlWidget { get; private set; }

        public static LogViewerWidget New()
        {
            return NewWithProperties([]);
        }

        partial void Initialize()
        {
            HtmlWidget = HTMLWidget.New();
            Append(HtmlWidget);
        }

        public DateTime GetLastFound()
        {
            DateTime lastFound = DateTime.MinValue;
            foreach (CacheLog log in logs)
            {
                if (log.LogDate > lastFound)
                    lastFound = log.LogDate;
            }
            return lastFound;
        }

        public DateTime GetLastFoundBy(string user)
        {
            DateTime lastFound = DateTime.MinValue;
            foreach (CacheLog log in logs)
            {
                if (IsFoundStatus(log.LogStatus) && log.LogDate > lastFound &&
                    (log.FinderID == user || log.LoggedBy == user))
                    lastFound = log.LogDate;
            }
            return lastFound;
        }

        public DateTime GetLastDNFBy(string user)
        {
            DateTime lastDnf = DateTime.MinValue;
            foreach (CacheLog log in logs)
            {
                if (log.LogStatus == "Didn't find it" && log.LogDate > lastDnf &&
                    (log.FinderID == user || log.LoggedBy == user))
                    lastDnf = log.LogDate;
            }
            return lastDnf;
        }

        public void SetCacheLogs(List<CacheLog> cacheLogs)
        {
            logs = cacheLogs ?? new List<CacheLog>();
            StringBuilder html = new StringBuilder("<HTML><BODY>");
            if (cacheLogs == null)
                html.Append(Mono.Unix.Catalog.GetString("NO CACHE SELECTED"));
            else
            {
                foreach (CacheLog log in cacheLogs)
                    html.Append(log.toHTML());
            }
            html.Append("</BODY></HTML>");
            string baseUrl = "file://" + Environment.CurrentDirectory + "/icons/24x24/.";
            HtmlWidget.SetHTML(html.ToString(), baseUrl);
        }

        private static bool IsFoundStatus(string status)
        {
            return status == "Found it" || status == "find" ||
                   status == "Attended" || status == "Webcam Photo Taken";
        }
    }
}
