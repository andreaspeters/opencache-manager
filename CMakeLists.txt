cmake_minimum_required(VERSION 3.20)
project(OpenCacheManager LANGUAGES NONE)

find_program(DOTNET_EXECUTABLE NAMES dotnet REQUIRED)

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
    COMMAND "${DOTNET_EXECUTABLE}" build
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        --configuration Release
        --output "${CMAKE_BINARY_DIR}/out/app"
        "-p:BaseIntermediateOutputPath=${CMAKE_BINARY_DIR}/obj/app/"
        "-p:EngineOutputDir=${CMAKE_BINARY_DIR}/out"
        "-p:GeneratedSourceDir=${GENERATED_SOURCE_DIR}"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/web"
        "${CMAKE_BINARY_DIR}/out/app/web"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    DEPENDS opencache-engine
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/ocm-gtk.csproj"
        "${CMAKE_CURRENT_SOURCE_DIR}/ocm-gtk/Paths.cs.in"
    COMMENT "Building OpenCacheManager GTK application (.NET)"
    VERBATIM
)
