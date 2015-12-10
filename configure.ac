dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([OpenCacheManager], [1.0.14])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

#SHAMROCK_EXPAND_LIBDIR
#SHAMROCK_EXPAND_BINDIR
#SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROG(GMCS, mcs, no)
if test "x$GMCS" = "xno"; then
        AC_MSG_ERROR([mcs Not found])
fi


AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=NO]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=NO]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release_translated,
	AC_HELP_STRING([--enable-release_translated],
		[Use 'RELEASE_TRANSLATED' Configuration [default=YES]]),
		enable_release_translated=yes, enable_release_translated=no)
AM_CONDITIONAL(ENABLE_RELEASE_TRANSLATED, test x$enable_release_translated = xyes)
if test "x$enable_release_translated" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_RELEASE_TRANSLATED, true)
	enable_release_translated=yes
fi


dnl package checks, common for all configs
PKG_CHECK_MODULES([WEBKIT_SHARP_10], [webkit-sharp-1.0])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
PKG_CHECK_MODULES([GLIB_SHARP_20], [glib-sharp-2.0])
PKG_CHECK_MODULES([GCONF_SHARP_20], [gconf-sharp-2.0])
PKG_CHECK_MODULES([NDESK_DBUS_10], [dbus-sharp-2.0])
PKG_CHECK_MODULES([NDESK_DBUS_GLIB_10], [dbus-sharp-glib-2.0])

dnl package checks, per config


AC_CONFIG_FILES([
ocmengine/ocmengine.pc
ocmengine/Makefile
ocm-gtk/OCMApp.cs
ocm-gtk/Paths.cs
ocm-gtk/ocm-gtk
ocm-gtk/OCM.desktop
ocm-gtk/Makefile
Translations/Makefile
Makefile

])

AC_OUTPUT
