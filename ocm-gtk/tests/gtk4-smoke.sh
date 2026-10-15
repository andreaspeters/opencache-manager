#!/usr/bin/env bash
set -euo pipefail

repo=/data
build=/data/build-nix/gtk4-direct
out="$build/out/app"
smoke="$build/smoke"

cmake -S "$repo" -B "$build"
cmake --build "$build"
python3 -c 'import pathlib, sys; out=pathlib.Path(sys.argv[1]); names=("libWebKit.so", "libJavaScriptCore.so"); missing=[name for name in names if not (out/name).is_symlink() or not (out/name).exists()]; assert not missing, f"missing GTK4 native ABI aliases: {missing}"' "$out"
python3 -c 'import json, sys; d=json.load(open(sys.argv[1])); deps=[k for target in d["targets"].values() for k in target]; assert any(k.startswith("GirCore.Gtk-4.0/") for k in deps) and any(k.startswith("GirCore.WebKit-6.0/") for k in deps) and not any(k.startswith(("GtkSharp/", "WebkitGtkSharp/")) for k in deps), f"unexpected GTK dependencies: {deps}"' "$out/ocm-gtk.deps.json"

mkdir -p "$smoke/home"
rm -f "$smoke/cache.db" "$smoke/gio-calls" "$smoke/image-gio-calls" \
    "$smoke/finds.gpx" "$smoke/export.gpx" "$smoke/import.gpx" \
    "$smoke/import-second.gpx" "$smoke/import-loc.loc" "$smoke/import.zip" \
    "$smoke/home/.config/opencache-manager/data/cachelogs.txt" \
    "$smoke/home/.config/opencache-manager/data/ocm_images/cache/GC-SYNTHETIC/downloaded.svg"
