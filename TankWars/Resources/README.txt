A client for a 2D tank war game. 
Author: Willian Erignac & Huy Nguyen 
Date: 04/08/2021

<>
-Since TankWars client will be the same for everyone, there are no differences in basic inputs. 

<Features that graders should be aware of>
+Our game can take any number of tank, turret, and projectile sprite groups (i.e. red tank, red turret, and red projectile) as long as the image filenames follow the same naming scheme:
  Tank: COLOR"Tank.png"
  Turret: COLOR"Turret.png"
  Projectile: "shot-"COLOR".png"
    *COLOR is not case-sensitive
    **Exactly one sprite of each COLOR is needed to work
  To demonstrate this feature, we added a disco tank to the list of sprites
+Custom laser and tank explosion animations.


<Design decisions>
_We created enums to translate button presses into movements and firing actions for the GameController.

_The default size and clientID are 0 and -1, respectively. We expect that the server would never send invalid numbers like these. Thus, we deduce whether the handshake has been completed by checking to see if these values are their defaults.

_Most Json serializable game objects' properties have getters and setters for easier access.

_Since tanks only "die" for a single frame, we remove the tank from the model once died is true and wait for its health to be greater than zero to add it back into the model. This prevents dead tanks from being drawn.
  _When the client's tank dies, the view captures the last position of the tank to keep the camera still while the client's tank is not in the model.

_Just like with tanks, when any object that can dies does, we remove it from the model.Then we notify the controller to inform the view that an object has died through the return value of the method that deserializes game objects.
  _This enables us to play animations when an object dies.
  _We count beams as objects that can die, even if they don't have the died parameter. We just treat them as if they die on the frame they arrive so that we can play the laser animation.

_We created different animation classes with a inheritance heiarchy for each animation type (laser and explosion).
  _Animatable -> FrameByFrameAnimation -> TankExplosionAnimation, BeamAnimation

_To ensure that the frist 9 (since we have the disco tank) players detected by the client have a unique color, we created a queue with the different sprite groups. Whenever a new player is detected by the client,
  a sprite group is dequeued by the view, assigned to the player, and then enqueued back into the queue.

_The controller holds a private instance of a Control Command that is updated as input comes from the view. When a frame is rendered, the controller sends the current state of the control command to the server.
  _To prevent controller from sending an "alt" fire command two frames in a row, the controller sets fire to "none" if it was "alt" aften being sent.
  _If this feature wasn't implemented, a player with two powerups would shoot both of their lasers in a miniscule amount of time, effectively wasting one powerup.

_We use a string to keep track of the last movement direction of the tank before its direction is changed by a button press. When we revert to string (i.e. when the overriding button is lifted), we set the string to "none".
  _If the string is not none and the button corresponding to the string's direction is lifted before the overriding button has been lifted, then the string is set to "none" (i.e. press A and S, lift A then S).

<External code and resources> 
-Our own PS7 NetworkController. 

<Problems>
()Can not run the program.																												(Fixed by installing the Json package)
()The walls were drawned weirdly.																								        (Fixed by using another formula to calculate the DrawObjectWithTransform)
()The turret is not rotating full 360 degrees.																							(Fixed by subtracting half the drawing panel size from the cordinates of the mouse)
()The projectiles stay on the map after they have "died"																			    (Fixed by removing dead objects from the model (see above))
()When moving the tank, when holding a key then holding another key, after releasing the second key that we hold, the tank would stop.  (Fixed by using a string to keep track of the last movement direction (see above))
()When moving and click on the form(outside of drawing panel), the tank will move forward.                                              (Fixed by )
()When playing the game, clicking on the box will have make the form focus on the boxes, and the player lose controls.                  (Fixed by disabling the text boxes after connecting to the server)
()Invisible bullet (actually just bullets that blend in really well with the background)                                                (Fixed by changing the colors of the sprites)
()The health bar is not chanigng color.                                                                                                 (Fixed by correctly calculating the percentage of health a tank has)
()The player only shoots one laser when they get two powerups (they actually shoot both powerups in two frames) .                       (see Control Command design decision subpoint)
()Closing the application results in an extra message box popping up (view is called by the controller, but the view is disposed).      (Put a try/catch block around calls to the view from the controller)
()The tanks no longer explode and the player's view is no longer kept after dying.                                                      (Fixed by implementing LifeTime property in TankExplosionAnimation)
()Every time the player types something in one of the two input boxes, the view displays an error                                       (Fixed by moving the error message box into an if statement checking that the enter key is the button pressed)
complaining one of the two boxes is empty.