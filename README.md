# PixelPet
PixelPet is an image processing tool for retro games modding. It allows you to automatize converting common image files (such as PNG) to the binary formats supported by retro consoles. You can also go the other way, using PixelPet to extract and render binary images from game ROMs.

At the moment, PixelPet is geared mostly towards the Game Boy Advance and Nintendo DS family of systems, but should be easy to extend to other systems.

Note that PixelPet is a work-in-progress, and new features and functionality are added as the need for them arises. As such, the information described below is subject to change.

## Workbench

Internally PixelPet has a "workbench" that holds the main objects that it's processing on. Each of the commands supported by PixelPet operates on one or more of these objects. In this sense, PixelPet mimics the video system used by retro consoles.

The objects held by the workbench include:

 *  Set of palettes.
 *  Bitmap image.
 *  Stream of bytes.
 *  Tileset.
 *  Tilemap.

For instance, the loaded bitmap image can be converted to a combination of tileset + tilemap, which can then be output to a bytestream. Or vice versa, a tileset and tilemap can be read from a bytestream, then used to render a new bitmap image.

### Palettes

Every palette in PixelPet is assigned a unique palette slot number. Whenever a new palette is added, it gets the lowest unclaimed palette slot number (starting at 0), but it is also possible to manually specify the palette slot for each palette. With this mechanism, PixelPet fully supports generating and rendering multi-palette tilesets and tilemaps.

A palette can hold any number of colors, but a palette may have a maximum number of colors set; if you attempt to add any colors beyond the maximum, an error is thrown. The colors can be in any of the supported formats, such as 24-bit and 15-bit, and can be converted between formats with certain commands.

### Bitmap

The bitmap is simply a full image that is being operated on. This uses .NET's Bitmap class internally, and thus can be in any format that .NET's Bitmap class supports. The bitmap can be importanted from various file formats; this has not been extensively tested, however, so using standard PNG images in 24-bit RGB or 32-bit RGBA format is advised. You can also export the bitmap to a standard PNG image file.

### Bytestream

The bytestream is a sequence of bytes that any of the other objects in the workbench can be serialized to, or deserialized from. It can also be imported from or written to a binary file.

### Tileset

The tileset holds a set of tiles that can be used in conjunction with the tilemap to represent an image. Each tileset has a specific tile size as well as a color format. By default, the tileset has a tile size of 8 by 8 pixels, and uses a 32-RGBA color format.

Whenever a tile is added to the tileset, PixelPet will first check whether that tile already exists in the tileset, either as-is, or flipped in the horizontal or vertical direction, or both. If this is the case, then the tile will not be added to the tileset. However, it is possible to disable this behavior if you want to purposely add duplicate tiles to the tileset.

The tileset may be unindexed, meaning each pixel value in each tile holds the actual color value that will be displayed, or it may be indexed, meaning each pixel value represents an index in a palette, and the color value that is located in the palette at that index will be displayed instead.

### Tilemap

The tilemap holds a set of tile display entries, that can be used in conjunction with the tileset to represent an image. Each tile display entry holds the number of the tile, whether the tile should be flipped horizontally and/or vertically, and which palette slot should be used to render the tile (in case the tileset is indexed).

In PixelPet, the tilemap does not have a specific predefined width or height (i.e. number of tiles per row/column). Instead, tile entries can be added onto the tilemap indefinitely, and the tilemap may then be rendered at various specific widths and heights.

## Color formats

PixelPet currently supports the following color formats. A color format describes the range of each color component (Red, Green, Blue, and optionally, Alpha), and how these are stored in memory. Names of color formats follow *word order*, e.g. BGR555 is a format where the Blue component is stored in the upper bits and the Red component is stored in the lower bits.

 *  **2BPP** - General 2-bit grayscale, where `0x0` is pure black and `0x3` is pure white.
 *  **4BPP** - General 4-bit grayscale, where `0x0` is pure black and `0xF` is pure white.
 *  **8BPP** - General 8-bit grayscale, where `0x00` is pure black and `0xFF` is pure white.
 *  **GB** - 2-bit grayscale as it is used on the Game Boy, where the luminance is inverted. `0x0` is pure white, and `0x3` is pure black.
 *  **GBA** / **BGR555** - 15-bit color as it is used on the Game Boy Advance, where the Red, Green and Blue components are all 5 bits. `0x0000` is pure black, and `0x7FFF` is pure white.
 *  **NDS** / **ABGR5551** - 16-bit color as it is used on the Nintendo DS, where the Red, Green and Blue components are 5 bits, and the Alpha component is 1 bit. `0x8000` is pure black, and `0xFFFF` is pure white.
 *  **24BPP** / **RGB888** - General 24-bit color, where the Red, Green and Blue components are all 8 bits. `0x000000` is pure black, and `0xFFFFFF` is pure white.
 *  **32BPP** / **ARGB8888** - General 32-bit color, where the Red, Green, Blue and Alpha components are all 8 bits. `0xFF000000` is pure black, and `0xFFFFFFFF` is pure white.

