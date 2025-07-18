#!/bin/bash

set -e

RUNTIME="x64"
LINUXPLATFORM="linux-${RUNTIME}"
WINPLATFORM="win-${RUNTIME}"
WINDIR="${PWD}/app-${WINPLATFORM}"
LINUXDIR="${PWD}/app-${LINUXPLATFORM}"
PUBLISHDIR="${PWD}/publish"

VERSION="$( git tag | tail -n 1 )"

if [ -z "${VERSION}" ]; then
	echo "Cannot get current version from tag."
	exit 255
fi

rm -vrf "${WINDIR}"
rm -vrf "${LINUXDIR}"
rm -vrf "${PUBLISHDIR}"

if ! which dotnet; then
	echo "dotnet not found."
	exit 255
fi

if ! dotnet restore; then
	echo "Failed to restore nuget packages."
	exit 255
fi

if ! dotnet test; then
	echo "Unit tests failed!"
	exit 255
fi

if ! dotnet publish ./AutoVersionIt/AutoVersionIt.csproj -c Release -r "${LINUXPLATFORM}" -o "${LINUXDIR}"; then
	echo "Failed to build Linux binaries"
	exit 1
fi

if ! dotnet publish ./AutoVersionIt/AutoVersionIt.csproj -c Release -r "${WINPLATFORM}" -o "${WINDIR}"; then
	echo "Failed to build Windows binaries"
	exit 2
fi

# Windows zip
cd "${WINDIR}"
zip -r -9 "AutoVersionIt-${VERSION}-${WINPLATFORM}.zip" *
md5sum "AutoVersionIt.exe" > "AutoVersionIt.exe.md5"
sha1sum "AutoVersionIt.exe" > "AutoVersionIt.exe.sha1"
md5sum "AutoVersionIt-${VERSION}-${WINPLATFORM}.zip" > "AutoVersionIt-${VERSION}-${WINPLATFORM}.zip.md5"
sha1sum "AutoVersionIt-${VERSION}-${WINPLATFORM}.zip" > "AutoVersionIt-${VERSION}-${WINPLATFORM}.zip.sha1"

if ! md5sum -c "AutoVersionIt.exe.md5"; then
  echo "MD5 checksum failed for Windows executable file."
  exit 3
fi
if ! sha1sum -c "AutoVersionIt.exe.sha1"; then
  echo "SHA1 checksum failed for Windows executable file."
  exit 3
fi
if ! md5sum -c "AutoVersionIt-${VERSION}-${WINPLATFORM}.zip.md5"; then
  echo "MD5 checksum failed for Windows zip file."
  exit 3
fi
if ! sha1sum -c "AutoVersionIt-${VERSION}-${WINPLATFORM}.zip.sha1"; then
  echo "SHA1 checksum failed for Windows zip file."
  exit 3
fi
cd ..

# Linux tar.gz
cd "${LINUXDIR}"
tar -czvf "AutoVersionIt-${VERSION}-${LINUXPLATFORM}.tar.gz" *
md5sum "AutoVersionIt" > "AutoVersionIt.md5"
sha1sum "AutoVersionIt" > "AutoVersionIt.sha1"
md5sum "AutoVersionIt-${VERSION}-${LINUXPLATFORM}.tar.gz" > "AutoVersionIt-${VERSION}-${LINUXPLATFORM}.tar.gz.md5"
sha1sum "AutoVersionIt-${VERSION}-${LINUXPLATFORM}.tar.gz" > "AutoVersionIt-${VERSION}-${LINUXPLATFORM}.tar.gz.sha1"

if ! md5sum -c "AutoVersionIt.md5"; then
  echo "MD5 checksum failed for Linux executable file."
  exit 3
fi
if ! sha1sum -c "AutoVersionIt.sha1"; then
  echo "SHA1 checksum failed for Linux executable file."
  exit 3
fi
if ! md5sum -c "AutoVersionIt-${VERSION}-${LINUXPLATFORM}.tar.gz.md5"; then
  echo "MD5 checksum failed for Linux tar.gz file."
  exit 4
fi
if ! sha1sum -c "AutoVersionIt-${VERSION}-${LINUXPLATFORM}.tar.gz.sha1"; then
  echo "SHA1 checksum failed for Linux tar.gz file."
  exit 4
fi
cd ..

mkdir "${PUBLISHDIR}"
cp "${WINDIR}/AutoVersionIt.exe" "${PUBLISHDIR}"
cp "${WINDIR}/AutoVersionIt.exe.md5" "${PUBLISHDIR}"
cp "${WINDIR}/AutoVersionIt.exe.sha1" "${PUBLISHDIR}"
cp "${LINUXDIR}/AutoVersionIt" "${PUBLISHDIR}"
cp "${LINUXDIR}/AutoVersionIt.md5" "${PUBLISHDIR}"
cp "${LINUXDIR}/AutoVersionIt.sha1" "${PUBLISHDIR}"


cp -r ${WINDIR}/*.zip* "${PUBLISHDIR}"
cp -r ${LINUXDIR}/*.tar.gz* "${PUBLISHDIR}"

echo -e "\nPublish complete."

exit 0
