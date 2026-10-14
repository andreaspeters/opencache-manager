cmake_minimum_required(VERSION 3.20)
project(OpenCacheManager LANGUAGES NONE)

find_program(DOTNET_EXECUTABLE NAMES dotnet REQUIRED)

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
    COMMAND "${DOTNET_EXECUTABLE}" publish
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        -c Release
        -r linux-x64
        --self-contained true
        --output "${CMAKE_BINARY_DIR}/out/app"
        "-p:BaseIntermediateOutputPath=${CMAKE_BINARY_DIR}/obj/app/linux-x64-self-contained/"
        "-p:EngineOutputDir=${CMAKE_BINARY_DIR}/out"
        "-p:GeneratedSourceDir=${GENERATED_SOURCE_DIR}"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/web"
        "${CMAKE_BINARY_DIR}/out/app/web"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/icons"
        "${CMAKE_BINARY_DIR}/out/app/icons"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/maps"
        "${CMAKE_BINARY_DIR}/out/app/maps"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/version"
        "${CMAKE_BINARY_DIR}/out/app/version"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/licence"
        "${CMAKE_BINARY_DIR}/out/app/licence"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/docs"
        "${CMAKE_BINARY_DIR}/out/app/docs"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    DEPENDS opencache-engine
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Paths.cs.in"
    COMMENT "Publishing self-contained OpenCacheManager GTK application (.NET linux-x64)"
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