## Bitmap formats

PixelPet currently supports the following bitmap formats. A bitmap format describes how many bits are stored per pixel; whether the image is indexed (meaning a palette is needed to proper display it); if the image is indexed, which color format should be used to render the image if a palette is not present; and how tilemap entries are stored in memory.

 *  **GB** - 2 bits per pixel bitmap format as it used on the Game Boy, where a pair of 2 bytes collectively holds 8 pixels.
 *  **GBA-4BPP** / **NDS-4BPP** - 4 bits per pixel bitmap format as it is used on the Game Boy Advance and Nintendo DS. This bitmap format uses indexed colors, meaning a palette is required to properly display it. If no suitable palette is loaded, any rendered tiles will be rendered using the general `4BPP` grayscale color format instead.
 *  **GBA-8BPP** / **NDS-8BPP** - 8 bits per pixel bitmap format as it is used on the Game Boy Advance and Nintendo DS. This bitmap format uses indexed colors, meaning a palette is required to properly display it. If no suitable palette is loaded, any rendered tiles will be rendered using the general `8BPP` grayscale color format instead.

## Usage

PixelPet currently supports the following commands. Each command is specified as a separate command-line argument, followed by its parameters, also as command-line arguments. For example, to load an input image `input.png` and then store it elsewhere as `output.png`, you would run the following command from your favorite console:

```
PixelPet Import-Bitmap "input.png" Export-Bitmap "output.png"
```

Alternatively, you can also place the following in a basic text file `script.txt` (any extension may be used):

```
Import-Bitmap "input.png"
Export-Bitmap "output.png"
```

Then, run the script from your console as follows:

```
PixelPet Run-Script "script.txt"
```

## Commands

### Help
```
Help
```

Prints the list of commands supported by PixelPet along with their parameters.

### Run-Script
```
Run-Script <path> [--recursive/-r]
```

Runs the script from the basic text file specified by `<path>`. Said script can also include further `Run-Script` commands to run even more scripts from different files.

By default it is not allowed to recursively include any scripts that are already on the current stack. For instance, if a script named `script.txt` contains the command `Run-Script script.txt`, then an error is thrown. Moreover, if `script1.txt` calls `script2.txt`, which then calls `script1.txt` again, an error is thrown as well. However, this restriction can be disabled by specifying the `--recursive` or `-r` flag.

**Example usage:**
```
Run-Script "script.txt"
```

### Import-Bitmap
```
Import-Bitmap <path>
```

Imports the image file specified by `<path>` to the workbench bitmap. The previous workbench bitmap is discarded. Using standard 24-bit or 32-bit PNG images is advised.

**Example usage:**
```
Import-Bitmap "input.png"
```
Imports the image `input.png` to the workbench.

### Export-Bitmap
```
Export-Bitmap <path>
```

Exports the workbench bitmap to the file specified by `<path>`.

**Example usage:**
```
Export-Bitmap "output.png"
```
Exports the bitmap in the workbench to `output.png`.

### Import-Bytes
```
Import-Bytes <path> [--append/-a] [--offset/-o <count>] [--length/-l <count>]
```

Imports the binary file specified by `<path>` to the workbench bytestream. If `--append` is present, then the binary file is appended to the previous  workbench bytestream; otherwise, the previous workbench bytestream is discarded.

Optionally, you can use the `--offset` and/or `--length` parameters to specify the offset in the file from which bytes should be read (by default, this is 0), and/or the maximum number of bytes that should be loaded (by default, this is unbounded). Bytes will be loaded until the end of the file or the maximum length is reached, whichever comes first.

