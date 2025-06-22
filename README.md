# Simple Console LMU Stats Viewer
Welcome to the simple stats viewer repo.
## What is this thing?
A simple console .net 8 app to give you a rough idea of your hopefully enjoyable time in the quite wonderful LMU racing sim.
## Why is this not a nice looking webapp?
I rather spend my free time racing the sim than building webapps, since I build those for a living. Also, webapps require me to at least "think" about a "design" 🙄.
## Statistical caveats
Please note:
* Currently I take all the laps of a completed session (practice, qualifying or race) and deduct one. I will come up with a better way of deciding how many laps you actually completed when I have the time or feel like it.
* The game records no lap time of incomplete laps or invalidated laps, so I'm gonna have to maths my way to decide whether you completed a lap or not. Like I said, when I have the time or feel like it 😁.
# Download?
If you're brave/curious enough to have a try, there's a (far too fat) [executable](https://github.com/janbaerts/lmu-stats-viewer/blob/main/LmuStatsViewer.exe) in the repo for you to download and run. 
I recommend you put the file in the actual folder where you'd like to keep your stats file. The idea is that you're entire result log is not read every time you run the app.
For the techies amongst you, I can't trim unused assemblies from the executable without reconfiguring the JsonSerializer and I currently can't really be bothered. I'll put it with the nice-to-haves.
