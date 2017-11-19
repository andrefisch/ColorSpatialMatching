# Text File Serialization

A system to transform a text file into a playgrid.

## Setup

Two Numbers:

X Y  -- Represents the width and height of the board respectively.

Each space in the board is represented by 3 characters 0-b, 0-8, A-D

`00A   ---   a2A   ---   05C`

###  First Char:  Type

First one is a char from `0-9` and `a-b`.  Represents the type of block.

  *  0:  Regular Block
  *  1:  Remove One Color
  *  2:  Vert Clear  **Color Required**
  *  3:  Horiz Clear
  *  4:  Plus Clear   **Color Required**
  *  5:  Up Block
  *  6:  Waste of Space Block
  *  7:  Whitewash Block
  *  8:  Bomb Block   **Color Required**
  *  9:  Bubbles Block
  *  a:  Clock Block   **Color Required**
  *  b:  Colorwash Block


###  Second Char:  Color

Color coresponds to the gridpiece's const colors `0-9`

  *  0:  Edge, Clear, Not a Block
  *  1:  Red
  *  2:  Orange
  *  3:  Yellow
  *  4:  Green
  *  5:  Blue
  *  6:  Pink
  *  7:  Purple
  *  8:  Brown
  *  9:  White/Black


### Third Char:  Size

Size is different because there are 4 choices and it can override things.  The larger blocks should be linked togehter, but don't have to be.  `A-D`

  *  A:  Standard Block Size
  *  B:  1x2 Block Size
  *  C:  2x1 Block Size
  *  D:  2x2, Large Block Size

The grid is populated from right to left, then from top to bottom, therefore the text file is read similarly.

This means that if a D-2x2 block is seen, it will ignore spot to the left of it, below it, and below-left.  This is shown in the example below.

```
01D 01D    is equivalent to    43A 01D
01D 01D                        23D 03D 
```
The two sets of blocks are equivalent because the top-right block is a D block and therefor the rest of the blocks are ignored in the second example.


####  Notes:

The system is quite fault-tolerant and will give off warnings and such if the file doesn't conform properly.  It will also fill spaces with regular blocks or clear blocks if it finds chars outside of specified limits.

The first thing the board checks when adding the piece is if the color is 0-Clear, If it is, it will fill the piece with a 1x1 0-Color Block (of given blocktype) before looking at the size of the block

Note that larger blocks also should have type set at 0.  They will only be set as 0-type regular blocks and will give a warning if set something else.  They will default to 0-type regardless.
This happens in the `FillBoardBasedOnStringArray` method.