**Example usage:**
```
Import-Bytes "input.bin" --offset 0x4 --length 0x20
```
Imports `32 (0x20)` bytes from file `input.bin` starting at offset `4 (0x4)`.

### Export-Bytes
```
Export-Bytes <path>
```

Exports the entire workbench bytestream to the file specified by `<path>`.

**Example usage:**
```
Export-Bytes "output.bin"
```
Exports the workbench bytestream to `output.bin`.

### Clear-Palettes
```
Clear-Palettes
```

Discards all workbench palettes.

### Clear-Tileset
```
Clear-Tileset [--tile-size/-s <width> <height>]
```

Discards the workbench tileset, and initializes a new one. Optionally, the `--tile-size` parameter can be used to specify the tile width and height (in pixels) for the new tileset. By default, this is 8 by 8 pixels.

```
Clear-Tileset --tile-size 16 16
```
Initializes a new tileset with tile size 16 by 16 pixels.

### Clear-Tilemap
```
Clear-Tilemap
```

Discards the workbench tilemap, and initializes a new one.

### Extract-Palettes
```
Extract-Palettes [--append/-a] [--palette-number/-pn <number>] [--palette-size/-ps <count>] [--x/-x <pixels>] [--y/-y <pixels>] [--width/-w <pixels>] [--height/-h <pixels>] [--tile-size/-s <width> <height>]
```

Extracts palettes from the workbench bitmap.

This is done by first splitting the bitmap into tiles. For each tile, we then take the color values present and add these to one of the existing palettes (removing any duplicate colors) if possible, or add a new palette if this does not fit in any of the current palettes.

PixelPet attempts to minimize the number of palettes used, but a greedy algorithm is used, so it is not guaranteed that this does lead to the minimum number of palettes. If this is a concern, it may be better to create the templates manually and load them via the `Read-Palettes` command instead.

If `--append` is present, then the extracted palettes are added to the current workbench palettes. Otherwise, the current workbench palettes are discarded first.

The `--palette-number` option can be used to specify the initial palette slot number that will be used for any newly created palette. This slot number is increased until an empty slot is found. If this option is not specified, new palettes will get the highest slot number among the loaded palettes + 1.

The `--palette-size` option can be used to specify a maximum palette size for any newly create palette. Palettes that were created before this command is run retain their original maximum palette size. By default, the maximum palette size is unbounded.

The `--x`, `--y`, `--width` and `--height` options can be used to process only a portion of the workbench bitmap. The workbench bitmap will not be changed, but it will be processed as if it was cropped using the given parameters, and the first tile begins in the top left corner of the cropped bitmap. By default, the entire workbench bitmap is processed.

The `--tile-size` option specifies the size of each tile in pixels. If this option is omitted, then tile size of the current workbench tileset is used. Note: `--tile-size 1 1` is equivalent to equivalent to treating the entire bitmap as a single "tile".

**Example usage:**
```
Import-Bitmap "input.png"
Extract-Palettes --palette-size 16
```
Extracts 16-color palettes from the workbench bitmap.

### Read-Palettes
```
Read-Palettes [--append/-a] [--palette-number/-pn <number>] [--palette-size/-ps <count>] [--x/-x <pixels>] [--y/-y <pixels>] [--width/-w <pixels>] [--height/-h <pixels>]
```

Reads a palettes from a *palette image* loaded into the workbench bitmap. The colors read from the palette image are loaded into new palette(s).

This command can be used to load a predefined set of palettes. The palettes are stored as a *palette image*; an image consisting of 8 by 8 pixel blocks of a solid color, where each block represents one color in the palette. For example, a palette with 16 colors can be represented as a 128 by 8 pixels palette image. The blocks are read left-to-right, top-to-bottom. If any 8 by 8 pixel block does not consist solely of pixels with the exact same color value, an error will be thrown.

This command differs from `Extract-Palettes` in that the user specifies the palette explicitly; this allows for e.g. duplicate colors to be added, which would be removed if `Extract-Palettes` is used.

If `--append` is present, then the read palettes are added to the current workbench palettes. Otherwise, the current workbench palettes are discarded first.

The `--palette-number` option can be used to specify the initial palette slot number that will be used for any newly created palette. This slot number is increased until an empty slot is found. If this option is not specified, new palettes will get the highest slot number among the loaded palettes + 1.

