using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using Microsoft.Maui.Controls;
//using Windows.UI.ViewManagement;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.UI.Xaml.Controls;
//using Windows.Graphics.Printing3D;
//using Microsoft.UI.Xaml;
//using Microsoft.Graphics.Canvas.Svg;

namespace SnakeGame;
public class WorldPanel: IDrawable
{
    private long deathAnimator;
    private World world;
    private IImage wall;
    private IImage background;
    public delegate void ObjectDrawer(object o, ICanvas canvas);
    private bool initializedForDrawing = false;

    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images";
        using (Stream stream = assembly.GetManifestResourceStream($"{path}.{name}"))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }

    public WorldPanel()
    {
      
    }
    /// <summary>
    /// this method is needed to get the world
    /// </summary>
    /// <param name="world"></param>
    /// <param name="graphicsView"></param>
    public void GiveWorld(World world, GraphicsView graphicsView) { this.world = world;}

    private void InitializeDrawing()
    {
        wall = loadImage("wallsprite.png");
        background = loadImage("background.png");
        initializedForDrawing = true;
    }
    /// <summary>
    /// responsible to draw all objects
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public async void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (world != null && world.dataTransfered)
        {
            if (!world.snakes.ContainsKey(world.playerID))
            {
                world.dataTransfered = false;
                return;
            }

       
            canvas.ResetState();// undo previous transformations from last frame
            Snake s = world.snakes[world.playerID];
            float playerX = (float)s.body[s.body.Count-1].GetX();
            float playerY = (float)s.body[s.body.Count-1].GetY();

            canvas.Translate(-playerX + (900 / 2), -playerY + (900 / 2));


            if (!initializedForDrawing)
                InitializeDrawing();

            canvas.DrawImage(background, (-world.size / 2), (-world.size / 2),world.size,world.size);

            lock (world)
            {
               
                // draw each snake
                foreach (Snake snakeToDraw in world.snakes.Values)
                {
                    if (!snakeToDraw.alive)
                    {
                        // if the snake is dead draw them bliking
                        canvas.StrokeColor = Colors.Black;
                        canvas.StrokeSize = 10;
                        deathAnimator++;
                        if (deathAnimator % 10 == 0)
                        {
                            continue;
                            deathAnimator = 0;
                        }
                    }
                    else
                    {
                        SetSnakeColor(snakeToDraw, canvas); // set the color based on the snakes ID
                    }

                    // find the segments that need to be drawn
                    List<SnakeSegment> segments = new List<SnakeSegment>();
                    SnakeSegment segToAdd;
                    for (int i = 0; i < snakeToDraw.body.Count - 1; i++)
                    {
                        segToAdd = new SnakeSegment(snakeToDraw.body[i], snakeToDraw.body[i + 1]);
                        segments.Add(segToAdd);
                    }
                    // now that we have every snake segment we need to calculate the segment direction to draw the segment
                    foreach (SnakeSegment segment in segments)
                    {
                        DrawObjectWithTransform(canvas, segment, 0, 0, 0, SnakeSegmentDrawer);
                    }


                }
              
                // draw each powerup
                foreach (Powerup powerup in world.powerups.Values)
                {
                    canvas.StrokeColor = Colors.Red;
                    DrawObjectWithTransform(canvas, powerup, powerup.loc.X, powerup.loc.Y, 0, PowerupDrawer);
                }
                // draw each wall
                foreach (Wall wallToDraw in world.walls.Values)
                {

                    if (wallToDraw.p1.X == wallToDraw.p2.X) // the walls go up and down
                    {
                        if (wallToDraw.p1.Y < wallToDraw.p2.Y) // the walls are drawn from top to bottom 
                        {
                            double wallLength = Math.Abs(wallToDraw.p1.Y - wallToDraw.p2.Y);
                            for (double i = 0; i <= wallLength; i += 50) 
                            {
                                // DrawObjectWithTransform(canvas, new Vector2D(wallToDraw.p1.X, wallToDraw.p1.Y + i), wallToDraw.p1.X, wallToDraw.p1.Y + i, 0, WallDrawer);
                                DrawObjectWithTransform(canvas, new Vector2D(wallToDraw.p1.X, wallToDraw.p1.Y + i), 0, 0, 0, WallDrawer);
                            }
                        }
                        if (wallToDraw.p1.Y > wallToDraw.p2.Y) // the walls are drawn from bottom to top
                        {
                            double wallLength = Math.Abs(wallToDraw.p2.Y - wallToDraw.p1.Y);
                            for (double i = 0; i <= wallLength; i += 50)
                            {
                                DrawObjectWithTransform(canvas, new Vector2D(wallToDraw.p2.X, wallToDraw.p1.Y - i), 0, 0, 0, WallDrawer);
                            }
                        }
                    }
                    if (wallToDraw.p1.Y == wallToDraw.p2.Y) // the walls go horizotaly
                    {
                        if (wallToDraw.p1.X > wallToDraw.p2.X) // the walls are drawn from right to left
                        {
                            double wallLength = Math.Abs(wallToDraw.p1.X - wallToDraw.p2.X);
                            for (double i = 0; i < wallLength + 50; i += 50)
                            {
                                DrawObjectWithTransform(canvas, new Vector2D(wallToDraw.p1.X - i, wallToDraw.p1.Y), 0, 0, 0, WallDrawer);
                            }
                        }
                        if (wallToDraw.p1.X < wallToDraw.p2.X) // drawn from left to right
                        {
                            double wallLength = Math.Abs(wallToDraw.p2.X - wallToDraw.p1.X); 
                            for (double i = 0; i < wallLength + 50; i += 50)
                            {
                                DrawObjectWithTransform(canvas, new Vector2D(wallToDraw.p1.X + i, wallToDraw.p1.Y), 0, 0, 0, WallDrawer);
                            }
                        }

                    }
                }
            }
        }
    }

    /// <summary>
    /// sets the color of the snake based on the ID of the snake.
    /// </summary>
    /// <param name="snakeToDraw"></param>
    /// <param name="canvas"></param>
    private void SetSnakeColor(Snake snakeToDraw,ICanvas canvas)
    {
        int colorID = snakeToDraw.snake % 8;
        if (colorID == 0)
        {
            canvas.StrokeColor = Colors.Red;
        }
        else if (colorID == 1)
        {
            canvas.StrokeColor = Colors.Green;
        }
        else if (colorID == 2)
        {
            canvas.StrokeColor = Colors.Blue;
        }
        else if (colorID == 3)
        {
            canvas.StrokeColor = Colors.Magenta;
        }
        else if (colorID == 4)
        {
            canvas.StrokeColor = Colors.Yellow;
        }
        else if (colorID == 5)
        {
            canvas.StrokeColor = Colors.Beige;
        }
        else if (colorID == 6)
        {
            canvas.StrokeColor = Colors.Cyan;
        }
        else if (colorID == 7)
        {
            canvas.StrokeColor = Colors.Brown;
        }
        canvas.StrokeSize = 10;
    }

    private void SnakeSegmentDrawer(object o, ICanvas canvas)
    {
        SnakeSegment snakeSegment = (SnakeSegment)o;
        int width = 10;
        canvas.DrawLine((float)snakeSegment.p1.X,(float)snakeSegment.p1.Y,(float)snakeSegment.p2.X,(float)snakeSegment.p2.Y);
        float radius = 0.4f;
        canvas.DrawCircle((float)snakeSegment.p1.X, (float)snakeSegment.p1.Y, radius);
        canvas.DrawCircle((float)snakeSegment.p2.X, (float)snakeSegment.p2.Y, radius);
    }
    private void PowerupDrawer(object o, ICanvas canvas)
    {
        Powerup p = o as Powerup;
        int width = 16;

        canvas.FillColor = Colors.DeepSkyBlue;

        // Ellipses are drawn starting from the top-left corner.
        // So if we want the circle centered on the powerup's location, we have to offset it
        // by half its size to the left (-width/2) and up (-height/2)
        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
    }
    private void WallDrawer(object o, ICanvas canvas)
    {
        Vector2D vec = o as Vector2D;
        canvas.DrawImage(wall,(float) vec.X - 25, (float)vec.Y - 25, 50, 50);
    }

    /// <summary>
    /// This method performs a translation and rotation to draw an object.
    /// </summary>
    /// <param name="canvas">The canvas object for drawing onto</param>
    /// <param name="o">The object to draw</param>
    /// <param name="worldX">The X component of the object's position in world space</param>
    /// <param name="worldY">The Y component of the object's position in world space</param>
    /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
    /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();

    }
    /// <summary>
    /// private helpler class to represent a snake segment
    /// </summary>
    private class SnakeSegment
    {
        public Vector2D p1;
        public Vector2D p2;
        public SnakeSegment(Vector2D p1, Vector2D p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}