python3 -c 'import pathlib, sys; pathlib.Path(sys.argv[1]).write_text("""<?xml version=\"1.0\" encoding=\"UTF-8\"?><gpx version=\"1.0\" creator=\"OCM GTK4 smoke\" xmlns=\"http://www.topografix.com/GPX/1/0\"><wpt lat=\"47.6\" lon=\"-122.3\"><time>2026-10-15T12:00:00Z</time><name>RP-IMPORTED</name><desc>Imported reference point</desc><sym>Reference Point</sym><type>Reference Point</type><parent>GC-SYNTHETIC</parent></wpt></gpx>""")' "$smoke/import.gpx"
python3 -c 'import pathlib, sys; pathlib.Path(sys.argv[1]).write_text("""<?xml version=\"1.0\" encoding=\"UTF-8\"?><gpx version=\"1.0\" creator=\"OCM GTK4 smoke\" xmlns=\"http://www.topografix.com/GPX/1/0\"><wpt lat=\"47.7\" lon=\"-122.4\"><time>2026-10-15T12:01:00Z</time><name>RP-IMPORTED-SECOND</name><desc>Imported second reference point</desc><sym>Reference Point</sym><type>Reference Point</type><parent>GC-SECOND</parent></wpt></gpx>""")' "$smoke/import-second.gpx"
python3 -c 'import pathlib, sys; pathlib.Path(sys.argv[1]).write_text("""<?xml version=\"1.0\" encoding=\"UTF-8\"?><loc version=\"1.0\" src=\"OCM GTK4 smoke\"><waypoint><name id=\"RP-LOC\">LOC reference point</name><coord lat=\"47.8\" lon=\"-122.5\"/><type>Reference Point</type></waypoint></loc>""")' "$smoke/import-loc.loc"
python3 -c 'import sys, zipfile; archive=zipfile.ZipFile(sys.argv[1], "w", zipfile.ZIP_DEFLATED); archive.write(sys.argv[2], "one/cache.gpx"); archive.write(sys.argv[3], "two/cache.gpx"); archive.write(sys.argv[4], "nested/point.loc"); archive.close()' "$smoke/import.zip" "$smoke/import.gpx" "$smoke/import-second.gpx" "$smoke/import-loc.loc"
sqlite3 "$smoke/cache.db" "CREATE TABLE GEOCACHE(available TEXT, archived TEXT, container TEXT, hint TEXT, longdesc TEXT, shortdesc TEXT, type TEXT, state TEXT, country TEXT, terrain TEXT, difficulty TEXT, placedby TEXT, name TEXT PRIMARY KEY, fullname TEXT, id TEXT, owner TEXT, ownerID TEXT, notes TEXT, checkNotes TEXT, corlat TEXT, corlon TEXT, dnf TEXT, ftf TEXT, user1 TEXT, user2 TEXT, user3 TEXT, user4 TEXT); CREATE TABLE WAYPOINT(lastUpdate TEXT, parent TEXT, symbol TEXT, time TEXT, type TEXT, desc TEXT, urlname TEXT, url TEXT, lon TEXT, lat TEXT, name TEXT PRIMARY KEY); INSERT INTO GEOCACHE VALUES('True','False','Micro','Synthetic hidden hint','','<h1>Persisted cache description</h1><p>N 47 36.123 W 122 19.456</p><a href=https://example.invalid>External smoke</a>','TRADITIONAL','','','1','1','smoke','GC-SYNTHETIC','Fixture cache','id','owner','ownerid','','False',NULL,NULL,'False','False','','','',''); INSERT INTO GEOCACHE VALUES('False','True','Micro','Second hint','','<p>Second cache description</p>','TRADITIONAL','','','1','1','smoke','GC-SECOND','Second fixture','id2','owner','ownerid','','False',NULL,NULL,'False','False','','','',''); INSERT INTO WAYPOINT VALUES('2026-10-14','','Geocache','2026-10-14','Geocache','Synthetic cache','Open','https://www.geocaching.com','0','0','GC-SYNTHETIC'),('2026-10-14','','Geocache','2026-10-14','Geocache','Second synthetic cache','Open','https://www.geocaching.com','0','0','GC-SECOND'); CREATE TABLE LOGS(cache TEXT, date TEXT, loggedby TEXT, message TEXT, status TEXT, finderID TEXT, encoded TEXT, id TEXT, logkey TEXT); INSERT INTO LOGS VALUES('GC-SYNTHETIC','2026-10-14','smoke','synthetic log','Found it','smoke','false','1','GC-SYNTHETIC-LOG'),('GC-SYNTHETIC','2026-10-14','smoke','synthetic log','Found it','smoke','false','2','GC-SYNTHETIC-EDIT');"
sqlite3 "$smoke/cache.db" "CREATE TABLE TBUGS (cache TEXT, id TEXT, ref TEXT, name TEXT);"
sqlite3 "$smoke/cache.db" "UPDATE WAYPOINT SET symbol='Geocache Found' WHERE name='GC-SYNTHETIC';"
sqlite3 "$smoke/cache.db" "INSERT INTO WAYPOINT VALUES('2026-10-14','GC-SYNTHETIC','Final Location','2026-10-14','Waypoint','Synthetic final waypoint','','','0.1','0.2','GC-SYNTHETIC-1');"
sqlite3 "$smoke/cache.db" "UPDATE GEOCACHE SET fullname='Fixture cache O''Brien' WHERE name='GC-SYNTHETIC';"
sqlite3 "$smoke/cache.db" "UPDATE GEOCACHE SET ownerID='smoke' WHERE name='GC-SECOND';"
sqlite3 "$smoke/cache.db" "UPDATE GEOCACHE SET notes='Original note' WHERE name='GC-SYNTHETIC';"
sqlite3 "$smoke/cache.db" "UPDATE GEOCACHE SET state='Washington', country='USA' WHERE name='GC-SYNTHETIC';"
sqlite3 "$smoke/cache.db" "UPDATE WAYPOINT SET lat='47.60205', lon='-122.32427' WHERE name='GC-SYNTHETIC';"
sqlite3 "$smoke/cache.db" "CREATE TABLE ATTRIBUTES(cachename TEXT, id TEXT, inc TEXT, value TEXT); INSERT INTO ATTRIBUTES VALUES('GC-SYNTHETIC','1','True','Night cache');"
mkdir -p "$smoke/image-webroot"
python3 -c 'import pathlib, sys; pathlib.Path(sys.argv[1]).write_text("""<svg xmlns="http://www.w3.org/2000/svg" width="1" height="1"><rect width="1" height="1" fill="blue"/></svg>""")' "$smoke/image-webroot/downloaded.svg"
image_server_port=$(python3 -c 'import socket; s=socket.socket(); s.bind(("127.0.0.1",0)); print(s.getsockname()[1]); s.close()')
python3 -c 'import sqlite3, sys; db=sqlite3.connect(sys.argv[1]); db.execute("UPDATE GEOCACHE SET longdesc=longdesc || ? WHERE name=?", ("<img src=\"http://127.0.0.1:" + sys.argv[2] + "/downloaded.svg\">", "GC-SYNTHETIC")); db.commit()' "$smoke/cache.db" "$image_server_port"
python3 -m http.server "$image_server_port" --bind 127.0.0.1 --directory "$smoke/image-webroot" >"$smoke/image-http.log" 2>&1 &
image_server_pid=$!
mkdir -p "$smoke/bin"
export OCM_GTK4_REAL_GIO="$(command -v gio)"
export OCM_GTK4_GIO_LOG="$smoke/gio-calls"
export OCM_GTK4_IMAGE_GIO_LOG="$smoke/image-gio-calls"
printf '%s\n' '#!/bin/sh' 'if [ "$1" = open ] && { [ "$2" = https://example.invalid ] || [ "$2" = https://example.invalid/ ]; }; then' '    printf "%s\n" "$@" > "$OCM_GTK4_GIO_LOG"' '    exit 0' 'fi' 'if [ "$1" = open ] && case "$2" in file://*/ocm_images/cache/GC-SYNTHETIC|file://*/ocm_images/cache/GC-SECOND) true;; *) false;; esac; then' '    printf "%s\n" "$@" >> "$OCM_GTK4_IMAGE_GIO_LOG"' '    exit 0' 'fi' 'exec "$OCM_GTK4_REAL_GIO" "$@"' > "$smoke/bin/gio"
chmod +x "$smoke/bin/gio"
export PATH="$smoke/bin:$PATH"
export HOME="$smoke/home"
export XDG_CONFIG_HOME="$HOME/.config"
mkdir -p "$XDG_CONFIG_HOME/opencache-manager"
python3 -c 'import json, pathlib, sys; settings=pathlib.Path(sys.argv[1]); settings.write_text(json.dumps({"/apps/ocm/memberid":"smoke", "/apps/ocm/datadir":str(settings.parent / "data")}))' "$XDG_CONFIG_HOME/opencache-manager/settings.json"
python3 -c 'import pathlib, sys; folder=pathlib.Path(sys.argv[1]); folder.mkdir(parents=True, exist_ok=True); (folder / "synthetic.svg").write_text("""<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1\" height=\"1\"><rect width=\"1\" height=\"1\" fill=\"red\"/></svg>""")' "$XDG_CONFIG_HOME/opencache-manager/data/ocm_images/cache/GC-SYNTHETIC"
export OCM_GTK4_DATABASE="$smoke/cache.db"
export OCM_GTK4_SMOKE_TEST=1
export OCM_GTK4_EXPORT_FINDS_PATH="$smoke/finds.gpx"
export OCM_GTK4_EXPORT_ALL_PATH="$smoke/export.gpx"
export OCM_GTK4_IMPORT_PATH="$smoke/import.zip"
export OCM_GTK4_IMPORT_OPTIONS_SMOKE=1
export DISPLAY=:87

