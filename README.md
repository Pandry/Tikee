# Tikee
Tike is a simple program that manages to remind to the user every given amount of time to take a break.
The ispiration came since I ahve the tendency to stay for long times in front of the screen, and that's not really good for my eyes.

# THIS IS STILL IN PRE-ALPHA, DO NOT EXPECT ANYTHING.
# NOT EVEN A SOUND IS PROVIDED

The program is written in WPF, here are some screenshots:

### Initial screen:
![Initial Screen](https://vgy.me/Isdsiy.png)

### During coutdown
![During countdown](https://vgy.me/5k8zVy.png)

### Idle timer (your idle time)
![Idle time](https://vgy.me/4cxkSY.png)
As you can see the UI is pretty minimalistic.


The program looks at the mouse movement every second and compare the position with the last knew.
If the position remains the same for more time than the treshold time (15 minutes), when the user comes back, the timer starts anew.
Once the program recognizes that the user is not in front of the PC, its background is blue.
The time will continue to go on anyway.
