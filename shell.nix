with import <nixpkgs> {};

stdenv.mkDerivation {
name = "dotnet-env";


buildInputs = [
  dotnet-sdk
	cmake
];


SOURCE_DATE_EPOCH = 315532800;
PROJDIR = "${toString ./.}";

shellHook = ''
    '';
}


