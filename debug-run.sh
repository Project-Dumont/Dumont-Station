echo "Starting debugging"
set -o pipefail
dotnet build --tl:off 2>&1 | tee /tmp/ss14-build.log
grep -inE ': error|MSB[1-9]+' /tmp/ss14-build.log
echo "Finish"
