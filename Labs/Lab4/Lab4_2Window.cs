using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab4
{
    public class Lab4_2Window : GameWindow
    {
        private int[] mVertexArrayObjectIDArray = new int[2];
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;

        private Matrix4 mSquareMatrix;

        private Vector3 mCirclePosition;
        private Vector3 mCircleVelocity;
        private float mCircleRadius;

        private Vector3 mCirclePosition2;
        private Vector3 mCircleVelocity2;
        private float mCircleRadius2;

        private Vector3 mCirclePosition3;
        private Vector3 mCircleVelocity3;
        private float mCircleRadius3;

        private Timer mTimer;

        private Vector3 accelerationDueToGravity = new Vector3(0, -9.81f, 0);

        public Lab4_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 4_2 Physically Based Simulation",
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
            GL.ClearColor(Color4.AliceBlue);

            mShader = new ShaderUtility(@"Lab4/Shaders/vLab4.vert", @"Lab4/Shaders/fLab4.frag");
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.UseProgram(mShader.ShaderProgramID);

            float[] vertices = new float[] { 
                   -1f, -1f,
                   1f, -1f,
                   1f, 1f,
                   -1f, 1f
            };

            GL.GenVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.GenBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);

            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            vertices = new float[200];

            for (int i = 0; i < 100; ++i)
            {
                vertices[2 * i] = (float)Math.Cos(MathHelper.DegreesToRadians(i * 360.0 / 100));
                vertices[2 * i + 1] = (float)Math.Cos(MathHelper.DegreesToRadians(90.0 + i * 360.0 / 100));
            }

            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[1]);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            Matrix4 m = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4(uViewLocation, true, ref m);

            mCircleRadius = 0.2f;
            mCirclePosition = new Vector3(-2, 2.3f, 0);
            mCircleVelocity = new Vector3(2.0f, 0, 0);

            mCircleRadius2 = 0.2f;
            mCirclePosition2 = new Vector3(2, 1.7f, 0);
            mCircleVelocity2 = new Vector3(0, 0, 0);

            mCircleRadius3 = 0.2f;
            mCirclePosition3 = new Vector3(0, 2, 0);
            mCircleVelocity3 = new Vector3(2.0f, 0, 0);

            mSquareMatrix = Matrix4.CreateScale(4f) * Matrix4.CreateRotationZ(0.0f) * Matrix4.CreateTranslation(0, 0, 0);

            base.OnLoad(e);

            mTimer = new Timer();
            mTimer.Start();
        }

        private void SetCamera()
        {
            float height = ClientRectangle.Height;
            float width = ClientRectangle.Width;
            if (mShader != null)
            {
                Matrix4 proj;
                if (height > width)
                {
                    if (width == 0)
                    {
                        width = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10, 10 * height / width, 0, 10);
                }
                else
                {
                    if (height == 0)
                    {
                        height = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10 * width / height, 10, 0, 10);
                }
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                GL.UniformMatrix4(uProjectionLocation, true, ref proj);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            SetCamera();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelMatrixLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uColour");

            GL.Uniform4(uColourLocation, Color4.DodgerBlue);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            Matrix4 circleMatrix = Matrix4.CreateScale(mCircleRadius) * Matrix4.CreateTranslation(mCirclePosition);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            GL.Uniform4(uColourLocation, Color4.Red);

            circleMatrix = Matrix4.CreateScale(mCircleRadius2) * Matrix4.CreateTranslation(mCirclePosition2);
            
            GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            GL.Uniform4(uColourLocation, Color4.Green);

            circleMatrix = Matrix4.CreateScale(mCircleRadius3) * Matrix4.CreateTranslation(mCirclePosition3);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            float timestep = mTimer.GetElapsedSeconds();

            Vector3 mOldCirclePosition = mCirclePosition;

            double distance = Math.Sqrt(((mCirclePosition.X - mCirclePosition2.X) * (mCirclePosition.X - mCirclePosition2.X)) + ((mCirclePosition.Y - mCirclePosition2.Y) * (mCirclePosition.Y - mCirclePosition2.Y)));

            //mCircleVelocity = mCircleVelocity + accelerationDueToGravity * timestep;
            //mCirclePosition = mCirclePosition + mCircleVelocity * timestep;

            //Circle Collision
            if (distance < mCircleRadius + mCircleRadius2 + 0.05f)
            {
                Vector3 temp = mCircleVelocity;
                mCircleVelocity = mCircleVelocity2;
                mCircleVelocity2 = temp;

                mCirclePosition = mOldCirclePosition;

                Vector3 normal = (mCirclePosition - mCirclePosition2).Normalized();
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }

            mCircleVelocity = mCircleVelocity + accelerationDueToGravity * timestep;
            mCirclePosition = mCirclePosition + mCircleVelocity * timestep;

            Vector3 circleInSquareSpace = Vector3.Transform(mCirclePosition, mSquareMatrix.Inverted());

            //Circle 1
            if (circleInSquareSpace.X + (mCircleRadius / mSquareMatrix.ExtractScale().X) > 1)
            {
                mCirclePosition = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(-1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }
            else if (circleInSquareSpace.X - (mCircleRadius / mSquareMatrix.ExtractScale().X) < -1)
            {
                mCirclePosition = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }
            else if (circleInSquareSpace.Y + (mCircleRadius / mSquareMatrix.ExtractScale().Y) > 1)
            {
                mCirclePosition = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(0, 1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }
            else if (circleInSquareSpace.Y - (mCircleRadius / mSquareMatrix.ExtractScale().Y) < -1)
            {
                mCirclePosition = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(0, -1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }

            //mCircleVelocity = mCircleVelocity + accelerationDueToGravity * timestep;
            //mCirclePosition = mCirclePosition + mCircleVelocity * timestep;

            //Circle 2
            mOldCirclePosition = mCirclePosition2;
            mCircleVelocity2 = mCircleVelocity2 + accelerationDueToGravity * timestep;
            mCirclePosition2 = mCirclePosition2 + mCircleVelocity2 * timestep;
            circleInSquareSpace = Vector3.Transform(mCirclePosition2, mSquareMatrix.Inverted());

            if (circleInSquareSpace.X + (mCircleRadius2 / mSquareMatrix.ExtractScale().X) > 1)
            {
                mCirclePosition2 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(-1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity2 = mCircleVelocity2 - 2 * Vector3.Dot(normal, mCircleVelocity2) * normal;
            }
            else if (circleInSquareSpace.X - (mCircleRadius2 / mSquareMatrix.ExtractScale().X) < -1)
            {
                mCirclePosition2 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity2 = mCircleVelocity2 - 2 * Vector3.Dot(normal, mCircleVelocity2) * normal;
            }
            else if (circleInSquareSpace.Y + (mCircleRadius2 / mSquareMatrix.ExtractScale().Y) > 1)
            {
                mCirclePosition2 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(0, 1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity2 = mCircleVelocity2 - 2 * Vector3.Dot(normal, mCircleVelocity2) * normal;
            }
            else if (circleInSquareSpace.Y - (mCircleRadius2 / mSquareMatrix.ExtractScale().Y) < -1)
            {
                mCirclePosition2 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(0, -1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity2 = mCircleVelocity2 - 2 * Vector3.Dot(normal, mCircleVelocity2) * normal;
            }

            //Circle 3
            mOldCirclePosition = mCirclePosition3;

            mCirclePosition3 = mCirclePosition3 + mCircleVelocity3 * timestep + accelerationDueToGravity * timestep * timestep;

            circleInSquareSpace = Vector3.Transform(mCirclePosition3, mSquareMatrix.Inverted());

            if (circleInSquareSpace.X + (mCircleRadius3 / mSquareMatrix.ExtractScale().X) > 1)
            {
                mCirclePosition3 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(-1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity3 = mCircleVelocity3 - 2 * Vector3.Dot(normal, mCircleVelocity3) * normal;
            }
            else if (circleInSquareSpace.X - (mCircleRadius3 / mSquareMatrix.ExtractScale().X) < -1)
            {
                mCirclePosition3 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity3 = mCircleVelocity3 - 2 * Vector3.Dot(normal, mCircleVelocity3) * normal;
            }
            else if (circleInSquareSpace.Y + (mCircleRadius3 / mSquareMatrix.ExtractScale().Y) > 1)
            {
                mCirclePosition3 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(0, 1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity3 = mCircleVelocity3 - 2 * Vector3.Dot(normal, mCircleVelocity3) * normal;
            }
            else if (circleInSquareSpace.Y - (mCircleRadius3 / mSquareMatrix.ExtractScale().Y) < -1)
            {
                mCirclePosition3 = mOldCirclePosition;

                Vector3 normal = Vector3.Transform(new Vector3(0, -1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity3 = mCircleVelocity3 - 2 * Vector3.Dot(normal, mCircleVelocity3) * normal;
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);
            GL.DeleteVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}