
Dies ist ein Fork der eingestellen Geocaching Software "Opencachemanager" (http://opencachemanage.sourceforge.net/). Mein Ziel ist die Weiterentwickligung der Software. Hierfuer benoetige ich jedoch Eure unterstuetzung. Bitte testet die Software, benutzt sie, oeffnet Feature Requests und spendet auch etwas.

[![Donate](https://liberapay.com/assets/widgets/donate.svg)](https://liberapay.com/~10452/donate)

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
```

Anschliessend kann man es mit dem Befehl "ocm-gtk" starten.
