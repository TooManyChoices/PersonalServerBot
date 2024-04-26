# config.json guide
hello mysterious github wanderer, welcome to this repo. you may have noticed a lot of things mentioning a "config.json" and yet no config.json in sight. well, my dear friend (who is dealing with my lack of capitalization) that is because it, like, contains my bot token and stuff, so, i didn't want it here :thumbsup:

you must make the file yourself (preferably in the configs folder) and also add it's path as an executable argument when launching the program

here are the keys:
- **token (string, required)**: y'know, the bot's token.
- **ignore-log-severity (number, optional)**: a lot of logs can appear, luckily they all have a severity level between 0-5. i'd recommend setting this one to 2 when you aren't testing the bot so that the console isn't flooded too badly.