using SharpGL;
using System.Numerics;

namespace Whim_GEometry_Editor.Misc
{
    internal class MousePicker
    {
        Matrix4x4 projectionMatrix;
        Matrix4x4 viewMatrix;
        public Vector3 WorldRay;

        public MousePicker(OpenGL gl, float actualWidth, float actualHeight, Vector2 mousePos)
        {
            // Retrieve projection and modelview matrices from OpenGL
            float[] projectionMatrixArray = new float[16];
            gl.GetFloat(OpenGL.GL_PROJECTION_MATRIX, projectionMatrixArray);
            projectionMatrix = new Matrix4x4(
                projectionMatrixArray[0], projectionMatrixArray[1], projectionMatrixArray[2], projectionMatrixArray[3],
                projectionMatrixArray[4], projectionMatrixArray[5], projectionMatrixArray[6], projectionMatrixArray[7],
                projectionMatrixArray[8], projectionMatrixArray[9], projectionMatrixArray[10], projectionMatrixArray[11],
                projectionMatrixArray[12], projectionMatrixArray[13], projectionMatrixArray[14], projectionMatrixArray[15]);

            float[] viewMatrixArray = new float[16];
            gl.GetFloat(OpenGL.GL_MODELVIEW_MATRIX, viewMatrixArray);
            viewMatrix = new Matrix4x4(
                viewMatrixArray[0], viewMatrixArray[1], viewMatrixArray[2], viewMatrixArray[3],
                viewMatrixArray[4], viewMatrixArray[5], viewMatrixArray[6], viewMatrixArray[7],
                viewMatrixArray[8], viewMatrixArray[9], viewMatrixArray[10], viewMatrixArray[11],
                viewMatrixArray[12], viewMatrixArray[13], viewMatrixArray[14], viewMatrixArray[15]);

            // Calculate the ray based on the mouse position and window dimensions
            CalculateMouseRay(mousePos, actualWidth, actualHeight);
        }

        // Calculate the mouse ray in world space
        private void CalculateMouseRay(Vector2 mousePos, float actualWidth, float actualHeight)
        {
            float mouseX = mousePos.X;
            float mouseY = mousePos.Y;

            Vector2 normalizedDeviceCoords = GetNormalizedDeviceCoordinates(mouseX, mouseY, actualWidth, actualHeight);
            Vector4 clipCoords = new Vector4(normalizedDeviceCoords.X, normalizedDeviceCoords.Y, -1, 1);
            Vector4 eyeCoords = ToEyeCoords(clipCoords);
            WorldRay = ToWorldSpace(eyeCoords);
        }

        // Convert 2D mouse coordinates to normalized device coordinates
        private Vector2 GetNormalizedDeviceCoordinates(float mouseX, float mouseY, float actualWidth, float actualHeight)
        {
            // Convert mouse position to OpenGL's normalized device coordinates (-1 to 1)
            float x = (2f * mouseX) / actualWidth - 1f;
            float y = 1f - (2f * mouseY) / actualHeight; // OpenGL's Y is inverted compared to window coordinates
            return new Vector2(x, y);
        }

        // Convert from clip space to eye space
        private Vector4 ToEyeCoords(Vector4 clipCoords)
        {
            // Invert the projection matrix
            Matrix4x4 invertedProjectionMatrix;
            Matrix4x4.Invert(projectionMatrix, out invertedProjectionMatrix);

            // Transform the clip coordinates by the inverted projection matrix
            Vector4 eyeCoords = Vector4.Transform(clipCoords, invertedProjectionMatrix);

            // We only care about the direction, so we set z to -1 and w to 0 (for a ray)
            return new Vector4(eyeCoords.X, eyeCoords.Y, -1f, 0f);
        }

        // Convert from eye space to world space
        private Vector3 ToWorldSpace(Vector4 eyeCoords)
        {
            // Invert the view matrix
            Matrix4x4 invertedViewMatrix;
            Matrix4x4.Invert(viewMatrix, out invertedViewMatrix);

            // Transform the eye coordinates by the inverted view matrix to get the world ray
            Vector4 rayWorld = Vector4.Transform(eyeCoords, invertedViewMatrix);

            // Convert to a 3D vector and normalize
            Vector3 worldRay = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);
            worldRay = Vector3.Normalize(worldRay);

            return worldRay;
        }
    }
}
