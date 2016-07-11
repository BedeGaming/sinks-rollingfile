#!/bin/bash
echo ""
echo "Installing dotnet cli..."
echo ""

tools/install.sh

origPath=$PATH
export PATH="./dotnet/bin/:$PATH"

if [ $? -ne 0 ]; then
  echo >&2 ".NET Execution Environment installation has failed."
  exit 1
fi

export DOTNET_HOME="$DOTNET_INSTALL_DIR/cli"
export PATH="$DOTNET_HOME/bin:$PATH"

# Restore packages and build product
dotnet restore -v Minimal # Restore all packages

# Build all
# Note the exclude: https://github.com/dotnet/cli/issues/1342
for d in src/*; do 
	echo "Building $d"
	pushd "$d"
	dotnet build
	popd
done

# Run tests
for d in test/*; do 
    echo "Testing $d"
    pushd "$d"
    dotnet test
    popd
done

export PATH=$origPath