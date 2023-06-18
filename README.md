# AVTTool
Small commandline utility to parse and extract both metadata and textures from the .avt format
used in certain Sega arcade racing games. Useful for making card editors, or if you're just curious.

## Features
 - Save textures as .png including transparency
 - Save avatar part metadata as .json

## Usage
Navigate to the folder containing AVTTool in a cmd, then execute
```
AVTTool.exe wmn_256_fix_bis_full.avt
```
where the second argument is the path to the .avt file you want to extract.\
It will create a new folder next to AVTTool.exe with the same name as your avt file\
containing the .json and all textures. 
