cmake_minimum_required(VERSION 3.20)
project(OpenCacheManager LANGUAGES NONE)

find_program(DOTNET_EXECUTABLE NAMES dotnet REQUIRED)
find_package(PkgConfig REQUIRED)
pkg_check_modules(WEBKITGTK6 QUIET webkitgtk-6.0)
pkg_check_modules(JAVASCRIPTCOREGTK6 QUIET javascriptcoregtk-6.0)
if(NOT WEBKITGTK6_FOUND OR NOT JAVASCRIPTCOREGTK6_FOUND)
    message(FATAL_ERROR "GTK4 requires the WebKitGTK 6.0 development/runtime packages (pkg-config modules webkitgtk-6.0 and javascriptcoregtk-6.0). Configure inside shell-gtk4.nix or install those packages.")
endif()
pkg_get_variable(WEBKITGTK6_LIBDIR webkitgtk-6.0 libdir)
pkg_get_variable(JAVASCRIPTCOREGTK6_LIBDIR javascriptcoregtk-6.0 libdir)
set(WEBKITGTK6_NATIVE_LIBRARY "${WEBKITGTK6_LIBDIR}/libwebkitgtk-6.0.so.4")
set(JAVASCRIPTCOREGTK6_NATIVE_LIBRARY "${JAVASCRIPTCOREGTK6_LIBDIR}/libjavascriptcoregtk-6.0.so")
if(NOT EXISTS "${WEBKITGTK6_NATIVE_LIBRARY}" OR NOT EXISTS "${JAVASCRIPTCOREGTK6_NATIVE_LIBRARY}")
    message(FATAL_ERROR "WebKitGTK 6.0 pkg-config metadata was found, but its native shared libraries are missing from the reported libdir.")
endif()

set(QUICK_SHARUN_URL "https://raw.githubusercontent.com/pkgforge-dev/Anylinux-AppImages/f7f6232a2270278d5387da283d453bf21392ca61/useful-tools/quick-sharun.sh")
set(QUICK_SHARUN_SHA256 "b38ac233f0b56a03bb10565f6b074a43ce016939faee44ae8bb23d6e66c2d553")
set(QUICK_SHARUN_SCRIPT "${CMAKE_BINARY_DIR}/tools/quick-sharun.sh")

add_custom_command(
    OUTPUT "${QUICK_SHARUN_SCRIPT}"
    COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_BINARY_DIR}/tools"
    COMMAND "${CMAKE_COMMAND}"
        "-DQUICK_SHARUN_OUTPUT=${QUICK_SHARUN_SCRIPT}"
        "-DQUICK_SHARUN_URL=${QUICK_SHARUN_URL}"
        "-DQUICK_SHARUN_SHA256=${QUICK_SHARUN_SHA256}"
        -P "${CMAKE_CURRENT_SOURCE_DIR}/cmake_helpers/quick-sharun-download.cmake"
    DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/cmake_helpers/quick-sharun-download.cmake"
    COMMENT "Downloading pinned AnyLinux quick-sharun"
    VERBATIM
)

file(STRINGS "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/version/Version.txt" OCM_VERSION LIMIT_COUNT 1)
set(OCM_APPIMAGE_DIR "${CMAKE_BINARY_DIR}/appimage")
set(OCM_APPIMAGE_FILE "${OCM_APPIMAGE_DIR}/OpenCacheManager-${OCM_VERSION}-x86_64.AppImage")
set(OCM_GTK4_APPIMAGE_DIR "${CMAKE_BINARY_DIR}/appimage-gtk4")
set(OCM_GTK4_APPIMAGE_FILE "${OCM_GTK4_APPIMAGE_DIR}/OpenCacheManager-${OCM_VERSION}-gtk4-x86_64.AppImage")

set(expanded_datadir "${CMAKE_INSTALL_PREFIX}/share")
set(GENERATED_SOURCE_DIR "${CMAKE_BINARY_DIR}/generated")
file(MAKE_DIRECTORY "${GENERATED_SOURCE_DIR}")
configure_file(
    "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Paths.cs.in"
    "${GENERATED_SOURCE_DIR}/Paths.cs"
    @ONLY
)