The `--palette-size` option can be used to specify the maximum palette size for the newly created palettes. Once the maximum size for the current palette is reached, a new palette is added. As such, by using the `--palette-size` option, multiple palettes can be read at once. If the `--palette-size` option is not used, all colors will be placed into a single, unbounded palette.

The `--x`, `--y`, `--width` and `--height` options can be used to process only a portion of the workbench bitmap. The workbench bitmap will not be changed, but it will be processed as if it was cropped using the given parameters, and the first tile begins in the top left corner of the cropped bitmap. By default, the entire workbench bitmap is processed.

**Example usage:**
```
Import-Bitmap "128x16.png"
Read-Palettes --palette-size 16
```
Imports a 128 by 16 pixels palette image representing 2 palettes of 16 colors. Then, reads the 2 palettes from the palette image.

### Render-Palettes
```
Render-Palettes [--colors-per-row/-cw <count>]
```

Renders the workbench palettes as a *palette image*, and writes this to the workbench bitmap. The old workbench bitmap is discarded. This is the same sort of palette image as is used for the `Read-Palettes` command. Each loaded palette is placed on a separate row, with each color rendered as an 8-by-8 pixels square.. The palette image is converted/rendered in 32-bit color.

The `--colors-per-row` option can be used to specify the number of colors that are rendered per row. If there are more colors than fit on the row, then the colors wrap around to the next row. If the end of a palette is reached, but there is still room on the current row, then PixelPet moves on to the next row regardless and leaves the remainder of the current row empty. This option can be used e.g. to render a 256-color palette as a 16-by-16 color palette image rather than a 256-by-1 color palette image. If the `--colors-per-row` option is not specified, the palette image will use the width of the largest palette.

**Example usage:**
```
Import-Bitmap "input-16-color.png"
Extract-Palettes --palette-size 16
Render-Palettes
Export-Bitmap "palette.png"
```
Imports a 16-color image, generates a 16-color palette from it, renders this palette as a palette image, and then writes this palette image to `palette.png`.

### Render-Tileset
```
Render-Tileset [--tiles-per-row/-tw <count>]
```

Renders the workbench tileset as an image, and writes this to the workbench bitmap. The old workbench bitmap is discarded. The tileset image is rendered in 32-bit color.

If the workbench tileset uses indexed colors, then the workbench palettes are used when rendering the tiles. If there is a palette loaded with the same slot number as the palette that was originally used to index the tile, then that palette is used to render the tile. If there is no longer a palette loaded with that slot number, then the first palette that is currently loaded is used. Otherwise, if there are no palettes loaded at all, the tile is rendered using a generic grayscale palette.

If the workbench tileset does not used indexed colors, then each pixel is rendered as-is.

The `--tiles-per-row` option can be used to specify how many tiles should be rendered per row. By default, this is 32 tiles. For a tileset using 8 by 8 pixel tiles, this results in a tileset image that is 256 pixels wide. The height of the tileset image is computed automatically.

**Example usage:**
```
Import-Bitmap "input.png"
Extract-Palettes
Generate-Tilemap GBA-4BPP
Render-Tileset --tiles-per-row 16
Export-Bitmap "tileset.png"
```
Imports an image, generates a palette from it, generates a tilemap+tileset. Renders this tileset as an image with 16 tiles per row, and then writes this rendered tileset to `tileset.png`.

### Render-Tilemap
```
Render-Tilemap <tiles-per-row> <tiles-per-column>
```

Renders the workbench tilemap + tileset together as an image, and writes this to the workbench bitmap. The old workbench bitmap is discarded. The tilemap is rendered in 32-bit color.

Unlike the `Render-Tileset` command, both `<tiles-per-row>` and `<tiles-per-column>` must be specified. These indicate the number of tiles per row and column that will be rendered. For example, `Render-Tilemap 30 20` with 8 by 8 pixel tiles results in a bitmap of 240 by 160 pixels.

The tile entries in the tilemap are rendered left-to-right, top-to-bottom.

**Example usage:**
``` 
Import-Bytes "tilemap.bin"
Deserialize-Tilemap
Import-Bytes "tileset.bin"
Deserialize-Tileset GBA-4BPP
Render-Tilemap 30 20
Export-Bitmap "tilemap.png"
```
Imports and deserializes a tilemap and tileset from binary files. Then, renders the tilemap + tileset to a 240 by 160 pixel image, and then writes this rendered tilemap to `tilemap.png`.

