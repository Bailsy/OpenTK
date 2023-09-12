using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing.Imaging;

namespace Triangle
{
    public class Game: GameWindow
    {
        Shader shader;
        bool pressed = false;
        
        float[] vertices =
       {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, 0.5f, 0.0f
        };

        private int VBO; //vertex buffer object
        private int VAO; //vertex array object

        static int LoadTexture(string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(filename); //used to indicate in the cmd that the file can't be located
            }

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap(filename);
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            //disable mipmapping since we haven't uploaded them
            //newer video cards allow GL.GenerateMipmaps()
            bmp.UnlockBits(bmp_data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.LinearSharpenAlphaSgis);

            return id;
        }

        //We do this by overriding a base constructor included with OpenTK:
        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {

            base.OnLoad(e);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            VBO = GL.GenBuffer();
       
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            //creating and initialising the Vertex Array Object

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            //With this knowledge we can tell OpenGL how it should interpret the vertex data (per vertex attribute) using GL.VertexAttribPointer:
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            //helps to define an array of generic vertex attribute data
            GL.EnableVertexAttribArray(0);
            //GL.BufferData is a function specifically targeted to copy user-defined data into the currently-bound buffer. Its first argument is the type of the buffer we want to copy data into: the vertex buffer object currently bound to the BufferTarget.ArrayBuffer target. The second argument specifies the size of the data (in bytes) we want to pass to the buffer; a simple sizeof of the data type, multiplied by the length of the vertices, suffices. The third parameter is the actual data we want to send.
            shader = new Shader("shader.vert", "shader.frag");
            shader.use();

        }
        //Then, we have Context.SwapBuffers. Almost any modern OpenGL context is what's known as "double-buffered". Double-buffering means that there are two areas that OpenGL draws to. In essence: One area is displayed, while the other is being rendered to. Then, when you call SwapBuffers, the two are reversed. A single-buffered context could have issues such as screen tearing.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            //After drawing you call this specific function swap function which displays exactly what you have rendered on the screen
            //bind the shader
            shader.use();

            GL.BindVertexArray(VAO);
            // And then call our drawing function.
            // For this tutorial, we'll use GL.DrawArrays, which is a very simple rendering function.
            // Arguments:
            //   Primitive type; What sort of geometric primitive the vertices represent.
            //     OpenGL used to support many different primitive types, but almost all of the ones still supported
            //     is some variant of a triangle. Since we just want a single triangle, we use Triangles.
            //   Starting index; this is just the start of the data you want to draw. 0 here.
            //   How many vertices you want to draw. 3 for a triangle.
            GL.DrawArrays(PrimitiveType.LineLoop, 0 , 3);

            GL.DrawPixels(10, 10, OpenTK.Graphics.OpenGL.PixelFormat.ColorIndex, PixelType.Short, IntPtr.Zero );
            Context.SwapBuffers();
        }
        protected override void OnResize(EventArgs e)
        {
            //this line is needed in order to resize the background when the window is resized
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                shader.Dispose();
                Exit();
            }
            if (input.IsKeyDown(Key.F) && pressed == false)
            {
                Console.WriteLine(pressed);
                GL.ClearColor(0.56f, 0.88f, 1.00f, 1f);
                
                pressed = true;
            }
            if (input.IsKeyDown(Key.Space) && pressed == true)
            {
                Console.WriteLine(pressed);
                GL.ClearColor(0.21f, 0.6f, 0.24f, 1.0f);
                GL.End();

                pressed = false;
            }

            base.OnUpdateFrame(e);
        }

        private int LoadTexture()
        {
            throw new NotImplementedException();
        }



    }
    public class Shader 
    {
        public int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            int VertexShader;
            int FragmentShader; 

            string VertexShaderSource = File.ReadAllText(vertexPath);
            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            //here we will compile and log the success of compiling the shaders
            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success1);
            if(success1 == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int success2);
            //GL.GetShaderInfoLog is a function used specifically for the purpose of debugging.
            if (success2 == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(infoLog);
            }
            //Our individual shaders are compiled, but to actually use them, we have to link them together into a program that can be run on the GPU. This is what we mean when we talk about a "shader" from here on out. We do that like this

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success3);
            if(success3 == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.Write(infoLog);
            }

            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);


        }

        public void use()
        {
            GL.UseProgram(Handle);
        }


        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }

        ~Shader()
        {
            if(disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! What a numpty!");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    class Program
    {
         static void Main(string[] args)
        {
            using (Game game = new Game(800, 600, "LearnOpenTK"))
            {
                game.Run();
            }
        }
    }
}
