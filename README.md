# Car-Emulator

### Simple Cars Emulator (IRobot like) - Recieve locations &amp; Send commands

![carsEmu.png](https://s22.postimg.org/k6ghujmjl/cars_Emu.png)

### Basic Operations

Add New Car: 	Mouse right click

Move Car: 		Press '1' and Keyboard Arraows to move Car #1

### Get Cars Location

Recieve each car location by UDP.

Format:

    'x1, y1, angle1 | x_2, y2, angle2 |'

### Send Commands

Send command to each car (also UDP).

Format:

    'angle=359 | angle=4 | f=1|'    #Rotate car #1 to angle 359, and cmmand car #3 to move forward






