#!/bin/bash
HOME_FOLDER=/home/pi
VK_FOLDER=$HOME_FOLDER/VasilyKengele
DOTNET=$HOME_FOLDER/.dotnet/dotnet

compileVK()
{
	echo "Compiling Vasily Kengele..."
	$DOTNET publish -c Release $VK_FOLDER/src/VasilyKengele.csproj
}

startVK()
{
	echo "Starting and enabling the service on boot."
	systemctl enable VasilyKengele.service
	systemctl start VasilyKengele.service
}

stopVK()
{
	echo "Stopping and disabling the service."
	systemctl stop VasilyKengele.service
	systemctl disable VasilyKengele.service
}

# Copy only if service file does not exist already or is updated.
cp --update $VK_FOLDER/VasilyKengele.service /etc/systemd/system/

echo "What do you want to do with Vasily Kengele service? (start/stop/update)"
read action

if [[ $action == "start" ]]
then
	# Compile Vasily Kengele C# project if needed.
    if [[ ! -f $VK_FOLDER/src/bin/Release/net6.0/publish/VasilyKengele.dll ]]
    then
        compileVK
	else
		echo "Vasily Kengele project is already compiled. Recompile? (y/n)"
		read compile
		if [[ $compile == "y" ]]
		then
			compileVK
		fi	
	fi

	startVK

elif [[ $action == "stop" ]]
then
	stopVK

elif [[ $action == "update" ]]
then
	stopVK
	git pull
	compileVK
	startVK

else
	echo "Invalid command!"
	echo "Available commands: start, stop"
fi