### Crop-Bitmap
```
Crop-Bitmap [--x/-x <pixels>] [--y/-y <pixels>] [--width/-w <pixels>] [--height/-h <pixels>]
```

Crops the workbench bitmap using the specified parameters. The `--x` and `--y` options specify the x- and y-offset of the left and top edges respectively, whereas the `--width` and `--height` options specify the new width and height of the bitmap respectively. If any of the specified parameters fall outside the range of the original bitmap, they will be automatically adjusted to the bounds of the bitmap.

**Example usage:**
```
Crop-Bitmap --width 32 --height 32
```
Crops the workbench bitmap to the top-left 32 by 32 pixels.

### Convert-Bitmap
```
Convert-Bitmap <format> [--sloppy/-s]
```

Converts the workbench bitmap to the specified color format.

By default, most command produce a workbench bitmap that uses 32-bit color. Before the bitmap can be further processed for display on a retro console's display, usually, it must be converted to the proper color format first.

For every pixel in the bitmap, the Red, Green, Blue and Alpha components of each color value are scaled as follows:

```
out = (in + (in_max / 2) * out_max) / in_max
```
Where each division floors the result.

This is roughly equivalent to the following:
```
out = in * out_max / in_max
```
Where each division applies rounding in the correct direction.

For example, converting a 5-bit color component `31` to an 8-bit color component goes as follows:
```
31 * 255 / 31 = 255
```

However, many emulators and other image processing tools instead use an incorrect color scaling algorithm, where the color component is simply left-shifted, and the added bits are simply filled with zeroes. For example, again converting 5-bit color component `31` to 8-bit, the conversion is performed as follows:
```
31 << 3 = 248
```

Or, vice versa:
```
248 >> 3 = 31
```

By specifying the `--sloppy` option, PixelPet can use this bit-shift color scaling algorithm instead. This can be used to process images that have been ripped from inaccurate emulators.

**Example usage:**
```
Import-Bitmap "input.png"
Convert-Bitmap GBA --sloppy
Convert-Bitmap 32BPP
Export-Bitmap "output.png"
```
This loads a 32-bit image `input.png` which has been ripped from an inaccurate emulator, converts it back to its original GBA color format with the `--sloppy` option, and then converts it back to a 32-bit image using proper color scaling; effectively "fixing" the image. The fixed image is then written to `output.png`.

### Convert-Palettes
```
Convert-Palettes <format> [--sloppy/-s]
```

Converts all workbench palettes from their current color format to the specified color format.

This is very similar to the `Convert-Bitmap` command, but instead of operating on each pixel in the workbench bitmap, it operates on each color in the workbench palettes. Note that this is almost always significantly faster than `Convert-Bitmap`.

### Deduplicate-Palettes
```
Deduplicate-Palettes
```

Replaces all duplicate colors in all workbench palettes with new, unique colors.

This command uses a binary search algorithm that runs in `O(m log 2^n)`, where `m` is the number of new colors that need to be generated, and `n` is number of bits in the color space, and is guaranteed to generate unique colors -- as long as the total palette size does not exceed the size of the color space.

The new colors generated by `Deduplicate-Palettes` are random and not guaranteed to be consistent.

### Pad-Palettes
```
Pad-Palettes <width> [--color/-c <value>]
```

Pads all workbench palettes by adding colors until the palette has at least the specified width.

The `--color` option can be used to specify which color value will be added; by default, this is color value 0.

If there are no palettes in the workbench, a palette is created with an unbounded maximum size, and filled to the specified width.

**Example usage:**
```
Import-Bitmap "input.png"
Extract-Palettes --palette-size 16
Pad-Palettes 16
```
Imports the image `input.png`, extracts palettes from it and then pads all palettes to 16 colors.

### Pad-Tileset
```
Pad-Tileset <width> [--tile-size/-s <width> <height>]
```

Pads the workbench tileset by adding tiles until the tileset has at least the specified number of tiles.

All tiles that are added consist only of pixels with color value 0.

The `--tile-size` option specifies the size of each tile in pixels. If this is omitted, the tile size of the current tileset will be used. If the current tileset is nonempty, the tile size specified with `--tile-size` must match the tile size of the current tileset.

