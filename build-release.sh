#!/bin/bash

# The goal of this script is to ensure that released packages are an exact
# match to code which has been made available on github.
# It checks out a new copy of the repo, switches to the specified tag name,
# and builds a package with the name of that tag.

# Usage:
#  ./build-release.sh <tag-name>

tag=$1
repo="https://github.com/KrimZon/JsonKerman.git"
project="JsonKerman"
package="JsonKerman"
package_ext="zip"

# Get filename to use, sanitized.
filename=$1
filename=${filename//\.\./}
filename=${filename//\//}
filename=${filename//\\/}

if [ "$filename" != "$tag" ]; then
	echo "Invalid tag name"
	exit
fi

if [ -z "$filename" ]; then
	echo "Must specify a tag name"
	exit
fi

# Clone the repository into the new folder.
folder="release/${filename}"
mkdir "${folder}"
git clone "${repo}" "${folder}"

# Symlink the references.
rm -rf "release/${filename}/externalReferences"
ln -s "../../externalReferences" "release/${filename}/externalReferences"

# Switch to the tag.
cd "${folder}"
if git show-ref --tags | egrep -q "refs/tags/${filename}$"
then
	git checkout "tags/${filename}"
else
	echo "The tag does not exist"
	exit
fi

# Build the project.
mdtool build -c:Release "source/${project}.csproj"

# Create the package.
./package.sh
mv "${package}.${package_ext}" "../${package}_${filename}.${package_ext}"

