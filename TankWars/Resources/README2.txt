()Extra newline from the wall was causing the view to stop updating.
// need to switch the powerup system to have multiple powerups
//Hotpotato mode crashes when only one player is left (trying to kill a null hot potato tanks) - Solution, reincrement the number of tanks alive after they've respawned.



//Hot potato how to play.
//The game will instantly start when there ]more than 3 players connect to the server. 
//One person would be chosen at hot potato randomly, hot potato is the only one that can shoot. When the hot potato's projectile hit other player, that player will be the hot potato.
//Hot potato will have limited time before they explode. Everyone can see this time through the timer, which replaces the score. 
//Last person alives would win, and after 5 seconds, everyone will spawn, and the game starts again.


//Client would show the tank oafter they have disconnected 
//Hot potato game mode instantly respawn
//

Design decision
//A bunch of getters and setters to access the information in the model. 
// as long as there are x and y coordinate in the wall for 2 points, the order of x and y doesn't matter in xml. This also apply to p1 and p2
// We move the serialize and desrilzie process into the controller. 
// Everytime we update world, which handles all collision, since beam would only appear in one frame, we return a beam list when updating the world, and use that beam list in controller to send the beam to the clients.
// We use the same model class for hot potato game mode, and seperate the game mode with normal mode with boolean instance variable

Hot potato design deciesion 
//The score for every memeber will be the timer until the hot potato explode 
// Hot potato doesn't start the game until we have 3 players. However, if a player disconnected , and the number reduces to below 3, the game still run.
// The name of the player changes to identify who is the hot potato.
//Player who are not the hot potato can't shoot
//Powerup are disables in this mode/ they won't spawn.
//