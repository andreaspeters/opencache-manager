using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gtk;
using Mono.Unix;
using ocmengine;

namespace ocmgtk
{
    [SupportedOSPlatform("linux")]
    public sealed class CacheInfoWidget
    {
        private static readonly HttpClient imageHttpClient = new HttpClient();
        private const long MaximumImageBytes = 20 * 1024 * 1024;
        public Notebook Widget { get; }
        public Box QuickInfo { get; }
        public ListBox WaypointList { get; }
        public DescriptionWidget Description { get; }
        public LogViewerWidget LogViewer { get; }
        public event Action<Geocache> SaveNotesRequested;
        public event Action<string> OpenImageFolderRequested;
        public string NotesText
        {
            get { return notesEditor.Buffer.Text; }
            set { notesEditor.Buffer.Text = value ?? string.Empty; }
        }
        private readonly List<ListBoxRow> waypointRows = new List<ListBoxRow>();
        private Label noWaypointsLabel;
        private TextView notesEditor;
        private Button saveNotesButton;
        private Label notesErrorLabel;
        private Geocache currentCache;
        private bool loadingNotes;
        private ListBox imageList;
        private readonly List<ListBoxRow> imageRows = new List<ListBoxRow>();
        private readonly Dictionary<ListBoxRow, string> imagePaths = new Dictionary<ListBoxRow, string>();
        private Picture imagePreview;
        private Label noImagesLabel;
        private Label imagesFolderLabel;
        private Label imageFolderErrorLabel;
        private Label imageDownloadStatusLabel;
        private Button openImageFolderButton;
        private Button grabImagesButton;
        private Label cacheCodeLabel;
        private Label cacheNameLabel;
        private Label ownerLabel;
        private Label locationLabel;
        private Label coordinatesLabel;
        private Label statusLabel;
        private Label typeLabel;
        private Label containerLabel;
        private Label difficultyLabel;
        private Label terrainLabel;
        private Label attributesLabel;

