
 11/20
      today we started off with creating the classes that will be needed to deserialize the json from the server
we started out by simply following some of the instructions and creating blank console libraries for the game model and the game controller
We also added the readME file but we will be updating it in google docs for ease.
First we added the snake class the wall class and the power up class with all of the specified fields for deserialization

11/23 - 11/25
      today we added a lot to the game controller and most of the methods are empty however after watching the lecture we believe we are handling all of the events properly
We started by adding events for errors and displaying error messages in the view. next we added events that trigger at the appropriate times (detailed description below)
on the 25th we realized we need a class to represent the state of the world and we decided to use lists to store all of the game objects and add the game objects to the lists
as they come in from the server. the server connection is done via the ps7 networking library. We  then use the library to create the connection between the client and server
much of the code for receiving information from the server was directly from the chat system provided.

view => direct link to the controller
view has a controller inside of it to call a method that requests a connection
the view is also subscribed to an event that happens once the world has been updated by the server this happens when the controller says so.

controller => direct link to the model
controller then creates a world via the model
Once the server sends information we receive it and tell the world to update using the data we receive.

model has no links and strictly communicates to the controller via events
Once we update the world we let the controller know via an event. The word keeps track of all game objects.


11 / 26
      we realized we need to store the game objects in dictionaries for easier access when finding specific game objects. we started looking at a lot of lab 11 code and
tried to decipher it into our code. we needed the draw with transform method and the object drawing delegate. after a lot of time trying to understand the code
we did not.

Hiram got food poisoning...

11 / 27
      Today we debug a lot of our code and found a few issues firstly we realized errors in our world class as well and we realized we were not updating game objects
after fixing the game objects and having them update we began to draw objects that were updating correctly and we also were able to calculate the canvas transformation
correctly


============================================== PS9 ================================================


11/30 we started on the assignment and added the server project into the solution. Added base code for server connedctions this includes
Send, connect new clients, start server, process message and recieve message. Additionally our main was added so we could start the server
on the main thread.

12/1/23 Added some code in the main to be able to start the server on a different thread, and the main thread could be in a busy loop to
update the world.

12/2/23 Added our update world method so we could connect and disconnect clients properly, this will later serve to do game logic on snakes and
power ups

12/3/23 We worked on the handshake, we added a send wall/ send start up frame that would send all of the walls from the
settings and it would send it to each client only once upon connection, our client still does not draw at this momement.

12/4/23 We added the snake to world using a a give good point method. This makes sure that the snake doesn't spawn too
close to a wall so that it doesn't die immediatly when we start detecting snake collisions. At this point the client will
draw snake that doesn't move but everything is drawn correctly.

12/5/23 Some physics was added to our game so we could move around in different directions, additionally we added a wall
collision method, however it is quite buggy at this point in time and doesn't detect vertical walls, additionally it is changing
the walls when other clients connect. Movement still needs a lot of work we can only move left and up.

12/6/23 We fixed the walls for other clients so they drew the same on all clients. Additionally we found an issue with
our momvement where it would randomly move a random direction, however we do have the ability to move at any direction
and the movements speed is working well but this random movement gets worse depending on the servers load. We added snake
collision detection however it only works sometimes but we have a good start. We also are using a powerup spawner that
"spawns" a powerup when it is consumed.

12/7/23 Our wall collision method was changed but fully works now, additionally with our snake collisions. We fixed our random snake
movement so now it won't randomly move. We added snake self detection so now a snake will die if it hits itself. Additionally
we are adding our special mode and additional settings to our setting xml.The special mode that we addeds makes the snakes
head become its tail and its tail to become its head. To enable this you must go to the game settings and have mode be 1 for this feature
(Make sure all tags in settings.XML are in alphabetical order, but caps take priority, example shown below).We are also are working on
adding wrap around code to ensure that a snake cannot espace the world if there are no walls but instead wrap around the world.

//IMPORTANT ""settings"" must be the name of the file to read from in the bin also 0 is normal game mode and 1 is the special game mode described above
============ Example XML File with only one wall =================

<GameSettings>
  <GrowthBuffer>24</GrowthBuffer>        // this is how many frames a snake will grow after hitting a power up
  <MSPerFrame>34</MSPerFrame>            
  <Mode>1</Mode>                         // game mode 0 is normal game mode 1 inverts the snake after hitting powerups
  <RespawnRate>50</RespawnRate>         // respawn rate ... best with something like 50
  <UniverseSize>2000</UniverseSize>
  <Walls>
    <Wall>
      <ID>0</ID>
      <p1>
        <x>-975</x>
        <y>-975</y>
      </p1>
      <p2>
        <x>975</x>
        <y>-975</y>
      </p2>
    </Wall>
  </Walls>
</GameSettings>