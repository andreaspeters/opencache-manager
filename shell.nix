
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
		gettext
		libint
    dotnetPkg
  ];

  runtimeLibs = lib.makeLibraryPath (
    [ stdenv.cc.cc ] ++ deps
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
    export LD_LIBRARY_PATH="${runtimeLibs}:''${LD_LIBRARY_PATH:-}"
  '';
}

