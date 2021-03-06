# Vasily Kengele

Knock-off version of the original [Asili Kengele](https://linktr.ee/asilikengele) by **[@fpetru104](https://github.com/fpetru104)**.

Runs as a Telegram bot at: [t.me/vasilykengele_bot](t.me/vasilykengele_bot).

Optionally this bot provides a possibility to subscribe to e-mail notification as well. Our current domain and SMTP server provider is [forpsi.com](https://www.forpsi.com/) which enables us to use an e-mail address **``wakeupwith@vasilykengele.eu``**. *Warning* if your're about to install this bot for your own use: e-mail notifications may end in users spam folder if you don't provide proper authentication and your SMTP server doesn't sign messages using *DKIM*.

<img src="img/vasily.jpg" alt="Vasily Kengele photo" width="166"/>

## Install your own Telegram bot
- Install .NET 6.0 to use ``dotnet`` CLI.
- Go to the source directory (**``cd src``**).
- Set the Telegram bot token in [appsettings.json](VasilyKengele/appsettings.json).
- *Optional*: logging to **InfluxDB** (put credentials to appsettings.json and set *LoggingToDbEnabled* to *true*).
- *Optional*: To send periodic e-mails with Telegram Bot messages, set email credentials in appsettings.json and set *Enabled* to *true* to activate.
1. Build and run the bot yourself.
    - Build and run in *Release* mode (bot then sends wake up messages at 5 AM):
        - ``dotnet publish --configuration Release``
        - ``dotnet bin/Release/net6.0/publish/VasilyKengele.dll``
    - Or run in *Debug* mode (bot sends test wake up messages every 5 seconds):
        - ``dotnet run``
1. Use the [**VKServiceManager.sh**](VKServiceManager.sh) bash script (it starts up the Vasily Kengele bot to run as a daemon under systemd and can also stop it).
    - Open VKServiceManager.sh in your favorite editor and check if paths to dotnet and VasilyKengele folder is correct in the **DOTNET** and **VK_FOLDER** variables.
    - Run as root: ``sudo ./VKServiceManager.sh``

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

And after setting your e-mail address with ``/email`` command, you'll receiver these notifications there as well.

<img src="img/email.jpg" alt="Telegram notifications example" width="400"/>
