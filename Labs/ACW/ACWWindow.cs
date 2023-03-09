using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        //Shader Object
        private ShaderUtility mShader;

        //Integers to store handles for our VBOs and VAOs
        private int[] mVertexArrayObjectIDArray = new int[4];
        private int[] mVertexBufferObjectIDArray = new int[8];

        //This stores the cube vertices, which is 100cm cubed

        private float [] mCubeVertices = new float[] {-5, 10,  5,  0,  1,  0, 1, 1, //Top
                                                       5, 10,  5,  0,  1,  0, 0, 1,
                                                       5, 10, -5,  0,  1,  0, 0, 0,
                                                      -5, 10, -5,  0,  1,  0, 1, 0,
                                                      -5, 10,  5, -1,  0,  0, 1, 1, //Left
                                                      -5, 10, -5, -1,  0,  0, 0, 1,
                                                      -5,  0, -5, -1,  0,  0, 0, 0,
                                                      -5,  0,  5, -1,  0,  0, 1, 0,
                                                       5, 10,  5,  0,  0,  1, 1, 1, //Front
                                                      -5, 10,  5,  0,  0,  1, 0, 1,
                                                      -5,  0,  5,  0,  0,  1, 0, 0,
                                                       5,  0,  5,  0,  0,  1, 1, 0,
                                                       5, 10, -5,  1,  0,  0, 1, 1, //Right
                                                       5, 10,  5,  1,  0,  0, 0, 1,
                                                       5,  0,  5,  1,  0,  0, 0, 0,
                                                       5,  0, -5,  1,  0,  0, 1, 0,
                                                      -5, 10, -5,  0,  0, -1, 1, 1, //Back
                                                       5, 10, -5,  0,  0, -1, 0, 1,
                                                       5,  0, -5,  0,  0, -1, 0, 0,
                                                      -5,  0, -5,  0,  0, -1, 1, 0,
                                                      -5,  0, -5,  0, -1,  0, 1, 1, //Bottom
                                                       5,  0, -5,  0, -1,  0, 0, 1,
                                                       5,  0,  5,  0, -1,  0, 0, 0,
                                                      -5,  0,  5,  0, -1,  0, 1, 0
                                                       };

        //My cube indices for rendering with efficiency
        private int[] mCubeIndices = new int[] {0,  1,  2,  2,  3,  0,
                                                4,  5,  6,  6,  7,  4,
                                                8,  9,  10, 10, 11, 8,
                                                12, 13, 14, 14, 15, 12,
                                                16, 17, 18, 18, 19, 16, 
                                                20, 21, 22, 22, 23, 20};

        //Portal vertices
        private float[] mPortalVertices = new float[] {-5,  0, -5,  0, -1,  0, 1, 1,
                                                       5,  0, -5,  0, -1,  0, 0, 1,
                                                       5,  0,  5,  0, -1,  0, 0, 0,
                                                      -5,  0,  5,  0, -1,  0, 1, 0};

        //Triangle strip indices, used for rendering the object efficiency
        private int[] mPortalStripIndices = new int[] { 1, 2, 0, 3};

        //enum for camera states
        private enum mCameraMode
        {
            FixedCamera,
            UserControlled,
            SpecificPath,
            FollowObject
        };

        //Set default camera mode to user controlled
        private mCameraMode mCurCameraMode = mCameraMode.UserControlled;

        //enum for changing to different physic integrations
        private enum mPhysicIntegration
        {
            Euler,
            SymplecticEuler,
            VelocityVerlet,
            HitmanVerlet,
            Freeze
        }

        //Sets the default physics to Euler
        private mPhysicIntegration mCurPhysicIntegration = mPhysicIntegration.Euler;

        //For loading the cylinder and sphere models into
        private ModelUtility mCylinderUtility, mSphereUtility;

        //Matrix object for a view (camera), Matrix for our Cubes and portal
        private Matrix4 mView;
        private Matrix4 mEmitterBox;
        private Matrix4 mGridBoxL1;
        private Matrix4 mGridBoxL2;
        private Matrix4 mDoomBox, mDoomSphereModel;
        private Matrix4 mPortalBox;
        private Matrix4 mPortal;

        //For holding a list of sphere structures
        private List<mSphere> mSphereStorage = new List<mSphere>();
        
        //Sphere structure for holding sphere values
        private struct mSphere
        {
            public Matrix4 matrix;
            public Vector3 position;
            public bool colourRed;
            public float density;
            public Vector3 velocity;
            public float radius;

            public mSphere(Vector3 newPosition, bool newColourRed, Vector3 newVelocity, float newRadius)
            {
                position = newPosition;
                colourRed = newColourRed;
                velocity = newVelocity;
                radius = newRadius;

                matrix = Matrix4.CreateScale(radius) * Matrix4.CreateTranslation(position);

                if (colourRed) {
                    density = (0.014f * radius) * (0.014f * radius) * (0.014f * radius);
                }
                else {
                    density = (0.01f * radius) * (0.01f * radius) * (0.01f * radius);
                }

            }
        }

        //Seperated DoomSphere from other sphere arrays for simplication with loops
        private Vector3 mDoomSpherePosition = new Vector3(0, 16f, 0f);
        private float mDoomSphereRadius = 3.0f;

        //For holding a list of cylinder structures
        private List<mCylinder> mCylinderStorage = new List<mCylinder>();

        //Structure that holds the values of cylinders
        private struct mCylinder
        {
            public Matrix4 matrix;
            public Vector3 position;
            public float radius;
            public float length;
            public float matrixRotationX;
            public float matrixRotationY;

            public mCylinder(Vector3 newPosition, float newRadius, float newLength, float newMatrixRotationX, float newMatrixRotationY)
            {
                position = newPosition;
                radius = newRadius;
                length = newLength;
                matrixRotationX = newMatrixRotationX;
                matrixRotationY = newMatrixRotationY;

                matrix = Matrix4.CreateScale(radius, length, radius) * Matrix4.CreateRotationX(matrixRotationX) * Matrix4.CreateRotationY(matrixRotationY) * Matrix4.CreateTranslation(position);
            }
        }

        //Physics Vector for Gravity
        private Vector3 mAccelerationDueToGravity = new Vector3(0, -18.65f, 0);

        //Texture ID
        private int mTexture_ID0;

        //Timer object for animations
        private Timer mTimer;

        //Random object for Sphere spawns, plus total elapsed time and time interval so i can spawn balls every second
        private Random number = new Random();
        private float totalElapsedTime = 0;
        private int timeInterval = 1;

        //Bool for sphere tracking camera, it picks one follows that sphere until destroyed
        private bool targetSphereDestroyed = true;
        //For holding the target sphere choice
        int targetSphereChoice = 0;

        //For holding and changing the maximum amount of spheres allowed to be spawned by the emitter
        int maxNumOfSpheres = 1;

        public ACWWindow()
            : base(
                1728, // Width
                972, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
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
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            //This loads in different textures and sets the id of each
            LoadTexture(@"ACW/cubeTexture1.png", TextureUnit.Texture0, 0);
            LoadTexture(@"ACW/cubeTexture2.png", TextureUnit.Texture1, 1);
            LoadTexture(@"ACW/cubeTexture3.png", TextureUnit.Texture2, 2);
            LoadTexture(@"ACW/cubeTexture4.png", TextureUnit.Texture3, 3);
            LoadTexture(@"ACW/cubeTexture5.png", TextureUnit.Texture4, 4);
            LoadTexture(@"ACW/cubeTexture6.png", TextureUnit.Texture5, 5);

            //Loads in the shaders
            mShader = new ShaderUtility(@"ACW/Shaders/vLighting.vert", @"ACW/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");

            int vTexCoordsLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");

            //This uniforms different textures with the sampler in the shader
            int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler0");
            GL.Uniform1(uTextureSamplerLocation, 0);

            uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler1");
            GL.Uniform1(uTextureSamplerLocation, 1);

            uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler2");
            GL.Uniform1(uTextureSamplerLocation, 2);

            uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler3");
            GL.Uniform1(uTextureSamplerLocation, 3);

            uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler4");
            GL.Uniform1(uTextureSamplerLocation, 4);

            uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler5");
            GL.Uniform1(uTextureSamplerLocation, 5);

            GL.GenVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.GenBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);

            //Loads Cube
            LoadObject(0, 0, mCubeVertices.Length * sizeof(float), mCubeVertices, mCubeIndices.Length * sizeof(int), mCubeIndices);
            EnablePositionNormalLocation(vPositionLocation, vNormalLocation, 8);

            GL.EnableVertexAttribArray(vTexCoordsLocation);
            GL.VertexAttribPointer(vTexCoordsLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            //Loads Cylinder
            mCylinderUtility = ModelUtility.LoadModel(@"ACW/Models/cylinder.bin");

            LoadObject(1, 2, mCylinderUtility.Vertices.Length * sizeof(float), mCylinderUtility.Vertices, mCylinderUtility.Indices.Length * sizeof(int), mCylinderUtility.Indices);
            EnablePositionNormalLocation(vPositionLocation, vNormalLocation, 6);

            //Loads Sphere
            mSphereUtility = ModelUtility.LoadModel(@"ACW/Models/sphere.bin");

            LoadObject(2, 4, mSphereUtility.Vertices.Length * sizeof(float), mSphereUtility.Vertices, mSphereUtility.Indices.Length * sizeof(int), mSphereUtility.Indices);
            EnablePositionNormalLocation(vPositionLocation, vNormalLocation, 6);

            mView = Matrix4.CreateTranslation(0f, -26f, -55f);
            MoveCamera();

            //Loads Portal
            LoadObject(3, 6, mPortalVertices.Length * sizeof(float), mPortalVertices, mPortalStripIndices.Length * sizeof(int), mPortalStripIndices);
            EnablePositionNormalLocation(vPositionLocation, vNormalLocation, 8);

            GL.EnableVertexAttribArray(vTexCoordsLocation);
            GL.VertexAttribPointer(vTexCoordsLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            //Just assigns and sets position values

            mEmitterBox = Matrix4.CreateTranslation(0, 41, 0);

            mGridBoxL1 = Matrix4.CreateTranslation(0, 31, 0);
            
            mCylinderStorage.Add(new mCylinder(new Vector3(0, 38.5f, 0), 0.75f, 5f, 1.57f, 0.0f));
            mCylinderStorage.Add(new mCylinder(new Vector3(0, 38.5f, 0), 0.75f, 5f, 1.57f, -1.57f));
            mCylinderStorage.Add(new mCylinder(new Vector3(0, 33.5f, 0), 1.50f, 5f, 1.57f, 0.0f));
            mCylinderStorage.Add(new mCylinder(new Vector3(0, 33.5f, 0), 1.50f, 5f, 1.57f, -1.57f));

            mGridBoxL2 = Matrix4.CreateTranslation(0, 21, 0);
            mCylinderStorage.Add(new mCylinder(new Vector3(0, 28.5f, 0), 1.50f, 5f, 1.57f, 0.785f));
            mCylinderStorage.Add(new mCylinder(new Vector3(0, 23.5f, 0), 1.50f, 5f, 1.57f, -0.785f));

            mDoomBox = Matrix4.CreateTranslation(0, 11, 0);

            mPortalBox = Matrix4.CreateTranslation(0, 1, 0);

            mPortal = Matrix4.CreateTranslation(0, 2, 0);

            timeInterval = 1;

            //Create lights 1, 2 with properties (Both just point lights)
            CreateLight("0", new Vector4(-2.5f, 51f, -2f, 1), new Vector3(0.2f, 0.2f, 0.2f), 
                                                               new Vector3(0.2f, 0.2f, 0.2f), 
                                                               new Vector3(0.2f, 0.2f, 0.2f));

            CreateLight("1", new Vector4(4.5f, 17f, 4f, 1), new Vector3(0.2f, 0.2f, 0.2f),
                                                            new Vector3(0.2f, 0.2f, 0.2f),
                                                            new Vector3(0.2f, 0.2f, 0.2f));

            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(0, -4, 0, 1), mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);

            GL.BindVertexArray(0);

            base.OnLoad(e);

            mTimer = new Timer();
            mTimer.Start();
        }

        //This loads a texture
        private void LoadTexture(string filepath, TextureUnit textureUnit, int GenTextureID)
        {
            if (System.IO.File.Exists(filepath))
            {
                Bitmap TextureBitmap = new Bitmap(filepath);

                TextureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                BitmapData TextureData = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height),
                                                                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                                    System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                GL.ActiveTexture(textureUnit);
                GL.GenTextures(GenTextureID, out mTexture_ID0);
                GL.BindTexture(TextureTarget.Texture2D, mTexture_ID0);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height,
                                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                TextureBitmap.UnlockBits(TextureData);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }
        }

        //This loads an object
        private void LoadObject(int objectIDAr, int objectIDBu, int vertCount, float[] vertData, int indCount, int[] indData)
        {
            GL.BindVertexArray(mVertexArrayObjectIDArray[objectIDAr]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[objectIDBu]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertCount), vertData, BufferUsageHint.StaticDraw);

            //Checks that the correct amount of data has been copied onto the graphics card
            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertCount != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[++objectIDBu]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indCount), indData, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indCount != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }
        }

        //When creating objects this enables position and normal locations
        private void EnablePositionNormalLocation(int vPositionLocation, int vNormalLocation, int strideSize)
        {
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, strideSize * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, strideSize * sizeof(float), 3 * sizeof(float));
        }

        //Method for creating the lights in the onload
        private void CreateLight(string lightNumber, Vector4 position, Vector3 ambient, Vector3 diffuse, Vector3 specular)
        {
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + lightNumber + "].Position");
            Vector4 lightPosition = new Vector4(position);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + lightNumber + "].AmbientLight");
            Vector3 colour = new Vector3(ambient);
            GL.Uniform3(uAmbientLightLocation, colour);

            int uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + lightNumber + "].DiffuseLight");
            colour = new Vector3(diffuse);
            GL.Uniform3(uDiffuseLightLocation, colour);

            int uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + lightNumber + "].SpecularLight");
            colour = new Vector3(specular);
            GL.Uniform3(uSpecularLightLocation, colour);
        }

        //Used for mView (camera) movements
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            //This activates the fixed camera
            if (e.KeyChar == '1')
                mCurCameraMode = mCameraMode.FixedCamera;

            //Activates use controlled camera, also resets view to a certain position
            else if (e.KeyChar == '2')
            {
                mCurCameraMode = mCameraMode.UserControlled;
                mView = Matrix4.CreateTranslation(0f, -26f, -55f);
                MoveCamera();
            }

            //Actives specific path camera (Just fixes the camera and rotates everything
            else if (e.KeyChar == '3')
            {
                mCurCameraMode = mCameraMode.SpecificPath;
                mView = Matrix4.CreateTranslation(0f, -26f, -55f);
                MoveCamera();
            }

            //Actives the follow object camera, which follows one of the circles
            else if (e.KeyChar == '4')
                mCurCameraMode = mCameraMode.FollowObject;

            //This changes the physic integration for the sphere velocity
            else if (e.KeyChar == '5')
                mCurPhysicIntegration = mPhysicIntegration.Euler;

            else if (e.KeyChar == '6')
                mCurPhysicIntegration = mPhysicIntegration.SymplecticEuler;

            else if (e.KeyChar == '7')
                mCurPhysicIntegration = mPhysicIntegration.VelocityVerlet;

            else if (e.KeyChar == '8')
                mCurPhysicIntegration = mPhysicIntegration.HitmanVerlet;

            else if (e.KeyChar == '9')
                mCurPhysicIntegration = mPhysicIntegration.Freeze;

            //This adds a sphere
            else if (e.KeyChar == '-')
            {
                //Need to do this to re-sync as adding spheres will break the code
                totalElapsedTime = timeInterval;
                addSphere(80);  //Red Sphere
            }
            else if (e.KeyChar == '=')
            {
                totalElapsedTime = timeInterval;
                addSphere(30);  //Green Sphere
            }

            //Increase and decrease maximum spheres possible
            else if (e.KeyChar == '[')
            {
                //Need to do this due to when the max spheres are already in the simulation
                //they both become out of sync and will never work
                totalElapsedTime = timeInterval;
                maxNumOfSpheres -= 12;
            }
            else if (e.KeyChar == ']')
            {
                totalElapsedTime = timeInterval;
                maxNumOfSpheres += 12;
            }

            //This applies certain camera modes
            if (mCurCameraMode == mCameraMode.FixedCamera)
            {
                CreateCameraTranslation(-16f, -54f, -31f);
                CreateCameraRotation(0.6f, -0.45f, 0.1f);
            }
            //User controlled camera
            else if (mCurCameraMode == mCameraMode.UserControlled)
            {
                if (e.KeyChar == 'w')
                    CreateCameraTranslation(0.0f, 0.0f, 0.75f);

                else if (e.KeyChar == 's')
                    CreateCameraTranslation(0.0f, 0.0f, -0.75f);

                else if (e.KeyChar == 'a')
                    CreateCameraTranslation(0.75f, 0.0f, 0.0f);

                else if (e.KeyChar == 'd')
                    CreateCameraTranslation(-0.75f, 0.0f, 0.0f);

                else if (e.KeyChar == 'q')
                    CreateCameraRotation(0, -0.075f, 0);

                else if (e.KeyChar == 'e')
                    CreateCameraRotation(0, 0.075f, 0);

                else if (e.KeyChar == 'r')
                    CreateCameraTranslation(0.0f, -0.5f, 0.0f);

                else if (e.KeyChar == 'f')
                    CreateCameraTranslation(0.0f, 0.5f, 0.0f);

                else if (e.KeyChar == 'c')
                    RotateCameraAroundCentre(-0.025f);

                else if (e.KeyChar == 'v')
                    RotateCameraAroundCentre(0.025f);
            }
        }

        //Method to rotate the camera around the centre, used by different camera settings
        private void RotateCameraAroundCentre(float value)
        {
            Vector3 t = mView.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            mView = mView * inverseTranslation * Matrix4.CreateRotationY(value) * translation;
            MoveCamera();
        }

        //Creates camera translation (different if the camera is fixed or following an object, then it updates with that instead)
        private void CreateCameraTranslation(float x, float y, float z)
        {
            if (mCurCameraMode == mCameraMode.FixedCamera || mCurCameraMode == mCameraMode.FollowObject)
                mView = Matrix4.CreateTranslation(x, y, z);
            else
                mView = mView * Matrix4.CreateTranslation(x, y, z);
            
            MoveCamera();
        }

        //Creates camera rotation
        private void CreateCameraRotation(float x, float y, float z)
        {
            mView = mView * Matrix4.CreateRotationX(x);
            mView = mView * Matrix4.CreateRotationY(y);
            mView = mView * Matrix4.CreateRotationZ(z);
            MoveCamera();
        }

        //Used to confirm camera changes with the shader
        private void MoveCamera()
        {
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            Vector4 lightPosition = new Vector4(-2.5f, 51f, -2f, 1);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            lightPosition = new Vector4(4.5f, 17f, 4f, 1);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(0, -4, 0, 1), mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);
        }

        //Used for when the window is resized
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);

            //When viewport is resized
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 100);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        bool moveCamera = true;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
 	        base.OnUpdateFrame(e);

            //Camera animations that follow a specific path
            if (mCurCameraMode == mCameraMode.SpecificPath)
            {
                RotateCameraAroundCentre(-0.0125f);
                Vector3 t = mView.ExtractTranslation();

                if (moveCamera)
                {
                    CreateCameraTranslation(0.0f, -0.1f, 0.0f);
                    t = mView.ExtractTranslation();
                    if (t.Y <= -41)
                        moveCamera = false;
                }
                else if (moveCamera == false)
                {
                    CreateCameraTranslation(0.0f, 0.1f, 0.0f);
                    t = mView.ExtractTranslation();
                    if (t.Y >= 0)
                        moveCamera = true;
                }
            }
            //Camera follows a sphere
            if (mCurCameraMode == mCameraMode.FollowObject)
            {
                if (mSphereStorage.Count > 0)
                {
                    if (targetSphereDestroyed)
                    {
                        targetSphereDestroyed = false;
                        targetSphereChoice = number.Next(0, mSphereStorage.Count-1);
                    }
                    Vector3 translation = mSphereStorage[targetSphereChoice].matrix.ExtractTranslation();
                    CreateCameraTranslation(-translation.X, -translation.Y, -translation.Z + -35);
                }
            }

            //Physic functions
            float timestep = mTimer.GetElapsedSeconds();

            //Adds current timestep to totalElapsedTime
            totalElapsedTime += timestep;

            //For storing old sphere positions
            List<Vector3> spherePositionOld = new List<Vector3>();

            //Adds a sphere
            int random = number.Next(1, 201); // Number between 1 and 200

            //If 1 second has passed then this is true and there's a chance another sphere will be added
            if ((int)totalElapsedTime == timeInterval)
            {
                addSphere(random);
            }

            if (mSphereStorage.Count >= 1)
            {
                for (int i = 0; i < mSphereStorage.Count; i++)
                {
                    spherePositionOld.Add(mSphereStorage[i].position);
                }

                //Sphere on Sphere Collision
                for (int i = 0; i < mSphereStorage.Count; i++)
                {
                    for (int j = 0; j < mSphereStorage.Count; j++)
                    {
                        //So it doesn't check a collision with itself
                        if (i != j)
                        {
                            if ((mSphereStorage[i].position - mSphereStorage[j].position).Length < mSphereStorage[i].radius + mSphereStorage[j].radius)
                            {
                                
                                //Changes spheres to the old position and then stores the new positions in the place of old position for other calculations
                                mSphereStorage[i] = new mSphere(spherePositionOld[i], mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);
                                mSphereStorage[j] = new mSphere(spherePositionOld[j], mSphereStorage[j].colourRed, mSphereStorage[j].velocity, mSphereStorage[j].radius);

                                //Set coefficient
                                float coefficient = 0.9f;

                                //Create the spheres accurate volumes based on radius
                                float sphereVolumeOne = ((4/3) * (float)Math.PI) * (float)Math.Pow((double)mSphereStorage[i].radius, 3);
                                float sphereVolumeTwo = ((4/3) * (float)Math.PI) * (float)Math.Pow((double)mSphereStorage[i].radius, 3);

                                //Create the spheres accurate mass based on the volume and the density
                                float sphereMassOne = sphereVolumeOne * mSphereStorage[i].density;
                                float sphereMassTwo = sphereVolumeTwo * mSphereStorage[j].density;

                                //Stores the velocity of both spheres
                                Vector3 sphereVelocityOne = mSphereStorage[i].velocity;
                                Vector3 sphereVelocityTwo = mSphereStorage[j].velocity;

                                //Create the normal
                                Vector3 normal = (mSphereStorage[j].position - mSphereStorage[i].position).Normalized();

                                //Creates the parralel velocity based on the spheres stored velocity
                                Vector3 parralelVelocityOne = Vector3.Dot(normal, sphereVelocityOne) * normal;
                                Vector3 parralelVelocityTwo = Vector3.Dot(normal, sphereVelocityTwo) * normal;

                                //Gets the perpendicular velocity of both spheres
                                Vector3 perpendicularVelocityOne = sphereVelocityOne - parralelVelocityOne;
                                Vector3 perpendicularVelocityTwo = sphereVelocityTwo - parralelVelocityTwo;

                                //Swaps the velocitys of both with coefficient included
                                Vector3 parVelFromMomentumOne = ((sphereMassOne * parralelVelocityOne) + (sphereMassTwo * parralelVelocityTwo) + ((coefficient * sphereMassTwo) * (parralelVelocityTwo - parralelVelocityOne))) / (sphereMassOne + sphereMassTwo);

                                Vector3 parVelFromMomentumTwo = ((sphereMassTwo * parralelVelocityTwo) + (sphereMassOne * parralelVelocityOne) + ((coefficient * sphereMassOne) * (parralelVelocityOne - parralelVelocityTwo))) / (sphereMassOne + sphereMassTwo);

                                //Adds the swapped velocity of the spheres plus the perpendicular one
                                mSphereStorage[i] = new mSphere(mSphereStorage[i].position, mSphereStorage[i].colourRed, (parVelFromMomentumOne) + perpendicularVelocityOne, mSphereStorage[i].radius);
                                mSphereStorage[j] = new mSphere(mSphereStorage[j].position, mSphereStorage[j].colourRed, (parVelFromMomentumTwo) + perpendicularVelocityTwo, mSphereStorage[j].radius);

                                //Gets the collision direction of the spheres
                                Vector3 colDirection = (mSphereStorage[j].position - mSphereStorage[i].position);
                                //Stores the length of that collision direction as a single value
                                float distance = colDirection.Length;

                                //Creates an offset for both spheres so they don't merge after update
                                float offset = Math.Abs(distance - (mSphereStorage[i].radius + mSphereStorage[j].radius));

                                //Updates position of the spheres with the normal and offset
                                mSphereStorage[i] = new mSphere(mSphereStorage[i].position - normal * offset, mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);
                                mSphereStorage[j] = new mSphere(mSphereStorage[j].position + normal * offset, mSphereStorage[j].colourRed, mSphereStorage[j].velocity, mSphereStorage[j].radius);
                            }
                        }
                    }
                }

                //Sphere Velocity Integration
                for (int i = 0; i < mSphereStorage.Count; i++)
                {
                    if (mCurPhysicIntegration == mPhysicIntegration.Euler)
                    {
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position + mSphereStorage[i].velocity * timestep, mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position, mSphereStorage[i].colourRed, mSphereStorage[i].velocity + mAccelerationDueToGravity * timestep, mSphereStorage[i].radius);
                    }
                    else if (mCurPhysicIntegration == mPhysicIntegration.SymplecticEuler)
                    {
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position, mSphereStorage[i].colourRed, mSphereStorage[i].velocity + mAccelerationDueToGravity * timestep, mSphereStorage[i].radius);
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position + mSphereStorage[i].velocity * timestep, mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);
                    }
                    else if (mCurPhysicIntegration == mPhysicIntegration.VelocityVerlet)
                    {
                        Vector3 oldVelocity = mSphereStorage[i].velocity;
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position, mSphereStorage[i].colourRed, mSphereStorage[i].velocity + mAccelerationDueToGravity * timestep, mSphereStorage[i].radius);
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position + (oldVelocity + mSphereStorage[i].velocity) * 0.5f * timestep, mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);
                    }
                    else if (mCurPhysicIntegration == mPhysicIntegration.HitmanVerlet)
                    {
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position + mSphereStorage[i].velocity * timestep + mAccelerationDueToGravity * timestep * timestep, mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);
                    }
                    else if (mCurPhysicIntegration == mPhysicIntegration.Freeze)
                    {
                        //Empty so everything stops moving (Freeze)
                    }
                }

                //Sphere on Cube Collision (In Cube Space)
                for (int i = 0; i < mSphereStorage.Count; i++)
                {
                    Vector3 sphereInCubeSpace = Vector3.Transform(mSphereStorage[i].position, mEmitterBox.Inverted());

                    //Checks for collisions of the spheres with the box on all axis' and then makes the neccessary outcomes happen
                    //Also when I do the portals
                    if (sphereInCubeSpace.X + (mSphereStorage[i].radius / mEmitterBox.ExtractScale().X) > 4.9 && sphereInCubeSpace.Y + (mSphereStorage[i].radius / mEmitterBox.ExtractScale().Y) > 0.0 && mSphereStorage[i].velocity.X > 0)
                    {
                        mSphereStorage[i] = new mSphere(new Vector3(mSphereStorage[i].position.X - 5, mSphereStorage[i].position.Y - 40, mSphereStorage[i].position.Z), mSphereStorage[i].colourRed, new Vector3(0, mSphereStorage[i].velocity.X, 0), mSphereStorage[i].radius);
                    }
                    else if (sphereInCubeSpace.X + (mSphereStorage[i].radius / mEmitterBox.ExtractScale().X) > 5.0 && sphereInCubeSpace.Y + (mSphereStorage[i].radius / mEmitterBox.ExtractScale().Y) < 0.0)
                    {
                        collisionBounce(i, spherePositionOld[i], new Vector3(-1, 0, 0));
                    }
                    else if (sphereInCubeSpace.X - (mSphereStorage[i].radius / mEmitterBox.ExtractScale().X) < -5.0)
                    {
                        collisionBounce(i, spherePositionOld[i], new Vector3(1, 0, 0));
                    }
                    else if (sphereInCubeSpace.Y + (mSphereStorage[i].radius / mEmitterBox.ExtractScale().Y) > 10.0)
                    {
                        collisionBounce(i, spherePositionOld[i], new Vector3(0, -1, 0));
                    }
                    else if (sphereInCubeSpace.Y - (mSphereStorage[i].radius / mEmitterBox.ExtractScale().Y) < -39.0)
                    {
                        mSphereStorage[i] = new mSphere(new Vector3(4, 46 + mSphereStorage[i].position.X, mSphereStorage[i].position.Z), mSphereStorage[i].colourRed, new Vector3(mSphereStorage[i].velocity.Y, 0, 0), mSphereStorage[i].radius);
                    }
                    else if (sphereInCubeSpace.Z + (mSphereStorage[i].radius / mEmitterBox.ExtractScale().Z) > 5.0)
                    {
                        collisionBounce(i, spherePositionOld[i], new Vector3(0, 0, -1));
                    }
                    else if (sphereInCubeSpace.Z - (mSphereStorage[i].radius / mEmitterBox.ExtractScale().Z) < -5.0)
                    {
                        collisionBounce(i, spherePositionOld[i], new Vector3(0, 0, 1));
                    }
                }

                //Sphere on cylinder collision
                for (int i = 0; i < mSphereStorage.Count; i++)
                {

                    for (int j = 0; j < mCylinderStorage.Count; j++)
                    {
                        Vector3 L1 = new Vector3(mCylinderStorage[j].position.X, mCylinderStorage[j].position.Y, mCylinderStorage[j].position.Z);
                        Vector3 L2 = new Vector3(mCylinderStorage[j].position.X, mCylinderStorage[j].position.Y, mCylinderStorage[j].position.Z);
                        if (j == 0 || j == 2)
                        {
                            L1.X -= mCylinderStorage[j].length;
                            L2.X += mCylinderStorage[j].length;
                        }
                        else if (j == 1 || j == 3)
                        {
                            L1.Z -= mCylinderStorage[j].length;
                            L2.Z += mCylinderStorage[j].length;
                        }
                        else if (j == 4)
                        {
                            L1.X -= mCylinderStorage[j].length;
                            L1.Z -= mCylinderStorage[j].length;
                            L2.X += mCylinderStorage[j].length;
                            L2.Z += mCylinderStorage[j].length;
                        }
                        else if (j == 5)
                        {
                            L1.X -= mCylinderStorage[j].length;
                            L1.Z += mCylinderStorage[j].length;
                            L2.X += mCylinderStorage[j].length;
                            L2.Z -= mCylinderStorage[j].length;
                        }

                        Vector3 A = Vector3.Dot((mSphereStorage[i].position - L2), (L1 - L2).Normalized()) * (L1 - L2).Normalized();

                        if (((L2 + A) - mSphereStorage[i].position).Length < (mSphereStorage[i].radius + mCylinderStorage[j].radius))
                        {
                            mSphereStorage[i] = new mSphere(spherePositionOld[i], mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);

                            float coefficient = 0.7f;

                            Vector3 normal = (mSphereStorage[i].position - (L2 + A)).Normalized();
                            mSphereStorage[i] = new mSphere(mSphereStorage[i].position, mSphereStorage[i].colourRed, mSphereStorage[i].velocity - (1 + coefficient) * Vector3.Dot(normal, mSphereStorage[i].velocity) * normal, mSphereStorage[i].radius);
                        }
                    }
                }

                //Sphere on DoomBall Collision
                for (int i = 0; i < mSphereStorage.Count; i++)
                {
                    //Pythagoras theoram for collision detection
                    if ((mSphereStorage[i].position - mDoomSpherePosition).Length < mSphereStorage[i].radius + mDoomSphereRadius)
                    {
                        //Apply old position so it doesn't glitch out and merge
                        mSphereStorage[i] = new mSphere(spherePositionOld[i], mSphereStorage[i].colourRed, mSphereStorage[i].velocity, mSphereStorage[i].radius);

                        //Get the normal
                        Vector3 normal = (mSphereStorage[i].position - mDoomSpherePosition).Normalized();
                        //Change the velocity
                        Vector3 velocity = new Vector3(0, -5, 0);
                        mSphereStorage[i] = new mSphere(mSphereStorage[i].position, mSphereStorage[i].colourRed, velocity - Vector3.Dot(normal, velocity) * normal, mSphereStorage[i].radius - 0.05f);

                        //Remove the ball if the radius gets below a certain radius
                        if (mSphereStorage[i].radius < 0.0001f || mSphereStorage[i].radius < -0.001f)
                            mSphereStorage.RemoveAt(i);
                    }
                }
            }
        }

        private void addSphere(int random)
        {
            if (mSphereStorage.Count < maxNumOfSpheres)
            {
                //One second has passed so add another second to the test
                timeInterval += 1;

                //Random co-ordinates between 1 and 8
                float randomX = number.Next(1, 9);
                float randomY = number.Next(1, 9);
                float randomZ = number.Next(1, 9);

                //Add a double cast to a float between 0.0 and 1.0, more random locations in the Emitter
                //Also adds coordinates for the back bottom left corner of the emitter box so it covers whole area
                randomX += (float)number.NextDouble() + -5;
                randomY += (float)number.NextDouble() + 41f;
                randomZ += (float)number.NextDouble() + -5f;

                if (random >= 51 && random <= 100) // Between 51-100 then it spawns a red ball
                    mSphereStorage.Add(new mSphere(new Vector3(randomX, randomY, randomZ), true, new Vector3(0, 0, 0), 0.6f));
                else if (random >= 1 && random <= 50) // Between 1-50 then it spawns a green ball
                    mSphereStorage.Add(new mSphere(new Vector3(randomX, randomY, randomZ), false, new Vector3(0, 0, 0), 0.8f));
                
                //If it's between 101-200 then nothing will happen, no spawn
            }
        }

        private void collisionBounce(int sphereChoice, Vector3 oldPosition, Vector3 transformVec)
        {
            mSphereStorage[sphereChoice] = new mSphere(oldPosition, mSphereStorage[sphereChoice].colourRed, mSphereStorage[sphereChoice].velocity, mSphereStorage[sphereChoice].radius);

            float coefficient = 0.7f;

            Vector3 normal = Vector3.Transform(new Vector3(transformVec), mEmitterBox.ExtractRotation());
            mSphereStorage[sphereChoice] = new mSphere(mSphereStorage[sphereChoice].position, mSphereStorage[sphereChoice].colourRed, mSphereStorage[sphereChoice].velocity - (1 + coefficient) * Vector3.Dot(normal, mSphereStorage[sphereChoice].velocity) * normal, mSphereStorage[sphereChoice].radius);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uTextureChoice = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureChoice");
            GL.Uniform1(uTextureChoice, 0);

            //EmitterBox
            GL.CullFace(CullFaceMode.Front);

            setMaterials(new Vector3(0.2125f, 0.1275f, 0.027451f), new Vector3(0.780392f, 0.568627f, 0.113725f), new Vector3(0.992157f, 0.941176f, 0.807843f), 0.21794872f);
            
            renderObject(mEmitterBox, 0, mCubeIndices.Length - 6, 0);

            //Spheres
            if (mSphereStorage.Count >= 1)
            {
                GL.CullFace(CullFaceMode.Back);

                for (int i = 0; i < mSphereStorage.Count; i++)
                {
                    if (mSphereStorage[i].colourRed)
                        setMaterials(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.7f, 0.0f, 0.0f), new Vector3(0.0225f, 0.022f, 0.022f), 12.8f);
                    else
                        setMaterials(new Vector3(0.01f, 0.1f, 0.01f), new Vector3(0.13f, 0.55f, 0.13f), new Vector3(0.633f, 0.727811f, 0.633f), 0.2f);

                    renderObject(mSphereStorage[i].matrix, 2, mSphereUtility.Indices.Length, 0);
                }
            }

            GL.Uniform1(uTextureChoice, 1);

            //GridBox - Level 1
            GL.CullFace(CullFaceMode.Front);

            setMaterials(new Vector3(0.2125f, 0.1275f, 0.027451f), new Vector3(0.780392f, 0.568627f, 0.113725f), new Vector3(0.992157f, 0.941176f, 0.807843f), 0.21794872f);
            
            renderObject(mGridBoxL1, 0, mCubeIndices.Length - 12, 6 * sizeof(int));

            //Cylinder 1 to 4
            GL.CullFace(CullFaceMode.Back);

            setMaterials(new Vector3(0.24725f, 0.1995f, 0.0745f), new Vector3(0.75164f, 0.60648f, 0.22648f), new Vector3(0.628281f, 0.555802f, 0.366065f), 0.4f);

            for (int i = 0; i < mCylinderStorage.Count-2; i++)
            {
                renderObject(mCylinderStorage[i].matrix, 1, mCylinderUtility.Indices.Length, 0);
            }

            GL.Uniform1(uTextureChoice, 2);

            //GridBox - Level 2
            GL.CullFace(CullFaceMode.Front);

            setMaterials(new Vector3(0.2125f, 0.1275f, 0.027451f), new Vector3(0.780392f, 0.568627f, 0.113725f), new Vector3(0.992157f, 0.941176f, 0.807843f), 0.21794872f);
            
            renderObject(mGridBoxL2, 0, mCubeIndices.Length - 12, 6 * sizeof(int));

            //Cylinder 5 to 6
            GL.CullFace(CullFaceMode.Back);

            setMaterials(new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.4f, 0.4f, 0.4f), new Vector3(0.774597f, 0.774597f, 0.774597f), 0.6f);

            for (int i = 4; i < mCylinderStorage.Count; i++)
            {
                renderObject(mCylinderStorage[i].matrix, 1, mCylinderUtility.Indices.Length, 0);
            }

            GL.Uniform1(uTextureChoice, 3);

            //SphereOfDoomBox
            GL.CullFace(CullFaceMode.Front);

            setMaterials(new Vector3(0.2125f, 0.1275f, 0.027451f), new Vector3(0.780392f, 0.568627f, 0.113725f), new Vector3(0.992157f, 0.941176f, 0.807843f), 0.21794872f);

            renderObject(mDoomBox, 0, mCubeIndices.Length - 12, 6 * sizeof(int));

            //Sphere of Doom
            GL.CullFace(CullFaceMode.Back);

            mDoomSphereModel = Matrix4.CreateScale(mDoomSphereRadius) * Matrix4.CreateTranslation(mDoomSpherePosition);

            setMaterials(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.58f, 0.44f, 0.86f), new Vector3(0.0225f, 0.022f, 0.022f), 12.8f);

            renderObject(mDoomSphereModel, 2, mSphereUtility.Indices.Length, 0);

            //PortalBox
            GL.CullFace(CullFaceMode.Front);

            GL.Uniform1(uTextureChoice, 4);

            setMaterials(new Vector3(0.2125f, 0.1275f, 0.027451f), new Vector3(0.780392f, 0.568627f, 0.113725f), new Vector3(0.992157f, 0.941176f, 0.807843f), 0.21794872f);

            renderObject(mPortalBox, 0, mCubeIndices.Length - 6, 6 * sizeof(int));

            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");

            //Portal 1
            GL.CullFace(CullFaceMode.Front);

            GL.Uniform1(uTextureChoice, 5);

            GL.UniformMatrix4(uModel, true, ref mPortal);

            GL.BindVertexArray(mVertexArrayObjectIDArray[3]);
            GL.DrawElements(PrimitiveType.TriangleStrip, mPortalStripIndices.Length, DrawElementsType.UnsignedInt, 0);

            //Portal 2
            Matrix4 portal2 = Matrix4.CreateRotationX(1.57f) * Matrix4.CreateRotationY(-1.57f) * Matrix4.CreateTranslation(4.8f, 46, 0);

            GL.UniformMatrix4(uModel, true, ref portal2);

            GL.BindVertexArray(mVertexArrayObjectIDArray[3]);
            GL.DrawElements(PrimitiveType.TriangleStrip, mPortalStripIndices.Length, DrawElementsType.UnsignedInt, 0);

            //^^ Just demonstraiting multiple drawing primitives by drawing these seperatly with a TriangleStrip ^^

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        private void setMaterials(Vector3 ambientColour, Vector3 diffuseColour, Vector3 specularColour, float shininess)
        {
            //Material Properties
            int uAmbientReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            GL.Uniform3(uAmbientReflectivityLocation, ambientColour);

            int uDiffuseReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            GL.Uniform3(uDiffuseReflectivityLocation, diffuseColour);

            int uSpecularReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            GL.Uniform3(uSpecularReflectivityLocation, specularColour);

            int uMaterialShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(uMaterialShininessLocation, shininess);
        }

        private void renderObject(Matrix4 refMatrix, int objectID, int drawCount, int offSet)
        {
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");

            GL.UniformMatrix4(uModel, true, ref refMatrix);

            GL.BindVertexArray(mVertexArrayObjectIDArray[objectID]);
            GL.DrawElements(PrimitiveType.Triangles, drawCount, DrawElementsType.UnsignedInt, offSet);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.DeleteBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}