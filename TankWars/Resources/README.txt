A client for a 2D tank war game. 
Author: Willian Erignac & Huy Nguyen 
Date: 04/05/2021

<>
-Since TankWars client will be the same for everyone, there are no different in basic inputs. 

<Features that graders should be aware of> 

<Design decisions>
_
_We create an enum to hold the input make by the view to pass it into the GameController. 
_The initial mapsize of the world is 0 and the initial clientID is -1. By setting up these default value, we expect the server would never send invalid numbers such like these. Thus, when we start to receive valid value from the server, we can start the receiving process of the handshake. 
_Every JSon's properties has an getter and setter for easier access. 
_Since tank only "died" for a single frame. By default, when a tank's heath reaches below or equal to 0, we remove the tank, and we don't immediately add it to the Model dictionary so the tank "body" will not be redrawned by the view after it has "died". 
The dictionary will automatically add the tank back when the server signifies its new spawn location. This will make the view draw the tank in a new location. 
_The "death" of objects on the map. When an object is marked as "death" by the server, we remove it from the Dictionary that contain the object and its id in the Model. Then we notify the controller to inform the View through an action event. 
If the object that "died" is the client's tank. We force the camera to the new location after the tank has been revived. 
_Create a new class in Model to handel the beam animation. We will have a method that update the information with form.invalidiate. By doing this, we can modify 


<External code and resources> 
-Our own PS7 NetworkController. 

<Problems>
()Can not run the program																												(Fixed by installing the Json package)
()The walls was drawned weirdly																											(Fixed by using another formula to calculate the DrawObjectWithTransform)
()The turret is not rotating full 360 degree																							(Fixed by using formula for getting the cordinates of the mouse )
()The projectiles staying on the map after they have "died""																			(Fixed by    )
()When moving the tank, when holding a key then holding another key, after releasing the second key  that we hold, the tank would stop. 