### Generate-Tilemap
```
Generate-Tilemap <format> [--append/-a] [--no-reduce/-nr] [--x/-x <pixels>] [--y/-y <pixels>] [--width/-w <pixels>] [--height/-h <pixels>] [--tile-size/-s <width> <height>]
```

Generates a tilemap + tileset from the workbench bitmap. The bitmap is processed left-to-right, top-to-bottom.

The `<format>` parameter specifies the bitmap format that is used. This also specifies whether the generated tiles will be indexed. Note that, to generate indexed tiles, the appropriate palettes must first be loaded or extracted.

If `--append` is present, then any new tilemap entries and tiles are appended to the current workbench tilemap and tileset. Otherwise, both the current tilemap and tileset are cleared first.

By default, PixelPet will attempt to reduce the number of tiles in the tileset by eliminating tiles that are already in the tileset, either as-is or flipped horizontally, vertically or in both directions -- provided that the bitmap format supports this. However, you can use the `--no-reduce` option to disable this; if this option is present, PixelPet will add every single tile from the workbench bitmap to the tileset as a new tile.

The `--x`, `--y`, `--width` and `--height` options can be used to process only a portion of the workbench bitmap. The workbench bitmap will not be changed, but it will be processed as if it was cropped using the given parameters, and the first tile begins in the top left corner of the cropped bitmap. By default, the entire workbench bitmap is processed.

The `--tile-size` option specifies the size of each tile in pixels. If this is omitted, the tile size of the current tileset will be used. If the current tileset is nonempty, the tile size specified with `--tile-size` must match the tile size of the current tileset.

**Example usage:**
```
Import-Bitmap "input-256-colors.png"
Extract-Palettes --palette-size 256
Generate-Tilemap GBA-8BPP
```
This imports an image `input-256-colors.png`, extracts a 256-color palette from it, and then uses the palette to generate an optimized tilemap + tileset in the `GBA-8BPP` indexed bitmap format.

### Deserialize-Palettes
```
Deserialize-Palettes <format> [--append/-a] [--palette-number/-pn <number>] [--palette-size/-ps <count>] [--palette-count/-pc <count>] [--offset/-o <count>]
```

Deserializes a series of palettes from the workbench bytestream. `<format>` specifies the color format in which the palettes are stored.

By default this command will read from the start to the end of the bytestream, one unbounded palette. The `--palette-size` option can be used to specify the size of each palette; once the maximum number of colors for the palette has been reached, PixelPet will create a new one, until the end of the bytestream is reached. The `--palette-count` option can be used to set a maximum on the number of palettes, to stop reading earlier.

If `--append` is present, then the new palettes will be added to the current workbench palettes. Otherwise, the current palettes are first discarded.

The `--palette-number` option can be used to specify the initial palette slot number that will be used for any newly created palette. This slot number is increased until an empty slot is found. If this option is not specified, new palettes will get the highest slot number among the loaded palettes + 1.

Finally, the `--offset` option can be used to specify an offset in the bytestream from which to start reading.

**Example usage:**
```
Import-Bytes "rom.gba"
Deserialize-Palettes GBA --palette-size 16 --palette-count 2 --offset 0x600000
```
Imports a ROM `rom.gba`, and reads two 16-color, GBA color format palettes from it at offset `0x600000`.

### Serialize-Palettes
```
Serialize-Palettes [--append/-a]
```

Serializes all palettes currently in the workbench to the workbench bytestream. Each color is serialized in accordance with its current color format; for instance, a 15-bit color palette is serialized as 2 bytes per color. The palettes are serialized in the order that they were added to the workbench.

If `--append` is present, then the serialized palettes are appended to the current workbench bytestream. Otherwise, the current workbench bytestream is discarded first.

### Deserialize-Tileset
```
Deserialize-Tileset <image-format> [--append/-a] [--ignore-palette/-ip] [--tile-count/-tc <count>] [--offset/-o <count>] [--tile-size/-s <width> <height>]
```

Deserializes a tileset from the workbench bytestream.

If `--append` is present, then the tiles are added to the existing workbench tileset; otherwise, the current tileset is discarded.

