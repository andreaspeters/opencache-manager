Dies ist ein Fork von Opencachemanager (http://opencachemanage.sourceforge.net/) und angepasst an die VerÃ¤nderungen in ndesk und der Mono GTK Library.

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



