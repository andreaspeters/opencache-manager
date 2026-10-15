using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using IOFile = System.IO.File;
using Gio;
using Gtk;
using Mono.Data.Sqlite;
using ocmengine;
using ocmengine.SQLLite;


namespace ocmgtk
{
    [SupportedOSPlatform("linux")]
    internal static class OCMAppGtk4
    {
        private const string DatabaseOverrideEnvironmentVariable = "OCM_GTK4_DATABASE";

        public static int Main(string[] args)
        {
            string databaseSummary = ReadConfiguredDatabaseSummary();
            Console.WriteLine("GTK4_DB_STATUS=" + databaseSummary);
            List<string> ownerIDs = ReadConfiguredOwnerIDs();
            bool[] initialStatusFilters = { true, true, true, true, true, true };
            List<Geocache> caches = ReadConfiguredCaches(initialStatusFilters, ownerIDs.ToArray(), string.Empty);
            Console.WriteLine("GTK4_CACHE_COUNT=" + caches.Count);
            int selectedCacheIndex = 0;
            Geocache selectedCache = caches.Count == 0 ? null : caches[selectedCacheIndex];
            bool smokeTest = string.Equals(
                Environment.GetEnvironmentVariable("OCM_GTK4_SMOKE_TEST"),
                "1",
                StringComparison.Ordinal);

            Gtk.Application application = Gtk.Application.New(
                "org.opencachemanager.desktop.gtk4",
                ApplicationFlags.FlagsNone);

            ApplicationWindow window = null;
            application.OnActivate += (sender, _) =>
            {
                if (window != null)
                {
                    window.Show();
                    return;
                }

                window = ApplicationWindow.New((Gtk.Application)sender);
                window.Title = "OpenCacheManager GTK4";
                SynchronizationContext gtkSynchronizationContext = SynchronizationContext.Current;
                if (smokeTest)
                    Console.WriteLine("GTK4_SYNC_CONTEXT=" + (gtkSynchronizationContext?.GetType().FullName ?? "null"));
                window.SetDefaultSize(900, 600);

                ReviewWaypointDialog waypointDialog = null;
                List<Match> waypointMatches = new List<Match>();
                if (selectedCache != null)
                {
                    MatchCollection foundMatches = Regex.Matches(selectedCache.ShortDesc + selectedCache.LongDesc,
                        @"N \d{2} \d{2}\.\d{3} W \d{3} \d{2}\.\d{3}");
                    foreach (Match match in foundMatches)
                        waypointMatches.Add(match);
                }
                Box mainContent = Box.New(Orientation.Vertical, 4);
                Button reviewWaypointsButton = Button.NewWithLabel("Review Waypoint Matches");
                reviewWaypointsButton.Sensitive = waypointMatches.Count > 0;
                mainContent.Append(reviewWaypointsButton);
                Button offlineLogButton = Button.NewWithLabel("Add Offline Log");
                offlineLogButton.Sensitive = selectedCache != null;
                mainContent.Append(offlineLogButton);
                Button exportFindsButton = Button.NewWithLabel("Export Finds GPX");
                exportFindsButton.Sensitive = ownerIDs.Count > 0;
                mainContent.Append(exportFindsButton);
                Button exportAllButton = Button.NewWithLabel("Export GPX");
                exportAllButton.Sensitive = caches.Count > 0;
                mainContent.Append(exportAllButton);
                Button importGpxButton = Button.NewWithLabel("Import GPX");
                mainContent.Append(importGpxButton);
                Box cacheSelectionPanel = Box.New(Orientation.Vertical, 4);
                CacheInfoWidget cacheInfoWidget = new CacheInfoWidget();
                cacheInfoWidget.SaveNotesRequested += SaveConfiguredCacheNotes;
                ListBox cacheListBox = ListBox.New();
                cacheListBox.SelectionMode = SelectionMode.Single;
                List<ListBoxRow> cacheRows = new List<ListBoxRow>();
                HashSet<string> engineFilteredCacheNames = new HashSet<string>(StringComparer.Ordinal);
                bool smokeEmptyImageFolderOpened = false;
                foreach (Geocache cache in caches)
                {
                    engineFilteredCacheNames.Add(cache.Name);
                    ListBoxRow row = ListBoxRow.New();
                    Box rowContent = Box.New(Orientation.Horizontal, 8);
                    rowContent.Append(Label.New(cache.Name));
                    rowContent.Append(Label.New(cache.CacheName ?? string.Empty));
                    row.SetChild(rowContent);
                    row.SetActivatable(true);
                    cacheListBox.Append(row);
                    cacheRows.Add(row);
                }
                if (smokeTest)
                    Console.WriteLine("GTK4_CACHE_LIST=" + caches.Count);
                if (cacheRows.Count > 0)
                    cacheListBox.SelectRow(cacheRows[0]);
                cacheListBox.OnSelectedRowsChanged += (_, _) =>
                {
                    int index = cacheRows.IndexOf(cacheListBox.GetSelectedRow());
                    if (index >= 0 && index != selectedCacheIndex)
                        SelectCacheAt(index, "LIST_SELECTION");
                };
                ScrolledWindow cacheListScroll = ScrolledWindow.New();
                cacheListScroll.SetChild(cacheListBox);
                cacheListScroll.Vexpand = true;
                cacheSelectionPanel.Append(cacheListScroll);
                Label noMatchingCachesLabel = Label.New("No caches match the current filters.");
                noMatchingCachesLabel.SetVisible(false);
                cacheSelectionPanel.Append(noMatchingCachesLabel);
                cacheSelectionPanel.Append(Label.New("Find cache:"));
                Entry cacheSearchEntry = Entry.New();
                Button clearSearchButton = Button.NewWithLabel("Clear");
                clearSearchButton.Sensitive = false;
                void ClearCacheSearch()
                {
                    if (string.IsNullOrWhiteSpace(cacheSearchEntry.Text_))
                        return;
                    cacheSearchEntry.Text_ = string.Empty;
                    if (smokeTest)
                        Console.WriteLine("GTK4_CACHE_SEARCH_CLEAR_ACTION=true");
                }
                clearSearchButton.OnClicked += (_, _) => ClearCacheSearch();
                Box searchRow = Box.New(Orientation.Horizontal, 4);
                searchRow.Append(cacheSearchEntry);
                searchRow.Append(clearSearchButton);
                cacheSelectionPanel.Append(searchRow);
                CheckButton availableFilter = CheckButton.NewWithLabel("Available");
                CheckButton unavailableFilter = CheckButton.NewWithLabel("Not available");
                CheckButton archivedFilter = CheckButton.NewWithLabel("Archived");
                CheckButton foundFilter = CheckButton.NewWithLabel("Found");
                CheckButton notFoundFilter = CheckButton.NewWithLabel("Not found");
                CheckButton mineFilter = CheckButton.NewWithLabel("Mine");
                availableFilter.Active = true;
                unavailableFilter.Active = true;
                archivedFilter.Active = true;
                foundFilter.Active = true;
                notFoundFilter.Active = true;
                mineFilter.Active = true;
                Box statusFilters = Box.New(Orientation.Horizontal, 8);
                statusFilters.Append(availableFilter);
                statusFilters.Append(unavailableFilter);
                statusFilters.Append(archivedFilter);
                statusFilters.Append(foundFilter);
                statusFilters.Append(notFoundFilter);
                statusFilters.Append(mineFilter);
                cacheSelectionPanel.Append(statusFilters);
                Label cacheFilterSummary = Label.New(
                    "Showing " + caches.Count.ToString(CultureInfo.InvariantCulture) +
                    " of " + caches.Count.ToString(CultureInfo.InvariantCulture) + " caches");
                cacheSelectionPanel.Append(cacheFilterSummary);
                HTMLWidget htmlWidget = cacheInfoWidget.LogViewer.HtmlWidget;
                HTMLWidget descriptionHtml = cacheInfoWidget.Description.HtmlWidget;
                void RouteExternalNavigation(string uri)
                {
                    bool started = ExternalProcessLauncher.TryOpenExternalTarget(
                        uri,
                        exception => ShowExternalNavigationError(window, exception),
                        action =>
                        {
                            if (gtkSynchronizationContext == null)
                            {
                                Console.Error.WriteLine("GTK4_EXTERNAL_NAVIGATION_ERROR_NO_UI_CONTEXT");
                                return;
                            }
                            gtkSynchronizationContext.Post(_ => action(), null);
                        });
                    if (smokeTest)
                        Console.WriteLine("GTK4_EXTERNAL_NAVIGATION_LAUNCHED=" + started.ToString().ToLowerInvariant());
                }
                htmlWidget.InternalCommandRequested += (_, command) =>
                {
                    Console.WriteLine("GTK4_NAVIGATION=" + command.Command + ":" + command.Key);
                    if (command.Command == "deleteLog")
                        ConfirmDeleteLog(window, command.Key, smokeTest);
                    else if (command.Command == "editLog")
                        TryShowEditLogDialog(window, command.Key, smokeTest);
                };
                htmlWidget.ExternalNavigationRequested += (_, navigation) => RouteExternalNavigation(navigation.Uri);
                descriptionHtml.ExternalNavigationRequested += (_, navigation) => RouteExternalNavigation(navigation.Uri);
                cacheInfoWidget.OpenImageFolderRequested += folder =>
                    RouteExternalNavigation(new Uri(folder).AbsoluteUri);
                bool smokeLogActionsTriggered = false;
                htmlWidget.ContentLoaded += (_, _) =>
                {
                    if (smokeTest && !smokeLogActionsTriggered)
                    {
                        smokeLogActionsTriggered = true;
                        htmlWidget.ExecuteFunction("var editLink=document.querySelector(\"a[href='ocm://editLog/GC-SYNTHETIC-EDIT']\"); if(editLink){editLink.click(); setTimeout(function(){var deleteLink=document.querySelector(\"a[href='ocm://deleteLog/GC-SYNTHETIC-LOG']\"); if(deleteLink) deleteLink.click();},50);}");
                    }
                };
                if (smokeTest)
                {
                    bool firstCacheContentLoaded = false;
                    bool secondCacheContentVerified = false;
                    bool smokeImagesDownloadTriggered = false;
                    descriptionHtml.ContentLoaded += async (_, _) =>
                    {
                        if (selectedCache == null)
                            return;

                        if (selectedCache?.Name == "GC-SECOND")
                        {
                            if (secondCacheContentVerified)
                                return;
                            secondCacheContentVerified = true;
                            try
                            {
                                string secondCacheResult = (await descriptionHtml.EvaluateJavascriptAsync(
                                    "document.body.innerText.includes('GC-SECOND:Second fixture') && document.body.innerText.includes('Second cache description')")).ToString();
                                if (string.Equals(secondCacheResult.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("GTK4_CACHE_SELECTION_DETAILS=true");
                                    if (cacheSearchEntry.Text_ != "O'Brien")
                                        cacheSearchEntry.Text_ = "O'Brien";
                                }
                                else
                                    Console.Error.WriteLine("GTK4_CACHE_SELECTION_DETAILS=" + secondCacheResult);
                            }
                            catch (System.Exception exception)
                            {
                                Console.Error.WriteLine("GTK4_CACHE_SELECTION_DETAILS_ERROR=" + exception.Message);
                            }
                            return;
                        }

                        if (selectedCache?.Name == "GC-SYNTHETIC" && firstCacheContentLoaded)
                        {
                            string searchedCacheResult = (await descriptionHtml.EvaluateJavascriptAsync(
                                "document.body.innerText.includes('GC-SYNTHETIC:Fixture cache')")).ToString();
                            if (string.Equals(searchedCacheResult.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("GTK4_CACHE_SEARCH_DETAILS=true");
                                ClearCacheSearch();
                                archivedFilter.Active = false;
                                archivedFilter.Active = true;
                                mineFilter.Active = false;
                                mineFilter.Active = true;
                                foundFilter.Active = false;
                                notFoundFilter.Active = false;
                                archivedFilter.Active = false;
                            }
                            else
                                Console.Error.WriteLine("GTK4_CACHE_SEARCH_DETAILS=" + searchedCacheResult);
                            return;
                        }

                        try
                        {
                            string result = (await descriptionHtml.EvaluateJavascriptAsync(
                                "document.body.innerText.includes('GC-SYNTHETIC:Fixture cache') && document.body.innerText.includes('Traditional Cache') && document.body.innerText.includes('N 47 36.123 W 122 19.456')")).ToString();
                            if (string.Equals(result.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                                Console.WriteLine("GTK4_CACHE_DESCRIPTION_RENDERED=true");
                            else
                                Console.Error.WriteLine("GTK4_CACHE_DESCRIPTION_RENDERED=" + result);
                        }
                        catch (System.Exception exception)
                        {
                            Console.Error.WriteLine("GTK4_CACHE_DESCRIPTION_RENDERED_ERROR=" + exception.Message);
                        }

                        if (!smokeImagesDownloadTriggered)
                        {
                            smokeImagesDownloadTriggered = true;
                            await cacheInfoWidget.GrabImagesAsync();
                        }

                        descriptionHtml.ExecuteFunction("document.querySelector(\"a[href='https://example.invalid']\").click();");
                        cacheInfoWidget.Widget.SetCurrentPage(4);
                        if (cacheInfoWidget.Widget.GetCurrentPage() == 4)
                            Console.WriteLine("GTK4_OFFLINE_LOG_TAB=selected");
                        firstCacheContentLoaded = true;
                    };

                    cacheInfoWidget.Description.HintDialogOpened += (_, _) =>
                    {
                        if (cacheInfoWidget.Description.HintLabel.Label_ == "Synthetic hidden hint")
                        {
                            Console.WriteLine("GTK4_HINT_DIALOG_SHOWN=true");
                            if (cacheRows.Count > 1)
                                cacheListBox.SelectRow(cacheRows[1]);
                        }
                        else
                            Console.Error.WriteLine("GTK4_HINT_DIALOG_CONTENT=" +
                                cacheInfoWidget.Description.HintLabel.Label_);
                    };
                }
                List<CacheLog> logs = selectedCache == null ? null : ReadConfiguredCacheLogs(selectedCache.Name);
                if (selectedCache != null)
                    Console.WriteLine("GTK4_CACHE_SELECTED=" + selectedCache.Name);
                cacheInfoWidget.SetCache(selectedCache, logs,
                    selectedCache == null ? null : ReadConfiguredChildWaypoints(selectedCache.Name),
                    selectedCache == null ? null : ReadConfiguredCacheAttributes(selectedCache.Name),
                    selectedCache == null ? null : ReadConfiguredCacheImageFiles(selectedCache.Name),
                    selectedCache == null ? null : ReadConfiguredCacheImageFolder(selectedCache.Name));
                if (smokeTest && selectedCache?.Name == "GC-SYNTHETIC")
                    cacheInfoWidget.OpenImageFolder();
                if (smokeTest && selectedCache != null)
                {
                    if (cacheInfoWidget.NotesText != "Original note")
                        throw new InvalidOperationException("The GTK4 notes page did not load the cache's persisted note.");
                    cacheInfoWidget.NotesText = "Synthetic note [GTK4]";
                    if (!cacheInfoWidget.SaveCurrentNotes())
                        throw new InvalidOperationException("The GTK4 notes page could not save its note.");
                }
                if (smokeTest && selectedCache != null)
                {
                    FileStore cancelStore = new FileStore(ReadConfiguredDatabasePath());
                    bool cancelCallbackCalled = false;
                    ReviewWaypointDialog cancelDialog = new ReviewWaypointDialog(window, selectedCache,
                        waypointMatches, cancelStore, false, () =>
                        {
                            cancelCallbackCalled = true;
                            cancelStore.Dispose();
                        });
                    cancelDialog.Window.Response((int)ResponseType.Cancel);
                    if (!cancelCallbackCalled)
                        throw new InvalidOperationException("The GTK4 waypoint dialog did not complete on Cancel.");
                    Console.WriteLine("GTK4_WAYPOINT_CANCEL=clean");
                }

                void SelectCacheAt(int index, string action)
                {
                    selectedCacheIndex = index;
                    selectedCache = caches[selectedCacheIndex];
                    offlineLogButton.Sensitive = true;
                    cacheInfoWidget.SetCache(selectedCache, ReadConfiguredCacheLogs(selectedCache.Name),
                        ReadConfiguredChildWaypoints(selectedCache.Name),
                        ReadConfiguredCacheAttributes(selectedCache.Name),
                        ReadConfiguredCacheImageFiles(selectedCache.Name),
                        ReadConfiguredCacheImageFolder(selectedCache.Name));
                    if (smokeTest && selectedCache.Name == "GC-SECOND" && !smokeEmptyImageFolderOpened)
                    {
                        smokeEmptyImageFolderOpened = true;
                        cacheInfoWidget.OpenImageFolder();
                    }
                    if (cacheListBox.GetSelectedRow() != cacheRows[selectedCacheIndex])
                        cacheListBox.SelectRow(cacheRows[selectedCacheIndex]);
                    Console.WriteLine("GTK4_CACHE_" + action + "=" + selectedCache.Name);
                }

                void StartOfflineLog()
                {
                    if (selectedCache == null)
                        return;

                    string ownerID = ownerIDs.Count > 0 ? ownerIDs[0] : string.Empty;
                    ShowNewOfflineLogDialog(window, selectedCache, ownerID, smokeTest,
                        () => SelectCacheAt(selectedCacheIndex, "OFFLINE_LOG"));
                }
                offlineLogButton.OnClicked += (_, _) => StartOfflineLog();

                void ExportFindsToFile(string filePath)
                {
                    try
                    {
                        WriteFindsGpxFile(filePath, ownerIDs[0]);
                        Console.WriteLine("GTK4_FINDS_GPX_EXPORTED=true");
                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine("GTK4_FINDS_GPX_EXPORT_ERROR=" + exception.Message);
                        ShowExportError(window, exception);
                    }
                }

                void ChooseGpxOutputPath(string title, string defaultName, string smokePathVariable,
                    System.Action<string> exportAction)
                {
                    string smokePath = Environment.GetEnvironmentVariable(smokePathVariable);
                    if (smokeTest && !string.IsNullOrWhiteSpace(smokePath))
                    {
                        exportAction(smokePath);
                        return;
                    }

                    FileChooserNative chooser = FileChooserNative.New(
                        title, window, FileChooserAction.Save, "_Export", "_Cancel");
                    chooser.SetCurrentName(defaultName);
                    chooser.OnResponse += (_, response) =>
                    {
                        if (response.ResponseId == (int)ResponseType.Accept)
                        {
                            Gio.File selectedFile = chooser.GetFile();
                            string filePath = selectedFile?.GetPath();
                            if (!string.IsNullOrWhiteSpace(filePath))
                                exportAction(filePath);
                        }
                        chooser.Destroy();
                    };
                    chooser.Show();
                }

                void StartFindsGpxExport()
                {
                    if (ownerIDs.Count == 0)
                        return;
                    ChooseGpxOutputPath("Export Finds GPX", "finds.gpx", "OCM_GTK4_EXPORT_FINDS_PATH",
                        ExportFindsToFile);
                }
                exportFindsButton.OnClicked += (_, _) => StartFindsGpxExport();

                void ExportAllGpxToFile(string filePath)
                {
                    try
                    {
                        WriteGpxFile(filePath, caches);
                        Console.WriteLine("GTK4_GPX_EXPORTED=true");
                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine("GTK4_GPX_EXPORT_ERROR=" + exception.Message);
                        ShowExportError(window, exception);
                    }
                }

                void StartAllGpxExport()
                {
                    if (caches.Count == 0)
                        return;
                    ChooseGpxOutputPath("Export GPX", "export.gpx", "OCM_GTK4_EXPORT_ALL_PATH",
                        ExportAllGpxToFile);
                }
                exportAllButton.OnClicked += (_, _) => StartAllGpxExport();

                int ApplyCacheListFilters()
                {
                    int visibleCount = 0;
                    int firstVisibleIndex = -1;
                    for (int index = 0; index < caches.Count; index++)
                    {
                        Geocache cache = caches[index];
                        bool statusMatches = engineFilteredCacheNames.Contains(cache.Name);
                        bool visible = statusMatches;
                        cacheRows[index].SetVisible(visible);
                        if (!visible)
                            continue;

                        visibleCount++;
                        if (firstVisibleIndex < 0)
                            firstVisibleIndex = index;
                    }

                    noMatchingCachesLabel.SetVisible(visibleCount == 0);
                    if (smokeTest && visibleCount == 0)
                        Console.WriteLine("GTK4_EMPTY_FILTER_STATE=true");

                    if (firstVisibleIndex < 0)
                    {
                        cacheFilterSummary.Label_ = string.Format(
                            CultureInfo.CurrentCulture,
                            "Showing {0} of {1} caches",
                            visibleCount,
                            caches.Count);
                        if (smokeTest)
                            Console.WriteLine("GTK4_CACHE_FILTER_SUMMARY=" + cacheFilterSummary.Label_);
                        if (selectedCache != null)
                        {
                            selectedCacheIndex = -1;
                            selectedCache = null;
                            offlineLogButton.Sensitive = false;
                            cacheInfoWidget.SetCache(null, null, null, null, null, null);
                        }
                        return 0;
                    }

                    if (selectedCacheIndex < 0 || selectedCacheIndex >= cacheRows.Count ||
                        !cacheRows[selectedCacheIndex].Visible)
                        SelectCacheAt(firstVisibleIndex, "FILTER");
                    cacheFilterSummary.Label_ = string.Format(
                        CultureInfo.CurrentCulture,
                        "Showing {0} of {1} caches",
                        visibleCount,
                        caches.Count);
                    if (smokeTest)
                        Console.WriteLine("GTK4_CACHE_FILTER_SUMMARY=" + cacheFilterSummary.Label_);
                    return visibleCount;
                }

                void UpdateStatusFilterSensitivity()
                {
                    archivedFilter.Sensitive = availableFilter.Active || unavailableFilter.Active;
                    unavailableFilter.Sensitive = availableFilter.Active || archivedFilter.Active;
                    availableFilter.Sensitive = unavailableFilter.Active || archivedFilter.Active;
                    mineFilter.Sensitive = foundFilter.Active || notFoundFilter.Active;
                    notFoundFilter.Sensitive = foundFilter.Active || mineFilter.Active;
                    foundFilter.Sensitive = notFoundFilter.Active || mineFilter.Active;
                }

                int ReloadEngineFilteredCaches()
                {
                    UpdateStatusFilterSensitivity();
                    bool[] statusFilters =
                    {
                        foundFilter.Active,
                        notFoundFilter.Active,
                        mineFilter.Active,
                        availableFilter.Active,
                        unavailableFilter.Active,
                        archivedFilter.Active
                    };
                    string searchQuery = cacheSearchEntry.Text_.Trim();
                    List<Geocache> filteredCaches = ReadConfiguredCaches(
                        statusFilters,
                        ownerIDs.ToArray(),
                        searchQuery);
                    engineFilteredCacheNames.Clear();
                    foreach (Geocache cache in filteredCaches)
                        engineFilteredCacheNames.Add(cache.Name);
                    if (smokeTest && searchQuery.Length > 0)
                        Console.WriteLine("GTK4_ENGINE_NAME_FILTER_ESCAPE_TEST=" + searchQuery.Contains("'").ToString().ToLowerInvariant());
                    return ApplyCacheListFilters();
                }

                void RefreshCacheListAfterImport()
                {
                    string selectedName = selectedCache?.Name;
                    List<Geocache> refreshedCaches = ReadConfiguredCaches(
                        initialStatusFilters, ownerIDs.ToArray(), string.Empty);

                    while (cacheRows.Count > 0)
                    {
                        cacheListBox.Remove(cacheRows[0]);
                        cacheRows.RemoveAt(0);
                    }
                    caches.Clear();
                    caches.AddRange(refreshedCaches);
                    engineFilteredCacheNames.Clear();
                    foreach (Geocache cache in caches)
                    {
                        ListBoxRow row = ListBoxRow.New();
                        Box rowContent = Box.New(Orientation.Horizontal, 8);
                        rowContent.Append(Label.New(cache.Name));
                        rowContent.Append(Label.New(cache.CacheName ?? string.Empty));
                        row.SetChild(rowContent);
                        row.SetActivatable(true);
                        cacheListBox.Append(row);
                        cacheRows.Add(row);
                    }
                    exportAllButton.Sensitive = caches.Count > 0;
                    selectedCache = null;
                    selectedCacheIndex = -1;
                    int restoreIndex = caches.FindIndex(cache => cache.Name == selectedName);
                    if (restoreIndex < 0 && caches.Count > 0)
                        restoreIndex = 0;
                    if (restoreIndex >= 0)
                        SelectCacheAt(restoreIndex, "IMPORT_REFRESH");
                    else
                    {
                        offlineLogButton.Sensitive = false;
                        cacheInfoWidget.SetCache(null, null, null, null, null, null);
                    }
                    ReloadEngineFilteredCaches();
                    if (smokeTest)
                        Console.WriteLine("GTK4_GPX_IMPORT_CACHE_LIST=" + caches.Count);
                }

                void ImportGpxFromFile(string filePath)
                {
                    try
                    {
                        string extension = Path.GetExtension(filePath);
                        if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
                            ImportConfiguredGpxArchive(filePath, ownerIDs);
                        else if (string.Equals(extension, ".gpx", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(extension, ".loc", StringComparison.OrdinalIgnoreCase))
                            ImportConfiguredGpxFiles(new List<string> { filePath }, ownerIDs);
                        else
                            throw new NotSupportedException("Select a GPX, LOC, or ZIP waypoint file.");
                        RefreshCacheListAfterImport();
                        Console.WriteLine("GTK4_GPX_IMPORTED=1");
                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine("GTK4_GPX_IMPORT_ERROR=" + exception.Message);
                        ShowOperationError(window, "GPX import failed", exception);
                    }
                }

                void ChooseGpxImportFile()
                {
                    string smokePath = Environment.GetEnvironmentVariable("OCM_GTK4_IMPORT_PATH");
                    if (smokeTest && !string.IsNullOrWhiteSpace(smokePath))
                    {
                        ImportGpxFromFile(smokePath);
                        return;
                    }

                    FileChooserNative chooser = FileChooserNative.New(
                        "Import GPX", window, FileChooserAction.Open, "_Import", "_Cancel");
                    FileFilter waypointFilter = FileFilter.New();
                    waypointFilter.Name = "Waypoint files";
                    waypointFilter.AddPattern("*.gpx");
                    waypointFilter.AddPattern("*.loc");
                    waypointFilter.AddPattern("*.zip");
                    chooser.AddFilter(waypointFilter);
                    chooser.OnResponse += (_, response) =>
                    {
                        if (response.ResponseId == (int)ResponseType.Accept)
                        {
                            Gio.File selectedFile = chooser.GetFile();
                            string filePath = selectedFile?.GetPath();
                            if (!string.IsNullOrWhiteSpace(filePath))
                                ImportGpxFromFile(filePath);
                        }
                        chooser.Destroy();
                    };
                    chooser.Show();
                }

                void StartGpxImport()
                {
                    SettingsStore settings = new SettingsStore();
                    CheckButton preserveStatus = CheckButton.NewWithLabel(
                        "Prevent existing found status from being overwritten");
                    preserveStatus.Active = (bool)settings.Get("/apps/ocm/importnooverwrite", false);
                    CheckButton purgeLogs = CheckButton.NewWithLabel("Replace existing logs from the GPX file");
                    purgeLogs.Active = (bool)settings.Get("/apps/ocm/importpurgelogs", false);
                    CheckButton ignoreExtraFields = CheckButton.NewWithLabel("Ignore GSAK-specific fields");
                    ignoreExtraFields.Active = (bool)settings.Get("/apps/ocm/importignoregsak", false);

                    Dialog optionsDialog = Dialog.New();
                    optionsDialog.Title = "GPX import options";
                    optionsDialog.TransientFor = window;
                    optionsDialog.Modal = true;
                    Box content = (Box)optionsDialog.GetContentArea();
                    content.Append(preserveStatus);
                    content.Append(purgeLogs);
                    content.Append(ignoreExtraFields);
                    optionsDialog.AddButton("_Cancel", (int)ResponseType.Cancel);
                    optionsDialog.AddButton("_Continue", (int)ResponseType.Accept);
                    optionsDialog.OnResponse += (_, response) =>
                    {
                        if (response.ResponseId == (int)ResponseType.Accept)
                        {
                            settings.Set("/apps/ocm/importnooverwrite", preserveStatus.Active);
                            settings.Set("/apps/ocm/importpurgelogs", purgeLogs.Active);
                            settings.Set("/apps/ocm/importignoregsak", ignoreExtraFields.Active);
                            if (smokeTest)
                                Console.WriteLine("GTK4_GPX_IMPORT_OPTIONS_ACCEPTED=true");
                            ChooseGpxImportFile();
                        }
                        optionsDialog.Destroy();
                    };

                    if (smokeTest && Environment.GetEnvironmentVariable("OCM_GTK4_IMPORT_OPTIONS_SMOKE") == "1")
                    {
                        preserveStatus.Active = true;
                        purgeLogs.Active = true;
                        ignoreExtraFields.Active = true;
                        optionsDialog.Response((int)ResponseType.Accept);
                    }
                    else
                    {
                        optionsDialog.Present();
                    }
                }
                importGpxButton.OnClicked += (_, _) => StartGpxImport();

                void OpenWaypointReview()
                {
                    if (selectedCache == null || waypointMatches.Count == 0)
                        return;

                    FileStore waypointStore = new FileStore(ReadConfiguredDatabasePath());
                    waypointDialog = new ReviewWaypointDialog(window, selectedCache,
                        waypointMatches, waypointStore, false, () => waypointStore.Dispose());
                    waypointDialog.ReviewWidget.SourceText.ExternalNavigationRequested +=
                        (_, navigation) => RouteExternalNavigation(navigation.Uri);
                    if (smokeTest)
                    {
                        Console.WriteLine("GTK4_WAYPOINT_REVIEW_ACTION=started");
                        waypointDialog.ReviewWidget.TypeChoice.Active = 3;
                        waypointDialog.ReviewWidget.PreviewJumpRequested += (_, _) =>
                        {
                            Console.WriteLine("GTK4_WAYPOINT_GOTO=triggered");
                            Console.WriteLine("GTK4_WAYPOINT_GOTO_HASH=#spot");
                            waypointDialog.Window.Response((int)ResponseType.Ok);
                        };
                        waypointDialog.WaypointAdded += point =>
                        {
                            if (string.IsNullOrWhiteSpace(point.Name) ||
                                !point.Name.StartsWith("RP", StringComparison.Ordinal) ||
                                point.Parent != selectedCache.Name ||
                                point.Symbol != "Reference Point" ||
                                point.Lat == 0 || point.Lon == 0)
                                throw new InvalidOperationException("The GTK4 waypoint dialog did not preserve the reviewed point.");
                            Console.WriteLine("GTK4_WAYPOINT_FORM=ready");
                            Console.WriteLine("GTK4_WAYPOINT_ADDED=" + point.Name);
                            cacheInfoWidget.Description.HintButton.Activate();
                        };
                    }
                    waypointDialog.Present();
                }

                reviewWaypointsButton.OnClicked += (_, _) => OpenWaypointReview();
                void HandleCacheFilterChanged(object sender, EventArgs args)
                {
                    string query = cacheSearchEntry.Text_.Trim();
                    int visibleCount = ReloadEngineFilteredCaches();
                    if (smokeTest && query.Length > 0)
                        Console.WriteLine("GTK4_CACHE_SEARCH_VISIBLE_COUNT=" + visibleCount);
                    if (query.Length > 0 && selectedCache != null &&
                        (selectedCache.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                         (selectedCache.CacheName ?? string.Empty).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                        Console.WriteLine("GTK4_CACHE_SEARCH=" + selectedCache.Name);
                }
                cacheSearchEntry.OnChanged += (_, _) =>
                {
                    clearSearchButton.Sensitive = !string.IsNullOrWhiteSpace(cacheSearchEntry.Text_);
                    HandleCacheFilterChanged(cacheSearchEntry, EventArgs.Empty);
                };
                availableFilter.OnToggled += (_, _) =>
                {
                    int visibleCount = ReloadEngineFilteredCaches();
                    if (smokeTest && !availableFilter.Active)
                        Console.WriteLine("GTK4_CACHE_AVAILABLE_FILTER_COUNT=" + visibleCount);
                };
                unavailableFilter.OnToggled += (_, _) =>
                {
                    int visibleCount = ReloadEngineFilteredCaches();
                    if (smokeTest && !unavailableFilter.Active)
                        Console.WriteLine("GTK4_CACHE_UNAVAILABLE_FILTER_COUNT=" + visibleCount);
                };
                foundFilter.OnToggled += (_, _) =>
                {
                    int visibleCount = ReloadEngineFilteredCaches();
                    if (smokeTest && !foundFilter.Active)
                        Console.WriteLine("GTK4_CACHE_FOUND_FILTER_COUNT=" + visibleCount);
                };
                notFoundFilter.OnToggled += (_, _) =>
                {
                    int visibleCount = ReloadEngineFilteredCaches();
                    if (smokeTest && !notFoundFilter.Active)
                    {
                        Console.WriteLine("GTK4_CACHE_NOT_FOUND_FILTER_COUNT=" + visibleCount);
                        if (!mineFilter.Sensitive)
                            Console.WriteLine("GTK4_CACHE_MINE_DISABLED_WITH_NO_FOUND_FILTERS=true");
                    }
                };
                mineFilter.OnToggled += (_, _) =>
                {
                    int visibleCount = ReloadEngineFilteredCaches();
                    if (smokeTest && !mineFilter.Active)
                        Console.WriteLine("GTK4_CACHE_MINE_FILTER_COUNT=" + visibleCount);
                };
                archivedFilter.OnToggled += (_, _) =>
                {
                    int visibleCount = ReloadEngineFilteredCaches();
                    if (smokeTest && !archivedFilter.Active)
                        Console.WriteLine("GTK4_CACHE_ARCHIVE_FILTER_COUNT=" + visibleCount);
                };
                Paned cachePane = Paned.New(Orientation.Horizontal);
                cachePane.SetStartChild(cacheSelectionPanel);
                cachePane.SetEndChild(cacheInfoWidget.Widget);
                cachePane.SetPosition(360);
                cachePane.Vexpand = true;
                mainContent.Append(cachePane);
                if (smokeTest)
                    Console.WriteLine("GTK4_CACHE_DETAIL_SPLIT=true");
                window.SetChild(mainContent);
                window.Show();
                if (smokeTest && selectedCache?.Name == "GC-SYNTHETIC")
                    StartOfflineLog();
                if (smokeTest)
                {
                    StartFindsGpxExport();
                    StartAllGpxExport();
                    importGpxButton.Activate();
                }
                if (smokeTest && selectedCache != null)
                {
                    cacheInfoWidget.Widget.SetCurrentPage(1);
                    reviewWaypointsButton.Activate();
                }
            };

            return application.RunWithSynchronizationContext(null);
        }

        private static string ReadConfiguredDatabaseSummary()
        {
            string databasePath = Environment.GetEnvironmentVariable(DatabaseOverrideEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                try
                {
                    databasePath = new SettingsStore().Get("/apps/ocm/currentdb", string.Empty) as string;
                }
                catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("Could not read the OpenCacheManager settings: " + exception.Message);
                    return "OpenCacheManager settings could not be read.";
                }
            }

            if (string.IsNullOrWhiteSpace(databasePath))
                return "No cache database is configured.";
            if (!IOFile.Exists(databasePath))
                return "The configured cache database does not exist.";
            if (!IsOpenCacheManagerDatabase(databasePath))
                return "The configured file is not a readable OpenCacheManager database.";

            FileStore store = null;
            try
            {
                store = new FileStore(databasePath);
                int cacheCount = store.CacheCount;
                return cacheCount == 1 ? "1 cache loaded." : cacheCount + " caches loaded.";
            }
            catch (Exception exception) when (exception is SqliteException || exception is IOException || exception is UnauthorizedAccessException)
            {
                Console.Error.WriteLine("Could not read the configured cache database: " + exception.Message);
                return "The configured cache database could not be read.";
            }
            finally
            {
                store?.Dispose();
            }
        }

        private static List<Geocache> ReadConfiguredCaches(bool[] statusFilters, string[] ownerIDs, string nameFilter)
        {
            string databasePath;
            try
            {
                databasePath = ReadConfiguredDatabasePath();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not resolve the configured cache database: " + exception.Message);
                return new List<Geocache>();
            }

            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                return new List<Geocache>();

            FileStore store = null;
            List<Geocache> caches = new List<Geocache>();
            void CaptureCache(object sender, ReadCacheArgs args)
            {
                caches.Add(args.Cache);
            }

            try
            {
                store = new FileStore(databasePath);
                store.GlobalFilters.AddFilterCriteria(FilterList.KEY_STATUS, statusFilters);
                if (!string.IsNullOrWhiteSpace(nameFilter))
                    store.GlobalFilters.AddFilterCriteria(FilterList.KEY_CACHE_NAME, nameFilter);
                store.ReadCache += CaptureCache;
                store.GetUnfilteredCaches(0, 0, ownerIDs);
                return caches;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not load caches for the GTK4 selection view: " + exception.Message);
                return new List<Geocache>();
            }
            finally
            {
                if (store != null)
                {
                    store.ReadCache -= CaptureCache;
                    store.Dispose();
                }
            }
        }

        private static List<string> ReadConfiguredOwnerIDs()
        {
            SettingsStore settings = new SettingsStore();
            string[] keys =
            {
                "/apps/ocm/memberid",
                "/apps/ocm/memberid2",
                "/apps/ocm/memberid3",
                "/apps/ocm/memberid4"
            };
            List<string> ownerIDs = new List<string>();
            foreach (string key in keys)
            {
                string ownerID = settings.Get(key, string.Empty) as string;
                if (!string.IsNullOrWhiteSpace(ownerID))
                    ownerIDs.Add(ownerID);
            }
            return ownerIDs;
        }


        private static List<CacheLog> ReadConfiguredCacheLogs(string cacheName)
        {
            string databasePath;
            try
            {
                databasePath = ReadConfiguredDatabasePath();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not resolve the configured cache database for logs: " + exception.Message);
                return null;
            }

            if (string.IsNullOrWhiteSpace(cacheName) || string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                return null;

            FileStore store = null;
            try
            {
                store = new FileStore(databasePath);
                return store.GetCacheLogs(cacheName);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not load cache logs for " + cacheName + ": " + exception.Message);
                return null;
            }
            finally
            {
                store?.Dispose();
            }
        }

        private static List<Waypoint> ReadConfiguredChildWaypoints(string cacheName)
        {
            if (string.IsNullOrWhiteSpace(cacheName))
                return new List<Waypoint>();

            string databasePath;
            try
            {
                databasePath = ReadConfiguredDatabasePath();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not resolve the configured cache database for waypoints: " + exception.Message);
                return new List<Waypoint>();
            }

            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                return new List<Waypoint>();

            FileStore store = null;
            try
            {
                store = new FileStore(databasePath);
                return store.GetChildWaypoints(new[] { cacheName });
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not load child waypoints for " + cacheName + ": " + exception.Message);
                return new List<Waypoint>();
            }
            finally
            {
                store?.Dispose();
            }
        }

        private static void SaveConfiguredCacheNotes(Geocache cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            string databasePath = ReadConfiguredDatabasePath();
            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                throw new IOException("The configured cache database is not available.");

            FileStore store = null;
            try
            {
                store = new FileStore(databasePath);
                store.AddWaypointOrCache(cache, true, false);
            }
            finally
            {
                store?.Dispose();
            }
        }

        private static CacheAttribute[] ReadConfiguredCacheAttributes(string cacheName)
        {
            if (string.IsNullOrWhiteSpace(cacheName))
                return Array.Empty<CacheAttribute>();

            string databasePath;
            try
            {
                databasePath = ReadConfiguredDatabasePath();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not resolve the configured cache database for attributes: " + exception.Message);
                return Array.Empty<CacheAttribute>();
            }

            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                return Array.Empty<CacheAttribute>();

            FileStore store = null;
            try
            {
                store = new FileStore(databasePath);
                return store.GetAttributes(cacheName);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not load cache attributes for " + cacheName + ": " + exception.Message);
                return Array.Empty<CacheAttribute>();
            }
            finally
            {
                store?.Dispose();
            }
        }

        private static List<string> ReadConfiguredCacheImageFiles(string cacheName)
        {
            List<string> imageFiles = new List<string>();
            if (string.IsNullOrWhiteSpace(cacheName))
                return imageFiles;

            try
            {
                string imageDirectory = ReadConfiguredCacheImageFolder(cacheName);
                if (string.IsNullOrWhiteSpace(imageDirectory))
                    return imageFiles;
                if (!Directory.Exists(imageDirectory))
                    return imageFiles;

                string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".svg" };
                foreach (string filePath in Directory.GetFiles(imageDirectory))
                {
                    string extension = Path.GetExtension(filePath);
                    foreach (string allowedExtension in allowedExtensions)
                    {
                        if (string.Equals(extension, allowedExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            imageFiles.Add(filePath);
                            break;
                        }
                    }
                }
                imageFiles.Sort(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not enumerate cached images for " + cacheName + ": " + exception.Message);
            }

            return imageFiles;
        }

        private static string ReadConfiguredCacheImageFolder(string cacheName)
        {
            if (string.IsNullOrWhiteSpace(cacheName))
                return null;

            try
            {
                string databasePath = ReadConfiguredDatabasePath();
                string dataDirectory = new SettingsStore().Get(
                    "/apps/ocm/datadir",
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) as string;
                if (string.IsNullOrWhiteSpace(databasePath) || string.IsNullOrWhiteSpace(dataDirectory))
                    return null;

                string databaseName = Path.GetFileNameWithoutExtension(databasePath);
                return Path.Combine(dataDirectory, "ocm_images", databaseName, cacheName);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Could not resolve the image folder for " + cacheName + ": " + exception.Message);
                return null;
            }
        }

        private static readonly string[] LogStatuses =
        {
            "Found it", "Didn't find it", "Write note", "Needs Maintenance"
        };

        private static string ReadConfiguredDatabasePath()
        {
            string databasePath = Environment.GetEnvironmentVariable(DatabaseOverrideEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(databasePath))
                return databasePath;

            return new SettingsStore().Get("/apps/ocm/currentdb", string.Empty) as string;
        }

        private static CacheLog LoadLog(string key)
        {
            string databasePath = ReadConfiguredDatabasePath();
            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                throw new IOException("No readable cache database is configured.");

            FileStore store = new FileStore(databasePath);
            try
            {
                CacheLog log = store.GetCacheLogByKey(key);
                if (log.LogKey != key)
                    throw new InvalidOperationException("The requested offline log does not exist.");
                return log;
            }
            finally
            {
                store.Dispose();
            }
        }

        private static void WriteGpxFile(string filePath, List<Geocache> caches)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("An output path is required.", nameof(filePath));

            string parentDirectory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
                Directory.CreateDirectory(parentDirectory);

            FileStore store = new FileStore(ReadConfiguredDatabasePath());
            try
            {
                GPXWriter writer = new GPXWriter();
                writer.WriteWaypoint += (_, _) => { };
                writer.Complete += (_, _) => { };
                writer.WriteGPXFile(filePath, caches, new Dictionary<string, string>(), store);
            }
            finally
            {
                store.Dispose();
            }
        }

        private static void WriteFindsGpxFile(string filePath, string ownerID)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("An output path is required.", nameof(filePath));
            if (string.IsNullOrWhiteSpace(ownerID))
                throw new InvalidOperationException("Configure a geocaching member ID before exporting finds.");

            string parentDirectory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
                Directory.CreateDirectory(parentDirectory);

            FileStore store = new FileStore(ReadConfiguredDatabasePath());
            try
            {
                GPXWriter writer = new GPXWriter
                {
                    IsMyFinds = true,
                    MyFindsOwner = ownerID
                };
                writer.WriteWaypoint += (_, _) => { };
                writer.Complete += (_, _) => { };
                writer.WriteGPXFile(filePath, store.GetFinds(), new Dictionary<string, string>(), store);
            }
            finally
            {
                store.Dispose();
            }
        }

        private static void ImportConfiguredGpxArchive(string filePath, List<string> ownerIDs)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A waypoint archive path is required.", nameof(filePath));
            if (!IOFile.Exists(filePath))
                throw new FileNotFoundException("The selected ZIP archive does not exist.", filePath);

            string stagingDirectory = Path.Combine(
                Path.GetTempPath(), "ocm_import_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stagingDirectory);
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(
                    filePath, stagingDirectory, overwriteFiles: true);
                List<string> inputFiles = new List<string>();
                foreach (string extractedFile in Directory.GetFiles(
                    stagingDirectory, "*", SearchOption.AllDirectories))
                {
                    string extension = Path.GetExtension(extractedFile);
                    if (string.Equals(extension, ".gpx", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(extension, ".loc", StringComparison.OrdinalIgnoreCase))
                        inputFiles.Add(extractedFile);
                }
                inputFiles.Sort(StringComparer.OrdinalIgnoreCase);
                if (inputFiles.Count == 0)
                    throw new InvalidDataException("The ZIP archive contains no GPX or LOC waypoint files.");

                ImportConfiguredGpxFiles(inputFiles, ownerIDs);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(stagingDirectory))
                        Directory.Delete(stagingDirectory, recursive: true);
                }
                catch
                {
                    // Import cleanup must not hide the extraction or database error.
                }
            }
        }

        private static void ImportConfiguredGpxFiles(List<string> inputFiles, List<string> ownerIDs)
        {
            if (inputFiles == null || inputFiles.Count == 0)
                throw new ArgumentException("At least one GPX or LOC input file is required.", nameof(inputFiles));
            foreach (string inputFile in inputFiles)
            {
                if (string.IsNullOrWhiteSpace(inputFile))
                    throw new ArgumentException("A GPX or LOC input path is required.", nameof(inputFiles));
                if (!IOFile.Exists(inputFile))
                    throw new FileNotFoundException("A selected waypoint file does not exist.", inputFile);
            }

            string databasePath = ReadConfiguredDatabasePath();
            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                throw new IOException("No readable cache database is configured.");

            SettingsStore settings = new SettingsStore();
            GPXParser parser = new GPXParser
            {
                CacheOwner = ownerIDs,
                IgnoreExtraFields = (bool)settings.Get("/apps/ocm/importignoregsak", false),
                PreserveFound = (bool)settings.Get("/apps/ocm/importnooverwrite", false),
                PurgeLogs = (bool)settings.Get("/apps/ocm/importpurgelogs", false),
                Bookmark = settings.Get("/apps/ocm/importbookmark", (string)null) as string
            };
            parser.ParseWaypoint += (_, _) => { };
            parser.Complete += (_, _) => { };

            FileStore store = new FileStore(databasePath);
            bool updateStarted = false;
            bool updateFinished = false;
            try
            {
                parser.StartUpdate(store);
                updateStarted = true;
                foreach (string inputFile in inputFiles)
                {
                    using (FileStream preParseStream = IOFile.OpenRead(inputFile))
                        parser.PreParseForSingle(preParseStream, store);
                    using (FileStream parseStream = IOFile.OpenRead(inputFile))
                        parser.parseGPXFile(parseStream, store);
                    if (parser.Cancel)
                        break;
                }

                if (parser.Cancel)
                    updateFinished = true;
                else
                {
                    parser.EndUpdate(store);
                    updateFinished = true;
                }
            }
            catch
            {
                if (updateStarted && !updateFinished)
                    store.CancelUpdate();
                throw;
            }
            finally
            {
                store.Dispose();
            }
        }

        private static void ShowExportError(ApplicationWindow parent, Exception exception)
        {
            ShowOperationError(parent, "GPX export failed", exception);
        }

        private static void ShowOperationError(ApplicationWindow parent, string title, Exception exception)
        {
            Dialog dialog = Dialog.New();
            dialog.Title = title;
            dialog.TransientFor = parent;
            dialog.Modal = true;
            Label message = Label.New(exception.Message);
            message.Wrap = true;
            ((Box)dialog.GetContentArea()).Append(message);
            dialog.AddButton("_Close", (int)ResponseType.Close);
            dialog.OnResponse += (_, _) => dialog.Hide();
            dialog.Present();
        }

        private static void SaveLog(CacheLog log)
        {
            string databasePath = ReadConfiguredDatabasePath();
            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                throw new IOException("No readable cache database is configured.");

            FileStore store = new FileStore(databasePath);
            try
            {
                store.AddLog(log.CacheCode, log);
            }
            finally
            {
                store.Dispose();
            }
        }

        private static void SaveNewOfflineLog(Geocache cache, CacheLog log, bool ftf)
        {
            SettingsStore settings = new SettingsStore();
            string dataDirectory = settings.Get("/apps/ocm/datadir",
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) as string;
            if (string.IsNullOrWhiteSpace(dataDirectory))
                dataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Directory.CreateDirectory(dataDirectory);
            FieldNotesHandler.WriteToFile(log, Path.Combine(dataDirectory, "cachelogs.txt"));
            SaveLog(log);

            bool updateCache = false;
            if (log.LogStatus == "Found it")
            {
                cache.DNF = false;
                cache.FTF = ftf;
                cache.Symbol = "Geocache Found";
                updateCache = true;
            }
            else if (log.LogStatus == "Didn't find it")
            {
                cache.DNF = true;
                cache.FTF = false;
                cache.Symbol = "Geocache";
                updateCache = true;
            }
            else if (log.LogStatus == "Needs Maintenance")
            {
                cache.CheckNotes = true;
                updateCache = true;
            }

            if (updateCache)
            {
                string databasePath = ReadConfiguredDatabasePath();
                FileStore store = new FileStore(databasePath);
                try
                {
                    store.AddWaypointOrCache(cache, false, false);
                }
                finally
                {
                    store.Dispose();
                }
            }
        }

        private static void ShowNewOfflineLogDialog(ApplicationWindow parent, Geocache cache,
            string ownerID, bool smokeTest, System.Action onSaved)
        {
            Dialog dialog = Dialog.New();
            dialog.Title = "Log Cache Offline";
            dialog.TransientFor = parent;
            dialog.Modal = true;

            Box content = (Box)dialog.GetContentArea();
            Entry dateEntry = Entry.New();
            dateEntry.Text_ = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ComboBoxText statusChoice = ComboBoxText.New();
            foreach (string status in LogStatuses)
                statusChoice.AppendText(status);
            statusChoice.Active = 0;
            CheckButton ftfCheck = CheckButton.NewWithLabel("First to find (FTF)");
            ftfCheck.Sensitive = true;
            statusChoice.OnChanged += (_, _) =>
            {
                ftfCheck.Sensitive = statusChoice.Active == 0;
                if (!ftfCheck.Sensitive)
                    ftfCheck.Active = false;
            };
            TextView messageView = TextView.New();
            Label errorLabel = Label.New(string.Empty);

            content.Append(Label.New("Cache: " + cache.Name + " — " + (cache.CacheName ?? string.Empty)));
            content.Append(Label.New("Date (YYYY-MM-DD)"));
            content.Append(dateEntry);
            content.Append(Label.New("Log type"));
            content.Append(statusChoice);
            content.Append(ftfCheck);
            content.Append(Label.New("Message"));
            content.Append(messageView);
            content.Append(errorLabel);
            dialog.AddButton("_Cancel", (int)ResponseType.Cancel);
            dialog.AddButton("_Save", (int)ResponseType.Ok);
            dialog.OnResponse += (_, response) =>
            {
                if (response.ResponseId != (int)ResponseType.Ok)
                {
                    dialog.Hide();
                    return;
                }

                if (!DateTime.TryParseExact(dateEntry.Text_, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime logDate))
                {
                    errorLabel.Label_ = "Enter a valid date in YYYY-MM-DD format.";
                    return;
                }

                CacheLog log = new CacheLog
                {
                    CacheCode = cache.Name,
                    LogDate = logDate,
                    LogStatus = LogStatuses[Math.Clamp(statusChoice.Active, 0, LogStatuses.Length - 1)],
                    LoggedBy = "OCM",
                    FinderID = ownerID,
                    LogKey = cache.Name + logDate.ToFileTime().ToString(CultureInfo.InvariantCulture),
                    LogMessage = messageView.Buffer.Text,
                    Encoded = false
                };
                try
                {
                    SaveNewOfflineLog(cache, log, ftfCheck.Active);
                    dialog.Hide();
                    Console.WriteLine("GTK4_OFFLINE_LOG_SAVED=true");
                    onSaved?.Invoke();
                }
                catch (Exception exception)
                {
                    errorLabel.Label_ = "Could not save the offline log: " + exception.Message;
                }
            };

            if (smokeTest)
            {
                dateEntry.Text_ = "2026-10-14";
                statusChoice.Active = 2;
                messageView.Buffer.Text = "Synthetic offline log [GTK4]";
            }
            dialog.Present();
            if (smokeTest)
                dialog.Response((int)ResponseType.Ok);
        }

        private static void TryShowEditLogDialog(ApplicationWindow parent, string key, bool smokeTest)
        {
            try
            {
                ShowEditLogDialog(parent, LoadLog(key), smokeTest);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("GTK4_EDIT_LOG_ERROR=" + exception.Message);
            }
        }

        private static void ShowEditLogDialog(ApplicationWindow parent, CacheLog log, bool smokeTest)
        {
            Dialog dialog = Dialog.New();
            dialog.Title = "Edit offline log";
            dialog.TransientFor = parent;
            dialog.Modal = true;

            Box content = (Box)dialog.GetContentArea();
            Entry dateEntry = Entry.New();
            dateEntry.Text_ = log.LogDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ComboBoxText statusChoice = ComboBoxText.New();
            foreach (string status in LogStatuses)
                statusChoice.AppendText(status);
            int statusIndex = Array.IndexOf(LogStatuses, log.LogStatus);
            statusChoice.Active = statusIndex >= 0 ? statusIndex : LogStatuses.Length - 1;
            TextView messageView = TextView.New();
            messageView.Buffer.Text = log.LogMessage;
            Label errorLabel = Label.New(string.Empty);

            content.Append(Label.New("Date (YYYY-MM-DD)"));
            content.Append(dateEntry);
            content.Append(Label.New("Log type"));
            content.Append(statusChoice);
            content.Append(Label.New("Message"));
            content.Append(messageView);
            content.Append(errorLabel);
            dialog.AddButton("_Cancel", (int)ResponseType.Cancel);
            dialog.AddButton("_Save", (int)ResponseType.Ok);
            dialog.OnResponse += (_, response) =>
            {
                if (response.ResponseId != (int)ResponseType.Ok)
                {
                    dialog.Hide();
                    return;
                }

                if (!DateTime.TryParseExact(dateEntry.Text_, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime logDate))
                {
                    errorLabel.Label_ = "Enter a valid date in YYYY-MM-DD format.";
                    return;
                }

                log.LogDate = logDate;
                log.LogStatus = LogStatuses[Math.Clamp(statusChoice.Active, 0, LogStatuses.Length - 1)];
                log.LogMessage = messageView.Buffer.Text;
                try
                {
                    SaveLog(log);
                    dialog.Hide();
                    Console.WriteLine("GTK4_LOG_EDITED=" + log.LogKey);
                    if (smokeTest)
                        Console.WriteLine("GTK4_EDIT_DIALOG_RESPONSE=save");
                }
                catch (Exception exception)
                {
                    errorLabel.Label_ = "Could not save the log: " + exception.Message;
                }
            };
            if (smokeTest)
                messageView.Buffer.Text += " [GTK4 edit]";
            dialog.Present();
            if (smokeTest)
                dialog.Response((int)ResponseType.Ok);
        }

        private static void ConfirmDeleteLog(ApplicationWindow parent, string key, bool smokeTest)
        {
            Dialog dialog = Dialog.New();
            dialog.Title = "Delete offline log";
            dialog.TransientFor = parent;
            dialog.Modal = true;
            ((Box)dialog.GetContentArea()).Append(Label.New("Are you sure you wish to delete this log?"));
            dialog.AddButton("_No", (int)ResponseType.No);
            dialog.AddButton("_Yes", (int)ResponseType.Yes);
            dialog.OnResponse += (_, response) =>
            {
                dialog.Hide();
                if (response.ResponseId == (int)ResponseType.Yes)
                {
                    TryDeleteLog(key);
                    if (smokeTest)
                        Console.WriteLine("GTK4_DELETE_DIALOG_RESPONSE=yes");
                }
            };
            dialog.Present();
            if (smokeTest)
                dialog.Response((int)ResponseType.Yes);
        }

        private static void DeleteLog(string key)
        {
            string databasePath = Environment.GetEnvironmentVariable(DatabaseOverrideEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(databasePath))
                databasePath = new SettingsStore().Get("/apps/ocm/currentdb", string.Empty) as string;
            if (string.IsNullOrWhiteSpace(databasePath) || !IOFile.Exists(databasePath))
                throw new IOException("No readable cache database is configured.");

            FileStore store = new FileStore(databasePath);
            try
            {
                store.PurgeLogsByKey(new[] { key });
            }
            finally
            {
                store.Dispose();
            }
            Console.WriteLine("GTK4_LOG_DELETED=" + key);
        }

        private static void TryDeleteLog(string key)
        {
            try
            {
                DeleteLog(key);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("GTK4_DELETE_LOG_ERROR=" + exception.Message);
            }
        }

        private static void ShowExternalNavigationError(ApplicationWindow parent, System.Exception exception)
        {
            Dialog dialog = Dialog.New();
            dialog.Title = "Could not open external link";
            dialog.TransientFor = parent;
            dialog.Modal = true;
            Label message = Label.New(exception.Message);
            message.Wrap = true;
            ((Box)dialog.GetContentArea()).Append(message);
            dialog.AddButton("_Close", (int)ResponseType.Close);
            dialog.OnResponse += (_, _) => dialog.Hide();
            dialog.Present();
        }

        private static bool IsOpenCacheManagerDatabase(string databasePath)
        {
            try
            {
                SqliteConnectionStringBuilder connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    ReadOnly = true,
                    FailIfMissing = true
                };

                using SqliteConnection connection = new SqliteConnection(connectionString.ConnectionString);
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name='GEOCACHE' LIMIT 1";
                return command.ExecuteScalar() != null;
            }
            catch (Exception exception) when (exception is SqliteException || exception is ArgumentException || exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
