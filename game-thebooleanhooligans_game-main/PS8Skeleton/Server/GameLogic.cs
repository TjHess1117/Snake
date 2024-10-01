using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Server
{
    /// <summary>
    /// game logic is responsilbe for the bulk of the logic for the server
    /// </summary>
    internal class GameLogic
    {


        Random rand = new Random();
        public GameLogic() {}

        /// <summary>
        ///  this will update the movment of the snake and grow the length if they hit a powerup also keeps track of the game mode and flips the snake
        ///  around if they are playing the specil mode
        /// </summary>
        /// <param name="serverWorld"></param>
        /// <param name="movmentCommands"></param>
        public void movmentUpdates(World serverWorld, Dictionary<int,Vector2D> movmentCommands, GameSettings settings)
        {            
            // recive and process command requests
            foreach (Snake snake in serverWorld.snakes.Values)
            {
                if (SnakeHitPowerup(snake,serverWorld))
                {
                    if(snake.growthBuffer == 0)
                    {
                        snake.growthBuffer = 24;
                    }
                    snake.growthBuffer += settings.GrowthBuffer;
                    if (settings.Mode == 1)
                    {
                        Vector2D temp = TailDirection(snake.body);  // if the player is playing in special mode flip them around
                        temp.Rotate(180);
                        snake.dir = temp;
                        snake.body.Reverse();
                       
                    }
                }
              
                List<Vector2D> body = serverWorld.snakes[snake.snake].body;
                Vector2D velocity;

                if (movmentCommands.ContainsKey(snake.snake) && !movmentCommands[snake.snake].Equals(snake.dir))
                {
                    snake.dir = movmentCommands[snake.snake];
                    velocity = new Vector2D(snake.dir);
                    velocity.X = velocity.X * snake.speed;
                    velocity.Y = velocity.Y * snake.speed;
                    Vector2D nextPointForHead = new Vector2D(body[body.Count - 1].X, body[body.Count - 1].Y);
                    nextPointForHead = nextPointForHead + velocity;
                    // make a new head move the tail closer to the mid section untill the tail reaches the point above it
                    body.Add(nextPointForHead);
                    Vector2D directionForTailToTravle = TailDirection(body);
                    directionForTailToTravle.Y = directionForTailToTravle.Y * snake.speed;
                    directionForTailToTravle.X = directionForTailToTravle.X * snake.speed;
                    if(!(snake.growthBuffer > 0))
                    {
                        body[0] = body[0] + directionForTailToTravle; // move the tail the amount it needs to move closer to the mid section
                    }
                    if (tailFarEnough(body[0], body[1], directionForTailToTravle))
                    {
                        body.RemoveAt(0);
                    }
                }
                else
                {
                    // if the body is traviling down I need to add 1 in the y direction
                    velocity = new Vector2D(snake.dir);
                    velocity.X = velocity.X * snake.speed;
                    velocity.Y = velocity.Y * snake.speed;
                    Vector2D nextPointForHead = new Vector2D(body[body.Count - 1].X, body[body.Count - 1].Y);
                    nextPointForHead = nextPointForHead + velocity;
                    body.Remove(body[body.Count - 1]);
                    body.Add(nextPointForHead);

                    Vector2D directionForTailToTravle = TailDirection(body);
                    directionForTailToTravle.Y = directionForTailToTravle.Y * snake.speed;
                    directionForTailToTravle.X = directionForTailToTravle.X * snake.speed;
                    if(!(snake.growthBuffer > 0))
                    {
                        body[0] = body[0] + directionForTailToTravle; // move the tail the amount it needs to move closer to the mid section

                    }
                    if (tailFarEnough(body[0], body[1],directionForTailToTravle))
                    {
                        body.RemoveAt(0);
                    }
                }
                if(snake.growthBuffer > 0) { snake.growthBuffer--; }

            }
            movmentCommands.Clear();
        }
        /// <summary>
        /// detects if a snake head is close enough to a powerup if so return true
        /// </summary>
        /// <param name="snake"></param>
        /// <param name="serverWorld"></param>
        /// <returns></returns>
        private bool SnakeHitPowerup(Snake snake, World serverWorld)
        {

            Vector2D head = snake.body[snake.body.Count - 1];
            Vector2D pLocation;
            foreach(Powerup p in serverWorld.powerups.Values)
            {
                pLocation = p.loc;
                double x = Math.Pow((p.loc.X - head.X), 2);
                double y = Math.Pow((p.loc.Y - head.Y),2);
                double distance = Math.Sqrt(x + y);

                int distanceBetweenVector;
                if(distance < 15)
                {
                    p.died = true;
                    snake.score += 15;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// removes sections of the body that are no longer needed
        /// </summary>
        /// <param name="tail"></param>
        /// <param name="midPoint"></param>
        /// <param name="directionForTailToTravle"></param>
        /// <returns></returns>
        private bool tailFarEnough(Vector2D tail, Vector2D midPoint,Vector2D directionForTailToTravle)
        {
            if (tail.Equals(midPoint))
            {
                return true;
            }
            if (directionForTailToTravle.X > 0) // the tail is going right
            {
                if (tail.X > midPoint.X)
                {
                    return true;
                }
            }
            if (directionForTailToTravle.X < 0)// the tail is going to the left
            {
                if (tail.X < midPoint.X)
                {
                    return true;
                }
            }
            if (directionForTailToTravle.Y > 0) //  tail is going down
            {
                if (tail.Y > midPoint.Y)
                {
                    return true;
                }
            }
            if (directionForTailToTravle.Y < 0) // tail is going up
            {
                if (tail.Y < midPoint.Y)
                {
                    return true;
                }
            }
            return false;
        }
        private Vector2D TailDirection(List<Vector2D> body)
        {
            Vector2D directionForTailToTravle = new();
            directionForTailToTravle = body[1] - body[0];
            directionForTailToTravle.Normalize();
            return directionForTailToTravle;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverWorld"></param>
        /// <param name="settings"></param>
        public void ReplenishPowerups(World serverWorld,GameSettings settings)
        {
            foreach (Powerup powerup in serverWorld.powerups.Values)
            {
                if (powerup.died)
                {
                    powerup.died = false;
                    powerup.loc = GiveGoodPoint(settings);
                }
            }
        }


        /// <summary>
        ///  spwans the powerups
        /// </summary>
        /// <param name="powerupID"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Powerup SpawnPowerup(int powerupID,GameSettings settings)
        {
            Vector2D powerupLoc = GiveGoodPoint(settings);
            Powerup p = new(powerupID, powerupLoc, false);
            return p;
        }
 


        /// <summary>
        /// spwans snakes in a valid location
        /// </summary>
        /// <param name="snakeID"></param>
        /// <param name="snakeName"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Snake SpawnSnake(int snakeID, string snakeName,GameSettings settings)
        {
            List<Vector2D> body = new List<Vector2D>();
            Vector2D goodPoint = GiveGoodPoint(settings);
          
            Vector2D dir = new();
            int directionToStart = rand.Next(0, 4);
            if (directionToStart == 0)
            {
                body.Add(goodPoint);
                body.Add(new Vector2D(goodPoint.X + 120, goodPoint.Y)); // spawns moving right
                dir = new Vector2D(1, 0);
            }
            else if (directionToStart == 1)
            {
                body.Add(goodPoint);
                body.Add(new Vector2D(goodPoint.X - 120, goodPoint.Y)); // spawns moving left
                dir = new Vector2D(-1, 0);
            }
            else if (directionToStart == 2)
            {
                body.Add(goodPoint);
                body.Add(new Vector2D(goodPoint.X, goodPoint.Y - 120)); // spawns moving up
                dir = new Vector2D(0, -1);
            }
            else if (directionToStart == 3)
            {
                body.Add(goodPoint);
                body.Add(new Vector2D(goodPoint.X, goodPoint.Y + 120)); // spwans moving down
                dir = new Vector2D(0, 1);
            }
            Snake s = new Snake(snakeID, snakeName, body, dir, 15, false, true, false, true);
            s.deathtimer = settings.RespawnRate; // each snake will have an inital respawn rate
            return s;
        }

        /// <summary>
        /// gives a valid point for snakes and powerups to spawn
        /// </summary>
        /// <param name="gameSettings"></param>
        /// <returns></returns>
        public Vector2D GiveGoodPoint(GameSettings gameSettings)
        {
            int randIntX = rand.Next(-((gameSettings.UniverseSize / 2) - 150), ((gameSettings.UniverseSize / 2) - 150));
            int randIntY = rand.Next(-((gameSettings.UniverseSize / 2) - 150), ((gameSettings.UniverseSize / 2) - 150));
            bool badWall = false;
            for (int i = 0; i < gameSettings.Walls.Count; i++)
            {
                Vector2D p1 = gameSettings.Walls[i].p1;
                Vector2D p2 = gameSettings.Walls[i].p2;
                if (p1.X == p2.X)
                {
                    // the wall is up to down or down to up
                    if (p1.Y > p2.Y)
                    {
                        // the wall is traviling from up to down
                        if ((randIntX > p1.X + 170 || randIntX < p1.X - 170) || (randIntY < p1.Y - 170 || randIntY > p2.Y + 170))
                        {
                            // good in the x direction and y
                            continue;
                        }
                        else
                        {
                            badWall = true;
                        }
                    }
                    else
                    {
                        // the wall is traviling from down to up
                        if ((randIntX > p1.X + 170 || randIntX < p1.X - 170) || (randIntY > p1.Y + 170 || randIntY < p2.Y - 170))
                        {
                            // good in the x direction and y
                            continue;
                        }
                        else
                        {
                            badWall = true;
                        }
                    }
                }
                if (p1.Y == p2.Y)
                {
                    // the walls are traviling left to right or right to left
                    if (p1.X > p2.X)
                    {
                        // walls are traviling from right to left
                        if ((randIntY > p2.Y + 170 || randIntY < p2.Y - 170) || (randIntX > p1.X + 170 || randIntX < p2.X - 170))
                        {
                            // good in y direction and x
                            continue;
                        }
                        else
                        {
                            badWall = true;
                        }
                    }
                    else
                    {
                        // walls are traviling from left to right
                        if ((randIntY > p1.Y + 170 || randIntY < p1.Y - 170) || (randIntX < p1.X - 170 || randIntX > p2.X + 170))
                        {
                            // good in y and x
                            continue;
                        }
                        else
                        {
                            badWall = true;
                        }
                    }
                }
                if (badWall)
                {
                    i = -1;
                    randIntX = rand.Next(-((gameSettings.UniverseSize / 2) - 150), ((gameSettings.UniverseSize / 2) - 150));
                    randIntY = rand.Next(-((gameSettings.UniverseSize / 2) - 150), ((gameSettings.UniverseSize / 2) - 150));
                }
            }
            Vector2D ret = new Vector2D(randIntX, randIntY);
            return ret;
        }
        /// <summary>
        /// finds if a snake is hitting itself
        /// </summary>
        /// <param name="snake"></param>
        /// <returns></returns>
        public bool SelfCollisions(Snake snake)
        {
            Vector2D snakeDirection = snake.dir;
            Vector2D head = snake.body[snake.body.Count - 1];
            // loop over each point backwards throught the snake
            // AKA: from head to tail and find when the direction is oposit to the snake dir
            int count = snake.body.Count - 1;
            // if it is the head point we dont account for it so we will start from the head point -1 and go backwards throught the snke from there
            while (count > 0)
            {
                Vector2D p1 = snake.body[count - 1];
                count--;
                if(count == 0) { break; } // there is no "next" point so we are done
                Vector2D p2 = snake.body[count -1];
                Vector2D segDirection = p1 - p2;
                segDirection.Normalize();
                snakeDirection.Normalize();
                if (segDirection.IsOppositeCardinalDirection(snakeDirection))
                {
                    break;
                }
            }
            while (count > 0) // now that we know where the first segment is that is opposit to the snakes direction we can check collisions
            {
                Vector2D p1 = snake.body[count - 1];
                count--;
                if (count == 0)
                {
                    break;
                }
                Vector2D p2 = snake.body[count - 1];
                // check if the snake is going left to right, right to left, up to down or down to up
                // once we know the orientation we can create the width of the segment and check if the head is within the box
                if (p1.X == p2.X)
                {
                    // the segment goes from top to bottom or bottom to top
                    if (p1.Y > p2.Y)
                    {
                        // the segment goes from bottom to top
                        // check if the head is within the x area and y area
                        if (head.X < p1.X + 10 && head.X > p1.X - 10 && head.Y > p2.Y - 10 && head.Y < p1.Y + 10)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                    else
                    {
                        // top to bottom
                        if (head.X < p1.X + 10 && head.X > p1.X - 10 && head.Y < p2.Y + 10 && head.Y > p1.Y - 10)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                }
                if (p1.Y == p2.Y)
                {
                    // because of the way the segments
                    // are at this point p1 will be p2 and p2 will become p1
                    /*Vector2D temp = p1;
                    p1 = p2;
                    p2 = temp;*/
                    if (p1.X < p2.X)
                    {
                        // the segment goes from left to right
                        if (head.Y > p1.Y - 10 && head.Y < p1.Y + 10 && head.X > p1.X - 10 && head.X < p2.X + 10)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                    else
                    {
                        // right to left
                        if (head.Y > p1.Y - 10 && head.Y < p1.Y + 10 && head.X < p1.X + 10 && head.X > p2.X - 10)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// finds if a snake hit another snake
        /// </summary>
        /// <param name="serverWorld"></param>
        /// <param name="snake"></param>
        /// <returns></returns>
        internal bool SnakeCollisions(World serverWorld, Snake snake)
        {
            int count = 0;
            Vector2D head = snake.body[snake.body.Count - 1];
            foreach(Snake snakeToHit in serverWorld.snakes.Values)
            {
                if(snakeToHit.snake == snake.snake) { continue; } 
                // if the snake that we are checking collisions for is the same snake we will
                // check that in another method because of gottchas
                while(count < snakeToHit.body.Count)
                {
                    Vector2D p1 = snakeToHit.body[count];
                    count++;
                    if(count == snakeToHit.body.Count)
                    {
                        break;
                    }
                    Vector2D p2 = snakeToHit.body[count];
                    // check if the snake is going left to right, right to left, up to down or down to up
                    // once we know the orientation we can create the width of the segment and check if the head is within the box
                    if(p1.X == p2.X)
                    {
                        // the segment goes from top to bottom or bottom to top
                        if(p1.Y > p2.Y)
                        {
                            // the segment goes from bottom to top
                            // check if the head is within the x area and y area
                            if(head.X < p1.X + 10 && head.X > p1.X - 10 && head.Y > p2.Y - 10 && head.Y < p1.Y + 10)
                            {
                                // the snake has hit another snake... KILL IT!
                                return true;
                            }
                        }
                        else
                        {
                            // top to bottom
                            if (head.X < p1.X + 10 && head.X > p1.X - 10 && head.Y < p2.Y + 10 && head.Y > p1.Y - 10)
                            {
                                // the snake has hit another snake... KILL IT!
                                return true;
                            }
                        }
                    }
                    if(p1.Y == p2.Y) 
                    {
                        // because of the way the segments
                        // are at this point p1 will be p2 and p2 will become p1
                        /*Vector2D temp = p1;
                        p1 = p2;
                        p2 = temp;*/
                        if(p1.X < p2.X)
                        {
                            // the segment goes from left to right
                            if(head.Y > p1.Y - 10 && head.Y < p1.Y + 10 && head.X > p1.X - 10 && head.X < p2.X + 10)
                            {
                                // the snake has hit another snake... KILL IT!
                                return true;
                            }
                        }
                        else
                        {
                            // right to left
                            if (head.Y > p1.Y - 10 && head.Y < p1.Y + 10 && head.X < p1.X + 10 && head.X > p2.X - 10)
                            {
                                // the snake has hit another snake... KILL IT!
                                return true;
                            }
                        }
                    }
                        
                }
            }
            return false;
        }
        /// <summary>
        /// finds if a snake hit a wall
        /// </summary>
        /// <param name="snake"></param>
        /// <param name="gameSettings"></param>
        /// <returns></returns>
        internal bool WallCollision(Snake snake, GameSettings gameSettings)
        {
            Vector2D head = snake.body[snake.body.Count - 1];
            foreach (Wall wall in gameSettings.Walls)
            {
                Vector2D p1 = wall.p1;
                Vector2D p2 = wall.p2;

                if (p1.X == p2.X)
                {
                    // the segment goes from top to bottom or bottom to top
                    if (p1.Y > p2.Y)
                    {
                        // the segment goes from bottom to top
                        // check if the head is within the x area and y area
                        if (head.X < p1.X + 30 && head.X > p1.X - 30 && head.Y > p2.Y - 30 && head.Y < p1.Y + 30)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                    else
                    {
                        // top to bottom
                        if (head.X < p1.X + 30 && head.X > p1.X - 30 && head.Y < p2.Y + 30 && head.Y > p1.Y - 30)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                }
                if (p1.Y == p2.Y)
                {
                    // because of the way the segments
                    // are at this point p1 will be p2 and p2 will become p1
                    /*Vector2D temp = p1;
                    p1 = p2;
                    p2 = temp;*/
                    if (p1.X < p2.X)
                    {
                        // the segment goes from left to right
                        if (head.Y > p1.Y - 30 && head.Y < p1.Y + 30 && head.X > p1.X - 30 && head.X < p2.X + 30)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                    else
                    {
                        // right to left
                        if (head.Y > p1.Y - 30 && head.Y < p1.Y + 30 && head.X < p1.X + 30 && head.X > p2.X - 30)
                        {
                            // the snake has hit another snake... KILL IT!
                            return true;
                        }
                    }
                }
            }
            return false;
        }
 
        
        /// <summary>
        /// Helper method that checks if a snake is out of the universe boundaries.
        /// Method will also send a direction vectory
        /// </summary>
        /// <param name="snake"></param> snake to check if it is out of bounds.
        /// <returns></returns> Dictionary containing bool to see snake is out of bounds and
        /// dirction vector to help detect which way to wrap around.
        internal void SnakeWrapAround(Snake snake, GameSettings settings, World serverWorld)
        {
            // Vector2D representing the head of the snake.
            Vector2D snakeHead = snake.body[snake.body.Count - 1];
            Vector2D newPoint;
            int thisSnakesID = snake.snake;
            int thisSnakesScore = snake.score;
            string thisSnakeName = snake.name;
            Vector2D thisSnakeDir = snake.dir;
            // Checks if snake has gone beyond the + X of the universe
            if (snakeHead.X > settings.UniverseSize / 2)
            {
                newPoint = new Vector2D(snakeHead.X * -1, snakeHead.Y);
                serverWorld.snakes.Remove(thisSnakesID);
                List<Vector2D> newBody = new();
                newBody.Add(newPoint);
                newBody.Add(newPoint);
                serverWorld.snakes.Add(thisSnakesID, new Snake(thisSnakesID, thisSnakeName, newBody, thisSnakeDir, thisSnakesScore, false, true, false, false));
                serverWorld.snakes[thisSnakesID].growthBuffer = (thisSnakesScore / 15) * settings.GrowthBuffer;
            }
            // Checks if snake has gone beyond the - X of the universe.
            if (snakeHead.X < -(settings.UniverseSize / 2))
            {
                // add true and opposiste direction
                newPoint = new Vector2D(snakeHead.X * -1, snakeHead.Y);
                serverWorld.snakes.Remove(thisSnakesID);
                List<Vector2D> newBody = new();
                newBody.Add(newPoint);
                newBody.Add(newPoint);
                serverWorld.snakes.Add(thisSnakesID, new Snake(thisSnakesID, thisSnakeName, newBody, thisSnakeDir, thisSnakesScore, false, true, false, false));
                serverWorld.snakes[thisSnakesID].growthBuffer = (thisSnakesScore / 15) * settings.GrowthBuffer;
            }
            // Checks if snake has gone beyond the + Y of the universe
            if (snakeHead.Y > settings.UniverseSize / 2)
            {
                // add true and opposiste direction
                newPoint = new Vector2D(snakeHead.X, snakeHead.Y * -1);
                serverWorld.snakes.Remove(thisSnakesID);
                List<Vector2D> newBody = new();
                newBody.Add(newPoint);
                newBody.Add(newPoint);
                serverWorld.snakes.Add(thisSnakesID, new Snake(thisSnakesID, thisSnakeName, newBody, thisSnakeDir, thisSnakesScore, false, true, false, false));
                serverWorld.snakes[thisSnakesID].growthBuffer = (thisSnakesScore / 15) * settings.GrowthBuffer;

            }
            // Checks if snake has gone beyond the - Y of the universe
            if (snakeHead.Y < -(settings.UniverseSize / 2))
            {
                // add true and opposiste direction
                newPoint = new Vector2D(snakeHead.X, snakeHead.Y * -1);
                serverWorld.snakes.Remove(thisSnakesID);
                List<Vector2D> newBody = new();
                newBody.Add(newPoint);
                newBody.Add(newPoint);
                serverWorld.snakes.Add(thisSnakesID, new Snake(thisSnakesID, thisSnakeName, newBody, thisSnakeDir, thisSnakesScore, false, true, false, false));
                serverWorld.snakes[thisSnakesID].growthBuffer = (thisSnakesScore / 15) * settings.GrowthBuffer;
            }

        }
        /// <summary>
        /// if the snake dose a 180 too fast the input will be ignored
        /// </summary>
        /// <param name="serverWorld"></param>
        /// <param name="direction"></param>
        /// <param name="snakeID"></param>
        /// <returns></returns>
        internal bool TurnsTooFast(World serverWorld, Vector2D direction, int snakeID)
        {
            if (serverWorld.snakes[snakeID].body.Count < 3)
            {
                return false;
            }
            Snake snake = serverWorld.snakes[snakeID];
            if(direction.X > 0) // direction of disired travle is right
            {
                double currentSegmentLength = (snake.body[snake.body.Count - 1].Y) - (snake.body[snake.body.Count - 2].Y);
                Vector2D p1 = snake.body[snake.body.Count - 2];
                Vector2D p2 = snake.body[snake.body.Count - 3];
                
                Vector2D directionOfLastSegment = p1 - p2;
                directionOfLastSegment.Normalize();
                if (Math.Abs(currentSegmentLength) < 10 && direction.IsOppositeCardinalDirection(directionOfLastSegment)) 
                {
                    return true;
                }
            }
            else if (direction.X < 0) // direction of disired travle is left
            {
                double currentSegmentLength = (snake.body[snake.body.Count - 1].Y) - (snake.body[snake.body.Count - 2].Y);
                Vector2D p1 = snake.body[snake.body.Count - 2];
                Vector2D p2 = snake.body[snake.body.Count - 3];

                Vector2D directionOfLastSegment = p1 - p2;
                directionOfLastSegment.Normalize();
                if (Math.Abs(currentSegmentLength) < 10 && direction.IsOppositeCardinalDirection(directionOfLastSegment))
                {
                    return true;
                }
            }
            else if (direction.Y > 0) // direction of disired Travle is down
            {
                double currentSegmentLength = (snake.body[snake.body.Count - 1].X) - (snake.body[snake.body.Count - 2].X);
                Vector2D p1 = snake.body[snake.body.Count - 2];
                Vector2D p2 = snake.body[snake.body.Count - 3];

                Vector2D directionOfLastSegment = p1 - p2;
                directionOfLastSegment.Normalize();
                if (Math.Abs(currentSegmentLength) < 8 &&  direction.IsOppositeCardinalDirection(directionOfLastSegment))
                {
                    return true;
                }
            }
            else if(direction.Y < 0) // direciton of travle is up
            {
                double currentSegmentLength = (snake.body[snake.body.Count - 1].X) - (snake.body[snake.body.Count - 2].X);
                Vector2D p1 = snake.body[snake.body.Count - 2];
                Vector2D p2 = snake.body[snake.body.Count - 3];

                Vector2D directionOfLastSegment = p1 - p2;
                directionOfLastSegment.Normalize();
                if (Math.Abs(currentSegmentLength) < 10 && direction.IsOppositeCardinalDirection(directionOfLastSegment))
                {
                    return true;
                }
            }
            return false;
        }
    }
    
}