set(OCMENGINE_PROJECT "${CMAKE_CURRENT_SOURCE_DIR}/ocmengine/ocmengine.csproj")
set(OCMENGINE_SOURCES
    ocmengine/AssemblyInfo.cs
    ocmengine/CacheLog.cs
    ocmengine/Waypoint.cs
    ocmengine/GPXParser.cs
    ocmengine/Utilities.cs
    ocmengine/DegreeMinutes.cs
    ocmengine/Geocache.cs
    ocmengine/GPXWriter.cs
    ocmengine/TravelBug.cs
    ocmengine/WaypointEnumerator.cs
    ocmengine/FilterList.cs
    ocmengine/GPSBabelWriter.cs
    ocmengine/CacheAttribute.cs
    ocmengine/FieldNotesHandler.cs
    ocmengine/CacheStore/ACacheStore.cs
    ocmengine/CacheStore/ReadCacheArgs.cs
    ocmengine/CacheStore/SQLLite/FileStore.cs
    ocmengine/CacheStore/SQLLite/FileStoreDefs.cs
)

add_custom_target(opencache-engine
    COMMAND "${DOTNET_EXECUTABLE}" build "${OCMENGINE_PROJECT}"
        --configuration Release
        --output "${CMAKE_BINARY_DIR}/out"
        "-p:BaseIntermediateOutputPath=${CMAKE_BINARY_DIR}/obj/"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    DEPENDS "${OCMENGINE_PROJECT}" ${OCMENGINE_SOURCES}
    COMMENT "Building the OpenCacheManager data engine (.NET)"
    VERBATIM
)

add_custom_target(opencache-manager ALL
    COMMAND "${CMAKE_COMMAND}" -E rm -rf "${CMAKE_BINARY_DIR}/out/app"
    COMMAND "${DOTNET_EXECUTABLE}" publish
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        -c Release
        -r linux-x64
        --self-contained true
        --output "${CMAKE_BINARY_DIR}/out/app"
        "-p:BaseIntermediateOutputPath=${CMAKE_BINARY_DIR}/obj/app/gtk4-default/"
        "-p:BaseOutputPath=${CMAKE_BINARY_DIR}/bin/app/gtk4-default/"
        "-p:EngineOutputDir=${CMAKE_BINARY_DIR}/out"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/web" "${CMAKE_BINARY_DIR}/out/app/web"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/icons" "${CMAKE_BINARY_DIR}/out/app/icons"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/maps" "${CMAKE_BINARY_DIR}/out/app/maps"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/version" "${CMAKE_BINARY_DIR}/out/app/version"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/licence" "${CMAKE_BINARY_DIR}/out/app/licence"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/docs" "${CMAKE_BINARY_DIR}/out/app/docs"
    COMMAND "${CMAKE_COMMAND}" -E create_symlink "${WEBKITGTK6_NATIVE_LIBRARY}" "${CMAKE_BINARY_DIR}/out/app/libWebKit.so"
    COMMAND "${CMAKE_COMMAND}" -E create_symlink "${JAVASCRIPTCOREGTK6_NATIVE_LIBRARY}" "${CMAKE_BINARY_DIR}/out/app/libJavaScriptCore.so"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    DEPENDS opencache-engine
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/OCMApp.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/ExternalProcessLauncher.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/Widgets/DescriptionWidget.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/Widgets/LogViewerWidget.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/Widgets/CacheInfoWidget.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/Widgets/ReviewWaypointPreviewWidget.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/Dialogs/ReviewWaypointDialog.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Widgets/HTMLWidget.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/configuration/SettingsStore.cs"
    COMMENT "Publishing self-contained OpenCacheManager GTK4/WebKitGTK-6 application (.NET linux-x64)"
    VERBATIM
)

# Secondary GTK4 publish path retained for the GTK4-specific packaging target.
add_custom_target(opencache-manager-gtk4
    COMMAND "${CMAKE_COMMAND}" -E rm -rf "${CMAKE_BINARY_DIR}/out/app-gtk4"
    COMMAND "${DOTNET_EXECUTABLE}" publish
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        -c Release
        -r linux-x64
        --self-contained true
        --output "${CMAKE_BINARY_DIR}/out/app-gtk4"
        "-p:BaseIntermediateOutputPath=${CMAKE_BINARY_DIR}/obj/app/gtk4/"
        "-p:BaseOutputPath=${CMAKE_BINARY_DIR}/bin/app/gtk4/"
        "-p:EngineOutputDir=${CMAKE_BINARY_DIR}/out"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/web" "${CMAKE_BINARY_DIR}/out/app-gtk4/web"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/icons" "${CMAKE_BINARY_DIR}/out/app-gtk4/icons"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/maps" "${CMAKE_BINARY_DIR}/out/app-gtk4/maps"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/version" "${CMAKE_BINARY_DIR}/out/app-gtk4/version"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/licence" "${CMAKE_BINARY_DIR}/out/app-gtk4/licence"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/docs" "${CMAKE_BINARY_DIR}/out/app-gtk4/docs"
    COMMAND "${CMAKE_COMMAND}" -E create_symlink "${WEBKITGTK6_NATIVE_LIBRARY}" "${CMAKE_BINARY_DIR}/out/app-gtk4/libWebKit.so"
    COMMAND "${CMAKE_COMMAND}" -E create_symlink "${JAVASCRIPTCOREGTK6_NATIVE_LIBRARY}" "${CMAKE_BINARY_DIR}/out/app-gtk4/libJavaScriptCore.so"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    DEPENDS opencache-engine
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Application/OCMApp.Gtk4.cs"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/configuration/SettingsStore.cs"
    COMMENT "Publishing GTK4/WebKitGTK-6 build from the existing OCM application project"
    VERBATIM
)

