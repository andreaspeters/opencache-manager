
with import <nixpkgs> {};

let
  dotnetPkg = with dotnetCorePackages; combinePackages [
    sdk_8_0
  ];

  deps = [
    zlib
    openssl
    gtk3
    glib
    gdk-pixbuf
    pango
    icu
    gettext
    libint
    dotnetPkg
    webkitgtk_4_1
    librsvg
    glib-networking
    sqlite
    unzip
  ];

  # WebkitGtkSharp 3.24 hardcodes WebKitGTK 4.0's soname. Current nixpkgs
  # ships GTK3 WebKitGTK 4.1 instead; this local alias is smoke-tested for
  # WebView creation and local HTML loading, not a general 4.0/4.1 ABI guarantee.
  webkitGtkAbiCompat = runCommand "webkitgtk-4.0-abi-compat" {} ''
    mkdir -p $out/lib
    ln -s "${webkitgtk_4_1}/lib/libwebkit2gtk-4.1.so.0" "$out/lib/libwebkit2gtk-4.0.so.37"
  '';

  runtimeLibs = lib.makeLibraryPath (
    [ stdenv.cc.cc ] ++ deps ++ [ webkitGtkAbiCompat ]
  );
in
stdenv.mkDerivation {
  name = "dotnet-env";

  buildInputs = deps ++ [
    cmake
  ];

  NIX_LD_LIBRARY_PATH = runtimeLibs;
  NIX_LD = "${stdenv.cc.libc_bin}/bin/ld.so";

  SOURCE_DATE_EPOCH = 315532800;
  PROJDIR = "${toString ./.}";

  shellHook = ''
    export DOTNET_ROOT="${dotnetPkg}/share/dotnet"
    export GSETTINGS_SCHEMA_DIR="${gtk3}/share/gsettings-schemas/gtk+3-${gtk3.version}/glib-2.0/schemas"
    export GIO_EXTRA_MODULES="${glib-networking}/lib/gio/modules''${GIO_EXTRA_MODULES:+:$GIO_EXTRA_MODULES}"
    export LD_LIBRARY_PATH="${runtimeLibs}:''${LD_LIBRARY_PATH:-}"
    export LIBINTL_PATH="${gettext}/lib/preloadable_libintl.so"
  '';
}

