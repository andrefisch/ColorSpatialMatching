#  Gridpieces

Gridpieces have a number of aspects and public variables.  This should describe them a bit.


## Colors

These are the colors based on the animal sprites:


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
  * 10:  Washedout

## Types

There are currently 11 types of blocks

  *  0:  Regular Block:  Regular run of the mill block.  It will match with anything of its own color.
    *  If it doesn't have a color, it becomes an empty-space block
  *  1:  Remove One Color -- When a match or combo is made adjacent to this block it removes all blocks on the board with the same color as the block that was matched or comboed
  *  2:  Vert Clear  **Color Required**  -- When this block is matched or comboed with a block of the same color it will immediately remove all blocks in the same column
  *  3:  Horiz Clear  -- When this special block is next to another Horiz Clear block it removes all blocks between the two Horiz Clear blocks
  *  4:  Plus Clear  **Color Required**  -- When this block is matched or comboed with a block of the same color it will immediately remove all blocks in the same column and the same row
  *  5:  Up Block  -- When the countdown timer on this block hits 0 a new row is added. To remove it safely a match or combo must be made next to it before the countdown timer hits 0
  *  6:  Waste of Space Block  -- This block cannot be removed unless a match or combo is made next to it.
  *  7:  Whitewash Block  -- When the countdown timer on this block hits 0 all blocks on the board turn white visually for a short period of time but they still retain their actual color for matching. When a block is hilighted its true color is revealed. To remove it safely a match or combo must be made next to it before the countdown timer hits 0
  *  8:  Bomb Block  **Color Required**  -- When this block is matched or comboed with a block of the same color all blocks within two orthogonal steps of this block are removed (up and up, up and right, right and right, etc)
  *  9:  Bubbles Block  -- When the countdown timer on this block hits 0 random balloons will rise from the bottom of the screen for a short period of time to obscure the player's view of the field. Matches can still be made in spite of this, it is just a visual distraction. To remove it safely a match or combo must be made next to it before the countdown timer hits 0. THIS BLOCK HAS NOT BEEN IMPLEMENTED YET
  * 10:  Clock Block **Color Required**  -- When this block is matched or comboed wit ha block of the same color the countdown timer stops for a short period of time
  * 11:  Colorwash Block  -- When a match or combo is made adjacent to this block all other blocks on the board are turned the same color as the matched or comboed block