Xvfb "$DISPLAY" -screen 0 1024x768x24 -nolisten tcp -ac >"$smoke/xvfb.log" 2>&1 &
xvfb_pid=$!
app_pid=""
cleanup() {
    if [[ -n "$image_server_pid" ]]; then
        kill "$image_server_pid" 2>/dev/null || true
        wait "$image_server_pid" 2>/dev/null || true
    fi
    if [[ -n "$app_pid" ]]; then
        kill "$app_pid" 2>/dev/null || true
        wait "$app_pid" 2>/dev/null || true
    fi
    kill "$xvfb_pid" 2>/dev/null || true
    wait "$xvfb_pid" 2>/dev/null || true
}
trap cleanup EXIT

for _ in $(seq 1 40); do
    if xwininfo -display "$DISPLAY" -root >/dev/null 2>&1; then
        break
    fi
    sleep 0.25
done

export LD_LIBRARY_PATH="$(python3 -c 'import os; print(":".join(path for path in os.environ.get("LD_LIBRARY_PATH", "").split(":") if "webkitgtk-6.0-abi-compat" not in path))')"
WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1 "$out/ocm-gtk" >"$smoke/app.log" 2>&1 &
app_pid=$!

for _ in $(seq 1 40); do
    if xwininfo -display "$DISPLAY" -name "OpenCacheManager GTK4" >/dev/null 2>&1 && \
       python3 -c 'import pathlib, sqlite3, sys; log=pathlib.Path(sys.argv[1]).read_text(); db=sqlite3.connect(sys.argv[2]); gio_path=pathlib.Path(sys.argv[3]); gio=gio_path.read_text().splitlines() if gio_path.exists() else []; image_gio_path=pathlib.Path(sys.argv[4]); image_gio=image_gio_path.read_text().splitlines() if image_gio_path.exists() else []; second_image_dir=pathlib.Path(sys.argv[5]); remaining=db.execute("SELECT COUNT(*) FROM LOGS WHERE logkey=?", ("GC-SYNTHETIC-LOG",)).fetchone()[0]; edited=db.execute("SELECT message FROM LOGS WHERE logkey=?", ("GC-SYNTHETIC-EDIT",)).fetchone(); waypoints=db.execute("SELECT COUNT(*) FROM WAYPOINT WHERE name=?", ("RP-SYNTHETIC",)).fetchone()[0]; sys.exit(0 if "GTK4_DB_STATUS=2 caches loaded." in log and "GTK4_CACHE_LIST=2" in log and "GTK4_CACHE_SELECTED=GC-SYNTHETIC" in log and "GTK4_CACHE_LIST_SELECTION=GC-SECOND" in log and "GTK4_CACHE_SELECTION_DETAILS=true" in log and "GTK4_CACHE_SEARCH=GC-SYNTHETIC" in log and "GTK4_CACHE_SEARCH_VISIBLE_COUNT=1" in log and "GTK4_CACHE_ARCHIVE_FILTER_COUNT=1" in log and "GTK4_CACHE_SEARCH_DETAILS=true" in log and "GTK4_EXTERNAL_NAVIGATION_LAUNCHED=true" in log and gio and gio[0] == "open" and len(gio) == 2 and gio[1].startswith("https://example.invalid") and image_gio and image_gio[0] == "open" and len(image_gio) == 4 and "ocm_images/cache/GC-SYNTHETIC" in image_gio[1] and image_gio[2] == "open" and "ocm_images/cache/GC-SECOND" in image_gio[3] and second_image_dir.is_dir() and "GTK4_CACHE_DESCRIPTION_RENDERED=true" in log and "GTK4_HINT_DIALOG_SHOWN=true" in log and "GTK4_OFFLINE_LOG_TAB=selected" in log and "GTK4_WAYPOINT_GOTO=triggered" in log and "GTK4_WAYPOINT_GOTO_HASH=#spot" in log and "GTK4_WAYPOINT_REVIEW_ACTION=started" in log and "GTK4_WAYPOINT_FORM=ready" in log and "GTK4_WAYPOINT_ADDED=RP-SYNTHETIC" in log and "GTK4_WAYPOINT_CANCEL=clean" in log and "GTK4_NAVIGATION=editLog:GC-SYNTHETIC-EDIT" in log and "GTK4_NAVIGATION=deleteLog:GC-SYNTHETIC-LOG" in log and "GTK4_LOG_DELETED=GC-SYNTHETIC-LOG" in log and "GTK4_LOG_EDITED=GC-SYNTHETIC-EDIT" in log and "GTK4_DELETE_DIALOG_RESPONSE=yes" in log and "GTK4_EDIT_DIALOG_RESPONSE=save" in log and remaining == 0 and edited == ("synthetic log [GTK4 edit]",) and waypoints == 1 else 1)' "$smoke/app.log" "$smoke/cache.db" "$smoke/gio-calls" "$smoke/image-gio-calls" "$XDG_CONFIG_HOME/opencache-manager/data/ocm_images/cache/GC-SECOND"; then
        python3 -c 'import pathlib, sys; log=pathlib.Path(sys.argv[1]).read_text(); required=("GTK4_CACHE_FOUND_FILTER_COUNT=1", "GTK4_CACHE_NOT_FOUND_FILTER_COUNT=1", "GTK4_CACHE_ARCHIVE_FILTER_COUNT=0", "GTK4_CACHE_MINE_FILTER_COUNT=1", "GTK4_CACHE_MINE_DISABLED_WITH_NO_FOUND_FILTERS=true", "GTK4_CACHE_FILTER_SUMMARY=Showing 1 of 2 caches", "GTK4_EMPTY_FILTER_STATE=true", "GTK4_CACHE_SEARCH_CLEAR_ACTION=true", "GTK4_ENGINE_NAME_FILTER_ESCAPE_TEST=true", "GTK4_CACHE_DETAIL_SPLIT=true", "GTK4_CACHE_QUICK_INFO=GC-SYNTHETIC", "GTK4_CACHE_WAYPOINTS=GC-SYNTHETIC-1", "GTK4_CACHE_NOTES_LOADED=Original note", "GTK4_CACHE_NOTES_SAVED=true", "GTK4_QUICK_INFO_STATUS=Found", "GTK4_QUICK_INFO_ATTRIBUTES=Night cache", "GTK4_QUICK_INFO_LOCATION=Washington, USA", "GTK4_QUICK_INFO_COORDINATES=47.60205,-122.32427", "GTK4_CACHE_IMAGE_FILES=1", "GTK4_CACHE_IMAGE_PREVIEW=synthetic.svg", "GTK4_CACHE_IMAGE_EMPTY=true", "GTK4_IMAGE_FOLDER_OPEN_REQUESTED=true", "GTK4_IMAGE_FOLDER_EMPTY_OPEN_REQUESTED=true", "GTK4_CACHE_IMAGES_DOWNLOADED=1", "GTK4_CACHE_IMAGE_ADDED=downloaded.svg", "GTK4_OFFLINE_LOG_SAVED=true", "GTK4_FINDS_GPX_EXPORTED=true", "GTK4_GPX_EXPORTED=true", "GTK4_GPX_IMPORTED=1", "GTK4_GPX_IMPORT_OPTIONS_ACCEPTED=true"); missing=[marker for marker in required if marker not in log]; unexpected=[marker for marker in ("GTK4_CACHE_DESCRIPTION_RENDERED=false", "HTMLWidget GTK4 JavaScript failed:") if marker in log]; sys.exit(1 if missing or unexpected else 0)' "$smoke/app.log"
        python3 -c 'import pathlib, sys; assert pathlib.Path(sys.argv[1]).read_text() == "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1\" height=\"1\"><rect width=\"1\" height=\"1\" fill=\"blue\"/></svg>"' "$XDG_CONFIG_HOME/opencache-manager/data/ocm_images/cache/GC-SYNTHETIC/downloaded.svg"
        python3 -c 'import pathlib, sqlite3, sys; db=sqlite3.connect(sys.argv[1]); assert db.execute("SELECT COUNT(*) FROM LOGS WHERE message=?", ("Synthetic offline log [GTK4]",)).fetchone()[0] == 1; notes=pathlib.Path(sys.argv[2]).read_text(encoding="utf-16"); assert "Synthetic offline log [GTK4]" in notes' "$smoke/cache.db" "$XDG_CONFIG_HOME/opencache-manager/data/cachelogs.txt"
        python3 -c 'import pathlib, sys, xml.etree.ElementTree as ET; root=ET.parse(sys.argv[1]).getroot(); ns={"gpx":"http://www.topografix.com/GPX/1/0"}; names=[node.findtext("gpx:name", namespaces=ns) for node in root.findall("gpx:wpt", ns)]; assert names == ["GC-SYNTHETIC"], names' "$smoke/finds.gpx"
        python3 -c 'import pathlib, sys, xml.etree.ElementTree as ET; root=ET.parse(sys.argv[1]).getroot(); ns={"gpx":"http://www.topografix.com/GPX/1/0"}; names=[node.findtext("gpx:name", namespaces=ns) for node in root.findall("gpx:wpt", ns)]; assert len(names) == 3 and set(names) == {"GC-SYNTHETIC", "GC-SECOND", "GC-SYNTHETIC-1"}, names' "$smoke/export.gpx"
        python3 -c 'import sqlite3, sys; db=sqlite3.connect(sys.argv[1]); row=db.execute("SELECT parent, symbol, type FROM WAYPOINT WHERE name=?", ("RP-IMPORTED",)).fetchone(); assert row == ("GC-SYNTHETIC", "Waypoint", "Reference Point"), row' "$smoke/cache.db"
        python3 -c 'import sqlite3, sys; db=sqlite3.connect(sys.argv[1]); row=db.execute("SELECT parent, type FROM WAYPOINT WHERE name=?", ("RP-IMPORTED-SECOND",)).fetchone(); assert row == ("GC-SECOND", "Reference Point"), row' "$smoke/cache.db"
        python3 -c 'import sqlite3, sys; db=sqlite3.connect(sys.argv[1]); row=db.execute("SELECT type FROM WAYPOINT WHERE name=?", ("RP-LOC",)).fetchone(); assert row == ("Reference Point",), row' "$smoke/cache.db"
        python3 -c 'import json, sys; settings=json.load(open(sys.argv[1])); assert settings["/apps/ocm/importnooverwrite"] is True; assert settings["/apps/ocm/importpurgelogs"] is True; assert settings["/apps/ocm/importignoregsak"] is True' "$XDG_CONFIG_HOME/opencache-manager/settings.json"
        [[ "$(sqlite3 "$smoke/cache.db" "SELECT notes FROM GEOCACHE WHERE name='GC-SYNTHETIC';")" == "Synthetic note [GTK4]" ]]
        echo "GTK4 WebKit window mapped; edit/delete dialogs, internal navigation, and database mutations verified"
        exit 0
    fi
    if ! kill -0 "$app_pid" 2>/dev/null; then
        echo "GTK4 app exited before smoke assertions passed; log: $smoke/app.log" >&2
        exit 1
    fi
    sleep 0.25
done

echo "GTK4 window, engine count, or WebKit navigation assertion failed; log: $smoke/app.log" >&2
exit 1