# Optional AppImage target; the ordinary CMake build remains an AppImage-free publish.
add_custom_target(appimage
    COMMAND "${CMAKE_COMMAND}"
        "-DQUICK_SHARUN_SCRIPT=${QUICK_SHARUN_SCRIPT}"
        "-DAPPIMAGE_SOURCE_DIR=${CMAKE_CURRENT_SOURCE_DIR}"
        "-DAPPIMAGE_PUBLISH_DIR=${CMAKE_BINARY_DIR}/out/app"
        "-DAPPIMAGE_ROOT=${OCM_APPIMAGE_DIR}"
        "-DAPPIMAGE_FILE=${OCM_APPIMAGE_FILE}"
        -P "${CMAKE_CURRENT_SOURCE_DIR}/cmake_helpers/build-appimage.cmake"
    DEPENDS opencache-manager "${QUICK_SHARUN_SCRIPT}"
    COMMENT "Creating OpenCacheManager AppImage"
    VERBATIM
)

add_custom_target(appimage-test
    COMMAND "${CMAKE_COMMAND}" -E make_directory "${OCM_APPIMAGE_DIR}/test-home"
    COMMAND "${CMAKE_COMMAND}" -E env
        "HOME=${OCM_APPIMAGE_DIR}/test-home"
        "PATH=${OCM_APPIMAGE_DIR}/tools:$ENV{PATH}"
        "${QUICK_SHARUN_SCRIPT}" --test "${OCM_APPIMAGE_FILE}"
    DEPENDS appimage
    COMMENT "Smoke-testing the generated OpenCacheManager AppImage"
    VERBATIM
)

# Keep the GTK4/WebKitGTK-6 package isolated from the legacy GTK3 artifact.
add_custom_target(appimage-gtk4
    COMMAND "${CMAKE_COMMAND}"
        "-DAPPIMAGE_VARIANT=gtk4"
        "-DQUICK_SHARUN_SCRIPT=${QUICK_SHARUN_SCRIPT}"
        "-DAPPIMAGE_SOURCE_DIR=${CMAKE_CURRENT_SOURCE_DIR}"
        "-DAPPIMAGE_PUBLISH_DIR=${CMAKE_BINARY_DIR}/out/app-gtk4"
        "-DAPPIMAGE_ROOT=${OCM_GTK4_APPIMAGE_DIR}"
        "-DAPPIMAGE_FILE=${OCM_GTK4_APPIMAGE_FILE}"
        -P "${CMAKE_CURRENT_SOURCE_DIR}/cmake_helpers/build-appimage.cmake"
    DEPENDS opencache-manager-gtk4 "${QUICK_SHARUN_SCRIPT}"
    COMMENT "Creating OpenCacheManager GTK4/WebKitGTK-6 AppImage"
    VERBATIM
)

add_custom_target(appimage-gtk4-test
    COMMAND "${CMAKE_COMMAND}" -E make_directory "${OCM_GTK4_APPIMAGE_DIR}/test-home"
    COMMAND "${CMAKE_COMMAND}" -E env
        "HOME=${OCM_GTK4_APPIMAGE_DIR}/test-home"
        "PATH=${OCM_GTK4_APPIMAGE_DIR}/tools:$ENV{PATH}"
        "${QUICK_SHARUN_SCRIPT}" --test "${OCM_GTK4_APPIMAGE_FILE}"
    DEPENDS appimage-gtk4
    COMMENT "Smoke-testing the generated GTK4/WebKitGTK-6 AppImage"
    VERBATIM
)
