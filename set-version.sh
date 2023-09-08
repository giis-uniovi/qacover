# Set a new version for all project components

# Ensures this script is running in his folder
SCRIPT_DIR=$(readlink -f $0 | xargs dirname)
echo "Run command at directory: $SCRIPT_DIR"
cd $SCRIPT_DIR
pwd

read -p "Enter new version: " NEW_VERSION
#NEW_VERSION="2.3.4-SNAPSHOT"

# Java multimodule project
mvn versions:set -DnewVersion="$NEW_VERSION" --no-transfer-progress

# Net projects
NET_PROJECT="net/QACover/QACover.csproj"
echo "set-version $NEW_VERSION for $NET_PROJECT"
sed -i "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" $NET_PROJECT
sed -i "s/<InformationalVersion>.*<\/InformationalVersion>/<InformationalVersion>$NEW_VERSION<\/InformationalVersion>/" $NET_PROJECT
if [[ "$OSTYPE" == "msys" ]]; then # preserve CRLF when running from vs studio terminal in windows
  unix2dos $NET_PROJECT
fi

NET_PROJECT="net/QACover/QACoverReport.csproj"
echo "set-version $NEW_VERSION for $NET_PROJECT"
sed -i "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" $NET_PROJECT
sed -i "s/<InformationalVersion>.*<\/InformationalVersion>/<InformationalVersion>$NEW_VERSION<\/InformationalVersion>/" $NET_PROJECT
if [[ "$OSTYPE" == "msys" ]]; then # preserve CRLF when running from vs studio terminal in windows
  unix2dos $NET_PROJECT
fi
