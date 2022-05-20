# Vasily Kengele

Knock-off version of the original [Asili Kengele](https://linktr.ee/asilikengele) by **[@fpetru104](https://github.com/fpetru104)**.

Runs as a Telegram bot at: [t.me/vasilykengele_bot](t.me/vasilykengele_bot).

<img src="vasily.jpg" alt="Vasily Kengele photo" width="166"/>

### Installation
1. Install .NET 6.0 with ASP.NET Core 6.0 to use ``dotnet`` CLI.
1. Set the Telegram bot token in [appsettings.json](VasilyKengele/appsettings.json).
1. Build and run in *Release* mode (bot then sends wake up messages at 5 AM):
    - ``dotnet publish --configuration Release``
    - ``dotnet bin/Release/net6.0/publish/VasilyKengele.dll``
1. Or run in *Debug* mode (bot sends test wake up messages every 5 seconds):
    - ``dotnet run``

### Available bot commands
- ``/start``: Start receiving messages at 5 o'clock.
- ``/stop``: Stop receiving messages at 5 o'clock.
- ``/time_set``: Use this to tell Vasily your current time **HOUR**. He'll use it to calculate your timezone so your receive your wake up at your 5 o'clock. (Message format is: ``/time_set number``).
- ``/about_me``: Get JSON data stored about you by this bot.
- ``/delete_me``: Removes your data from out repository.
- ``/users_count``: Returns number of users currently waking up with us.
- ``/help``: Display help.
