The server for a 2D tank war game. 
Author: Willian Erignac & Huy Nguyen 
Date: 04/23/2021

<Special Game Mode: HOT POTATO>
-The game mode will start if there are 3 or more players connect to the server 
-One random person would be chosen as the HOT POTATO. HOT POTATO will have their name changed to signify it. 
-After a period of time, HOT POTATO tank will exploded. The HOT POTATO can pass the HOT POTATO status to another player by hitting other player with their projectiles. 
-Except for the HOT POTATO, tanks can not shoot, and power ups will not spawn. 
-The score for each person has been replaced by the count down until the HOT POTATO exploded. If the HOT POTATO explodes, the status will be passed to another random person, and the timer will be reset. 
-The last player that still alive would be the victor of the game. After 7 seconds, other players will respawn, the server would start the game again.
-The user can modify the time that a HOT POTATO has until it explodes and the time until the next match starts in the setting XML.

<Bugs>
()Tank stops moving after a few commands																						(Fixed by removing extra new line from the json that sends to the clients)
()HOT POTATO game mode crashes when only one player is left (Trying to kill a null HOT POTATO tank)								(Fixed by re-increment the number of tanks alive after they've respawned)
()Clients would still show the tank image after the other player has disconnected												(Fixed by setting the diconnected player's hitpoints to zero)
()HOT POTATO game mode would instantly start again after																		(Fixed by creating a timer system)
()Projectiles stops when hitting the location where a tank died																	(Fixed by projectiles passing through tank with zero hit points)
()Projectiles appear in the center of the tank instead of at the cannon															(Fixed by change the spawn point for projectiles at the tank's cannon)
()The client would crash when the cursor is at the middle of the tank															(Fixed by having a small deadzone at the middle of the tank so that DrawObjectWithTransform from the client would not receive any input
																																 when the cursor is at the middle point)
()The porgram would crash when a player disconnects																				(Fixed by implementing try catch around the server Async proccess)
()Tank can damage itself if its projectile speed is slowe enough																(Fixed by making sure that the projectile would pass through its owner)
()The power ups would instantly spawn if the number of power ups on the map does not reach the maximum							(Fixed by reconfiguring the timer for the respawn powerup time)
()The projectiles stay where they are spawned																					(Fixed by adding actual movement calculation to the projectiles)
()The score of tank does not updated when killing a player																		(Fixed by changing the logic of collision)

<HOT POTATO design decision> 
- The score for each players would indicate where the HOT POTATO will explode, so they can plan their movement.
- HOT POTATO game would not start until there are three players in the server. If someone disconnect, that match would continue, and the server will not automatically start the next game until there are 3 or more players in the server.
- The name of the player is changed to HOT POTATO + their name if they are the HOT POTATO.
- The player passes their HOT POTATO status by hitting another player with their projectiles. If they are not HOT POTATO, they can not shoot.
- Powerups would not spawn in this mode.


<Design decision>
- A bunch of getters and setters to access the information in the model. 
- as long as there are x and y coordinate in the wall for 2 points, the order of x and y doesn't matter in xml. This also apply to p1 and p2
- We move the serialize and desrilzie process into the controller. 
- Everytime we update world, which handles all collision, since beam would only appear in one frame, we return a beam list when updating the world, and use that beam list in controller to send the beam to the clients.
- We use the same model class for hot potato game mode, and seperate the game mode with normal mode with boolean instance variable
- A bool to identify if there is something wrong with the settings.XML, if it is true, then we would not attemp to connect
- The settings of the server has to follow a certain protocol: An XML file, with the name "settings" or else it would not work
- When the cursor is at the middle of the tank , we have a deadzone that we would not update the turrent direction so that DrawObjectWIthTransform doesn't throw an error.
- WE use real time (second) to dertime when a frame has passed in the method (The xml would sitll have the same parameter from the original xml)
- A colision check for between tank wall and powerup
- Projectiles would dies if it reach the end of the map