The `<image-format>` parameter specifies the bitmap format that is used. By default this command will begin deserializing from the start of the workbench bytestream until the end; however, the `--offset` option can be used to specify the starting address, and `--tile-count` can be used to specify the number of tiles that should be read. PixelPet will read tiles from the workbench bytestream until the number of tiles specified by `--tile-count` is reached, or the end of the bytestream is reached; whichever comes first.

By default, `Deserialize-Tileset` will apply the loaded palettes to the tileset as it's deserialized (if `<image-format>` is indexed). This results in a tileset where each pixel value is an actual color value. However, if `--ignore-palette` is present, this is omitted, and this creates a tileset where each pixel value is a palette color index.

The `--tile-size` option specifies the size of each tile in pixels. If this is omitted, the tile size of the current tileset will be used. If the current tileset is nonempty, the tile size specified with `--tile-size` must match the tile size of the current tileset.

**Example usage:**
```
Import-Bytes "rom.gba"
Deserialize-Tileset GBA-4BPP --tile-count 0x20 --offset 0x700000
```
Imports a ROM `rom.gba`, and reads 32 (`0x20`) tiles in GBA-4BPP format from offset `0x700000`.

### Serialize-Tileset
```
Serialize-Tileset [--append/a]
```

Serializes the entire workbench tileset to the workbench bytestream. Each tile is serialized in accordance with its current color format; for instance, an 8 by 8 pixels, 4BPP tile is serialized to 32 bytes. The tiles are serialized in the order that they were added to the workbench tileset.

If `--append` is present, the serialized tileset is appended to the end of the workbench bytestream. Otherwise, the previous workbench bytestream is discarded.

### Deserialize-Tilemap
```
Deserialize-Tilemap <image-format> [--append/-a] [--base-tile/-bt <index>] [--tile-count/-tc <count>] [--offset/-o <count>]
```

Deserializes a tilemap from the workbench bytestream.

The `<image-format>` specifies the bitmap format of the serialized tilemap.

If `--append` is present, the tilemap entries are added to the existing workbench tilemap. Otherwise, the workbench tilemap is cleared first.

The `--base-tile` option can be used to specify the tile number that corresponds with the first tile in the tileset. Whichever value is specified for `--base-tile` will be subtracted from the tile numbers referenced in the tilemap as it is being deserialized. By default this is 0.

By default this command begins deserialized from the start of the workbench bytestream, until the end of the bytestream is reached. The `--offset` option can be used to specify the offset in the bytestream from which to begin deserializing. The `--tile-count` option can be used to specify how many tilemap entries should be read; PixelPet will deserialize tilemap entries until the `--tile-count` is reached, or the end of the bytestream is reached; whichever comes first.

**Example usage:**
```
Import-Bytes "rom.gba"
Deserialize-Tilemap --tile-count 600 --offset 0x800000
```
Imports a ROM `rom.gba`, and reads 600 tilemap entries from offset `0x800000`. This could, for instance, correspond to a tilemap which is 30 by 20 tiles large.

### Serialize-Tilemap
```
Serialize-Tilemap [--append/-a] [--base-tile/-bt <index>] [--first-tile/-ft <tilemap-entry>]
```

Serializes the entire workbench tilemap to the workbench bytestream. Currently this command is hardcoded to use the GBA tilemap format.

If `--append` is present, the serialized tilemap is append to the end of the workbench bytestream. Otherwise, the previous workbench bytestream is discarded.

The `--base-tile` option can be used to specify the tile number that corresponds with the first tile in the tileset. Whichever value is specified for `--base-tile` will be added to the tile numbers referenced in the tilemap as it is being serialized. By default this is 0.

The `--first-tile` option can be used to replace any references to the very first tile in the tileset with a fixed value. For instance, one may want to set a `--base-tile` for the tileset but still use tile number 0 for the transparent tile; in this case, the value of the tilemap entry for the first tile can be specified with `--first-tile` directly.

**Example usage:**
```
Import-Bitmap "input-16-colors.png"
Extract-Palettes --palette-size 16
Generate-Tilemap GBA-4BPP
Serialize-Tilemap --base-tile 31 --first-tile 0
```
Imports an image `input-16-colors.png`, generates a 16-color palette from it, generates a tileset + tilemap, then serializes the tilemap whilst adding 31 to all tile numbers and using the value 0 for tilemap entries that reference the first tile.