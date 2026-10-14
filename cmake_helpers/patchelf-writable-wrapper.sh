#!/bin/sh
# quick-sharun copies readonly Nix-store ELF files into AppDir, then edits their RPATH.
# Make only AppDir copies writable; never alter their read-only sources.
set -u
: "${OCM_REAL_PATCHELF:?OCM_REAL_PATCHELF must name the real patchelf executable}"
: "${APPDIR:?APPDIR must name the staging AppDir}"
for _arg in "$@"; do
    case "$_arg" in
        "$APPDIR"/*)
            if [ -f "$_arg" ]; then
                chmod u+w -- "$_arg" 2>/dev/null || true
            fi
            ;;
    esac
done
exec "$OCM_REAL_PATCHELF" "$@"
