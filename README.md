# TextureChannelMixer
A simple Unity Editor tool to mix and match individual channels freely.
Useful in channel packing for particle and other custom shaders, since tools like Photoshop are cumbersome at alpha-editing.

Your textures need to be imported with read/write enabled and the format set to RGBA, personally i always use PNGs.
The window can be opened from Window > Generation > TextureChannelCombiner
It is also recommended to make safety copies of the textures you're copying into, because unforeseen issues could wipe your target texture's data.

Give me a shoutout on http://twitter.com/brumCGI if you like it!
Core of the texture copy operation was made by https://twitter.com/keepeetron/

<p align="center">
<img src="https://i.imgur.com/8Q8ethk.png">
</p>
