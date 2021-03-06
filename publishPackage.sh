rm ./bin/Debug/*.nupkg
dotnet pack
file=$(ls ./bin/Debug/*.nupkg | grep .nupkg)
echo Getting ready to publish $file

source ~/.bash_profile
dotnet nuget push $file -k $NUGET_KEY -s https://api.nuget.org/v3/index.json