        public CacheInfoWidget()
        {
            Widget = Notebook.New();
            QuickInfo = Box.New(Orientation.Vertical, 6);
            cacheCodeLabel = Label.New(string.Empty);
            cacheNameLabel = Label.New(string.Empty);
            ownerLabel = Label.New(string.Empty);
            locationLabel = Label.New(string.Empty);
            coordinatesLabel = Label.New(string.Empty);
            statusLabel = Label.New(string.Empty);
            typeLabel = Label.New(string.Empty);
            containerLabel = Label.New(string.Empty);
            difficultyLabel = Label.New(string.Empty);
            terrainLabel = Label.New(string.Empty);
            attributesLabel = Label.New(string.Empty);
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Cache code"), cacheCodeLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Cache name"), cacheNameLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Owner"), ownerLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Location"), locationLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Coordinates"), coordinatesLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Status"), statusLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Type"), typeLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Container"), containerLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Difficulty"), difficultyLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Terrain"), terrainLabel));
            QuickInfo.Append(CreateInfoRow(Catalog.GetString("Attributes"), attributesLabel));
            Description = DescriptionWidget.New();
            LogViewer = LogViewerWidget.New();
            notesEditor = TextView.New();
            notesEditor.Buffer.OnChanged += (_, _) =>
            {
                if (!loadingNotes)
                    saveNotesButton.Sensitive = currentCache != null;
            };
            Box notesPage = Box.New(Orientation.Vertical, 6);
            ScrolledWindow notesScroll = ScrolledWindow.New();
            notesScroll.SetChild(notesEditor);
            notesScroll.Vexpand = true;
            notesPage.Append(notesScroll);
            notesErrorLabel = Label.New(string.Empty);
            notesPage.Append(notesErrorLabel);
            saveNotesButton = Button.NewWithLabel(Catalog.GetString("Save notes"));
            saveNotesButton.Sensitive = false;
            saveNotesButton.OnClicked += (_, _) => SaveCurrentNotes();
            notesPage.Append(saveNotesButton);
            imageList = ListBox.New();
            imageList.SelectionMode = SelectionMode.Single;
            imagePreview = Picture.New();
            imagePreview.SetSizeRequest(320, 240);
            noImagesLabel = Label.New(Catalog.GetString("No local images for this cache."));
            imagesFolderLabel = Label.New(string.Empty);
            imageFolderErrorLabel = Label.New(string.Empty);
            imageDownloadStatusLabel = Label.New(string.Empty);
            Box imagesPage = Box.New(Orientation.Vertical, 6);
            imagesPage.Append(imagesFolderLabel);
            imagesPage.Append(noImagesLabel);
            Box imagesContent = Box.New(Orientation.Horizontal, 6);
            ScrolledWindow imageListScroll = ScrolledWindow.New();
            imageListScroll.SetChild(imageList);
            imageListScroll.Vexpand = true;
            imagesContent.Append(imageListScroll);
            imagesContent.Append(imagePreview);
            imagesPage.Append(imagesContent);
            imageList.OnSelectedRowsChanged += (_, _) =>
            {
                ListBoxRow selected = imageList.GetSelectedRow();
                if (selected != null && imagePaths.TryGetValue(selected, out string imagePath))
                {
                    imagePreview.SetFilename(imagePath);
                    noImagesLabel.SetVisible(false);
                    if (System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                        System.Console.WriteLine("GTK4_CACHE_IMAGE_PREVIEW=" + System.IO.Path.GetFileName(imagePath));
                }
            };
            openImageFolderButton = Button.NewWithLabel("Open Folder");
            openImageFolderButton.Sensitive = false;
            openImageFolderButton.OnClicked += (_, _) => OpenImageFolder();
            imagesPage.Append(openImageFolderButton);
            imagesPage.Append(imageFolderErrorLabel);
            grabImagesButton = Button.NewWithLabel(Catalog.GetString("Grab Images"));
            grabImagesButton.Sensitive = false;
            grabImagesButton.OnClicked += async (_, _) => await GrabImagesAsync();
            imagesPage.Append(grabImagesButton);
            imagesPage.Append(imageDownloadStatusLabel);
            WaypointList = ListBox.New();
            WaypointList.SelectionMode = SelectionMode.None;
            noWaypointsLabel = Label.New(Catalog.GetString("No waypoints for this cache."));
            Box waypointPage = Box.New(Orientation.Vertical, 6);
            waypointPage.Append(noWaypointsLabel);
            ScrolledWindow waypointScroll = ScrolledWindow.New();
            waypointScroll.SetChild(WaypointList);
            waypointScroll.Vexpand = true;
            waypointPage.Append(waypointScroll);
            QuickInfo.SetVexpand(true);
            waypointPage.SetVexpand(true);
            Description.SetVexpand(true);
            LogViewer.SetVexpand(true);
            Widget.AppendPage(QuickInfo, Label.New(Catalog.GetString("Quick Info")));
            Widget.AppendPage(Description, Label.New(Catalog.GetString("Cache Description")));
            Widget.AppendPage(waypointPage, Label.New(Catalog.GetString("Waypoints")));
            Widget.AppendPage(notesPage, Label.New(Catalog.GetString("Notes")));
            Widget.AppendPage(LogViewer, Label.New(Catalog.GetString("Cache Logs")));
            Widget.AppendPage(imagesPage, Label.New(Catalog.GetString("Images")));
        }

        private static Box CreateInfoRow(string caption, Label value)
        {
            Box row = Box.New(Orientation.Horizontal, 8);
            row.Append(Label.New(caption + ":"));
            row.Append(value);
            return row;
        }

        public void SetCache(Geocache cache, List<CacheLog> logs, List<Waypoint> waypoints = null,
            CacheAttribute[] attributes = null, List<string> imageFiles = null, string imageFolder = null)
        {
            currentCache = cache;
            cacheCodeLabel.Label_ = cache?.Name ?? string.Empty;
            cacheNameLabel.Label_ = cache?.CacheName ?? string.Empty;
            ownerLabel.Label_ = cache?.CacheOwner ?? string.Empty;
            string location = string.Empty;
            if (cache != null)
            {
                if (!string.IsNullOrWhiteSpace(cache.State))
                    location = cache.State.Trim();
                if (!string.IsNullOrWhiteSpace(cache.Country))
                    location += (location.Length == 0 ? string.Empty : ", ") + cache.Country.Trim();
            }
            locationLabel.Label_ = location;
            coordinatesLabel.Label_ = cache == null ? string.Empty : Utilities.getCoordString(cache.Lat, cache.Lon);
            string cacheStatus = GetCacheStatus(cache);
            statusLabel.Label_ = string.IsNullOrEmpty(cacheStatus) ? string.Empty : Catalog.GetString(cacheStatus);
            typeLabel.Label_ = cache?.TypeOfCache.ToString() ?? string.Empty;
            containerLabel.Label_ = cache?.Container ?? string.Empty;
            difficultyLabel.Label_ = cache == null ? string.Empty :
                cache.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture);
            terrainLabel.Label_ = cache == null ? string.Empty :
                cache.Terrain.ToString(System.Globalization.CultureInfo.InvariantCulture);
            List<string> attributeNames = new List<string>();
            if (attributes != null)
            {
                foreach (CacheAttribute attribute in attributes)
                    attributeNames.Add((attribute.Include ? string.Empty : "Not: ") + (attribute.AttrValue ?? string.Empty));
            }
            attributesLabel.Label_ = string.Join(", ", attributeNames);
            foreach (ListBoxRow row in waypointRows)
                WaypointList.Remove(row);
            waypointRows.Clear();
            if (waypoints != null)
            {
                foreach (Waypoint waypoint in waypoints)
                {
                    ListBoxRow row = ListBoxRow.New();
                    Box rowContent = Box.New(Orientation.Vertical, 2);
                    rowContent.Append(Label.New(waypoint.Name ?? string.Empty));
                    rowContent.Append(Label.New(waypoint.Desc ?? string.Empty));
                    row.SetChild(rowContent);
                    WaypointList.Append(row);
                    waypointRows.Add(row);
                }
            }
            noWaypointsLabel.SetVisible(waypointRows.Count == 0);
            foreach (ListBoxRow row in imageRows)
                imageList.Remove(row);
            imageRows.Clear();
            imagePaths.Clear();
            if (imageFiles != null)
            {
                foreach (string imagePath in imageFiles)
                {
                    ListBoxRow row = ListBoxRow.New();
                    row.SetChild(Label.New(System.IO.Path.GetFileName(imagePath)));
                    imageList.Append(row);
                    imageRows.Add(row);
                    imagePaths.Add(row, imagePath);
                }
            }
            imagesFolderLabel.Label_ = !string.IsNullOrWhiteSpace(imageFolder)
                ? imageFolder
                : imageFiles != null && imageFiles.Count > 0
                    ? System.IO.Path.GetDirectoryName(imageFiles[0])
                    : string.Empty;
            openImageFolderButton.Sensitive = cache != null && !string.IsNullOrWhiteSpace(imagesFolderLabel.Label_);
            imageFolderErrorLabel.Label_ = string.Empty;
            imageDownloadStatusLabel.Label_ = string.Empty;
            grabImagesButton.Sensitive = cache != null && !string.IsNullOrWhiteSpace(imagesFolderLabel.Label_) &&
                GetCacheImageUris(cache).Count > 0;
            noImagesLabel.SetVisible(imageRows.Count == 0);
            if (imageRows.Count > 0)
            {
                if (System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                    System.Console.WriteLine("GTK4_CACHE_IMAGE_FILES=" + imageRows.Count);
                imageList.SelectRow(imageRows[0]);
            }
            else
            {
                imagePreview.SetFilename(null);
                if (cache != null && System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                    System.Console.WriteLine("GTK4_CACHE_IMAGE_EMPTY=true");
            }
            loadingNotes = true;
            NotesText = cache?.Notes;
            loadingNotes = false;
            saveNotesButton.Sensitive = false;
            notesErrorLabel.Label_ = string.Empty;
            Description.SetCache(cache);
            LogViewer.SetCacheLogs(logs);
            if (cache != null && System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                System.Console.WriteLine("GTK4_CACHE_QUICK_INFO=" + cache.Name);
            if (cache != null && waypointRows.Count > 0 &&
                System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                System.Console.WriteLine("GTK4_CACHE_WAYPOINTS=" + waypoints[0].Name);
            if (cache != null && System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                System.Console.WriteLine("GTK4_CACHE_NOTES_LOADED=" + NotesText);
            if (cache != null && System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                System.Console.WriteLine("GTK4_QUICK_INFO_STATUS=" + cacheStatus);
            if (cache != null && System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
            {
                System.Console.WriteLine("GTK4_QUICK_INFO_LOCATION=" + location);
                System.Console.WriteLine("GTK4_QUICK_INFO_COORDINATES=" +
                    cache.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                    cache.Lon.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (cache != null && attributes != null && attributes.Length > 0 &&
                System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                System.Console.WriteLine("GTK4_QUICK_INFO_ATTRIBUTES=" + attributes[0].AttrValue);
        }

        public void OpenImageFolder()
        {
            string folder = imagesFolderLabel.Label_;
            Action<string> handler = OpenImageFolderRequested;
            if (handler == null || string.IsNullOrWhiteSpace(folder))
                return;

            try
            {
                System.IO.Directory.CreateDirectory(folder);
                handler(folder);
                if (System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                {
                    System.Console.WriteLine("GTK4_IMAGE_FOLDER_OPEN_REQUESTED=true");
                    if (imageRows.Count == 0)
                        System.Console.WriteLine("GTK4_IMAGE_FOLDER_EMPTY_OPEN_REQUESTED=true");
                }
                imageFolderErrorLabel.Label_ = string.Empty;
            }
            catch (Exception exception)
            {
                imageFolderErrorLabel.Label_ = Catalog.GetString("Could not open image folder: ") + exception.Message;
            }
        }

        public async Task GrabImagesAsync()
        {
            Geocache cache = currentCache;
            string folder = imagesFolderLabel.Label_;
            List<Uri> imageUris = GetCacheImageUris(cache);
            if (cache == null || string.IsNullOrWhiteSpace(folder) || imageUris.Count == 0)
                return;

            grabImagesButton.Sensitive = false;
            imageDownloadStatusLabel.Label_ = Catalog.GetString("Downloading images…");
            int downloaded = 0;
            int failed = 0;
            try
            {
                System.IO.Directory.CreateDirectory(folder);
                foreach (Uri imageUri in imageUris)
                {
                    string fileName = System.IO.Path.GetFileName(Uri.UnescapeDataString(imageUri.LocalPath));
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        failed++;
                        continue;
                    }

                    string destination = System.IO.Path.Combine(folder, fileName);
                    string temporaryFile = destination + ".partial";
                    try
                    {
                        using (HttpResponseMessage response = await imageHttpClient.GetAsync(
                                   imageUri, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();
                            if (response.Content.Headers.ContentLength > MaximumImageBytes)
                                throw new IOException("The image exceeds the 20 MB download limit.");

                            using (System.IO.Stream input = await response.Content.ReadAsStreamAsync())
                            using (System.IO.FileStream output = new System.IO.FileStream(
                                       temporaryFile, System.IO.FileMode.Create, System.IO.FileAccess.Write,
                                       System.IO.FileShare.None))
                            {
                                byte[] buffer = new byte[81920];
                                long totalBytes = 0;
                                int read;
                                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    totalBytes += read;
                                    if (totalBytes > MaximumImageBytes)
                                        throw new IOException("The image exceeds the 20 MB download limit.");
                                    await output.WriteAsync(buffer, 0, read);
                                }
                            }

                            System.IO.File.Move(temporaryFile, destination, true);
                        }

                        downloaded++;
                        if (currentCache != null && currentCache.Name == cache.Name)
                            AddImageFile(destination);
                        if (System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                            System.Console.WriteLine("GTK4_CACHE_IMAGE_ADDED=" + fileName);
                    }
                    catch (Exception exception)
                    {
                        failed++;
                        if (System.IO.File.Exists(temporaryFile))
                            System.IO.File.Delete(temporaryFile);
                        System.Console.Error.WriteLine("Could not download cache image " + imageUri + ": " + exception.Message);
                    }
                }

                imageDownloadStatusLabel.Label_ = string.Format(
                    Catalog.GetString("Downloaded {0} image(s); {1} failed."), downloaded, failed);
                if (System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                    System.Console.WriteLine("GTK4_CACHE_IMAGES_DOWNLOADED=" + downloaded);
            }
            catch (Exception exception)
            {
                imageFolderErrorLabel.Label_ = Catalog.GetString("Could not download cache images: ") + exception.Message;
            }
            finally
            {
                grabImagesButton.Sensitive = currentCache != null &&
                    !string.IsNullOrWhiteSpace(imagesFolderLabel.Label_) && GetCacheImageUris(currentCache).Count > 0;
            }
        }

        private static List<Uri> GetCacheImageUris(Geocache cache)
        {
            List<Uri> imageUris = new List<Uri>();
            if (cache == null || cache.URL == null || string.IsNullOrWhiteSpace(cache.LongDesc))
                return imageUris;

            const string imageSourcePattern = @"<img\b[^>]*\bsrc\s*=\s*(?:""(?<src>[^""]*)""|'(?<src>[^']*)'|(?<src>[^\s>]+))";
            MatchCollection matches = Regex.Matches(cache.LongDesc, imageSourcePattern,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                string source = WebUtility.HtmlDecode(match.Groups["src"].Value.Trim());
                if (source.Length == 0 || !Uri.TryCreate(cache.URL, source, out Uri imageUri))
                    continue;
                if ((imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps) ||
                    imageUris.Contains(imageUri))
                    continue;
                imageUris.Add(imageUri);
            }

            return imageUris;
        }

        private void AddImageFile(string imagePath)
        {
            if (imagePaths.ContainsValue(imagePath))
                return;

            ListBoxRow row = ListBoxRow.New();
            row.SetChild(Label.New(System.IO.Path.GetFileName(imagePath)));
            imageList.Append(row);
            imageRows.Add(row);
            imagePaths.Add(row, imagePath);
            imagesFolderLabel.Label_ = System.IO.Path.GetDirectoryName(imagePath);
            openImageFolderButton.Sensitive = true;
            noImagesLabel.SetVisible(false);
            imageList.SelectRow(row);
        }

        private static string GetCacheStatus(Geocache cache)
        {
            if (cache == null)
                return string.Empty;
            if (cache.Found)
                return "Found";
            if (cache.Archived)
                return "Archived";
            return cache.Available ? "Available" : "Unavailable";
        }

        public bool SaveCurrentNotes()
        {
            if (currentCache == null || SaveNotesRequested == null)
                return false;

            currentCache.Notes = NotesText;
            try
            {
                SaveNotesRequested(currentCache);
                notesErrorLabel.Label_ = string.Empty;
                if (System.Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST") == "1")
                    System.Console.WriteLine("GTK4_CACHE_NOTES_SAVED=true");
                saveNotesButton.Sensitive = false;
                return true;
            }
            catch (Exception exception)
            {
                notesErrorLabel.Label_ = Catalog.GetString("Could not save notes: ") + exception.Message;
                saveNotesButton.Sensitive = true;
                return false;
            }
        }
    }
}
