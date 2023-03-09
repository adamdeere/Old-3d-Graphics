﻿using OpenTK;
using System;
using OpenTK.Graphics;
using Labs.Utility;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab2
{
    public class Lab2_2Window : GameWindow
    {
        public Lab2_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_2 Understanding the Camera",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[2];
        private int mVAO_ID;
        private ShaderUtility mShader;
        private ModelUtility mModel;
        private Matrix4 mView;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.DodgerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mModel = ModelUtility.LoadModel(@"Utility/Models/lab22model.sjg");    
            mShader = new ShaderUtility(@"Lab2/Shaders/vLab22.vert", @"Lab2/Shaders/fSimple.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");

            mView = Matrix4.CreateTranslation(0, 0, -2);
            MoveCamera();

            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 100, 500);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            mVAO_ID = GL.GenVertexArray();
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);
            
            GL.BindVertexArray(mVAO_ID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModel.Vertices.Length * sizeof(float)), mModel.Vertices, BufferUsageHint.StaticDraw);           
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModel.Indices.Length * sizeof(float)), mModel.Indices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);

            base.OnLoad(e);
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");

            Matrix4[] squareX = new Matrix4[10];
            Matrix4[] squareZ = new Matrix4[squareX.Length * 9];
            Matrix4[] squareY = new Matrix4[squareX.Length * squareZ.Length];
            for (int i = 0; i <= 9; i++)
            {
                //Square 1
                squareX[i] = Matrix4.CreateTranslation(0.5f * i, 0, 0);
                GL.UniformMatrix4(uModelLocation, true, ref squareX[i]);

                GL.BindVertexArray(mVAO_ID);
                GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
                
                for (int j = 0; j <= 9; j++)
                {
                    squareZ[j] = Matrix4.CreateTranslation(0.5f * i, 0, 0.5f * j);
                    GL.UniformMatrix4(uModelLocation, true, ref squareZ[j]);

                    GL.BindVertexArray(mVAO_ID);
                    GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
                    for (int k = 0; k <= 9; k++)
                    {
                        squareY[k] = Matrix4.CreateTranslation(0.5f * i, 0.5f * k, 0.5f * j);
                        GL.UniformMatrix4(uModelLocation, true, ref squareY[k]);

                        GL.BindVertexArray(mVAO_ID);
                        GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
                    }
                }
            }

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArray(mVAO_ID);
            mShader.Delete();
            base.OnUnload(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == 'w')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0, 0.05f);
                MoveCamera();
            }
            if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0, -0.05f);
                MoveCamera();
            }
            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateTranslation(0.05f, 0, 0);
                MoveCamera();
            }
            if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateTranslation(-0.05f, 0, 0);
                MoveCamera();
            }
            if (e.KeyChar == 'q')
            {
                mView = mView * Matrix4.CreateRotationY(0.05f);
                MoveCamera();
            }
            if (e.KeyChar == 'e')
            {
                mView = mView * Matrix4.CreateRotationY(-0.05f);
                MoveCamera();
            }
            if (e.KeyChar == 'r')
            {
                mView = mView * Matrix4.CreateTranslation(0, -0.05f, 0);
                MoveCamera();
            }
            if (e.KeyChar == 'f')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0.05f, 0);
                MoveCamera();
            }
        }

        private void MoveCamera()
        {
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);

            //When the viewport is resized
            if (mShader != null)
            {
                //We get the projection from the shader
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 50);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }
    }
}
