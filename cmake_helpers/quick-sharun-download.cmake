if(NOT DEFINED QUICK_SHARUN_OUTPUT OR NOT DEFINED QUICK_SHARUN_URL OR NOT DEFINED QUICK_SHARUN_SHA256)
    message(FATAL_ERROR "QUICK_SHARUN_OUTPUT, QUICK_SHARUN_URL and QUICK_SHARUN_SHA256 are required")
endif()

file(DOWNLOAD
    "${QUICK_SHARUN_URL}"
    "${QUICK_SHARUN_OUTPUT}"
    EXPECTED_HASH "SHA256=${QUICK_SHARUN_SHA256}"
    STATUS _download_status
    TLS_VERIFY ON
    SHOW_PROGRESS
)
list(GET _download_status 0 _download_code)
list(GET _download_status 1 _download_message)
if(NOT _download_code EQUAL 0)
    file(REMOVE "${QUICK_SHARUN_OUTPUT}")
    message(FATAL_ERROR "Could not download verified quick-sharun: ${_download_message}")
endif()

file(CHMOD "${QUICK_SHARUN_OUTPUT}"
    PERMISSIONS OWNER_READ OWNER_WRITE OWNER_EXECUTE GROUP_READ GROUP_EXECUTE WORLD_READ WORLD_EXECUTE
)