i do not know how to use version control

# what the hell is this
this is the source code for a discord bot that i wanted to make the code for public. 

it has features such as user specific colors, a single moderation command, saying hi, and more that i don't feel like writing about.

the bot's responses are dictated by a person.json, giving the bot some variety in it's lack of actual features.

no, the actual bot isn't available to add to your server, host it yourself.

# config.json
this bot uses a config.json that contains my bot token and stuff, so, i didn't want it here :thumbsup:

make the file yourself, and add it's path as an executable argument when launching the program

here are the keys:
- **token (string, required)**: y'know, the bot's token.
- **ignore-log-severity (number, optional)**: a lot of logs can appear, luckily they all have a severity level between 0-5. i'd recommend setting this one to 2 when you aren't testing the bot so that the console isn't flooded too badly.
- **person (string, required)**: path from targetted config.json to the person.json.
- **database (string, required)**: path from targetted config.json to a database.json.
- **random-status-interval (number, optional)**: time in milliseconds it waits to change it's status.
- **disable-ready-messages (bool, optional)**: whether or not the bot sends an on_ready message to all subscribed channels.