# SeparateVolume
Separate volume for headphones and speakers.

SeparateVolume is a service that runs in the background and allows you to have separate volume levels for headphones/earphones and speakers - a feature which seems to be missing from the Realtek HD Audio drivers/applications.

The project also includes an installer for the service.

Supported devices:
* Realtek ALC892 (tested with driver v6.0.1.7246)
* Possibly any Realtek High Definition Audio device

Known issues:
* There's a short latency - I'm not sure if this can be improved in the future

The project uses [NAudio](https://github.com/naudio/NAudio).

Keywords: jack sensing, NAudio, Realtek
