
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/ocmengine.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

OCMENGINE_DLL_MDB_SOURCE=bin/Debug/ocmengine.dll.mdb
OCMENGINE_DLL_MDB=$(BUILD_DIR)/ocmengine.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Debug/ocmengine.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

OCMENGINE_DLL_MDB=

endif

if ENABLE_RELEASE_TRANSLATED
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release_Translated/ocmengine.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release_Translated

OCMENGINE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(OCMENGINE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(OCMENGINE_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	CacheLog.cs \
	Waypoint.cs \
	GPXParser.cs \
	Utilities.cs \
	DegreeMinutes.cs \
	Geocache.cs \
	GPXWriter.cs \
	TravelBug.cs \
	WaypointEnumerator.cs \
	FilterList.cs \
	GPSBabelWriter.cs \
	CacheAttribute.cs \
	FieldNotesHandler.cs \
	CacheStore/ACacheStore.cs \
	CacheStore/ReadCacheArgs.cs \
	CacheStore/SQLLite/FileStore.cs \
	CacheStore/SQLLite/FileStoreDefs.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	CacheStore \
	CacheStore/SQLLite \
	ocmengine.pc.in 

REFERENCES =  \
	System \
	System.Xml \
	Mono.Posix \
	System.Data \
	Mono.Data.Sqlite \
	System.Web

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

OCMENGINE_PC = $(BUILD_DIR)/ocmengine.pc

$(eval $(call emit-deploy-wrapper,OCMENGINE_PC,ocmengine.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
