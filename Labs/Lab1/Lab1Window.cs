using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab1
{
    public class Lab1Window : GameWindow
    {
        //Used to store the BufferID
        private int[] mVertexBufferObjectIDArray = new int [2];
        private ShaderUtility mShader;

        public Lab1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 1 Hello, Triangle",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            //This wipes the old frame and creates a new frame and sets each pixel to green
            GL.ClearColor(Color4.Green);

            //This makes it so triangles/vertices that are facing the wrong way to no longer draw, this is due to the way
            //the vertices locations are defined, this means we are drawing anti-clockwise (changed to clockwise now)
            GL.Enable(EnableCap.CullFace);

            //This holds the locations of 3 vertexes with x and y coordinates onto an array
            float[] vertices = new float[] {  0.0f,  0.2f, //0
                                                0.0f, -0.6f, //1
                                                0.2f, -0.6f, //2
                                                0.2f, -0.2f, //3
                                                0.6f, -0.2f, //4
                                                0.6f,  0.2f, //5
                                                0.8f,  0.2f, //6
                                                0.4f,  0.6f, //7
                                                0.0f,  0.6f, //8
                                                0.0f,  0.8f, //9
                                                 -0.2f,  0.8f, //10
                                                 -0.2f,  0.6f, //11
                                                 -0.4f,  0.6f, //12
                                                 -0.8f,  0.2f, //13
                                                 -0.6f, -0.6f, //14
                                                 0.0f, -0.6f,//15
                                                    0.0f, -0.2f, //16
                                                -0.4f, -0.2f, //17
                                                -0.4f,  0.2f, //18
                                                -0.6f,  0.2f };//19



            //This is how TriangleStrip works, it always uses the last 3, so that each new vertex passed into
            //the pipeline shares the previous two that were passed in
            uint[] indices = new uint[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19};

            //GenBuffers generates a buffer ID which is stored in mVertexBufferObjectID, we need a bufferID so it can be
            //referred to later in the program
            GL.GenBuffers(2, mVertexBufferObjectIDArray);
            //This binds the buffer, this means that any active buffer and any buffer related commands that follow will refer
            //to this buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            //This loads the data, this allocates memory on the graphics card and copies the contents of vertices into that 
            //memory, the second parameter gets the length of our vertices and * it with the size of a float to get
            //the memory needed to open up memory on the graphics card
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            //This checks that the correct amount of data has been copied onto the graphics card, if the size of the data
            //on the card is not as expected then it throws an exception
            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            //This is the same as the code above but we are binding the second part of the vertices array to the buffer
            //So we can load and allocate memory properly
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (indices.Length * sizeof(uint) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            #region Shader Loading Code - Can be ignored for now

            mShader = new ShaderUtility( @"Lab1/Shaders/vSimple.vert", @"Lab1/Shaders/fSimple.frag");

            #endregion

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            //This clears the colour data from the back buffer and gives a blank white canvas to draw on, white is set
            //in the OnLoad method mentioned before, (the backBuffer can't be seen, known as double buffering, we draw
            //to a backBuffer first and once it's done it will swap with the front buffer)
            GL.Clear(ClearBufferMask.ColorBufferBit);
            //This binds the buffer that we want to draw, in this case the mVertexBufferObjectID that has been created
            //This is to switch buffers when having multiple things to draw, we only have one thing so it is already
            //binded
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);

            // shader linking goes here
            #region Shader linking code - can be ignored for now

            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            #endregion

            //This does all the drawing, it sends the vertices in the array in order to the graphics pipeline, from
            //0th element to the 3rd element. The primitivetype specifies that the vertices should be used to draw
            //triangles
            GL.DrawElements(PrimitiveType.TriangleFan, 14, DrawElementsType.UnsignedInt, 0);
            GL.DrawElements(PrimitiveType.TriangleFan, 20, DrawElementsType.UnsignedInt, 14); //What is the offset to draw 14-19?

            //Then we finally swap the back buffer and front buffer showing what has being rendered
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            //This method is called when the program exits, this is so all data we've allocated can be
            //removed from the graphics card
            base.OnUnload(e);
            //This deletes the buffer which is stored in the BufferObjectID created in the GenBuffer
            GL.DeleteBuffers(2, mVertexBufferObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
