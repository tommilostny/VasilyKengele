# Vasily Kengele

Knock-off version of the original [Asili Kengele](https://linktr.ee/asilikengele) by **[@fpetru104](https://github.com/fpetru104)**.

Runs as a Telegram bot at: [t.me/vasilykengele_bot](t.me/vasilykengele_bot).

<img src="img/vasily.jpg" alt="Vasily Kengele photo" width="166"/>

## Installation
- Install .NET 6.0 with ASP.NET Core 6.0 to use ``dotnet`` CLI.
- Go to the source directory (**``cd src``**).
- Set the Telegram bot token in [appsettings.json](VasilyKengele/appsettings.json).
- *Optional*: logging to **InfluxDB** (put credentials to appsettings.json and set *LoggingToDbEnabled* to *true*).
- *Optional*: To send periodic e-mails with Telegram Bot messages, set email credentials in appsettings.json and set *Enabled* to *true* to activate.
- Build and run in *Release* mode (bot then sends wake up messages at 5 AM):
    - ``dotnet publish --configuration Release``
    - ``dotnet bin/Release/net6.0/publish/VasilyKengele.dll``
- Or run in *Debug* mode (bot sends test wake up messages every 5 seconds):
    - ``dotnet run``

## Available bot commands
- ``/start``: Start receiving messages at 5 o'clock.
- ``/stop``: Stop receiving messages at 5 o'clock.
- ``/time``: Use this to tell Vasily your current time **HOUR**. He'll use it to calculate your timezone so your receive your wake up at your correct 5 o'clock time.
- ``/about_me``: Get JSON data stored about you by this bot.
- ``/delete_me``: Removes your data from out repository.
- ``/users_count``: Returns number of users currently waking up with us.
- ``/email``: Subscribe to the e-mail wake up notifications as well. Message format: ``/email person@email.com``.
- ``/email_delete``: Remove your email from wake up notifications.
- ``/help``: Display help.

## Wake up daily with the bot
After setup with ``/start`` and ``/time`` commands, you'll receive Telegram notifications like this at 5 AM:

<img src="img/telegram.jpg" alt="Telegram notifications example" width="400"/>

<small>E-mail notifications are currently in development.</small>
