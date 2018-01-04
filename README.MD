[![Build Status](https://travis-ci.org/andreaspeters/opencache-manager.svg?branch=master)](https://travis-ci.org/andreaspeters/opencache-manager)
[![Donate](https://liberapay.com/assets/widgets/donate.svg)](https://liberapay.com/~10452/donate)


Dies ist ein Fork der eingestellen Geocaching Software "Opencachemanager" (http://opencachemanage.sourceforge.net/). Mein Ziel ist die Weiterentwickligung der Software. Hierfuer benoetige ich jedoch Eure unterstuetzung. Bitte testet die Software, benutzt sie, oeffnet Feature Requests und spendet auch etwas. Denn wie auch der Entwickler vor mir, so habe auch ich Familie! Wenn genug geld zusammen kommt, kann ich für bestimmte Tätigkeiten einen externen Entwickler beauftragen. :-)

### Kompilieren/Installieren

#### Unter Archlinux:

```
yaourt opencachemanager-git

```

#### Unter Ubuntu:

```
sudo apt-get install libwebkit-cil-dev libwebkit1.1-cil
sudo apt-get install gtk-sharp2 libgtk2.0-cil libgtk2.0-cil-dev
sudo apt-get install libgconf2.0-cil libgconf2.0-cil-dev
sudo apt-get install libdbus2.0-cil libdbus2.0-cil-dev libdbus-glib2.0-cil libdbus-glib2.0-cil-dev

./configure
make
sudo make install
```

Anschliessend kann man es mit dem Befehl "ocm-gtk" starten.


