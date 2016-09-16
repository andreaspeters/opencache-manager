pkgname=opencachemanager
pkgver=1.0.17
pkgrel=17
pkgdesc="Easy to use, linux based program for managing your geocaches"
arch=('i686' 'x86_64')
url="http://opencachemanage.sourceforge.net/"
license=('GPL')
depends=('mono' 'dbus-sharp' 'shared-mime-info' 'webkit-sharp' 'desktop-file-utils' 'dbus-sharp-glib' 'gconf-sharp')
optdepends=('qlandkartegt: display cache on map'
            'gpsbabel: send cache to GPS')
source=("$pkgname"::'git=https://github.com/andreaspeters/opencache-manager.git')
sha256sums=('SKIP')

build() {
  cd "${srcdir}"/$pkgname

  ./configure --prefix=/usr
  make
}

package() {
  cd "${srcdir}"/$pkgname

  make DESTDIR="${pkgdir}" install
}
